using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace Util.ExtensionMethods {
    public static class ListExtensions {
        /// <summary>
        ///  A generic extension method for a List class that returns random element from list. 
        /// </summary>
        /// <param name="list"> A List object from which we will get a random element. </param>
        /// <typeparam name="T"> Type of elements in list. </typeparam>
        /// <returns> A random element from list. </returns>
        public static T RandomElement<T>(this List<T> list) {
            return list[Random.Range(0, list.Count)];
        }

        public static List<T> Shuffled<T>(this IEnumerable<T> list) {
            var ret = new List<T>(list);
            if (ret.IsEmpty()) return ret;

            foreach (var n in 0.Until(ret.Count)) {
                var k = Random.Range(0, n + 1);
                (ret[k], ret[n]) = (ret[n], ret[k]);
            }

            return ret;
        }

        public static bool IsEmpty<T>(this List<T> list) => list.Count == 0;

        public static bool IsNotEmpty<T>(this List<T> list) => list.Count != 0;

        public static T LastElement<T>(this List<T> list) => list[list.Count - 1];

        public static bool ContainsAny<T>(this List<T> list, IEnumerable<T> other) =>
            other.Aggregate(false, (current, elem) => current && list.Contains(elem));

        public static List<T> TakeLast<T>(this IEnumerable<T> list, int n) {
            var enumerable = list as T[] ?? list.ToArray();
            return enumerable.ToList().Count <= n
                ? enumerable.ToList()
                : enumerable.Reverse().Take(n).Reverse().ToList();
        }

        public static List<T> Reversed<T>(this List<T> list) {
            list.Reverse();
            return list;
        }

        public static List<T> Take<T>(this List<T> list, int n) => list.TakeFirst(n);

        public static List<T> TakeFirst<T>(this List<T> list, int n) =>
            list.Count <= n ? list : list.GetRange(0, n);

        public static List<T> TakeLast<T>(this List<T> list, int n) => list.Reversed().TakeFirst(n);

        public static List<T> Drop<T>(this List<T> list, int n) => list.TakeLast(list.Count - n);

        public static List<T> DropLast<T>(this List<T> list, int n) => list.Take(list.Count - n);

        public static string JoinToString(this IEnumerable<char> list) =>
            string.Join("", list.GetEnumerator());
    }
}