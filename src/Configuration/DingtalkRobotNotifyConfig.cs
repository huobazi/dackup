using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using dackup.Extensions;

namespace dackup.Configuration
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