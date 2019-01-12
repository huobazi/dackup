using System;
using System.Threading.Tasks;
using System.Text;

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
        protected override NotifyResult Notify(Statistics statistics)
        {
            Log.Information($"Dackup start [{this.GetType().Name }.NotifyAsync]");
            
            var markdownBody = $@"Backup Completed Successfully!" + System.Environment.NewLine +
                 $"> Model: {statistics.ModelName} " + System.Environment.NewLine +
                 $"> Start: {statistics.StartedAt} " + System.Environment.NewLine +
                 $"> FinishedAt: {statistics.FinishedAt}  " + System.Environment.NewLine +
                 $"> Duration: {statistics.FinishedAt - statistics.StartedAt} " + System.Environment.NewLine;

            var message = new SlackMessage();
            if (this.UserName != null)
            {
                message.UserName = this.UserName;
            }
            if (this.Channel != null)
            {
                message.Channel = this.Channel;
            }
            message.Text = markdownBody;
            message.Icon = this.Icon_emoji;

            var client = new SlackClient(this.webHookUri);
            client.SendSlackMessage(message);

            return new NotifyResult();
        }
    }
}