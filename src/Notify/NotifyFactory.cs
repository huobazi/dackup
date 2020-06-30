using System;
using System.IO;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using dackup.Configuration;
using dackup.Extensions;

namespace dackup
{
    public static class NotifyFactory
    {
        public static SmtpEmailNotify CreateEmailSmtpNotify(EmailNotifyConfig cfg)
        {
            var emailNotify       = ServiceProviderFactory.ServiceProvider.GetService<SmtpEmailNotify>();
            
            emailNotify.Enable    = cfg.Enable;
            emailNotify.OnFailure = cfg.OnFailure;
            emailNotify.OnSuccess = cfg.OnSuccess;
            emailNotify.OnWarning = cfg.OnWarning;

            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.From = s, "from");
            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.To = s, "to");
            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.Address = s, "address");
            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.Domain = s, "domain");
            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.UserName = s, "user_name");
            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.Password = s, "password");
            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.Authentication = s, "authentication");
            cfg.OptionList.NullSafeSetTo<bool>(s => emailNotify.EnableStarttls = s, "enable_starttls");
            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.CC = s, "cc");
            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.BCC = s, "bcc");
            cfg.OptionList.NullSafeSetTo<int>(port => emailNotify.Port = port, "port");

            return emailNotify;
        }
        public static SlackNotify CreateSlackNotify(SlackNotifyConfig cfg)
        {
            var slackNotify       = ServiceProviderFactory.ServiceProvider.GetService<SlackNotify>();
           
            slackNotify.Enable    = cfg.Enable;
            slackNotify.OnFailure = cfg.OnFailure;
            slackNotify.OnSuccess = cfg.OnSuccess;
            slackNotify.OnWarning = cfg.OnWarning;

            cfg.OptionList.NullSafeSetTo<string>(s => slackNotify.WebHookUrl = s, "webhook_url");
            cfg.OptionList.NullSafeSetTo<string>(s => slackNotify.Channel = s, "channel");
            cfg.OptionList.NullSafeSetTo<string>(s => slackNotify.Icon_emoji = s, "icon_emoji");
            cfg.OptionList.NullSafeSetTo<string>(s => slackNotify.UserName = s, "username");

            return slackNotify;
        }
        public static DingtalkRobotNotify CreateDingtalkRobotNotify(DingtalkRobotNotifyConfig cfg)
        {
            var dingtalkRobotNotify = ServiceProviderFactory.ServiceProvider.GetService<DingtalkRobotNotify>();
            
            dingtalkRobotNotify.AtAll     = cfg.AtAll;
            dingtalkRobotNotify.Enable    = cfg.Enable;
            dingtalkRobotNotify.OnFailure = cfg.OnFailure;
            dingtalkRobotNotify.OnSuccess = cfg.OnSuccess;
            dingtalkRobotNotify.OnWarning = cfg.OnWarning;

            cfg.OptionList.NullSafeSetTo<string>(s => dingtalkRobotNotify.WebHookUrl = s, "url");

            if (cfg.AtMobiles != null && cfg.AtMobiles.Count > 0)
            {
                dingtalkRobotNotify.AtMobiles = new List<string>();
                cfg.AtMobiles.ForEach(header =>
                {
                    dingtalkRobotNotify.AtMobiles.AddRange(header.Value.Split(';', StringSplitOptions.RemoveEmptyEntries));
                });
            }
            dingtalkRobotNotify.AtMobiles = dingtalkRobotNotify.AtMobiles.Distinct().ToList();

            return dingtalkRobotNotify;
        }
        public static HttpPostNotify CreateHttpPostNotify(HttpPostNotifyConfig cfg)
        {
            var httpPostNotify       = ServiceProviderFactory.ServiceProvider.GetService<HttpPostNotify>();
            
            httpPostNotify.Enable    = cfg.Enable;
            httpPostNotify.OnFailure = cfg.OnFailure;
            httpPostNotify.OnSuccess = cfg.OnSuccess;
            httpPostNotify.OnWarning = cfg.OnWarning;

            cfg.OptionList.NullSafeSetTo<string>(s => httpPostNotify.WebHookUrl = s, "url");

            if (cfg.Headers != null)
            {
                httpPostNotify.Headers = new NameValueCollection();
                cfg.Headers.ForEach(header =>
                {
                    httpPostNotify.Headers[header.Name] = header.Value;
                });
            }
            var paramsList = cfg.OptionList?.Where(c => c.Name.ToLower() != "url")?.ToList();
            if (paramsList != null)
            {
                httpPostNotify.Params = new NameValueCollection();
                paramsList.ForEach(param =>
                {
                    httpPostNotify.Params[param.Name] = param.Value;
                });
            }

            return httpPostNotify;
        }
    }
}