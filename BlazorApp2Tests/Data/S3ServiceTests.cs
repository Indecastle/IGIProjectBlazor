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

namespace BlazorApp2.Data.Tests
{
    [TestClass()]
    public class S3ServiceTests
    {
        private const string _bucketName = "igi-project-tests";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUCentral1;
        private static IAmazonS3 s3Client;
        IS3Service _s3;

        [TestInitialize()]
        public void Setup()
        {
            s3Client = new AmazonS3Client(@"AKIAJIWS43VCUBTJTIHA", @"BeDW76mYMgTjLUVyQ//WK1uo7qw43z82ldegxwoE", bucketRegion);
            _s3 = new S3Service(s3Client, _bucketName);
            _s3.CreateBucketAsync(_bucketName).Wait();
        }

        [TestCleanup()]
        public void End()
        {
            _s3.DeleteFilesAsync("").Wait();
            s3Client.DeleteBucketAsync(_bucketName).Wait();
        }


        [TestMethod()]
        public void CreateBucketAsyncTest()
        {
            _s3.CreateBucketAsync(_bucketName).Wait();
            if (AmazonS3Util.DoesS3BucketExistAsync(s3Client, _bucketName).Result)
            {
                s3Client.DeleteBucketAsync(_bucketName).Wait();
                Assert.IsTrue(true);
                return;
            }
            Assert.Fail();
        }


        [TestMethod()]
        public void CreateUserAsyncTest()
        {
            _s3.CreateUserAsync("testUser").Wait();

            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                MaxKeys = 1000,
                Prefix = "Users/testUser"
            };
            ListObjectsV2Response response;
            response = s3Client.ListObjectsV2Async(request).Result;
            var listObj = response.S3Objects;
            string[] dirs = new string[] { "Users/testUser/", "Users/testUser/Files/", "Users/testUser/Settings/", "Users/testUser/Account/" };
            var result = dirs.All(d => listObj.Any(o => o.Key == d));
            Assert.AreEqual(result, true);
        }
    }
}





namespace BlazorApp2Tests.Data
{
    class S3ServiceTests
    {
    }
}
