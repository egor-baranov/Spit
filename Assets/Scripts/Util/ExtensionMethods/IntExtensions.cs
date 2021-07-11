using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Util.ExtensionMethods {
    public static class IntExtensions {
        public static IEnumerable<int> To(this int from, int to) {
            while (from <= to) {
                yield return from++;
            }
        }

        public static IEnumerable<int> Until(this int from, int to) => To(from, to - 1);

        public static IEnumerable<T> Step<T>(this IEnumerable<T> source, int step) {
            if (step == 0) {
                throw new ArgumentOutOfRangeException(nameof(step), "Param cannot be zero.");
            }

            return source.Where((x, i) => (i % step) == 0);
        }

        public static string GameFormat(this int n) {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            string formattedValue;

            if (n < Mathf.Pow(10, 3)) {
                formattedValue = n.ToString();
            }
            else if (n < Mathf.Pow(10, 6)) {
                formattedValue = $"{n / Mathf.Pow(10, 3):F1}K";
            }
            else {
                formattedValue = n < Mathf.Pow(10, 9)
                    ? $"{n / Mathf.Pow(10, 6):F1}M"
                    : $"{n / Mathf.Pow(10, 9):F1}B";
            }

            return formattedValue.Contains(".0")
                ? formattedValue.Replace(".0", "")
                : formattedValue;
        }
    }
}