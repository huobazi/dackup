using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace Dackup.Configuration
{
    [Serializable]
    public class EmailNotifyConfig : NotifyConfigBase
    {
        [XmlAttribute(AttributeName = "delivery_method")]
        public string DeliveryMethod { get; set; }
    }

    [Serializable]
    public class EmailNotifyConfigCollection : KeyedCollection<string, EmailNotifyConfig>
    {
        protected override string GetKeyForItem(EmailNotifyConfig item)
        {
            return item.Name;
        }
    }
}