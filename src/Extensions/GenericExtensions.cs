using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace dackup.Extensions
{
    /// <summary>
    /// Generic Extension Type
    /// </summary>
    public static class GenericExtensions
    {
        #region [ Private Variables ]

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Converts a given object representation to JSON format
        /// </summary>
        /// <typeparam name="T">Generic input parameter</typeparam>
        /// <param name="input">JSON representation of input value</param>
        /// <returns></returns>
        public static string ToJson<T>(this T input)
        {
            return input != null ? JsonConvert.SerializeObject(input) : null;
        }

        /// <summary>
        /// Converts a given string to the generic object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">The input.</param>
        /// <returns>Generic object </returns>
        public static T FromJson<T>(this string input)
        {
            return string.IsNullOrWhiteSpace(input) ? default : JsonConvert.DeserializeObject<T>(input);
        }

        /// <summary>
        /// Clones the specified target.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The type must be serializable. - target</exception>
        /// <exception cref="System.ArgumentException">The type must be serializable. - target</exception>
        public static T Clone<T>(this T target)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(target));
            }

            //if null, simply return the default object
            if (target == null)
            {
                return default;
            }

            var binaryFormatter = new BinaryFormatter();

            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, target);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Withes the specified action.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static T With<T>(this T target, Action<T> action)
        {
            action(target);
            return target;
        }

        /// <summary>
        /// Check if the given objet was in the specified items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static bool In<T>(this T target, params T[] items)
        {
            return items.Contains(target);
        }
        /// <summary>
        /// Check if the given objet was not in the specified items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static bool NotIn<T>(this T target, params T[] items)
        {
            return !In(target, items);
        }
        /// <summary>
        /// Ins the specified items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static bool In<T>(this T target, IEnumerable<T> items)
        {
            return items.Contains(target);
        }
        /// <summary>
        /// Nots the in.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static bool NotIn<T>(this T target, IEnumerable<T> items)
        {
            return !In(target, items);
        }

        #endregion
    }
}
