﻿using Amazon.S3;
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
                if(await AmazonS3Util.DoesS3BucketExistAsync(_client, bucketname) == false)
                {
                    var putBucketRequest = new PutBucketRequest
                    {
                        BucketName = bucketname,
                        UseClientRegion = true
                    };
                    var response = await _client.PutBucketAsync(putBucketRequest);
                }
            }
            catch(AmazonS3Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

        }
        public async Task UploadBucketAsync(Stream stream, string username, string filename)
        {
            try
            {
                //var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                //var user = authState.User;
                var fileTransferUtility = new TransferUtility(_client);
                await fileTransferUtility.UploadAsync(stream, BucketName, username + "/" + filename);
            }
            catch(AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message: '{0}' when writing an object", e.Message);
            }
            catch(Exception e)
            {
                Console.WriteLine("Unknown Exception {0}", e.Message);
                throw; 
            }
        }

        public async Task<IEnumerable<S3Object>> ListFilesAsync(string username, string dirpath)
        {
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = BucketName,
                MaxKeys = 1000,
                Prefix = $"{username}/{dirpath}"
            };
            ListObjectsV2Response response;
            do
            {
                response = await _client.ListObjectsV2Async(request);
                foreach (S3Object entry in response.S3Objects)
                {
                    Console.WriteLine("==================== key = {0} size = {1}",
                        entry.Key, entry.Size);
                }
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
        public string DirPath { get; set; }
        public string DirName { get; set; }
        public string BackDirPath { get; set; }
        public string BackDirName { get; set; }
        public List<string> SubDirs { get; set; }
        bool IsRoot { get; set; }
        public List<S3FileObject> S3Objs { get; set; }
        public S3Dir()
        {
            S3Objs = new List<S3FileObject>();
        }
        public S3Dir(IEnumerable<S3Object> s3objects)
        {
            DirPath = DirName = BackDirName = BackDirPath = "";
            IsRoot = true;
            S3Dir resultDir = new S3Dir();
            foreach (S3Object obj in s3objects)
            {
                string[] ar = obj.Key.Split('/');
                S3FileObject fobj = new S3FileObject
                {
                    Name = ar[1]
                }
                resultDir.S3Objs.Add();
            }
        }
        private 
    }

    public class S3FileObject
    {
        public string Name { get; set; }
        public string Direcory { get; set; }
        public string FullPathName { get; set; }
        public long Size { get; set; }
        public string Owner { get; set; }
        public DateTime LastModified { get; set; }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector)
        {
            return enumerable.GroupBy(keySelector).Select(grp => grp.First());
        }
    }
}