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

            services.AddScoped<BackupTaskFactory>();
            
            services.AddScoped<MongoDBBackupTask>();
            services.AddScoped<MsSqlBackupTask>();
            services.AddScoped<MySqlBackupTask>();
            services.AddScoped<PostgresBackupTask>();

            services.AddScoped<StorageFactory>();
            services.AddScoped<NotifyFactory>();

            // add dackup app
            services.AddTransient<DackupApplication>();
        }
    }
}