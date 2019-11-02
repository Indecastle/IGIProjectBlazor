using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using BlazorApp2.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp2.Data
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _client;
        public string BucketName => "igi-project";
        public S3Service(IAmazonS3 client)
        {
            _client = client;
        }

        public async Task CreateBucketAsync(string bucketname)
        {
            try
            {
                if (await AmazonS3Util.DoesS3BucketExistAsync(_client, bucketname) == false)
                {
                    var putBucketRequest = new PutBucketRequest
                    {
                        BucketName = bucketname,
                        UseClientRegion = true
                    };
                    var response = await _client.PutBucketAsync(putBucketRequest);
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task CreateUserAsync(string username)
        {
            string tempPath = username + "/";
            var fileTransferUtility = new TransferUtility(_client);
            await fileTransferUtility.UploadAsync(new MemoryStream(), BucketName, tempPath);

            //var deleteObjectRequest = new DeleteObjectRequest
            //{
            //    BucketName = BucketName,
            //    Key = tempPath
            //};
            //await _client.DeleteObjectAsync(deleteObjectRequest);

        }

        public async Task DeleteFilesAsync(IS3Object obj)
        {
            var S3ListFiles = await ListFilesAsync(obj.FullPathName);

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
                Console.WriteLine("Successfully deleted all the {0} items", response.DeletedObjects.Count);
            }
            catch (DeleteObjectsException e)
            {
                PrintDeletionErrorStatus(e);
            }
        }

        private static void PrintDeletionErrorStatus(DeleteObjectsException e)
        {
            // var errorResponse = e.ErrorResponse;
            DeleteObjectsResponse errorResponse = e.Response;
            Console.WriteLine("x {0}", errorResponse.DeletedObjects.Count);

            Console.WriteLine("No. of objects successfully deleted = {0}", errorResponse.DeletedObjects.Count);
            Console.WriteLine("No. of objects failed to delete = {0}", errorResponse.DeleteErrors.Count);

            Console.WriteLine("Printing error data...");
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
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message: '{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown Exception {0}", e.Message);
                throw;
            }
        }

        public async Task<IEnumerable<S3Object>> ListFilesAsync(string dirpath)
        {
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = BucketName,
                MaxKeys = 1000,
                Prefix = dirpath
            };
            ListObjectsV2Response response;
            do
            {
                response = await _client.ListObjectsV2Async(request);
                //foreach (S3Object entry in response.S3Objects)
                //{
                //    Console.WriteLine("==================== key = {0} size = {1}",
                //        entry.Key, entry.Size);
                //}
                return response.S3Objects;
            } while (response.IsTruncated);

            //var lista = _client.ListObjectsAsync(BucketName, $"{username}/{"someone"}.").Result;
            //var files = lista.S3Objects.Select(x => (x.Key, x.Size));
            //var arquivos = files.Select(x => Path.GetFileName(x.Key)).ToList();
        }

        public string GeneratePreSignedURL(string filepath)
        {
            string urlString = "";
            try
            {
                GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest
                {
                    BucketName = BucketName,
                    Key = filepath,
                    Expires = DateTime.Now.AddMinutes(5)
                };
                urlString = _client.GetPreSignedURL(request1);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
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
