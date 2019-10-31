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

        public async Task UploadBucketAsync(Stream stream, string dirpath, string filename)
        {
            try
            {
                //var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                //var user = authState.User;
                var fileTransferUtility = new TransferUtility(_client);
                await fileTransferUtility.UploadAsync(stream, BucketName, dirpath + "/" + filename);
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
    }





    public class S3Dir
    {
        public IS3Service _is3 { get; set; }
        public bool IsUpdating = false;
        public string UserName { get; private set; }
        public int Level { get; private set; }
        public S3DirObject CurrentDir { get; private set; }
        public S3DirObject OldDir { get; private set; }
        //public string DirPath { get; private set; }
        //public string DirName { get; private set; }
        public string BackDirPath { get; private set; }
        public string BackDirName { get; private set; }
        public bool IsRoot { get; private set; }
        public List<S3DirObject> SubDirs { get; private set; } = new List<S3DirObject>();
        public List<S3FileObject> S3Objs { get; private set; } = new List<S3FileObject>();
        public Stack<S3DirObject> DirStack { get; private set; } = new Stack<S3DirObject>();
        public S3Dir()
        {
            S3Objs = new List<S3FileObject>();
        }
        public S3Dir(IS3Service service, string userName)
        {
            _is3 = service;
            UserName = userName;
            //BackDirName = BackDirPath = "";
            //DirPath = DirName = userName;
            CurrentDir = new S3DirObject
            {
                Name = userName,
                FullPathName = userName
            };
            IsRoot = true;
            Level = 1;
            CurrentDir = new S3DirObject
            {
                Name = userName,
                FullPathName = userName
            };
        }

        public async Task UpdateDir()
        {
            SubDirs.Clear();
            S3Objs.Clear();

            var S3ListFiles = await _is3.ListFilesAsync(CurrentDir.FullPathName);
            IEnumerable<S3Object> diststrs = S3ListFiles.Where((s) =>
            {
                if (s.Key.StartsWith(CurrentDir.FullPathName))
                    return true;
                return false;
            }).DistinctBy((s) =>
            {
                string[] ar2 = s.Key.Split('/');
                return ar2[Level];
            });

            foreach (var s in diststrs)
            {
                string[] ar2 = s.Key.Split('/');
                if (ar2[Level] != "")
                    if (ar2.Length - 1 == Level)
                        S3Objs.Add(new S3FileObject
                        {
                            Name = ar2[Level],
                            DirPath = CurrentDir.FullPathName,
                            FullPathName = s.Key,
                            Owner = s.Owner,
                            Size = s.Size,
                            LastModified = s.LastModified,
                            ETag = s.ETag,
                            StorageClass = s.StorageClass.Value
                        });
                    else
                        SubDirs.Add(new S3DirObject
                        {
                            FullPathName = String.Join("/", ar2.Take(Level + 1)),
                            Name = ar2[Level]
                        });
            }
        }

        public async Task SubDir(S3DirObject subobj)
        {
            Level++;
            IsRoot = (Level == 1);
            OldDir = CurrentDir;
            DirStack.Push(CurrentDir);
            CurrentDir = subobj;

            await UpdateDir();
        }

        public async Task BackDir()
        {
            if (!IsRoot)
            {
                Level--;
                CurrentDir = DirStack.Pop();
                await UpdateDir();
            }
            if (Level == 1) // IsRoot
            {
                IsRoot = true;
                //BackDirName = BackDirPath = "";
            }
            else
            {
                OldDir = DirStack.Peek();
            }

        }
    }

    public class S3FileObject : IS3Object
    {
        public string Name { get; set; }
        public string DirPath { get; set; }
        public string FullPathName { get; set; }
        public long Size { get; set; }
        public Owner Owner { get; set; }
        public DateTime LastModified { get; set; }
        public string ETag { get; set; }
        public string StorageClass { get; set; }
    }

    public class S3DirObject : IS3Object
    {
        public string Name { get; set; }
        public string FullPathName { get; set; }
    }

    public interface IS3Object
    {
        string FullPathName { get; set; }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector)
        {
            return enumerable.GroupBy(keySelector).Select(grp => grp.First());
        }
    }
}
