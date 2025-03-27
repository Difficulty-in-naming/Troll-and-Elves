using System.Collections.Generic;

namespace Panthea.Utils
{
    public static class HashSetUtils
    {
        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> array)
        {
            foreach (var node in array)
            {
                hashSet.Add(node);
            }
        }
    }
}