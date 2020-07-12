using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using Renci.SshNet;

namespace Dackup.Storage
{
    public class SCPStorage : StorageBase
    {
        private readonly ILogger logger;
        private SCPStorage() { }
        public SCPStorage(ILogger<SCPStorage> logger) => this.logger = logger;
        protected override ILogger Logger => logger;
        public string Host { get; set; }
        public int Port { get; set; } = 22;
        public int Timeout { get; set; } = 30;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PrivateKeyFile { get; set; } = "~/.ssh/id_rsa";
        public string PrivateKeyPassphrase { get; set;}
        public string Path { get; set; } = "~/";
        public DateTime? RemoveThreshold { get; set; }
        protected override PurgeResult Purge()
        {
            if (RemoveThreshold == null || RemoveThreshold.Value > DateTime.Now)
            {
                return new PurgeResult();
            }
            if (string.IsNullOrWhiteSpace(Path))
            {
                Path = "~/";
            }

            logger.LogInformation($"Purge on SCP server {Host} removeThreshold: {RemoveThreshold}");

            if (RemoveThreshold == null || RemoveThreshold.Value > DateTime.Now)
            {
                return new PurgeResult();
            }
                
            var destPath = Utils.GetPathWithHome(this.Path);
            var scp      = CreateScpClient();

            using (var client = new SftpClient(scp.ConnectionInfo))
            {
                client.Connect();
                if (client.Exists(destPath))
                {
                    foreach (var file in from file in client.ListDirectory(destPath)
                                         where !file.IsDirectory && file.LastWriteTimeUtc.ToUniversalTime() <= RemoveThreshold.Value
                                         select file)
                    {
                        client.DeleteFile(file.FullName);
                    }
                }

                client.Disconnect();
            }

            return new PurgeResult();
        }
        protected override UploadResult Upload(string fileName)
        {
            if (string.IsNullOrWhiteSpace(this.Path))
            {
                Path = "~/";
            }

            var fileInfo = new System.IO.FileInfo(fileName);
            var destPath = Utils.GetPathWithHome(this.Path);

            using (var scp = CreateScpClient())
            {
                using (var sftp = new SftpClient(scp.ConnectionInfo))
                {
                    sftp.Connect();

                    if (!sftp.Exists(destPath))
                    {
                        logger.LogInformation($"Create the directory {destPath} on the remote server {Host}");
                        sftp.CreateDirectory(destPath);
                    }

                    sftp.Disconnect();
                }

                var destFile = System.IO.Path.Combine(destPath, System.IO.Path.GetFileName(fileName));
                scp.Connect();
                scp.Upload(fileInfo, destFile);
                scp.Disconnect();
            }

            logger.LogInformation($"Upload '{fileName}' by SCP to server {Host}");

            return new UploadResult();
        }
        private ScpClient CreateScpClient()
        {
            ScpClient client = null;

            if (!string.IsNullOrWhiteSpace(this.Password))
            {
                client = new ScpClient(this.Host, this.Port, this.UserName, this.Password);
            }
            else
            {
                var privateKeyFile = Utils.GetPathWithHome(this.PrivateKeyFile);
                client = new ScpClient(this.Host, this.Port, this.UserName, new PrivateKeyFile(privateKeyFile, this.PrivateKeyPassphrase));
            }

            client.OperationTimeout = TimeSpan.FromSeconds(this.Timeout);

            return client;
        }
    }
}