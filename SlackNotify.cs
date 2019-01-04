using System;
using System.Threading.Tasks;

namespace dackup
{
    public class SlackNotify
    {
        private Uri webHookUri;
        private string channel, userName,messageBody,icon_emoji; 
        public Task Notify()
        {
            return Task.Run(() =>
               {
                   var message = new SlackMessage();
                   message.UserName = this.userName;
                   message.Channel = this.channel;
                   message.Text = this.messageBody;
                   message.Icon = this.icon_emoji;
                   
                   var client = new SlackClient(this.webHookUri);
                   client.SendSlackMessage(message);
               });
        }
    }
}