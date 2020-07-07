using System;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace Dackup.Storage
{
    public class LocalStorage : StorageBase
    {
        private readonly ILogger logger;
        public string Path { get; set; }
        public DateTime? RemoveThreshold { get; set; }
        protected override ILogger Logger
        {
            get
            {
                return this.logger;
            }
        }
        private LocalStorage(){}
        public LocalStorage(ILogger<LocalStorage> logger) => this.logger = logger;
        protected override UploadResult Upload(string fileName)
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                throw new ArgumentException("Path can not be null or empty.");
            }

            var destPath = System.IO.Path.Combine(this.Path, $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}");
            System.IO.Directory.CreateDirectory(destPath);

            var fileInfo = new FileInfo(fileName);
            System.IO.File.Copy(fileName, System.IO.Path.Combine(destPath ,fileInfo.Name));

            return new UploadResult();
        }
        protected override PurgeResult Purge()
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                throw new ArgumentException("Path can not be null or empty.");
            }

            if (RemoveThreshold == null || RemoveThreshold.Value > DateTime.Now)
            {
                return new PurgeResult();
            }

            logger.LogInformation($"Purge to local  removeThreshold: {RemoveThreshold}");

            System.IO.DirectoryInfo di = new DirectoryInfo(Path);
            var filesToPurge = di.GetFiles().Where(file => file.LastWriteTime.ToUniversalTime() <= RemoveThreshold.Value);
            if (filesToPurge.Count() == 0)
            {
                logger.LogInformation("Nothing to purge.");
            }
            else
            {
                foreach (var file in filesToPurge)
                {
                    file.Delete();
                }
            }
            
            return new PurgeResult();
        }
    }
}