using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using BlazorApp2.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BlazorApp2.Data
{
    public class S3Service : IS3Service
    {
        public bool IsTest { get; set; } = false;
        private readonly ILogger _logger;
        private readonly IAmazonS3 _client;
        public string BucketName { get; private set; }
        public S3Service(IAmazonS3 client, ILogger<S3Service> logger, string bucketName = "igi-project", bool isTest = false)
        {
            IsTest = isTest;
            _logger = logger;
            //_client = client;
            _client = new AmazonS3Client(@"AKIAJIWS43VCUBTJTIHA", @"BeDW76mYMgTjLUVyQ//WK1uo7qw43z82ldegxwoE", RegionEndpoint.EUCentral1);
            //_client = new AmazonS3Client(new BasicAWSCredentials("kek", "lol"), new AmazonS3Config
            //{
            //    ServiceURL = "http://192.168.99.100:4572",
            //    // Localstack supports HTTP only
            //    UseHttp = true,
            //    // Force bucket name go *after* hostname 
            //    ForcePathStyle = true,
            //    AuthenticationRegion = "eu-central-1"
            //});
            BucketName = bucketName;
            CreateBucketAsync().Wait();
            InitBucketAsync().Wait();
        }

        public async Task CreateBucketAsync()
        {
            try
            {
                if (await AmazonS3Util.DoesS3BucketExistAsync(_client, BucketName) == false)
                {
                    var putBucketRequest = new PutBucketRequest
                    {
                        BucketName = BucketName,
                        UseClientRegion = true
                    };
                    var response = await _client.PutBucketAsync(putBucketRequest);
                    _logger.LogInformation("Created Bucket - BucketName: {0}", BucketName);
                }
                else
                    _logger.LogInformation("Bucket is exist - BucketName: {0}", BucketName);
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogInformation(e.Message);
                throw;
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
            }
        }

        public async Task InitBucketAsync()
        {
            await CreateFolderAsync("Users/");
            await CreateFolderAsync("TempFiles/");
        }

        public async Task CreateUserAsync(string username)
        {
            await CreateFolderAsync($"Users/{username}/");
            await CreateFolderAsync($"Users/{username}/Files/");
            await CreateFolderAsync($"Users/{username}/Settings/");
            await CreateFolderAsync($"Users/{username}/Account/");

            //var deleteObjectRequest = new DeleteObjectRequest
            //{
            //    BucketName = BucketName,
            //    Key = tempPath
            //};
            //await _client.DeleteObjectAsync(deleteObjectRequest);

        }

        public async Task DeleteFilesAsync(string fullPathName)
        {
            var S3ListFiles = await ListFilesAsync(fullPathName);

            if(S3ListFiles.Count() > 0)
            {
                var keysAndVersions = await PutObjectsAsync(S3ListFiles);

                DeleteObjectsRequest multiObjectDeleteRequest = new DeleteObjectsRequest
                {
                    BucketName = BucketName,
                    Objects = keysAndVersions // This includes the object keys and null version IDs.
                };
                // You can add specific object key to the delete request using the .AddKey.
                // multiObjectDeleteRequest.AddKey("TickerReference.csv", null);
                try
                {
                    DeleteObjectsResponse response = await _client.DeleteObjectsAsync(multiObjectDeleteRequest);
                    _logger.LogInformation("Successfully deleted all the {0} items", response.DeletedObjects.Count);
                }
                catch (DeleteObjectsException e)
                {
                    PrintDeletionErrorStatus(_logger, e);
                }
            }
            
        }

        private static void PrintDeletionErrorStatus(ILogger _logger, DeleteObjectsException e)
        {
            // var errorResponse = e.ErrorResponse;
            DeleteObjectsResponse errorResponse = e.Response;
            _logger.LogError("x {0}", errorResponse.DeletedObjects.Count);

            _logger.LogError("No. of objects successfully deleted = {0}" + Environment.NewLine +
                            "No. of objects failed to delete = {1}" + Environment.NewLine + "Printing error data...", 
                            errorResponse.DeletedObjects.Count, errorResponse.DeleteErrors.Count);

            foreach (DeleteError deleteError in errorResponse.DeleteErrors)
            {
                Console.WriteLine("Object Key: {0}\t{1}\t{2}", deleteError.Key, deleteError.Code, deleteError.Message);
            }
        }

        private async Task<List<KeyVersion>> PutObjectsAsync(IEnumerable<S3Object> objes)
        {
            var keys = new List<KeyVersion>();

            foreach(var obj in objes)
            {
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = BucketName,
                    Key = obj.Key,
                    //ContentBody = "This is the content body!",

                };

                var response = await _client.PutObjectAsync(request);
                KeyVersion keyVersion = new KeyVersion
                {
                    Key = obj.Key,
                    VersionId = response.VersionId
                };

                keys.Add(keyVersion);
            }
            return keys;
        }

        public async Task UploadObjectAsync(Stream stream, string pathObject)
        {
            try
            {
                //var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                //var user = authState.User;
                var fileTransferUtility = new TransferUtility(_client);
                await fileTransferUtility.UploadAsync(stream, BucketName, pathObject);
                if(pathObject[pathObject.Length-1] == '/')
                    _logger.LogInformation("Uploaded Folder in Bucket: {0}, PathDir: {1}", BucketName, pathObject);
                else
                    _logger.LogInformation("Uploaded object in Bucket: {0}, PathObject: {1}", BucketName, pathObject);
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogError("Error encountered on server. Message: '{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("Unknown Exception {0}", e.Message);
                throw;
            }
        }

        public async Task<IEnumerable<S3Object>> ListFilesAsync(string dirpath)
        {
            ListObjectsRequest request = new ListObjectsRequest
            {
                BucketName = BucketName,
                MaxKeys = 1000,
                Prefix = dirpath
            };
            ListObjectsResponse response;
            do
            {
                response = await _client.ListObjectsAsync(request);
                //foreach (S3Object entry in response.S3Objects)
                //{
                //    Console.WriteLine("==================== key = {0} size = {1}",
                //        entry.Key, entry.Size);
                //}
                _logger.LogInformation("List-Objects - Count: {}", response.S3Objects.Count);
                return response.S3Objects;
            } while (response.IsTruncated);

            //var lista = _client.ListObjectsAsync(BucketName, $"{username}/{"someone"}.").Result;
            //var files = lista.S3Objects.Select(x => (x.Key, x.Size));
            //var arquivos = files.Select(x => Path.GetFileName(x.Key)).ToList();
        }

        public string GeneratePreSignedURL(string filepath, bool attachment, string fileName = null, bool useHttp = false)
        {
            string urlString = "";
            try
            {
                GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest
                {
                    BucketName = BucketName,
                    Key = filepath,
                    Expires = DateTime.Now.AddMinutes(5),
                    Protocol = useHttp || IsTest ? Protocol.HTTP : Protocol.HTTPS
                };
                string typeget = attachment ? "attachment" : "inline";
                fileName = fileName ?? Path.GetFileName(filepath);
                request1.ResponseHeaderOverrides.ContentDisposition = $"{typeget}; filename={Uri.EscapeDataString(fileName)}";
                //Console.WriteLine("+++++++++++++++++++++" + request1.ResponseHeaderOverrides.ContentDisposition);
                urlString = _client.GetPreSignedURL(request1);
                _logger.LogInformation("Generated Link: {0}", urlString);
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogError("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            return urlString;
        }

        public async Task CreateFolderAsync(string newDirPath)
        {
            if (newDirPath[newDirPath.Length - 1] != '/')
                newDirPath = newDirPath + '/';
            await UploadObjectAsync(new MemoryStream(), newDirPath);
        }
    }





    

    
}
