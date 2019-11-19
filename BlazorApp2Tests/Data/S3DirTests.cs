using Microsoft.VisualStudio.TestTools.UnitTesting;
using BlazorApp2.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Amazon;
using Amazon.S3;
using Amazon.S3.Util;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Amazon.Runtime;

namespace BlazorApp2.Data.Tests
{
    [TestClass()]
    public class S3DirTests
    {
        private static readonly Logger<S3Service> _logger = new Logger<S3Service>(new LoggerFactory());
        private const string _bucketName = "igi-project-tests2";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUCentral1;
        private static IAmazonS3 s3Client;
        private static IS3Service _s3;
        public S3Dir _s3dir;
        string[] namefiles = new string[] { "TestFolder/",
                                                "TestFolder/file_1.txt",
                                                "TestFolder/file_2.jpg",
                                                "parentfile.kek"};

        [ClassInitialize]
        public static void StartInit(TestContext context)
        {
            //s3Client = new AmazonS3Client(@"AKIAJIWS43VCUBTJTIHA", @"BeDW76mYMgTjLUVyQ//WK1uo7qw43z82ldegxwoE", bucketRegion);
            s3Client = new AmazonS3Client(new BasicAWSCredentials("kek", "lol"), new AmazonS3Config
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
        public void S3DirTest_Service_UserName_TypeFile()
        {
            _s3dir = new S3Dir(_s3, "NoobMaster69", S3TypeFile.Files);
            Assert.AreEqual(_s3dir.rootPath, "Users/NoobMaster69/Files");
            Assert.AreEqual(_s3dir.UserName, "NoobMaster69");
            Assert.AreEqual(_s3dir.IsRoot, true);
            Assert.AreEqual(_s3dir.Level, 3);
            Assert.AreEqual(_s3dir.DirStack.Count, 0);
        }

        [TestMethod()]
        public void S3DirTest_Service_RootPath()
        {
            _s3dir = new S3Dir(_s3, "Test-Folder");
            Assert.AreEqual(_s3dir.rootPath, "Test-Folder");
            Assert.AreEqual(_s3dir.UserName, "");
            Assert.AreEqual(_s3dir.IsRoot, true);
            Assert.AreEqual(_s3dir.Level, 1);
            Assert.AreEqual(_s3dir.DirStack.Count, 0);
        }

        [TestMethod()]
        public void UpdateDirTest()
        {
            foreach (string filename in namefiles)
            {
                _s3.UploadObjectAsync(new MemoryStream(), filename).Wait();
            }

            _s3dir = new S3Dir(_s3, "");
            _s3dir.UpdateDirAsync().Wait();
            Assert.AreEqual(_s3dir.IsRoot, true);
            Assert.AreEqual(_s3dir.Level, 0);
            Assert.IsTrue(_s3dir.SubDirs.Any(o => o.Name == "TestFolder"));
            Assert.IsTrue(_s3dir.S3Objs.Any(o => o.Name == "parentfile.kek"));
        }

        [TestMethod()]
        public void SubDirTest()
        {
            foreach (string filename in namefiles)
            {
                _s3.UploadObjectAsync(new MemoryStream(), filename).Wait();
            }
            _s3dir = new S3Dir(_s3, "");
            _s3dir.UpdateDirAsync().Wait();
            _s3dir.SubDirAsync(_s3dir.SubDirs[0]).Wait();

            string[] dirs = new string[] { "file_1.txt", "file_2.jpg" };
            var isContains = dirs.All(d => _s3dir.S3Objs.Any(o => o.Name == d));
            Assert.AreEqual(isContains, true);
            Assert.AreEqual(!_s3dir.SubDirs.Any(), true);
            Assert.AreEqual(_s3dir.IsRoot, false);
            Assert.AreEqual(_s3dir.Level, 1);
            Assert.AreEqual(_s3dir.DirStack.Count, 1);
        }

        [TestMethod()]
        public void BackDirTest()
        {
            foreach (string filename in namefiles)
            {
                _s3.UploadObjectAsync(new MemoryStream(), filename).Wait();
            }
            _s3dir = new S3Dir(_s3, "");
            _s3dir.UpdateDirAsync().Wait();
            _s3dir.SubDirAsync(_s3dir.SubDirs[0]).Wait();
            _s3dir.BackDirAsync(1).Wait();

            Assert.AreEqual(_s3dir.IsRoot, true);
            Assert.AreEqual(_s3dir.Level, 0);
            Assert.IsTrue(_s3dir.SubDirs.Any(o => o.Name == "TestFolder"));
            Assert.IsTrue(_s3dir.S3Objs.Any(o => o.Name == "parentfile.kek"));
            Assert.AreEqual(_s3dir.DirStack.Count, 0);
        }

        [TestMethod()]
        public void CreateFolderTest()
        {
            foreach (string filename in namefiles)
            {
                _s3.UploadObjectAsync(new MemoryStream(), filename).Wait();
            }
            _s3dir = new S3Dir(_s3, "");
            _s3dir.UpdateDirAsync().Wait();
            _s3dir.SubDirAsync(_s3dir.SubDirs[0]).Wait();
            _s3dir.CreateFolderAsync("NewFolder").Wait();
            _s3dir.UpdateDirAsync().Wait();

            Assert.AreEqual(_s3dir.SubDirs.Count, 1);
        }

        [TestMethod()]
        public void UploadFileAsyncTest()
        {
            foreach (string filename in namefiles)
            {
                _s3.UploadObjectAsync(new MemoryStream(), filename).Wait();
            }
            _s3dir = new S3Dir(_s3, "");
            _s3dir.UpdateDirAsync().Wait();
            _s3dir.SubDirAsync(_s3dir.SubDirs[0]).Wait();
            _s3dir.CreateFolderAsync("NewFolder", true).Wait();
            _s3dir.SubDirAsync(_s3dir.SubDirs[0]).Wait();
            _s3dir.UploadFileAsync(new MemoryStream(), "NewFile.KEK", true).Wait();

            Assert.AreEqual(_s3dir.S3Objs.Count, 1);
        }
    }
}