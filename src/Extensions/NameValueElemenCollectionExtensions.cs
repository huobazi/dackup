
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using dackup.Extensions;
using dackup.Configuration;

namespace dackup.Extensions
{
    public static class NameValueElementCollectionExtensions
    {
        public static void NullSafeSetTo<T>(this NameValueElementCollection source,  Action<T> setter, params string[] nameArray)
        {
            var value = source?.FirstOrDefault(c => c.Name.ToLower().In(nameArray.Select(c => c.ToLower())))?.Value;
            if (value != null)
            {
                setter((T)Convert.ChangeType(value, typeof(T)));
            }
        }
    }
}