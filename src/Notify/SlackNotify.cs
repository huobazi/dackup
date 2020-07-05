using System;

using Microsoft.Extensions.Logging;

namespace Dackup.Notify
{
    public class SlackNotify: NotifyBase
    {
        private readonly ILogger logger;
        protected override ILogger Logger
        {
            get{ return this.logger;}
        }
        public string WebHookUrl { get; set; }
        public string Channel { get; set; }
        public string UserName { get; set; }
        public string Icon_emoji { get; set; } = ":ghost:";

        public SlackNotify(ILogger<SlackNotify> logger) => this.logger = logger;
        protected override NotifyResult Notify(Statistics statistics)
        {
            logger.LogInformation($"Dackup start [{this.GetType().Name }.NotifyAsync]");
            
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

            var client = new SlackClient(new Uri(this.WebHookUrl));
            client.SendSlackMessage(message);

            return new NotifyResult();
        }
    }
}