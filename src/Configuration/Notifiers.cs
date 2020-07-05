using System;
using System.Xml;
using System.Xml.Serialization;

namespace dackup.Configuration
{
    [Serializable]
    public class Notifiers
    {
        [XmlElement(ElementName = "http_post")]
        public HttpPostNotifyConfigCollection HttpPostList { get; set; }

        [XmlElement(ElementName = "dingtalk_robot")]
        public DingtalkRobotNotifyConfigCollection DingtalkRobotList { get; set; }

        [XmlElement(ElementName = "slack")]
        public SlackNotifyConfigCollection SlackList { get; set; }

        [XmlElement(ElementName = "email")]
        public EmailNotifyConfigCollection EmailList { get; set; }
    }
}