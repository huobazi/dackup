using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace dackup.Configuration
{
    [Serializable, XmlRoot(ElementName = "perform")]
    public class PerformConfig
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "compress")]
        public string Compress { get; set; }

        [XmlArray(ElementName = "archives")]
        [XmlArrayItem(ElementName = "archive")]
        public ArchiveConfigCollection Archives { get; set; }

        [XmlArray(ElementName = "databases")]
        [XmlArrayItem(ElementName = "database")]
        public DatabaseConfigCollection Databases { get; set; }

        [XmlArray(ElementName = "storages")]
        [XmlArrayItem(ElementName = "storage")]
        public StorageConfigCollection Storages { get; set; }

        [XmlElement(ElementName = "notifiers")]
        public Notifiers Notifiers { get; set; }
    }
}