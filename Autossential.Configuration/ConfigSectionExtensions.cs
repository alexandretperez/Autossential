using System;
using System.Collections.Generic;
using System.Linq;

namespace Autossential.Configuration
{
    public static class ConfigSectionExtensions
    {
        private static T StructValue<T>(object value, T defaultValue) where T : struct
        {
            if (value == null) return defaultValue;
            if (value is T valueT) return valueT;
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch { return defaultValue; }
        }

        public static string AsString(this ConfigSection section, string keyPath, string defaultValue = default)
        {
            var value = section[keyPath];
            if (value == null) return defaultValue;
            if (value is string valueStr) return valueStr;
            return value?.ToString() ?? defaultValue;
        }

        public static int AsInt(this ConfigSection section, string keyPath, int defaultValue = default)
            => StructValue(section[keyPath], defaultValue);

        public static double AsDouble(this ConfigSection section, string keyPath, double defaultValue = default)
            => StructValue(section[keyPath], defaultValue);

        public static decimal AsDecimal(this ConfigSection section, string keyPath, decimal defaultValue = default)
            => StructValue(section[keyPath], defaultValue);

        public static float AsFloat(this ConfigSection section, string keyPath, float defaultValue = default)
            => StructValue(section[keyPath], defaultValue);

        public static long AsLong(this ConfigSection section, string keyPath, long defaultValue = default)
            => StructValue(section[keyPath], defaultValue);

        public static DateTime AsDateTime(this ConfigSection section, string keyPath, DateTime defaultValue = default)
            => StructValue(section[keyPath], defaultValue);

        public static T[] AsArray<T>(this ConfigSection section, string keyPath, T[] defaultValue = default)
        {
            var value = section[keyPath];
            if (value == null) return defaultValue;
            if (value is T[] valueArray) return valueArray;
            if (value is object[] objArray)
            {
                try
                {
                    return Array.ConvertAll(objArray, ConvertItem<T>());
                }
                catch { }
            }
            return FromCollection(value, items => Array.ConvertAll(items.ToArray(), ConvertItem<T>()), defaultValue);
        }
        public static object[] AsArray(this ConfigSection section, string keyPath, object[] defaultValue = default) =>
          AsArray<object>(section, keyPath, defaultValue);

        public static List<T> AsList<T>(this ConfigSection section, string keyPath, List<T> defaultValue = null)
        {
            var value = section[keyPath];
            if (value == null) return defaultValue;
            if (value is List<T> valueList) return valueList;
            if (value is List<object> objList)
            {
                try
                {
                    return objList.ConvertAll(ChangeType<T>);
                }
                catch { }
            }
            return FromCollection(value, items => items.ToList().ConvertAll(ChangeType<T>), defaultValue);
        }
        public static List<object> AsList(this ConfigSection section, string keyPath, List<object> defaultValue = default) =>
            AsList<object>(section, keyPath, defaultValue);

        private static T FromCollection<T>(object value, Func<IEnumerable<object>, T> converter, T defaultValue)
        {
            if (value is IEnumerable<object> valueEnumerable)
                return converter(valueEnumerable);

            return defaultValue;
        }

        private static Converter<object, T> ConvertItem<T>()
        {
            return new Converter<object, T>(ChangeType<T>);
        }

        private static T ChangeType<T>(object input)
        {
            if (input is T inputT) return inputT;
            return (T)Convert.ChangeType(input, typeof(T));
        }
    }
}