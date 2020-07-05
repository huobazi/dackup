using System;
using System.Collections.ObjectModel;

namespace Dackup.Configuration
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