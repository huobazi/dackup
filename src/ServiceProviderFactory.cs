using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;
namespace dackup
{
    public static class ServiceProviderFactory
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        static ServiceProviderFactory()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
        private static void ConfigureServices(IServiceCollection services)
        {
            var serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(DackupContext.Current.LogFile, rollingInterval: RollingInterval.Month, rollOnFileSizeLimit: true)
            .CreateLogger();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddSerilog(logger: serilogLogger, dispose: true);
            });

            // add backup tasks
            services.AddTransient<ArchiveBackupTask>();
            services.AddTransient<MongoDBBackupTask>();
            services.AddTransient<MsSqlBackupTask>();
            services.AddTransient<MySqlBackupTask>();
            services.AddTransient<PostgresBackupTask>();
            services.AddTransient<RedisBackupTask>();
            
            // add storage tasks
            services.AddTransient<LocalStorage>();
            services.AddTransient<AliyunOssStorage>();
            services.AddTransient<S3Storage>();

            // add notify tasks
            services.AddTransient<DingtalkRobotNotify>();
            services.AddTransient<HttpPostNotify>();
            services.AddTransient<SlackNotify>();
            services.AddTransient<SmtpEmailNotify>();

            // add dackup app
            services.AddSingleton<DackupApplication>();
        }
    }
}