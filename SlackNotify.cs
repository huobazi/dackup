using System;
using System.Threading.Tasks;

using Serilog;

namespace dackup
{
    public class SlackNotify : NotifyBase
    {
        private Uri webHookUri;
        private string channel, userName,messageBody,icon_emoji; 

        public SlackNotify(string webHookUrl,string channel, string userName, string messageBody, string icon_emoji)
        {
            this.webHookUri = new Uri(webHookUrl);
            this.channel = channel;
            this.userName = userName;
            this.messageBody = messageBody;
            this.icon_emoji = icon_emoji;
        }
        protected override Task<NotifyResult> Notify()
        {
            Log.Information("======== SlackNotify start ========");

            return Task<NotifyResult>.Run(() =>
               {
                   var message = new SlackMessage();
                   message.UserName = this.userName;
                   message.Channel = this.channel;
                   message.Text = this.messageBody;
                   message.Icon = this.icon_emoji;
                   
                   var client = new SlackClient(this.webHookUri);
                   client.SendSlackMessage(message);

                   return new NotifyResult();
               });
        }
    }
}