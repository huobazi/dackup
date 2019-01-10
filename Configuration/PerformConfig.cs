
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace dackup.Configuration
{
    public class NameValueElement
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
    }

    public class Archives
    {
        [XmlArray(ElementName = "includes")]
        [XmlArrayItem(ElementName = "path")]
        public List<string> Includes { get; set; }

        [XmlArray(ElementName = "excludes")]
        [XmlArrayItem(ElementName = "path")]
        public List<string> Excludes { get; set; }
    }

    public class Database
    {
        [XmlElement(ElementName = "option")]
        public List<NameValueElement> OptionList { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "enable")]
        public bool Enable { get; set; } = true;
    }

    public class Storage
    {
        [XmlElement(ElementName = "option")]
        public List<NameValueElement> OptionList { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "enable")]
        public bool Enable { get; set; } = true;
    }

    public class NotifyBase
    {
        [XmlElement(ElementName = "option")]
        public List<NameValueElement> OptionList { get; set; }

        [XmlAttribute(AttributeName = "on_success")]
        public bool OnSuccess { get; set; }

        [XmlAttribute(AttributeName = "on_warning")]
        public bool OnWarning { get; set; }

        [XmlAttribute(AttributeName = "on_failure")]
        public bool OnFailure { get; set; }

        [XmlAttribute(AttributeName = "enable")]
        public bool Enable { get; set; } = true;

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    public class HttpPost : NotifyBase
    {
        [XmlElement(ElementName = "header")]
        public List<NameValueElement> Headers { get; set; }
    }

    public class Slack : NotifyBase
    {
    }

    public class Email : NotifyBase
    {
        [XmlAttribute(AttributeName = "delivery_method")]
        public string DeliveryMethod { get; set; }
    }

    public class Notifiers
    {
        [XmlElement(ElementName = "http_post")]
        public List<HttpPost> HttpPostList { get; set; }

        [XmlElement(ElementName = "slack")]
        public List<Slack> SlackList { get; set; }

        [XmlElement(ElementName = "email")]
        public List<Email> EmailList { get; set; }
    }

    [XmlRoot(ElementName = "perform")]
    public class PerformConfig
    {
        [XmlElement(ElementName = "archives")]
        public Archives Archives { get; set; }

        [XmlArray(ElementName = "databases")]
        [XmlArrayItem(ElementName = "database")]
        public List<Database> Databases { get; set; }

        [XmlArray(ElementName = "storages")]
        [XmlArrayItem(ElementName = "storage")]
        public List<Storage> Storages { get; set; }

        [XmlElement(ElementName = "notifiers")]
        public Notifiers Notifiers { get; set; }

        [XmlAttribute(AttributeName = "compress")]
        public string Compress { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }
}