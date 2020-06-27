using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Net;
using System.Text;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace dackup
{
    public class DingtalkRobotNotify : NotifyBase
    {
        private readonly ILogger logger;
        public bool AtAll { get; set; }
        public List<string> AtMobiles { get; set; }
        public string WebHookUrl { get; set; }
        protected override ILogger Logger
        {
            get{ return this.logger;}
        }
        private DingtalkRobotNotify(){}
        public DingtalkRobotNotify(ILogger<DingtalkRobotNotify> logger, string url)
        {
            this.logger = logger;
        }
        public override async Task<NotifyResult> NotifyAsync(Statistics statistics)
        {
            Logger.LogInformation($"Dackup start [{this.GetType().Name }.NotifyAsync]");

            var markdownBody = $@"### Backup Completed Successfully!" + System.Environment.NewLine + System.Environment.NewLine +
                 $"> Model: {statistics.ModelName} " + System.Environment.NewLine + System.Environment.NewLine +
                 $"> Start: {statistics.StartedAt} " + System.Environment.NewLine + System.Environment.NewLine +
                 $"> FinishedAt: {statistics.FinishedAt}  " + System.Environment.NewLine + System.Environment.NewLine +
                 $"> Duration: {statistics.FinishedAt - statistics.StartedAt} " + System.Environment.NewLine + System.Environment.NewLine;

            dynamic msg = new JObject();

            msg.msgtype        = "markdown";
            msg.markdown       = new JObject();
            msg.markdown.title = $"Backup [{statistics.ModelName}] Completed Successfully!";
            msg.markdown.text  = markdownBody;
            msg.at             = new JObject();
            msg.at.isAtAll     = false;
            if (this.AtMobiles != null)
            {
                msg.at.atMobiles = new JArray(this.AtMobiles.ToArray());
            }

            var payload         = JsonConvert.SerializeObject(msg);
            var client          = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
            client.Headers.Add("Content-Type", "application/json");

            var data = await client.UploadStringTaskAsync(this.WebHookUrl, "POSST", payload);

            return new NotifyResult();
        }
    }
}