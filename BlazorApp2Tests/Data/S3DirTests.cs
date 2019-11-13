using Microsoft.VisualStudio.TestTools.UnitTesting;
using BlazorApp2.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Amazon;
using Amazon.S3;
using Amazon.S3.Util;

namespace BlazorApp2.Data.Tests
{
    [TestClass()]
    public class S3DirTests
    {
        private const string _bucketName = "igi-project-tests";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUCentral1;
        private static IAmazonS3 s3Client;
        IS3Service _s3;
        public S3Dir _s3dir;
        string[] namefiles = new string[] { "TestFolder/",
                                                "TestFolder/file_1.txt",
                                                "TestFolder/file_2.jpg",
                                                "parentfile.kek"};

        [TestInitialize()]
        public void Setup()
        {
            s3Client = new AmazonS3Client(@"AKIAJIWS43VCUBTJTIHA", @"BeDW76mYMgTjLUVyQ//WK1uo7qw43z82ldegxwoE", bucketRegion);
            _s3 = new S3Service(s3Client, _bucketName);
            _s3.CreateBucketAsync(_bucketName).Wait();
            _s3dir = new S3Dir(_s3, "");
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
        public void S3DirTest_Service_UserName_TypeFile()
        {
            _s3dir = new S3Dir(_s3, "Lox", S3TypeFile.Files);
           // pass
        }

        [TestMethod()]
        public void S3DirTest2()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateDirTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SubDirTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void BackDirTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CreateFolderTest()
        {
            Assert.Fail();
        }
    }
}