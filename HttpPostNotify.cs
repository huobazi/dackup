using System;
using System.Collections;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Net;

using Serilog;

namespace dackup
{
    public class HttpPostNotify : NotifyBase
    {
        private Uri webHookUri;

        public NameValueCollection Params { get; set; }

        public NameValueCollection Headers { get; set; }

        public HttpPostNotify(string url)
        {
            this.webHookUri = new Uri(url);
        }

        public override async Task<NotifyResult> NotifyAsync(string messageBody)
        {
            Log.Information($"Dackup start [{this.GetType().Name }.NotifyAsync]");
            var nv = new NameValueCollection(this.Params);
            nv["msg"] = messageBody;
            var client = new WebClient();
            if(this.Headers != null && this.Headers.Count >0){
                foreach (var key in this.Headers.AllKeys)
                {
                    client.Headers.Add(key,this.Headers[key]);
                }
            }
            var method = "POST";
            var data = await client.UploadValuesTaskAsync(this.webHookUri, method, nv);
            return new NotifyResult();
        }
    }
}