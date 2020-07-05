
using System;
using System.Linq;
using Dackup.Configuration;

namespace Dackup.Extensions
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