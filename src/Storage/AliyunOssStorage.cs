using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using Aliyun.OSS;

namespace dackup
{
    public class AliyunOssStorage : StorageBase
    {
        private ILogger logger;
        public string Endpoint { get; set; }
        public string AccessKeyId { get; set; }
        public string AccessKeySecret { get; set; }
        public string BucketName { get; set; }
        private AliyunOssStorage() { }
        public string PathPrefix { get; set; }
        public DateTime? RemoveThreshold { get; set; }
        protected override ILogger Logger
        {
            get { return logger; }
        }
        public AliyunOssStorage(ILogger<AliyunOssStorage> logger) => this.logger = logger;
        protected override UploadResult Upload(string fileName)
        {
            OssClient client = new OssClient(Endpoint, AccessKeyId, AccessKeySecret);
            string key = this.PathPrefix + $"/{DateTime.Now:yyyy_MM_dd_HH_mm_ss}/" + fileName.Replace(DackupContext.Current.TmpPath, string.Empty).TrimStart('/');
            key = key.Trim('/');

            logger.LogInformation($"Upload to aliyun file: {fileName} key: {key} pathPrefix: {this.PathPrefix}");

            client.PutObject(BucketName, key, fileName);
            
            return new UploadResult();
        }
        protected override PurgeResult Purge()
        {
            if (RemoveThreshold == null || RemoveThreshold.Value > DateTime.Now)
            {
                return new PurgeResult();
            }

            logger.LogInformation($"Purge to aliyun  removeThreshold: {RemoveThreshold}");

            OssClient client = new OssClient(Endpoint, AccessKeyId, AccessKeySecret);
            var objectListing = client.ListObjects(BucketName, this.PathPrefix);

            var objectsToDelete = new List<string>();

            foreach (var summary in objectListing.ObjectSummaries)
            {
                if (summary.LastModified.ToUniversalTime() <= RemoveThreshold.Value)
                {
                    objectsToDelete.Add(summary.Key);
                }
            }

            if (objectsToDelete.Count == 0)
            {
                logger.LogInformation("Nothing to purge.");
            }
            else
            {
                objectsToDelete.ForEach(item =>
                {
                    logger.LogInformation($"Prepare to purge: {item}");
                });

                DeleteObjectsRequest request = new DeleteObjectsRequest(BucketName, objectsToDelete);
                client.DeleteObjects(request);

                logger.LogInformation("Aliyun oss purge done.");
            }

            return new PurgeResult();
        }
    }
}