
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
    public class NameValueElement
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
    }
    [Serializable]
    public class NameValueElementCollection : KeyedCollection<string, NameValueElement>
    {
        protected override string GetKeyForItem(NameValueElement item)
        {
            return item.Name;
        }
    }
}