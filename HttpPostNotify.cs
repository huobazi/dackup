using System;
using System.Collections;
using System.Collections.Specialized;
using System.Threading.Tasks;

using Serilog;

namespace dackup
{
    public class HttpPostNotify : NotifyBase
    {
        private Uri webHookUri;

        public NameValueCollection Params {get;set;}

        public HttpPostNotify(string url)
        {
            this.webHookUri = new Uri(url);
        }
        protected override Task<NotifyResult> Notify(string messageBody)
        {
            Log.Information("======== SlackNotify start ========");

            return Task<NotifyResult>.Run(() =>
               {
                   if(Params!= null)
                   {
                       
                   }
                   return new NotifyResult();
               });
        }
    }
}