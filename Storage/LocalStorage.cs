using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using Serilog;

namespace dackup
{
    public class LocalStorage: StorageBase
    {   
        private string path;
        public DateTime? RemoveThreshold {get;set;} 
        public LocalStorage(string path)
        {
            if(string.IsNullOrWhiteSpace(path))
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
            
            Log.Information($"Purge to local  removeThreshold: {RemoveThreshold}");

            System.IO.DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles())
            {
                if(file.LastWriteTime.ToUniversalTime() <= RemoveThreshold.Value)
                {
                    file.Delete(); 
                }
            }

            return new PurgeResult();
        }
    }
}