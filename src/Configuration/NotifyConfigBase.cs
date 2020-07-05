using System;
using System.Xml;
using System.Xml.Serialization;

namespace Dackup.Configuration
{
    [Serializable]
    public class NotifyConfigBase
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "enable")]
        public bool Enable { get; set; } = true;

        [XmlAttribute(AttributeName = "on_success")]
        public bool OnSuccess { get; set; }

        [XmlAttribute(AttributeName = "on_warning")]
        public bool OnWarning { get; set; }

        [XmlAttribute(AttributeName = "on_failure")]
        public bool OnFailure { get; set; }

        [XmlElement(ElementName = "option")]
        public NameValueElementCollection OptionList { get; set; }
    }
}