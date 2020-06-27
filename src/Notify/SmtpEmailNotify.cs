using System;
using System.Collections;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Linq;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace dackup
{
    public class SmtpEmailNotify : NotifyBase
    {
        private readonly ILogger logger;
        protected override ILogger Logger
        {
            get{ return this.logger;}
        }
        public string From { get; set; }
        public string To { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Address { get; set; }
        public string Domain { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Authentication { get; set; }
        public bool EnableStarttls { get; set; }
        public int Port { get; set; } = 25;
        private SmtpEmailNotify() { }
        public SmtpEmailNotify(ILogger logger)
        {
            this.logger         = logger;
        }
        public override async Task<NotifyResult> NotifyAsync(Statistics statistics)
        {
            logger.LogInformation($"Dackup start [{this.GetType().Name }.NotifyAsync]");
            var sb = new StringBuilder();
            sb.AppendLine($"Backup Completed Successfully!");
            sb.AppendLine($"Model={statistics.ModelName}");
            sb.AppendLine($"Start={statistics.StartedAt}");
            sb.AppendLine($"Finished={statistics.FinishedAt}");
            sb.AppendLine($"Duration={statistics.FinishedAt - statistics.StartedAt}");
            string emailBody = sb.ToString();

            SmtpClient client      = new SmtpClient();
                       client.Host = this.Address;
                       client.Port = this.Port;
            // now dotnet core have no client domain settings
            // https://github.com/dotnet/corefx/issues/33123
            //client.ClientDomain = this.domain;
            client.EnableSsl             = this.EnableStarttls;
            client.UseDefaultCredentials = false;
            client.Credentials           = new NetworkCredential(this.UserName, this.Password);

            MailMessage mailMessage      = new MailMessage();
                        mailMessage.From = new MailAddress(this.From);
            this.To.Split(';').ToList().ForEach(to =>
            {
                if (!string.IsNullOrEmpty(to))
                {
                    mailMessage.To.Add(to);
                }
            });
            this.CC.Split(';').ToList().ForEach(to =>
            {
                if (!string.IsNullOrEmpty(to))
                {
                    mailMessage.CC.Add(to);
                }
            });
            this.BCC.Split(';').ToList().ForEach(to =>
            {
                if (!string.IsNullOrEmpty(to))
                {
                    mailMessage.Bcc.Add(to);
                }
            });

            mailMessage.Body    = emailBody;
            mailMessage.Subject = $"Backup [{statistics.ModelName}] Completed Successfully!";

            client.SendCompleted += (s, e) =>
            {
                client.Dispose();
                mailMessage.Dispose();
            };

            await client.SendMailAsync(mailMessage);

            return new NotifyResult();
        }
    }
}