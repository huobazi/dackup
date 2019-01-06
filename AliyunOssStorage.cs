using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Serilog;

using Aliyun.OSS;

namespace dackup
{
    public class AliyunOssStorage : StorageBase
    {
        private string endpoint, accessKeyId, accessKeySecret, bucketName, pathPrefix;

        private DateTime removeThreshold;

        private AliyunOssStorage() { }
        public AliyunOssStorage(string endpoint, string accessKeyId, string accessKeySecret, string bucketName, string pathPrefix, DateTime removeThreshold)
        {
            this.endpoint = endpoint;
            this.accessKeyId = accessKeyId;
            this.accessKeySecret = accessKeySecret;
            this.bucketName = bucketName;
            this.pathPrefix = pathPrefix;
            this.removeThreshold = removeThreshold;
        }
        protected override UploadResult Upload(string fileName)
        {
            OssClient client = new OssClient(endpoint, accessKeyId, accessKeySecret);
            string key = fileName;

            Log.Information($"Upload to aliyun oss: {fileName} key: {key} pathPrefix: {pathPrefix}");

            client.PutObject(bucketName, key, pathPrefix);
            return new UploadResult();
        }

        protected override PurgeResult Purge()
        {
            Log.Information($"Purge to aliyun  removeThreshold: {removeThreshold}");

            OssClient client = new OssClient(endpoint, accessKeyId, accessKeySecret);
            var objectListing = client.ListObjects(bucketName, pathPrefix);

            var objectsToDelete = new List<string>();

            foreach (var summary in objectListing.ObjectSummaries)
            {
                if (summary.LastModified.ToUniversalTime() <= removeThreshold)
                {
                    objectsToDelete.Add(summary.Key);
                }
            }

            if (objectsToDelete.Count == 0)
            {
                Log.Information("Nothing to purge.");
            }
            else
            {
                objectsToDelete.ForEach(item =>
                {
                    Log.Information($"Prepare to purge: {item}");
                });

                DeleteObjectsRequest request = new DeleteObjectsRequest(bucketName, objectsToDelete);
                client.DeleteObjects(request);

                Log.Information("Aliyun oss purge done.");
            }
            return new PurgeResult();
        }
    }
}