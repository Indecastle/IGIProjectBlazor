using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
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
        public async Task UploadBucketAsync(Stream stream)
        {
            try
            {
                var fileTransferUtility = new TransferUtility(_client);
                await fileTransferUtility.UploadAsync(stream, BucketName, "FileStreamUpload");
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
    }
}
