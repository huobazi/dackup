using System;
using System.Threading.Tasks;

using Serilog;

namespace dackup
{
    public abstract class NotifyBase : INotify
    {
        public bool OnSuccess{get;set;}
        public bool OnWarning{get;set;}
        public bool OnFailure{get;set;}

        public async Task<NotifyResult> NotifyAsync(string messageBody)
        {
            Log.Information($"======== Dackup start [{this.GetType().Name }.NotifyAsync] ========");

            var task = Task.Run(() => Notify(messageBody));
            return await task;
        }
        protected abstract Task<NotifyResult> Notify(string messageBody);
    }
}