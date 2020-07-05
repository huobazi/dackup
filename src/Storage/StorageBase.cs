using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Dackup.Storage
{
    public abstract class StorageBase: IStorage
    {
        protected abstract ILogger Logger
        {
            get;
        }
        public virtual async Task<UploadResult> UploadAsync(string fileName)
        {            
            Logger.LogInformation($"Dackup start [{this.GetType().Name }.UploadAsync]");

            var task = Task.Run(() => Upload(fileName));
            
            return await task;
        }
        public virtual async Task<PurgeResult> PurgeAsync()
        {
            Logger.LogInformation($"Dackup start [{this.GetType().Name }.PurgeAsync]");

            var task = Task.Run(() => Purge());

            return await task;
        }
        protected abstract UploadResult Upload(string fileName);
        protected abstract PurgeResult Purge();
    }
}