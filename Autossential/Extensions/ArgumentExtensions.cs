using System.Activities;
using System.Collections.Generic;
using System.Linq;

namespace Autossential.Extensions
{
    public static class ArgumentExtensions
    {
        public static bool IsStringOrCollectionOfString(this Argument arg)
        {
            return arg.ArgumentType == typeof(string) || typeof(IEnumerable<string>).IsAssignableFrom(arg.ArgumentType);
        }

        public static T[] GetAsArray<T>(this Argument arg, CodeActivityContext context)
        {
            var result = new HashSet<T>();
            var value = arg?.Get(context) ?? default(T);

            if (value == null)
                return result.ToArray();

            void forEachItem(IEnumerable<T> collection)
            {
                foreach (var v in collection)
                {
                    result.Add(v);
                }
            }

            if (value is IList<T> valueList)
            {
                forEachItem(valueList);
            }
            else if (value is T[] valueArray)
            {
                forEachItem(valueArray);
            }
            else
            {
                result.Add((T)value);
            }

            return result.ToArray();
        }
    }
}