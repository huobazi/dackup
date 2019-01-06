using System;
using System.Threading.Tasks;

using Serilog;

namespace dackup
{
    public abstract class NotifyBase : INotify
    {
        public async Task<NotifyResult> NotifyAsync()
        {
            Log.Information($"======== Dackup start [{this.GetType().Name }.NotifyAsync] ========");

            var task = Task.Run(() => Notify());
            return await task;
        }
        protected abstract Task<NotifyResult> Notify();
    }
}