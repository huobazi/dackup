using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace dackup
{
    public class LocalStorage : StorageBase
    {   
        private string path;
        public DateTime? RemoveThreshold {get;set;}
        
        public LocalStorage(string path)
        {
            this.path = path;
        }

        protected override UploadResult Upload(string fileName)
        {    
            return new UploadResult();
        }

        protected override PurgeResult Purge()
        {
            if (RemoveThreshold == null || RemoveThreshold.Value > DateTime.Now)
            {
                return new PurgeResult();
            }

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