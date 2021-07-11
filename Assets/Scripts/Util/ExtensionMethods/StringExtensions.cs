using System;
using System.Collections.Generic;
using System.Linq;

namespace Util.ExtensionMethods {
    public static class StringExtensions {
        public static string Interpolate(this string s, Dictionary<string, string> d) =>
            d.Keys.Aggregate(s, (c, k) => c.Replace($"{k}", d[k]));

        public static string ReplaceAll(this string s, string oldSubString, string newSubString) =>
            string.Join(newSubString, s.Split(new[] {oldSubString}, StringSplitOptions.None));

        public static void Clear(this string s) {
            s = "";
        }

        public static string Take(this string s, int n) {
            return s.ToList().Take(n).ToList().JoinToString();
        }

        public static string TakeLast(this string s, int n) {
            return s.ToList().Take(n).ToList().JoinToString();
        }
    }
}