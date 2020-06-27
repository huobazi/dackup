
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
    public static class NameValueElemenListExtensions
    {
        public static void NullSafeSetTo<T>(this NameValueElementList list,  Action<T> setter, params string[] nameArray)
        {
            var value = list?.ToList().Find(c => c.Name.ToLower().In(nameArray.Select(c => c.ToLower())))?.Value;
            if (value != null)
            {
                setter((T)Convert.ChangeType(value, typeof(T)));
            }
        }
        // public static void NullSafeSetTo<T>(this NameValueElementList list, string name, Action<T> setter)
        // {
        //     NullSafeSetTo<T>(list, setter, name);
        // }
    }
}