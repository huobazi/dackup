using System;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Net;
using System.Text;

using Newtonsoft.Json;

namespace dackup
{
    public sealed class SlackClient
    {
        public static readonly Uri DefaultWebHookUri = new Uri("https://hooks.slack.com/services/.../.../...");
        private readonly Uri webHookUri;
        public SlackClient(Uri webHookUri)
        {
            this.webHookUri = webHookUri;
        }
        public void SendSlackMessage(SlackMessage message)
        {
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                byte[] request  = System.Text.Encoding.UTF8.GetBytes("payload=" + JsonConvert.SerializeObject(message));
                byte[] response = webClient.UploadData(this.webHookUri, "POST", request);
            }
        }
    }
    public sealed class SlackMessage
    {
        [JsonProperty("channel")]
        public string Channel
        {
            get; set;
        }

        [JsonProperty("username")]
        public string UserName
        {
            get; set;
        }

        [JsonProperty("text")]
        public string Text
        {
            get; set;
        }

        [JsonProperty("icon_emoji")]
        public string Icon
        {
            get; set;
        }
    }
}