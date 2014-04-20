using System.Collections.Generic;

namespace System.Linq
{
    internal static class EnumerableEx
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var elm in source)
                action(elm);
        }

        public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var elm in source)
            {
                action(elm);
                yield return elm;
            }
        }
    }
}
