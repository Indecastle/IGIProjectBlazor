﻿using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp2.Data
{
    public interface IS3Service
    {
        Task CreateBucketAsync(string bucketname);
        Task CreateUserAsync(string username);
        Task UploadBucketAsync(Stream stream, string username, string filename);
        Task DeleteFilesAsync(IS3Object obj);
        Task<IEnumerable<S3Object>> ListFilesAsync(string dirpath);
        string GeneratePreSignedURL(string filepath);
    }
}
