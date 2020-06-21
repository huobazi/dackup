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
        private string from, to, cc, bcc, address, domain, userName, password, authentication;
        private bool enableStarttls;
        private readonly ILogger logger;
        protected override ILogger Logger
        {
            get{ return this.logger;}
        }
        public int Port { get; set; } = 25;
        private SmtpEmailNotify() { }
        public SmtpEmailNotify(ILogger logger,string from, string to, string address, string domain, string userName, string password,
        string authentication, bool enableStarttls, string cc = null, string bcc = null)
        {
            this.logger         = logger;
            this.from           = from;
            this.to             = to;
            this.address        = address;
            this.domain         = domain;
            this.userName       = userName;
            this.password       = password;
            this.authentication = authentication;
            this.enableStarttls = enableStarttls;
            this.cc             = cc;
            this.bcc            = bcc;
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
                       client.Host = this.address;
                       client.Port = this.Port;
            // now dotnet core have no client domain settings
            // https://github.com/dotnet/corefx/issues/33123
            //client.ClientDomain = this.domain;
            client.EnableSsl             = this.enableStarttls;
            client.UseDefaultCredentials = false;
            client.Credentials           = new NetworkCredential(this.userName, this.password);

            MailMessage mailMessage      = new MailMessage();
                        mailMessage.From = new MailAddress(this.from);
            this.to.Split(';').ToList().ForEach(to =>
            {
                if (!string.IsNullOrEmpty(to))
                {
                    mailMessage.To.Add(to);
                }
            });
            this.cc.Split(';').ToList().ForEach(to =>
            {
                if (!string.IsNullOrEmpty(to))
                {
                    mailMessage.CC.Add(to);
                }
            });
            this.bcc.Split(';').ToList().ForEach(to =>
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