using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class ConvexHullIteration : MonoBehaviour
{
    private ControllerHullConvex controller;
    private float vitesseGeneration;

    

    //se lance via le controller
    public void StartVisualizer(HashSet<Vector3> points)
    {
        controller = GetComponent<ControllerHullConvex>();
        vitesseGeneration = controller.vitesseGeneration;
        //permet de representer la coque convexe
        HalfEdgeData3 convexHull = new HalfEdgeData3();
        
        //on construit le premier tetrahedre
        HullAlgorithm.BuildFirstTetrahedre(points, convexHull);
        
        
        StartCoroutine(GenerateHull(points, convexHull));
    }


//sert a coordonner la visualisation étape par étape du processus de construction itérative de la coque convexe
    private IEnumerator GenerateHull(HashSet<Vector3> points, HalfEdgeData3 convexHull)
    {
        //Affiche la coque convexe initiale et masque tous les points visuels associés aux sommets de la coque
        controller.DisplayMeshMain(convexHull.faces);
        controller.HideAllVisiblePoints(convexHull.verts);//
        
        //yield return new WaitForSeconds(5f);

        List<Vector3> pointsToAdd = new List<Vector3>(points);

        foreach (Vector3 p in pointsToAdd)
        {
            //verification si le point est déjà à l'intérieur de la coque convexe
            bool isWithinHull = _Intersections.PointWithinConvexHull(p, convexHull);

            if (isWithinHull)
            {
                points.Remove(p);
                //si le point est a l'interieur, on l'enleve
                controller.HideVisiblePoint(p);

                continue;
            }

            //sinon il est affiché comme point actif
            controller.DisplayActivePoint(p);

            HashSet<HalfEdgeFace3> visibleTriangles = null;
            HashSet<HalfEdge3> borderEdges = null;

            HullAlgorithm.FindVisibleTrianglesAndBorderEdgesFromPoint(p, convexHull, out visibleTriangles, out borderEdges);
            //trouver les triangles visibles et les arêtes de bord à partir du point actif

            foreach (HalfEdgeFace3 triangle in visibleTriangles)
            {
                convexHull.DeleteFace(triangle);
            }

            controller.DisplayMeshMain(convexHull.faces);
            controller.DisplayMeshOther(visibleTriangles);
            controller.HideAllVisiblePoints(convexHull.verts);

            yield return new WaitForSeconds(vitesseGeneration);


            List<HalfEdgeFace3> visibleTrianglesList = new List<HalfEdgeFace3>(visibleTriangles);

            for (int i = 0; i < visibleTrianglesList.Count; i++)
            {
                visibleTriangles.Remove(visibleTrianglesList[i]);//on enleve les triangles visibles depuis le points

                controller.DisplayMeshOther(visibleTriangles);

                //yield return new WaitForSeconds(0.5f);
            }

            
            HashSet<HalfEdge3> newEdges = new HashSet<HalfEdge3>();

            foreach (HalfEdge3 borderEdge in borderEdges)
            {
                Vector3 p1 = borderEdge.prevEdge.v.position;
                Vector3 p2 = borderEdge.v.position;

                HalfEdgeFace3 newTriangle = convexHull.AddTriangle(p2, p1, p);//creation nouveaux triangles en connectant les points du bord avec le point actif


                controller.DisplayMeshMain(convexHull.faces);

                //yield return new WaitForSeconds(0.5f);

                
                //chercher les aretes oppposés dans la coqque convexe
                HalfEdge3 edgeToConnect = newTriangle.edge.nextEdge;
                
                edgeToConnect.oppositeEdge = borderEdge;
                borderEdge.oppositeEdge = edgeToConnect;

                HalfEdge3 e1 = newTriangle.edge;
                HalfEdge3 e3 = newTriangle.edge.nextEdge.nextEdge;

                newEdges.Add(e1);
                newEdges.Add(e3);
            }

            foreach (HalfEdge3 e in newEdges)
            {
                if (e.oppositeEdge != null)
                {
                    continue;
                }

                convexHull.TryFindOppositeEdge(e, newEdges);
            }

            controller.HideVisiblePoint(p);
        }

        //desactive le dernier point actif
        controller.HideActivePoint();
        
        yield return null;
    }
}
