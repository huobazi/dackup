using Microsoft.Extensions.DependencyInjection;

using dackup.Configuration;
using dackup.Extensions;

namespace dackup
{
    public static class StorageFactory
    {   
        public static AliyunOssStorage CreateAliyunOssStorage(StorageConfig storageConfig)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<AliyunOssStorage>();

            storageConfig.OptionList.NullSafeSetTo<string>(s => task.Endpoint = s, "endpoint");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.AccessKeyId = s, "access_key_id");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.AccessKeySecret = s, "access_key_secret", "secret_access_key");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.BucketName = s, "bucket");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.PathPrefix = s, "path");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.RemoveThreshold = Utils.ConvertRemoveThresholdToDateTime(s), "remove_threshold");

            return task;
        }
        public static S3Storage CreateS3Storage(StorageConfig storageConfig)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<S3Storage>();

            storageConfig.OptionList.NullSafeSetTo<string>(s => task.PathPrefix = s, "path");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.Region = s, "region");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.AccessKeyId = s, "access_key_id");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.AccessKeySecret = s, "secret_access_key");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.BucketName = s, "bucket");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.RemoveThreshold = Utils.ConvertRemoveThresholdToDateTime(s), "remove_threshold");

            return task;
        }
    }
}