using System;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

using FluentFTP;

using Microsoft.Extensions.Logging;

namespace Dackup.Storage
{
    public class FTPStorage : StorageBase
    {
        private readonly ILogger logger;
        private FTPStorage(){}
        public FTPStorage(ILogger<FTPStorage> logger) => this.logger = logger;
        protected override ILogger Logger => logger;      
        public string Host { get; set; }
        public int Port { get; set; } = 21;
        public int Timeout { get; set; } = 30;
        public string UserName { get; set; }
        public string Password { get; set; } 
        public string Path { get; set; } = "/";
        public DateTime? RemoveThreshold { get; set; }
        public override async Task<PurgeResult> PurgeAsync()
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                Path = "/";
            }

            logger.LogInformation($"Purge to FTP  removeThreshold: {RemoveThreshold}");

            if (RemoveThreshold == null || RemoveThreshold.Value > DateTime.Now)
            {
                return new PurgeResult();
            }
            
            var client = await CreateFtpClient();

            var filesToPurge = (await client.GetListingAsync(this.Path, FtpListOption.AllFiles))
                                            .Where(item => item.Type == FtpFileSystemObjectType.File)
                                            .Where(item => client.GetModifiedTime(item.FullName).ToUniversalTime() <= RemoveThreshold.Value)
                                            .ToList();
            if (filesToPurge.Count() == 0)
            {
                logger.LogInformation("Nothing to purge.");
            }
            else
            {
                List<Task> tasks = new List<Task>();
                foreach (var item in filesToPurge)
                {
                    logger.LogInformation($"Prepare to purge: {item.FullName}");
                    tasks.Add(client.DeleteFileAsync(item.FullName));
                }
                await Task.WhenAll(tasks);
            }
            logger.LogInformation("FTP purge done.");

            await client.DisconnectAsync();

            return new PurgeResult();
        }

        public override async Task<UploadResult> UploadAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                Path = "/";
            }
            var fileInfo = new System.IO.FileInfo(fileName);

            var client = await CreateFtpClient();

            logger.LogInformation($"Upload '{fileName}' to FTP");

            var status = await client.UploadFileAsync( localPath: fileName,
                                                remotePath: System.IO.Path.Combine(this.Path, fileInfo.Name),
                                                createRemoteDir: true);
            await client.DisconnectAsync();

            return new UploadResult();
            
        }
        protected override UploadResult Upload(string fileName)
        {
            throw new NotImplementedException();
        }
        protected override PurgeResult Purge()
        {
            throw new NotImplementedException();
        }
        private async Task<FtpClient> CreateFtpClient()
        {
            FtpClient client                              = new FtpClient(this.Host);
                      client.Port                         = this.Port;
                      client.Credentials                  = new NetworkCredential(this.UserName, this.Password);
                      client.DataConnectionConnectTimeout = this.Timeout * 1000;

            await client.ConnectAsync();
            
            return client;
        }
    }
}