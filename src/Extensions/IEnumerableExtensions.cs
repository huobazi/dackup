using System;
using System.Collections.Generic;

namespace Dackup.Extensions
{
    public static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null || action == null ) 
            {
                return;
            }
            
            foreach (var element in source)
            {
                action(element);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            if (source == null || action == null ) 
            {
                return;
            }

            var index = 0;
            foreach (var element in source)
            {
                action(element, index++);
            }
        }
    }
}