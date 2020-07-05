using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Net;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dackup.Notify
{
    public class HttpPostNotify: NotifyBase
    {
        private readonly ILogger logger;
        protected override ILogger Logger
        {
            get{ return this.logger;}
        }
        public string WebHookUrl { get; set; }
        public NameValueCollection Params { get; set; }
        public NameValueCollection Headers { get; set; }
        public HttpPostNotify(ILogger<HttpPostNotify> logger) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public override async Task<NotifyResult> NotifyAsync(Statistics statistics)
        {
            logger.LogInformation($"Dackup start [{this.GetType().Name }.NotifyAsync]");

            dynamic msg            = new JObject();
                    msg.Title      = "Backup Completed Successfully!";
                    msg.ModelName  = statistics.ModelName;
                    msg.StartedAt  = statistics.StartedAt;
                    msg.FinishedAt = statistics.FinishedAt;
                    msg.Duration   = statistics.FinishedAt - statistics.StartedAt;
                    msg.Tags       = new JArray("Dackup", "OnSale");

            var nv     = new NameValueCollection(this.Params);
            nv ["msg"] = JsonConvert.SerializeObject(msg);

            var client = new WebClient();
            if (this.Headers != null && this.Headers.Count > 0)
            {
                foreach (var key in this.Headers.AllKeys)
                {
                    client.Headers.Add(key, this.Headers[key]);
                }
            }
            var method = "POST";
            var data   = await client.UploadValuesTaskAsync(this.WebHookUrl, method, nv);
            
            return new NotifyResult();
        }
    }
}