using Microsoft.VisualStudio.TestTools.UnitTesting;
using BlazorApp2.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Amazon;
using Amazon.S3;
using Amazon.S3.Util;
using Amazon.S3.Model;
using System.IO;
using Amazon.S3.Transfer;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Amazon.Runtime;
using System.Threading;

namespace BlazorApp2.Data.Tests
{
    [TestClass()]
    public class S3ServiceTests
    {
        private static readonly Logger<S3Service> _logger = new Logger<S3Service>(new LoggerFactory());
        private const string _bucketName = "igi-project-tests3";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUCentral1;
        private static IAmazonS3 s3Client;
        private static IS3Service _s3;
        string[] namefiles = new string[] { "TestFolder/",
                                                "TestFolder/file_1.txt",
                                                "TestFolder/file_2.jpg",
                                                "parentfile.kek"};

        [ClassInitialize]
        public static void StartInit(TestContext context)
        {
            //s3Client = new AmazonS3Client(@"AKIAJIWS43VCUBTJTIHA", @"BeDW76mYMgTjLUVyQ//WK1uo7qw43z82ldegxwoE", bucketRegion);
            s3Client = new AmazonS3Client(new BasicAWSCredentials("kek2", "lol2"), new AmazonS3Config
            {
                ServiceURL = "http://192.168.99.100:4572",
                // Localstack supports HTTP only
                UseHttp = true,
                // Force bucket name go *after* hostname 
                ForcePathStyle = true,
                AuthenticationRegion = "eu-central-1"
            });
            _s3 = new S3Service(s3Client, _logger, _bucketName);
        }

        [TestInitialize()]
        public void Setup()
        {
            _s3.CreateBucketAsync().Wait();
        }

        [TestCleanup()]
        public void End()
        {
            if (AmazonS3Util.DoesS3BucketExistAsync(s3Client, _bucketName).Result)
            {
                _s3.DeleteFilesAsync("").Wait();
                s3Client.DeleteBucketAsync(_bucketName).Wait();
            }
        }


        [TestMethod()]
        public void CreateBucketAsyncTest()
        {
            bool isOk = AmazonS3Util.DoesS3BucketExistAsync(s3Client, _bucketName).Result;
            Assert.IsTrue(isOk);
        }


        [TestMethod()]
        public void CreateUserAsyncTest()
        {
            _s3.CreateUserAsync("testUser").Wait();
            Thread.Sleep(100);

            ListObjectsRequest request = new ListObjectsRequest
            {
                BucketName = _bucketName,
                MaxKeys = 1000,
                Prefix = "Users/testUser"
            };
            ListObjectsResponse response;
            response = s3Client.ListObjectsAsync(request).Result;
            var listObj = response.S3Objects;
            string[] dirs = new string[] { "Users/testUser/", "Users/testUser/Files/", "Users/testUser/Settings/", "Users/testUser/Account/" };
            var result = dirs.All(d => listObj.Any(o => o.Key == d));
            Assert.AreEqual(result, true);
        }

        [TestMethod()]
        public void DeleteFilesAsyncTest()
        {
            var fileTransferUtility = new TransferUtility(s3Client);

            foreach (string filename in namefiles)
            {
                fileTransferUtility.UploadAsync(new MemoryStream(), _bucketName, filename).Wait();
            }
            Thread.Sleep(100);

            _s3.DeleteFilesAsync("TestFolder").Wait();
            Thread.Sleep(100);

            Assert.IsTrue(_s3.ListFilesAsync("TestFolder/").Result.Count() == 0);
        }

        [TestMethod()]
        public void ListFilesAsyncTest()
        {
            bool IsOk = false;
            var fileTransferUtility = new TransferUtility(s3Client);
            foreach (string filename in namefiles)
            {
                fileTransferUtility.UploadAsync(new MemoryStream(), _bucketName, filename).Wait();
            }
            Thread.Sleep(100);


            var listActual = _s3.ListFilesAsync("TestFolder").Result.Select(o => o.Key);

            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                MaxKeys = 1000,
                Prefix = "TestFolder"
            };
            var listExcepted = s3Client.ListObjectsV2Async(request).Result.S3Objects.Select(o => o.Key);

            if (listExcepted.Count() == listActual.Count())
            {
                var reslist = listExcepted.Except(listActual);
                if (!reslist.Any())
                {
                    IsOk = true;
                }
            }
            Assert.IsTrue(IsOk);
        }

        [TestMethod()]
        public void GeneratePreSignedURLTest()
        {
            bool IsOk = false;

            var fileTransferUtility = new TransferUtility(s3Client);
            foreach (string filename in namefiles)
            {
                fileTransferUtility.UploadAsync(new MemoryStream(Encoding.ASCII.GetBytes("Hello World")), _bucketName, filename).Wait();
            }
            Thread.Sleep(100);

            string Url = _s3.GeneratePreSignedURL("parentfile.kek", true, useHttp: true);
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(Url).Result;
                IsOk = response.StatusCode == System.Net.HttpStatusCode.OK && response.Content.Headers.ContentType.MediaType == "application/octet-stream";
            }
            Assert.IsTrue(IsOk);
        }

        [TestMethod()]
        public void CreateFolderAsyncTest()
        {
            bool IsOk = false;
            string testDirName = "TestCreateFolder/";
            _s3.CreateFolderAsync(testDirName).Wait();
            Thread.Sleep(100);

            var objlist = _s3.ListFilesAsync(testDirName).Result;
            if (objlist.Count() == 1)
            {
                IsOk = true;
            }

            Assert.IsTrue(IsOk);
        }

        [TestMethod()]
        public void InitBucketAsyncTest()
        {
            _s3.InitBucketAsync();
            Thread.Sleep(100);
            ListObjectsRequest request = new ListObjectsRequest
            {
                BucketName = _bucketName,
                MaxKeys = 1000,
                Prefix = ""
            };
            ListObjectsResponse response;
            response = s3Client.ListObjectsAsync(request).Result;
            var listObj = response.S3Objects;
            string[] dirs = new string[] { "TempFiles/", "Users/" };
            var result = dirs.All(d => listObj.Any(o => o.Key == d));
            Assert.AreEqual(result, true);
        }

        [TestMethod()]
        public void UploadObjectAsyncTest()
        {
            _s3.UploadObjectAsync(new MemoryStream(), "kek.KEK");
            Thread.Sleep(100);
            ListObjectsRequest request = new ListObjectsRequest
            {
                BucketName = _bucketName,
                MaxKeys = 1000,
                Prefix = "kek.KEK"
            };
            ListObjectsResponse response = s3Client.ListObjectsAsync(request).Result;

            Assert.IsTrue( response.S3Objects.Any(o => o.Key == "kek.KEK") );
        }
    }
}





namespace BlazorApp2Tests.Data
{
    class S3ServiceTests
    {
    }
}
