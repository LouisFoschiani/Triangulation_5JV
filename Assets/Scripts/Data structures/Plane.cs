using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //3D
    public class Plane3
    {
        public Vector3 pos;

        public Vector3 normal;


        public Plane3(Vector3 pos, Vector3 normal)
        {
            this.pos = pos;

            this.normal = normal;
        }
    }
}
