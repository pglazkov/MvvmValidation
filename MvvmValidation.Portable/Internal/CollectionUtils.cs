using System.Collections.Generic;
using System.Linq;

namespace MvvmValidation.Internal
{
    internal static class CollectionUtils
    {
        public static bool ItemsEqual<T>(this IEnumerable<T> collection1, IEnumerable<T> collection2)
        {
            if (collection1 == null && collection2 == null)
            {
                return true;
            }

            if (collection1 == null || collection2 == null)
            {
                return false;
            }

            if (collection1.Count() != collection2.Count())
            {
                return false;
            }

            foreach (var item in collection1)
            {
                if (!collection2.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }
    }
}