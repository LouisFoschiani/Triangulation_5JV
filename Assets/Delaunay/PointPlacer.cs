using System.Collections.Generic;
using UnityEngine;

namespace Delaunay
{
    public class PointPlacer : MonoBehaviour
    {

        public GameObject tempSpherePrefab;
        public GameObject hullSpherePrefab;
        public GameObject centroidSpherePrefab;
        public GameObject relaxedSpherePrefab;
        
        private Delaunay _delaunay;
        private List<IPoint> points = new();
        private List<GameObject> tempPoints = new();
        private List<GameObject> circles = new();
        private List<GameObject> hullPoints = new();
        private List<GameObject> centroidPoints = new();
        private List<GameObject> circumPoints = new();
        private List<GameObject> edges = new();
        private List<GameObject> cells = new();
        private List<GameObject> cellsLines = new();
        private List<GameObject> hullLines = new();
        private List<GameObject> cellsObj = new();
        public int distanceToCamera = 50;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = Camera.main.WorldToScreenPoint(new Vector3(0, 0, distanceToCamera)).z;
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                worldPosition.z = distanceToCamera;

                tempPoints.Add(Instantiate(tempSpherePrefab, worldPosition, Quaternion.identity));
                points.Add(new Point(worldPosition.x, worldPosition.y));

                if (points.Count >= 3)
                {
                    _delaunay = new Delaunay(points.ToArray());
                    PlacePoints();
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    hit.transform.gameObject.SetActive(false);
                }
            }
            
        }

        private void PlacePoints()
        {

            foreach (var cell in cellsObj)
            {
                Destroy(cell);
            }
            
            foreach (var centroid in centroidPoints)
            {
                Destroy(centroid);
            }
            
            
            foreach (var circum in circumPoints)
            {
                Destroy(circum);
            }
            
            foreach (var circle in circles)
            {
                Destroy(circle);
            }
            
            foreach (var hull in hullPoints)
            {
                Destroy(hull);
            }
            
            foreach (var edge in edges)
            {
                Destroy(edge);
            }
            
            foreach (var cell in cellsLines)
            {
                Destroy(cell);
            }
            
            foreach (var cell in cells)
            {
                Destroy(cell);
            }
            
            foreach (var cell in hullLines)
            {
                Destroy(cell);
            }
            
            centroidPoints.Clear();
            hullPoints.Clear();
            edges.Clear();
            cells.Clear();
            cellsLines.Clear();
            circumPoints.Clear();
            cellsObj.Clear();
            circles.Clear();
            
            foreach (var point in tempPoints)
            {
                point.SetActive(false);
            }
            

            // SHOW EDGES HULL
            // foreach (var edge in _delaunay.GetHullEdges())
            // {
            //     GameObject edgeObject = new GameObject();
            //     
            //     LineRenderer lr = edgeObject.AddComponent<LineRenderer>();
            //     lr.sortingOrder = 3;
            //     lr.positionCount = 2;
            //     lr.startWidth = 0.5f;
            //     lr.endWidth = 0.5f;
            //     lr.sharedMaterial = new Material(Shader.Find("Standard"));
            //     lr.sharedMaterial.SetColor("_Color", Color.gray);
            //     lr.SetPosition(0, new Vector3((float) edge.P.X, (float) edge.P.Y, distanceToCamera));
            //     lr.SetPosition(1, new Vector3((float) edge.Q.X, (float) edge.Q.Y, distanceToCamera));
            //     hullLines.Add(edgeObject);
            // }
            
            foreach (var edge in _delaunay.GetEdges())
            {
                
                GameObject edgeObject = new GameObject();
                
                LineRenderer lr = edgeObject.AddComponent<LineRenderer>();
                lr.sortingOrder = 1;
                lr.positionCount = 2;
                lr.startWidth = 0.5f;
                lr.endWidth = 0.5f;
                lr.sharedMaterial = new Material(Shader.Find("Standard"));
                lr.sharedMaterial.SetColor("_Color", Color.gray);
                lr.SetPosition(0, new Vector3((float) edge.P.X, (float) edge.P.Y, distanceToCamera));
                lr.SetPosition(1, new Vector3((float) edge.Q.X, (float) edge.Q.Y, distanceToCamera));
                centroidPoints.Add(edgeObject);
            }
            
            // SHOW CIRCUM GRID
            foreach (var edge in _delaunay.GetVoronoiEdgesBasedOnCircumCenter())
            {
                
                GameObject edgeObject = new GameObject();
                
                LineRenderer lr = edgeObject.AddComponent<LineRenderer>();
                lr.sortingOrder = 2;
                lr.positionCount = 2;
                lr.startWidth = 0.5f;
                lr.endWidth = 0.5f;
                lr.sharedMaterial = new Material(Shader.Find("Standard"));
                lr.sharedMaterial.SetColor("_Color", Color.red);
                lr.SetPosition(0, new Vector3((float) edge.P.X, (float) edge.P.Y, distanceToCamera));
                lr.SetPosition(1, new Vector3((float) edge.Q.X, (float) edge.Q.Y, distanceToCamera));
                circumPoints.Add(edgeObject);
            }
            
            
            // SHOW CENTROID GRID
            // foreach (var edge in _delaunay.GetVoronoiEdgesBasedOnCentroids())
            // {
            //     
            //     GameObject edgeObject = new GameObject();
            //     
            //     LineRenderer lr = edgeObject.AddComponent<LineRenderer>();
            //     lr.sortingOrder = 1;
            //     lr.positionCount = 2;
            //     lr.startWidth = 0.5f;
            //     lr.endWidth = 0.5f;
            //     lr.sharedMaterial = new Material(Shader.Find("Standard"));
            //     lr.sharedMaterial.SetColor("_Color", Color.blue);
            //     lr.SetPosition(0, new Vector3((float) edge.P.X, (float) edge.P.Y, distanceToCamera));
            //     lr.SetPosition(1, new Vector3((float) edge.Q.X, (float) edge.Q.Y, distanceToCamera));
            //     centroidPoints.Add(edgeObject);
            // }

            foreach (var triangle in _delaunay.GetTriangles())
            {

                IPoint circlePoint = new Point();
                foreach (var point in triangle.Points)
                {
                    circlePoint = point;
                    break;
                }
                
                IPoint center = _delaunay.GetTriangleCircumcenter(triangle.Index);
                float radius = Vector3.Distance(new Vector3((float) center.X, (float) center.Y, distanceToCamera), new Vector3((float) circlePoint.X, (float) circlePoint.Y, distanceToCamera));
                DrawCircumscribedCircle(new Vector3((float) center.X, (float) center.Y, distanceToCamera), radius);
                
            }
        }

        
        void DrawCircumscribedCircle(Vector3 center, float radius)
        {
            float x;
            float y;
            float z = 0f;
            int segments = 50; // Nombre de segments pour le cercle
            float lineWidth = 0.1f; // Largeur de la ligne
            Color lineColor = Color.green;
            GameObject circleObject = new GameObject();
            LineRenderer line = circleObject.AddComponent<LineRenderer>();
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.startColor = lineColor;
            line.endColor = lineColor;
            line.positionCount = segments + 1;
            line.useWorldSpace = false;

            float angle = 20f;

            for (int i = 0; i < (segments + 1); i++)
            {
                x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
                y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

                line.SetPosition(i, new Vector3(x, y, z) + center);

                angle += (360f / segments);
            }
            circles.Add(circleObject);
        }
    }
}