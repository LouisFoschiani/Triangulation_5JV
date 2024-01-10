using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class _Geometry
    {
        public static float GetSignedDistanceFromPointToPlane(Vector3 pointPos, Plane3 plane)
        {
            float distance = Vector3.Dot(plane.normal, pointPos - plane.pos);//on detecte la dsitance signée pour savoir si le point est a l'extieur ou a l'interieur du plan
            //si positif, alors point est en en direction de la normale = visible depuis ce plan
            return distance;
        }

        public static bool IsPointOutsidePlane(Vector3 pointPos, Plane3 plane) 
        {
            float distance = GetSignedDistanceFromPointToPlane(pointPos, plane);
            
            float epsilon = MathUtility.EPSILON;//marge d'erreur

            if (distance > 0f + epsilon)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public static Vector3 GetClosestPointOnLine(Edge3 e, Vector3 p, bool withinSegment)
        {
            Vector3 a = e.p1;
            Vector3 b = e.p2;

            Vector3 ab = b - a;
            Vector3 ap = p - a;

            float distance = Vector3.Dot(ap, ab) / Vector3.SqrMagnitude(ab);


            float epsilon = MathUtility.EPSILON;

            if (withinSegment && distance < 0f - epsilon)
            {
                return a;
            }
            else if (withinSegment && distance > 1f + epsilon)
            {
                return b;
            }
            else
            {
                return a + ab * distance;
            }
        }
        public static float SqrDistance(Vector3 a, Vector3 b)
        {
            float distance = Vector3.SqrMagnitude(a - b);

            return distance;
        }
    }
    
    
}
