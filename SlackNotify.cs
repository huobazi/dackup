using System;
using System.Threading.Tasks;

using Serilog;

namespace dackup
{
    public class SlackNotify : NotifyBase
    {
        private Uri webHookUri;
        public string Channel { get; set; }
        public string UserName { get; set; }
        public string Icon_emoji { get; set; } = ":ghost:";

        public SlackNotify(string webHookUrl)
        {
            this.webHookUri = new Uri(webHookUrl);
        }
        protected override Task<NotifyResult> Notify(string messageBody)
        {
            Log.Information("======== SlackNotify start ========");

            return Task<NotifyResult>.Run(() =>
               {
                   var message = new SlackMessage();
                   if (this.UserName != null)
                   {
                       message.UserName = this.UserName;
                   }
                   if (this.Channel != null)
                   {
                       message.Channel = this.Channel;
                   }
                   message.Text = messageBody;
                   message.Icon = this.Icon_emoji;

                   var client = new SlackClient(this.webHookUri);
                   client.SendSlackMessage(message);

                   return new NotifyResult();
               });
        }
    }
}