using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class ExtensionMethods
    {
        public static Vector3 ToMyVector3(this Vector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static Vector3 ToVector3(this Vector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
    }
}
