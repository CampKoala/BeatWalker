using UnityEngine;

namespace BeatWalker.Utils
{
    public static class Vector2Utils
    {
        public static Vector2 FromPolar(Vector2 origin, float radius, float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            return radius * new Vector2(Mathf.Sin(radians), Mathf.Cos(radians)) + origin;
        }
    }
}