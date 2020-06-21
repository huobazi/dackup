using Microsoft.Extensions.Logging;

namespace dackup
{
    public class NotifyFactory
    {
        private readonly ILogger logger;

        public NotifyFactory(ILogger<StorageFactory> logger)
        {
            this.logger = logger;
        }
        public DingtalkRobotNotify CreateDingtalkRobotNotify(string url)
        {
            return new DingtalkRobotNotify(this.logger, url);
        }
        public SlackNotify CreateSlackNotify(string webHookUrl)
        {
            return new SlackNotify(this.logger, webHookUrl);
        }
        public HttpPostNotify CreateHttpPostNotify(string url)
        {
            return new HttpPostNotify(this.logger, url);
        }
        public SmtpEmailNotify CreateSmtpEmailNotify(string from, string to, string address, string domain, string userName, string password, string authentication, bool enableStarttls, string cc = null, string bcc = null)
        {
            return new SmtpEmailNotify(this.logger, from, to, address, domain, userName, password, authentication, enableStarttls, cc, bcc);
        }
    }
}