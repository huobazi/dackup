using System;
using System.Threading.Tasks;

using Serilog;

namespace dackup
{
    public abstract class StorageBase: IStorage
    {
        public virtual async Task<UploadResult> UploadAsync(string fileName)
        {            
            Log.Information($"Dackup start [{this.GetType().Name }.UploadAsync]");

            var task = Task.Run(() => Upload(fileName));
            return await task;
        }

        public virtual async Task<PurgeResult> PurgeAsync()
        {
            Log.Information($"Dackup start [{this.GetType().Name }.PurgeAsync]");

            var task = Task.Run(() => Purge());
            return await task;
        }

        protected abstract UploadResult Upload(string fileName);

        protected abstract PurgeResult Purge();
    }
}