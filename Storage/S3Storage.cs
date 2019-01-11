using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Serilog;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace dackup
{
    public class S3Storage : StorageBase
    {
        private string region, bucket, accessKeyId, accessKeySecret;
        private S3Storage() { }
        public DateTime? RemoveThreshold { get; set; }
        public string PathPrefix { get; set; }
        public S3Storage(string region, string accessKeyId, string accessKeySecret, string bucket)
        {
            this.region = region;
            this.accessKeyId = accessKeyId;
            this.accessKeySecret = accessKeySecret;
            this.bucket = bucket;
        }
        public override async Task<UploadResult> UploadAsync(string fileName)
        {
            Log.Information($"Dackup start [{this.GetType().Name }.UploadAsync]");

            using (var s3Client = new AmazonS3Client(this.accessKeyId, this.accessKeySecret, RegionEndpoint.GetBySystemName(this.region)))
            {
                var fileTransferUtility = new TransferUtility(s3Client);
                
                string key = this.PathPrefix + $"/{DateTime.Now:s}/" + fileName.Replace(DackupContext.Current.TmpPath,string.Empty).TrimStart('/');
                key = key.Trim('/');
            
                Log.Information($"Upload to s3 file: {fileName} key: {key}");

                await fileTransferUtility.UploadAsync(fileName, this.bucket, key);
                return new UploadResult();
            }
        }
        public override async Task<PurgeResult> PurgeAsync()
        {           
            Log.Information($"Purge to s3  removeThreshold: {RemoveThreshold}");

            if (RemoveThreshold == null || RemoveThreshold.Value > DateTime.Now)
            {
                return new PurgeResult();
            }

            using (var s3Client = new AmazonS3Client(this.accessKeyId, this.accessKeySecret, RegionEndpoint.GetBySystemName(this.region)))
            {
                var objectListing = s3Client.ListObjectsAsync(this.bucket, this.PathPrefix);
                await objectListing;
                var deleteRequest = new DeleteObjectsRequest();
                deleteRequest.BucketName = this.bucket;

                foreach (var s3Object in objectListing.Result.S3Objects)
                {
                    if (s3Object.LastModified.ToUniversalTime() <= RemoveThreshold.Value)
                    {
                        deleteRequest.AddKey(s3Object.Key);
                    }
                }

                if (deleteRequest.Objects.Count == 0)
                {
                    Log.Information("Nothing to purge.");
                }
                else
                {
                    deleteRequest.Objects.ForEach(item =>
                    {
                        Log.Information($"Prepare to purge: {item}");
                    });

                    await s3Client.DeleteObjectsAsync(deleteRequest);

                    Log.Information("S3 purge done.");
                }

                return new PurgeResult();
            }
        }

        protected override UploadResult Upload(string fileName)
        {
            throw new NotImplementedException();
        }
        protected override PurgeResult Purge()
        {
            throw new NotImplementedException();
        }

    }
}