using Microsoft.Extensions.Logging;

namespace dackup
{
    public class StorageFactory
    {
        private readonly ILogger logger;

        public StorageFactory(ILogger<StorageFactory> logger)
        {
            this.logger = logger;
        }
        public LocalStorage CreateLocalStorage(string path)
        {
            return new LocalStorage(this.logger, path);
        }
        public AliyunOssStorage CreateAliyunOssStorage(string endpoint, string accessKeyId, string accessKeySecret, string bucketName)
        {
            return new AliyunOssStorage(this.logger, endpoint, accessKeyId, accessKeySecret, bucketName);
        }
        public S3Storage CreateS3Storage(string region, string accessKeyId, string accessKeySecret, string bucket)
        {
            return new S3Storage(this.logger, region, accessKeyId, accessKeySecret, bucket);
        }
    }
}