
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace dackup.Configuration
{
    [Serializable]
    public class DatabaseConfig
    {
        [XmlElement(ElementName = "option")]
        public NameValueElementCollection OptionList { get; set; }

        [XmlElement(ElementName = "additional_option")]
        public NameValueElementCollection AdditionalOptionList { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "enable")]
        public bool Enable { get; set; } = true;
    }

    public class DatabaseConfigCollection : KeyedCollection<string, DatabaseConfig>
    {
        protected override string GetKeyForItem(DatabaseConfig item)
        {
            return item.Name;
        }
    }
}