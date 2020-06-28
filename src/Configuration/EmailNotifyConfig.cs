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