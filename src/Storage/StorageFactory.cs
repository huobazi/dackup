using Microsoft.Extensions.DependencyInjection;

using Dackup.Configuration;
using Dackup.Extensions;

namespace Dackup.Storage
{
    public static class StorageFactory
    {   
        public static LocalStorage CreateLocalStorage(StorageConfig storageConfig)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<LocalStorage>();

            storageConfig.OptionList.NullSafeSetTo<string>(s => task.Path = s, "path");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.RemoveThreshold = Utils.ConvertRemoveThresholdToDateTime(s), "remove_threshold");
                    
            return task;
        }
        public static FTPStorage CreateFTPStorage(StorageConfig storageConfig)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<FTPStorage>();

            storageConfig.OptionList.NullSafeSetTo<string>(s => task.Host = s, "host");
            storageConfig.OptionList.NullSafeSetTo<int>(s => task.Port = s, "port");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.UserName = s, "username");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.Password = s, "password");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.Path = s, "path");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.RemoveThreshold = Utils.ConvertRemoveThresholdToDateTime(s), "remove_threshold");

            return task;
        }
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

        public static SCPStorage CreateSCPStorage(StorageConfig storageConfig)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<SCPStorage>();

            storageConfig.OptionList.NullSafeSetTo<string>(s => task.Host = s, "host");
            storageConfig.OptionList.NullSafeSetTo<int>(s => task.Port = s, "port");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.UserName = s, "username");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.Password = s, "password");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.Path = s, "path");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.PrivateKeyFile = s, "private_key_file");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.RemoveThreshold = Utils.ConvertRemoveThresholdToDateTime(s), "remove_threshold");

            return task;
        }
    }
}