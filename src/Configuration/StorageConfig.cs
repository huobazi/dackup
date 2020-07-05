using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace Dackup.Configuration
{
    [Serializable]
    public class StorageConfig
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "enable")]
        public bool Enable { get; set; } = true;

        [XmlElement(ElementName = "option")]
        public NameValueElementCollection OptionList { get; set; }
    }
    public class StorageConfigCollection : KeyedCollection<string, StorageConfig>
    {
        protected override string GetKeyForItem(StorageConfig item)
        {
            return item.Name;
        }
    }
}