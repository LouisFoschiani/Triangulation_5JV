using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class HullAlgorithm
    {
        public static void FindVisibleTrianglesAndBorderEdgesFromPoint(Vector3 p, HalfEdgeData3 convexHull, out HashSet<HalfEdgeFace3> visibleTriangles, out HashSet<HalfEdge3> borderEdges)
        {
            borderEdges = new HashSet<HalfEdge3>();
            visibleTriangles = new HashSet<HalfEdgeFace3>();
            HalfEdgeFace3 visibleTriangle = FindVisibleTriangleFromPoint(p, convexHull.faces);

            if (visibleTriangle == null)
            {
                Debug.LogWarning("Couldn't find a visible triangle so will ignore the point");

                return;
            }


            Queue<HalfEdgeFace3> trianglesToFloodFrom = new Queue<HalfEdgeFace3>();

            trianglesToFloodFrom.Enqueue(visibleTriangle);

            List<HalfEdge3> edgesToCross = new List<HalfEdge3>();

            int safety = 0;

            while (true)
            {
                if (trianglesToFloodFrom.Count == 0)
                {
                    break;
                }
                
                HalfEdgeFace3 triangleToFloodFrom = trianglesToFloodFrom.Dequeue();
                //on prend un par un les triangles a traiter pour les integrer a la liste des triangles visibles
                visibleTriangles.Add(triangleToFloodFrom);

                edgesToCross.Clear();

                edgesToCross.Add(triangleToFloodFrom.edge);
                edgesToCross.Add(triangleToFloodFrom.edge.nextEdge);
                edgesToCross.Add(triangleToFloodFrom.edge.nextEdge.nextEdge);

                foreach (HalfEdge3 edgeToCross in edgesToCross)
                {
                    HalfEdge3 oppositeEdge = edgeToCross.oppositeEdge;

                    if (oppositeEdge == null)
                    {
                        Debug.LogWarning("Found an opposite edge which is null");

                        break;
                    }

                    HalfEdgeFace3 oppositeTriangle = oppositeEdge.face;

                    if (trianglesToFloodFrom.Contains(oppositeTriangle) || visibleTriangles.Contains(oppositeTriangle))
                    {
                        continue;
                    }
                    
                    Plane3 plane = new Plane3(oppositeTriangle.edge.v.position, oppositeTriangle.edge.v.normal);

                    bool isPointOutsidePlane = _Geometry.IsPointOutsidePlane(p, plane);

                    if (isPointOutsidePlane)
                    {
                        trianglesToFloodFrom.Enqueue(oppositeTriangle);
                    }
                    else
                    {
                        borderEdges.Add(oppositeEdge);
                    }
                }


                safety += 1;

                if (safety > 50000)
                {
                    Debug.Log("Stuck in infinite loop when flood-filling visible triangles");

                    break;
                }
            }
        }

        private static HalfEdgeFace3 FindVisibleTriangleFromPoint(Vector3 p, HashSet<HalfEdgeFace3> triangles)
        {
            HalfEdgeFace3 visibleTriangle = null;

            foreach (HalfEdgeFace3 triangle in triangles)
            {
                Plane3 plane = new Plane3(triangle.edge.v.position, triangle.edge.v.normal);

                bool isPointOutsidePlane = _Geometry.IsPointOutsidePlane(p, plane);

                if (isPointOutsidePlane)
                {
                    visibleTriangle = triangle;

                    break;
                }
            }

            return visibleTriangle;
        }
        
        public static void BuildFirstTetrahedre(HashSet<Vector3> points, HalfEdgeData3 convexHull)
        {
            Edge3 eFurthestApart = FindEdgeFurthestApart(points);

            points.Remove(eFurthestApart.p1);
            points.Remove(eFurthestApart.p2);


            Vector3 pointFurthestAway = FindPointFurthestFromEdge(eFurthestApart, points);

            points.Remove(pointFurthestAway);
            
            Vector3 p1 = eFurthestApart.p1;
            Vector3 p2 = eFurthestApart.p2;
            Vector3 p3 = pointFurthestAway;

            //on créé 2 triangles avec leur normale opposé
            convexHull.AddTriangle(p1, p2, p3);
            convexHull.AddTriangle(p1, p3, p2);
            
            List<HalfEdgeFace3> triangles = new List<HalfEdgeFace3>(convexHull.faces);

            HalfEdgeFace3 triangle = triangles[0];

            Plane3 plane = new Plane3(triangle.edge.v.position, triangle.edge.v.normal);

            Vector3 p4 = FindFarPointFromPlane(points, plane);

            points.Remove(p4);
            
            HalfEdgeFace3 triangleToRemove = triangles[0];
            HalfEdgeFace3 triangleToKeep = triangles[1];

            if (_Geometry.GetSignedDistanceFromPointToPlane(p4, plane) < 0f)//on supprime le triangle qui a une distance signée < 0
            {
                triangleToRemove = triangles[1];
                triangleToKeep = triangles[0];
            }
            
            convexHull.DeleteFace(triangleToRemove);

            Vector3 p1_opposite = triangleToKeep.edge.v.position;
            Vector3 p2_opposite = triangleToKeep.edge.nextEdge.v.position;
            Vector3 p3_opposite = triangleToKeep.edge.nextEdge.nextEdge.v.position;

            convexHull.AddTriangle(p1_opposite, p3_opposite, p4);
            convexHull.AddTriangle(p3_opposite, p2_opposite, p4);
            convexHull.AddTriangle(p2_opposite, p1_opposite, p4);

            convexHull.ConnectAllEdgesSlow();

        }

       
        private static Vector3 FindFarPointFromPlane(HashSet<Vector3> points, Plane3 plane)
        {
            Vector3 bestPoint = default;

            float bestDistance = -Mathf.Infinity;

            foreach (Vector3 p in points)
            {
                float distance = _Geometry.GetSignedDistanceFromPointToPlane(p, plane);

                float epsilon = MathUtility.EPSILON;

                if (distance > -epsilon && distance < epsilon)
                {
                    continue;
                }

                if (distance < 0f) distance *= -1f;

                if (distance > bestDistance)
                {
                    bestDistance = distance;

                    bestPoint = p;
                }
            }

            return bestPoint;
        }

        private static Edge3 FindEdgeFurthestApart(HashSet<Vector3> pointsHashSet)
        {
            List<Vector3> points = new List<Vector3>(pointsHashSet);


            Vector3 maxX = points[0];
            Vector3 minX = points[0];
            Vector3 maxY = points[0];
            Vector3 minY = points[0];
            Vector3 maxZ = points[0];
            Vector3 minZ = points[0];

            for (int i = 1; i < points.Count; i++)
            {
                Vector3 p = points[i];
            
                if (p.x > maxX.x)
                {
                    maxX = p;
                }
                if (p.x < minX.x)
                {
                    minX = p;
                }

                if (p.y > maxY.y)
                {
                    maxY = p;
                }
                if (p.y < minY.y)
                {
                    minY = p;
                }

                if (p.z > maxZ.z)
                {
                    maxZ = p;
                }
                if (p.z < minZ.z)
                {
                    minZ = p;
                }
            }

            HashSet<Vector3> extremePointsHashSet = new HashSet<Vector3>();

            extremePointsHashSet.Add(maxX);
            extremePointsHashSet.Add(minX);
            extremePointsHashSet.Add(maxY);
            extremePointsHashSet.Add(minY);
            extremePointsHashSet.Add(maxZ);
            extremePointsHashSet.Add(minZ);

            points = new List<Vector3>(extremePointsHashSet);


            List<Edge3> pointCombinations = new List<Edge3>();

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 p1 = points[i];

                for (int j = i + 1; j < points.Count; j++)
                {
                    Vector3 p2 = points[j];

                    Edge3 e = new Edge3(p1, p2);

                    pointCombinations.Add(e);
                }
            }

            Edge3 eFurthestApart = pointCombinations[0];

            float maxDistanceBetween = _Geometry.SqrDistance(eFurthestApart.p1, eFurthestApart.p2);

            for (int i = 1; i < pointCombinations.Count; i++)
            {
                Edge3 e = pointCombinations[i];

                float distanceBetween = _Geometry.SqrDistance(e.p1, e.p2);

                if (distanceBetween > maxDistanceBetween)
                {
                    maxDistanceBetween = distanceBetween;

                    eFurthestApart = e;
                }
            }

            return eFurthestApart;
        }


        private static Vector3 FindPointFurthestFromEdge(Edge3 edge, HashSet<Vector3> pointsHashSet)
        {
            List<Vector3> points = new List<Vector3>(pointsHashSet);

            Vector3 pointFurthestAway = points[0];

            //
            Vector3 closestPointOnLine = _Geometry.GetClosestPointOnLine(edge, pointFurthestAway, withinSegment: false);

            float maxDistSqr = _Geometry.SqrDistance(pointFurthestAway, closestPointOnLine);

            for (int i = 1; i < points.Count; i++)
            {
                Vector3 thisPoint = points[i];
                
                closestPointOnLine = _Geometry.GetClosestPointOnLine(edge, thisPoint, withinSegment: false);

                float distSqr = _Geometry.SqrDistance(thisPoint, closestPointOnLine);

                if (distSqr > maxDistSqr)
                {
                    maxDistSqr = distSqr;

                    pointFurthestAway = thisPoint;
                }
            }


            return pointFurthestAway;
        }
    }
}
