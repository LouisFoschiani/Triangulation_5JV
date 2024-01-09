using System.Collections.Generic;
using UnityEngine;

namespace Delaunay
{
    public struct Segment
    {
        public Vector3 Start;
        public Vector3 End;

        public Segment(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;
        }
        
        // Calcule le point milieu du segment
        public Vector2 GetMidpoint()
        {
            return (Start + End) / 2;
        }    }

    
 [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VoronoiCutter : MonoBehaviour
{
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            
            Cut();
            
        }
    }
    
    public bool LineIntersectsSegment(Vector3 linePoint, Vector3 lineDir, Segment segment, out Vector3 intersection)
    {
        intersection = Vector3.zero;

        Vector3 segmentDir = segment.End - segment.Start;
        Vector3 startToLine = linePoint - segment.Start;
        Vector3 cross1 = Vector3.Cross(lineDir, segmentDir);
        Vector3 cross2 = Vector3.Cross(startToLine, segmentDir);

        float planarFactor = Vector3.Dot(startToLine, cross1);
        if (Mathf.Abs(planarFactor) < 0.0001f && cross1.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(cross2, cross1) / cross1.sqrMagnitude;
            if (s >= 0 && s <= 1)
            {
                intersection = linePoint + lineDir * s;
                return true;
            }
        }

        return false;
    }

    

    void Cut()
    {
        
        List<IPoint> points = new List<IPoint>();

        Vector3 cubeTopCenter = transform.position + transform.up * transform.localScale.y / 2;

        List<Vector2> poissonPoints = new List<Vector2>();

        do
        {
            poissonPoints = UniformPoissonDiskSampler.SampleRectangle(new Vector2(
                transform.position.x - transform.localScale.x * 10 / 2,
                transform.position.z - transform.localScale.z * 10 / 2), new Vector2(
                transform.position.x + transform.localScale.x * 10 / 2,
                transform.position.z + transform.localScale.z * 10 / 2), 5f, 3);
        } while (poissonPoints.Count<5);

        foreach (var p in poissonPoints)
        {
            
            points.Add(new Point(p.x, p.y));
            // GameObject debug = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            // debug.transform.position = (cubeTopCenter)+new Vector3(p.x, 0, p.y);
            // debug.transform.localScale = Vector3.one * 0.1f;
        }
        
        
        Delaunay delaunay = new Delaunay(points.ToArray());

        
        delaunay.ForEachVoronoiCell(cell =>
        {
            foreach(var point in cell.Points)
            {
                
            }
        });
        
        List<Segment> listA = new List<Segment>();
        List<Segment> listB = CreerSegmentsFaceCube(transform.position, transform.localScale.x * 10);
        List<Segment> savedSegments = new List<Segment>();

        foreach (var edge in delaunay.GetHullEdges())
        {
            listA.Add(new Segment(new Vector3((float) edge.P.X, transform.localScale.y / 2, (float) edge.P.Y), new Vector3((float) edge.Q.X, transform.localScale.y / 2, (float) edge.Q.Y)));
        }

        delaunay.ForEachTriangleEdge(edge =>
        {
            GameObject edgeObject = new GameObject();
            LineRenderer lr = edgeObject.AddComponent<LineRenderer>();
            lr.sortingOrder = 1;
            lr.positionCount = 2;
            lr.startWidth = 0.025f;
            lr.endWidth = 0.025f;
            lr.sharedMaterial = new Material(Shader.Find("Standard"));
            lr.sharedMaterial.SetColor("_Color", Color.magenta);
            lr.SetPosition(0, new Vector3((float) edge.P.X, transform.localScale.y / 2, (float) edge.P.Y));
            lr.SetPosition(1, new Vector3((float) edge.Q.X, transform.localScale.y / 2, (float) edge.Q.Y));
            
        });
        
        
        
        // foreach (var segment in TrouverSegmentsIntersectants(segmentsCells, segmentsTriangles, segmentsSquare))
        // {
        //     GameObject edgeObject = new GameObject();
        //     LineRenderer lr = edgeObject.AddComponent<LineRenderer>();
        //     lr.sortingOrder = 2;
        //     lr.positionCount = 2;
        //     lr.startWidth = 0.05f;
        //     lr.endWidth = 0.05f;
        //     lr.sharedMaterial = new Material(Shader.Find("Standard"));
        //     lr.sharedMaterial.SetColor("_Color", Color.cyan);
        //     lr.SetPosition(0, segment.Start);
        //     lr.SetPosition(1, segment.End);
        // }
        
        foreach (var edge in  delaunay.GetVoronoiEdgesBasedOnCircumCenter())
        {
            savedSegments.Add(new Segment(new Vector3((float) edge.P.X, transform.localScale.y / 2, (float) edge.P.Y), new Vector3((float) edge.Q.X, transform.localScale.y / 2, (float) edge.Q.Y)));
            
            // GameObject edgeObject = new GameObject();
            // LineRenderer lr = edgeObject.AddComponent<LineRenderer>();
            // lr.sortingOrder = 2;
            // lr.positionCount = 2;
            // lr.startWidth = 0.05f;
            // lr.endWidth = 0.05f;
            // lr.sharedMaterial = new Material(Shader.Find("Standard"));
            // lr.sharedMaterial.SetColor("_Color", Color.cyan);
            // lr.SetPosition(0, new Vector3((float) edge.P.X, transform.localScale.y / 2, (float) edge.P.Y));
            // lr.SetPosition(1, new Vector3((float) edge.Q.X, transform.localScale.y / 2, (float) edge.Q.Y));
        }
        
        foreach (Segment segmentB in listB)
        {
            Vector2 midpointB = segmentB.GetMidpoint();
            foreach (Segment segmentA in listA)
            {
                if (CheckIntersection(midpointB, segmentA) && !IntersectsOtherSegments(midpointB, segmentA, listA))
                {
                    savedSegments.Add(new Segment(midpointB, segmentA.Start)); // ou segmentA.end selon votre besoin
                }
            }
        }

        foreach (var segment in savedSegments)
        {
            
            GameObject edgeObject = new GameObject();
            LineRenderer lr = edgeObject.AddComponent<LineRenderer>();
            lr.sortingOrder = 2;
            lr.positionCount = 2;
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.sharedMaterial = new Material(Shader.Find("Standard"));
            lr.sharedMaterial.SetColor("_Color", Color.cyan);
            lr.SetPosition(0, segment.Start);
            lr.SetPosition(1, segment.End);
            
        }
        
    }


    bool CheckIntersection(Vector3 point, Segment segment)
    {
        // Calculer la direction du segment
        Vector3 segmentDirection = segment.End - segment.Start;
        Vector3 pointDirection = point - segment.Start;

        // Vérifier si le point est aligné avec la direction du segment
        if (Vector3.Cross(segmentDirection, pointDirection).magnitude > Mathf.Epsilon)
        {
            return false; // Le point n'est pas sur la ligne
        }

        // Projeter le point sur la direction du segment
        float dotProduct = Vector2.Dot(pointDirection, segmentDirection.normalized);

        // Vérifier si le point projeté est entre les extrémités du segment
        return dotProduct >= 0 && dotProduct <= segmentDirection.magnitude;
    }

    bool IntersectsOtherSegments(Vector2 point, Segment segment, List<Segment> listA)
    {
        foreach (Segment otherSegment in listA)
        {
            if (otherSegment.Start != segment.Start && otherSegment.End != segment.End && CheckIntersection(point, otherSegment))
            {
                return true;
            }
        }
        return false;
    }
    
    List<Segment> CreerSegmentsFaceCube(Vector3 centre, float taille)
    {
        // Ajuster les sommets en fonction du centre et de la taille du cube
        Vector3 hautGauche = centre + new Vector3(-taille/2, taille/2, taille/2);
        Vector3 hautDroit = centre + new Vector3(taille/2, taille/2, taille/2);
        Vector3 basGauche = centre + new Vector3(-taille/2, -taille/2, taille/2);
        Vector3 basDroit = centre + new Vector3(taille/2, -taille/2, taille/2);

        List<Segment> segments = new List<Segment>
        {
            new Segment(hautGauche, hautDroit),
            new Segment(hautDroit, basDroit),
            new Segment(basDroit, basGauche),
            new Segment(basGauche, hautGauche)
        };

        return segments;
    }

}   
}


