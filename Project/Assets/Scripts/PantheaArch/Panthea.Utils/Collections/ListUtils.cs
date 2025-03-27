using System.Collections.Generic;

namespace Panthea.Utils
{
    public static class ListUtils
    {
        public static void Overwrite<T>(this List<T> source, List<T> destination)
        {
            destination.Clear();
            destination.AddRange(source);
        }

        public static T Random<T>(this List<T> source) => source[UnityEngine.Random.Range(0, source.Count)];
    }
}