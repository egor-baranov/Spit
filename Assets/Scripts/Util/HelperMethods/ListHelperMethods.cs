using System.Collections.Generic;
using System.Linq;

namespace Util.HelperMethods {
    public class ListHelperMethods {
        public List<T> ListOf<T>(params T[] list) => list.ToList();
    }
}