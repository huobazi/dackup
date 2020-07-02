using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

namespace dackup
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

            var fileInfo = new FileInfo(fileName);
            System.IO.Directory.CreateDirectory(this.Path);
            System.IO.File.Copy(fileName, System.IO.Path.Combine(this.Path, fileInfo.Name));

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

            foreach (FileInfo file in di.GetFiles())
            {
                if (file.LastWriteTime.ToUniversalTime() <= RemoveThreshold.Value)
                {
                    file.Delete();
                }
            }

            return new PurgeResult();
        }
    }
}