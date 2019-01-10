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
            

            var duration = (statistics.FinishedAt - statistics.StartedAt);
            var sb = new StringBuilder();
            sb.AppendLine($"Backup Completed Successfully!");
            sb.AppendLine($"Model={statistics.ModelName}");
            sb.AppendLine($"Start={statistics.StartedAt}");
            sb.AppendLine($"Finished={statistics.FinishedAt}");
            sb.AppendLine($"Duration={duration}");

            var message = new SlackMessage();
            if (this.UserName != null)
            {
                message.UserName = this.UserName;
            }
            if (this.Channel != null)
            {
                message.Channel = this.Channel;
            }
            message.Text = sb.ToString();
            message.Icon = this.Icon_emoji;

            var client = new SlackClient(this.webHookUri);
            client.SendSlackMessage(message);

            return new NotifyResult();
        }
    }
}