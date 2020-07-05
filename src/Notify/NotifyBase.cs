using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace dackup
{
    public abstract class NotifyBase : INotify
    {
        public bool OnSuccess { get; set; }
        public bool OnWarning { get; set; }
        public bool OnFailure { get; set; }
        public bool Enable { get; set; }
        protected abstract ILogger Logger{ get; }
        public virtual async Task<NotifyResult> NotifyAsync(Statistics statistics)
        {
            Logger.LogInformation($"Dackup start [{this.GetType().Name }.NotifyAsync]");

            var task = Task.Run(() => Notify(statistics));
            
            return await task;
        }
        protected virtual NotifyResult Notify(Statistics statistics)
        {
            return null;
        }
    }
}