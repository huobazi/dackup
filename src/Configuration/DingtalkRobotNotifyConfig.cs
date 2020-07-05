using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace Dackup.Configuration
{
    public class DingtalkRobotNotifyConfig : NotifyConfigBase
    {
        [XmlAttribute(AttributeName = "atAll")]
        public bool AtAll { get; set; }

        [XmlElement(ElementName = "at")]
        public NameValueElementCollection AtMobiles { get; set; }
    }

    [Serializable]
    public class DingtalkRobotNotifyConfigCollection : KeyedCollection<string, DingtalkRobotNotifyConfig>
    {
        protected override string GetKeyForItem(DingtalkRobotNotifyConfig item)
        {
            return item.Name;
        }
    }
}