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

        private string path;
        public DateTime? RemoveThreshold { get; set; }
        protected override ILogger Logger
        {
            get
            {
                return this.logger;
            }
        }
        private LocalStorage(){}
        public LocalStorage(ILogger logger, string path)
        {
            this.logger = logger;
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("path can not be null or empty.");
            }
            this.path = path;
        }
        protected override UploadResult Upload(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            System.IO.Directory.CreateDirectory(this.path);
            System.IO.File.Copy(fileName, Path.Combine(this.path, fileInfo.Name));

            return new UploadResult();
        }
        protected override PurgeResult Purge()
        {
            if (RemoveThreshold == null || RemoveThreshold.Value > DateTime.Now)
            {
                return new PurgeResult();
            }

            logger.LogInformation($"Purge to local  removeThreshold: {RemoveThreshold}");

            System.IO.DirectoryInfo di = new DirectoryInfo(path);

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