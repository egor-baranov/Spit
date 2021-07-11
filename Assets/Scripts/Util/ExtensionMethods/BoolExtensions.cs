using System;
using System.Collections.Generic;
using System.Linq;

namespace Util.ExtensionMethods {
    public static class BoolExtensions {
        public static bool Or(this bool v, bool other) => v || other;
        public static bool And(this bool v, bool other) => v && other;
        public static bool Xor(this bool v, bool other) => (v || other) && !(v && other);

        public static T Condition<T>(this bool v, T ifValue, T elseValue) => v ? ifValue : elseValue;
    }
}