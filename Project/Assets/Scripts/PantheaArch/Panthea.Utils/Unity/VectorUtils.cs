using UnityEngine;

namespace Panthea.Utils
{
    public static class VectorUtils
    {
        public static string BetterString(this Vector2 vec)
        {
            return $"({vec.x}, {vec.y})";
        }
        
        public static string BetterString(this Vector3 vec)
        {
            return $"({vec.x}, {vec.y}, {vec.z})";
        }
    }
}