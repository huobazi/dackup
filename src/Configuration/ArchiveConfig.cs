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
    public class ArchiveConfig
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlArray(ElementName = "includes")]
        [XmlArrayItem(ElementName = "path")]
        public List<string> Includes { get; set; }

        [XmlArray(ElementName = "excludes")]
        [XmlArrayItem(ElementName = "path")]
        public List<string> Excludes { get; set; }
    }

    public class ArchiveConfigCollection : KeyedCollection<string, ArchiveConfig>
    {
        protected override string GetKeyForItem(ArchiveConfig item)
        {
            return item.Name;
        }
    }
}