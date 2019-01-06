using System;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace dackup
{
    public class S3Storage : StorageBase
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
        protected override UploadResult Upload(string fileName)
        {    
            return new UploadResult();
        }

        protected override PurgeResult Purge()
        {
            return new PurgeResult();
        }
    }
}