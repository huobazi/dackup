using System;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace dackup
{
    public class S3Storage : IStorage
    {

        private string region, bucket, accessKeyId, accessKeySecret, pathPrefix;

        private DateTime removeThreshold;

        private S3Storage(){}
        public S3Storage(string region, string accessKeyId, string accessKeySecret, string bucket, string pathPrefix, DateTime removeThreshold)
        {
            this.region = region;
            this.accessKeyId = accessKeyId;
            this.accessKeySecret = accessKeySecret;
            this.bucket = bucket;
            this.pathPrefix = pathPrefix;
            this.removeThreshold = removeThreshold;
        }
        public Task Upload(string fileName)
        {
            return Task.Run(() =>
            {
                
            });
        }

        public Task Purge()
        {
            return Task.Run(() =>
            {
                
            });
        }
    }
}