using System;
using System.Threading.Tasks;

using Serilog;

namespace dackup
{
    public class NotifyBase : INotify
    {
        public bool OnSuccess { get; set; }
        public bool OnWarning { get; set; }
        public bool OnFailure { get; set; }

        public bool Enable { get; set; }

        public virtual async Task<NotifyResult> NotifyAsync(Statistics statistics)
        {
            Log.Information($"Dackup start [{this.GetType().Name }.NotifyAsync]");

            var task = Task.Run(() => Notify(statistics));
            return await task;
        }
        protected virtual NotifyResult Notify(Statistics statistics)
        {
            return null;
        }
    }
}