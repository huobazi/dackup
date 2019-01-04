using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Aliyun.OSS;

namespace dackup
{
    public class AliyunOssStorage : IStorage
    {

        private string endpoint, accessKeyId, accessKeySecret, bucketName, pathPrefix;

        private DateTime removeThreshold;

        private AliyunOssStorage(){}
        public AliyunOssStorage(string endpoint, string accessKeyId, string accessKeySecret, string bucketName, string pathPrefix, DateTime removeThreshold)
        {
            this.endpoint = endpoint;
            this.accessKeyId = accessKeyId;
            this.accessKeySecret = accessKeySecret;
            this.bucketName = bucketName;
            this.pathPrefix = pathPrefix;
            this.removeThreshold = removeThreshold;
        }
        public Task Upload(string fileName)
        {
            return Task.Run(() =>
            {
                OssClient client = new OssClient(endpoint, accessKeyId, accessKeySecret);
                string key = fileName;
                client.PutObject(bucketName, key, pathPrefix);
            });
        }

        public Task Purge()
        {
            return Task.Run(() =>
            {
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
                    Console.WriteLine("Nothing to purge.");
                }
                else
                {
                    Console.WriteLine("Prepare to purge...");
                    objectsToDelete.ForEach(item =>
                    {
                        Console.WriteLine(item);
                    });
                }

                DeleteObjectsRequest request = new DeleteObjectsRequest(bucketName, objectsToDelete);
                client.DeleteObjects(request);
            });
        }
    }
}