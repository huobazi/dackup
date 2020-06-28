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
    public class SlackNotifyConfig : NotifyConfigBase
    {
    }

    [Serializable]
    public class SlackNotifyConfigCollection : KeyedCollection<string, SlackNotifyConfig>
    {
        protected override string GetKeyForItem(SlackNotifyConfig item)
        {
            return item.Name;
        }
    }
}