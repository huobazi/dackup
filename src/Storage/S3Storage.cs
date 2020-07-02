using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace dackup
{
    public class S3Storage: StorageBase
    {
        private ILogger logger;
        private S3Storage() { }
        public string Region { get; set; }
        public string BucketName { get; set; }
        public string AccessKeyId { get; set; }
        public string AccessKeySecret { get; set; }
        public DateTime? RemoveThreshold { get; set; }
        public string PathPrefix { get; set; }
        protected override ILogger Logger
        {
            get { return logger; }
        }
        public S3Storage(ILogger<S3Storage> logger) => this.logger = logger;
        public override async Task<UploadResult> UploadAsync(string fileName)
        {
            logger.LogInformation($"Dackup start [{this.GetType().Name }.UploadAsync]");

            using (var s3Client = new AmazonS3Client(this.AccessKeyId, this.AccessKeySecret, RegionEndpoint.GetBySystemName(this.Region)))
            {
                var fileTransferUtility = new TransferUtility(s3Client);
                
                string key = this.PathPrefix + $"/{DateTime.Now:yyyy_MM_dd_HH_mm_ss}/" + fileName.Replace(DackupContext.Current.TmpPath,string.Empty).TrimStart('/');
                       key = key.Trim('/');
            
                logger.LogInformation($"Upload to s3 file: {fileName} key: {key}");

                await fileTransferUtility.UploadAsync(fileName, this.BucketName, key);
                
                return new UploadResult();
            }
        }
        public override async Task<PurgeResult> PurgeAsync()
        {           
            logger.LogInformation($"Purge to s3  removeThreshold: {RemoveThreshold}");

            if (RemoveThreshold == null || RemoveThreshold.Value > DateTime.Now)
            {
                return new PurgeResult();
            }

            using (var s3Client = new AmazonS3Client(this.AccessKeyId, this.AccessKeySecret, RegionEndpoint.GetBySystemName(this.Region)))
            {
                var objectListing = s3Client.ListObjectsAsync(this.BucketName, this.PathPrefix);
                await objectListing;
                var deleteRequest            = new DeleteObjectsRequest();
                    deleteRequest.BucketName = this.BucketName;

                foreach (var s3Object in objectListing.Result.S3Objects)
                {
                    if (s3Object.LastModified.ToUniversalTime() <= RemoveThreshold.Value)
                    {
                        deleteRequest.AddKey(s3Object.Key);
                    }
                }

                if (deleteRequest.Objects.Count == 0)
                {
                    logger.LogInformation("Nothing to purge.");
                }
                else
                {
                    deleteRequest.Objects.ForEach(item =>
                    {
                        logger.LogInformation($"Prepare to purge: {item.Key}");
                    });

                    await s3Client.DeleteObjectsAsync(deleteRequest);

                    logger.LogInformation("S3 purge done.");
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