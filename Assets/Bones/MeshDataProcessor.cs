using System;
using UnityEngine;
using System.Collections.Generic;

public class MeshDataProcessor : MonoBehaviour
{
    private Matrix4x4 covarianceMatrix;
    private List<Vector3> vertices;
    private List<Vector3> barycenters = new List<Vector3>();
    private List<string> segmentNames = new List<string> { "arm_l", "arm_r", "buste", "feet_l", "feet_r", "head", "leg_lower_l", "leg_lower_r", "leg_upper_l", "leg_upper_r", "pelvis" };


    public void init()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh; // Récupérer le mesh de l'objet
        vertices = new List<Vector3>(mesh.vertices); // Récupérer les vertices du mesh

        Vector3 barycenter = CalculateBarycenter(vertices);
        List<Vector3> centeredVertices = CenterVertices(vertices, barycenter);
        covarianceMatrix = CalculateCovarianceMatrix(centeredVertices);
        
        Debug.DrawRay(barycenter, Vector3.up, Color.red, duration: 100f);
        //Debug.Log("Matrice de covariance: \n" + MatrixToString(covarianceMatrix));
        barycenters.Add(CalculateBarycenter(vertices));
        //Debug.Log("Barycentre calculé pour " + mesh + ": " + barycenter);

    }
    public List<string> GetSegmentNames()
    {
        return segmentNames;
    }

    public List<Vector3> GetVertices()
    {
        return vertices;
    }
    public List<Vector3> GetBarycenters()
    {
        return barycenters;
    }
    Vector3 CalculateBarycenter(List<Vector3> segmentVertices)
    {
        if (segmentVertices == null || segmentVertices.Count == 0)
        {
            Debug.LogError("Aucun vertex fourni pour calculer le barycentre.");
            return Vector3.zero;
        }

        Vector3 sum = Vector3.zero;
        foreach (Vector3 vertex in segmentVertices)
        {
            sum += vertex;
        }
        return sum / segmentVertices.Count;
    }


    public int GetSegmentIndex(string segmentName)
    {
        // Retourne l'index du segment dans la liste, qui est utilisé pour accéder au barycentre correspondant
        int index = segmentNames.IndexOf(segmentName);
        if (index == -1)
        {
            Debug.LogError("Segment name not found: " + segmentName);
        }
        return index;
    }
    List<Vector3> CenterVertices(List<Vector3> vertices, Vector3 barycenter)
    {
        List<Vector3> centeredVertices = new List<Vector3>();
        foreach (Vector3 vertex in vertices)
        {
            centeredVertices.Add(vertex - barycenter);
        }
        return centeredVertices;
    }

    Matrix4x4 CalculateCovarianceMatrix(List<Vector3> vertices)
    {
        float sumXX = 0, sumXY = 0, sumXZ = 0;
        float sumYY = 0, sumYZ = 0, sumZZ = 0;
        int n = vertices.Count;

        foreach (Vector3 vertex in vertices)
        {
            sumXX += vertex.x * vertex.x;
            sumXY += vertex.x * vertex.y;
            sumXZ += vertex.x * vertex.z;
            sumYY += vertex.y * vertex.y;
            sumYZ += vertex.y * vertex.z;
            sumZZ += vertex.z * vertex.z;
        }

        Matrix4x4 covarianceMatrix = new Matrix4x4();
        covarianceMatrix[0, 0] = sumXX / (n - 1);
        covarianceMatrix[0, 1] = covarianceMatrix[1, 0] = sumXY / (n - 1);
        covarianceMatrix[0, 2] = covarianceMatrix[2, 0] = sumXZ / (n - 1);
        covarianceMatrix[1, 1] = sumYY / (n - 1);
        covarianceMatrix[1, 2] = covarianceMatrix[2, 1] = sumYZ / (n - 1);
        covarianceMatrix[2, 2] = sumZZ / (n - 1);

        return covarianceMatrix;
    }
    public Matrix4x4 GetCovarianceMatrix()
    {
        return covarianceMatrix;
    }
    // Méthode pour convertir la matrice en chaîne de caractères
    string MatrixToString(Matrix4x4 matrix)
    {
        return matrix[0, 0] + ", " + matrix[0, 1] + ", " + matrix[0, 2] + ", " + matrix[0, 3] + "\n" +
               matrix[1, 0] + ", " + matrix[1, 1] + ", " + matrix[1, 2] + ", " + matrix[1, 3] + "\n" +
               matrix[2, 0] + ", " + matrix[2, 1] + ", " + matrix[2, 2] + ", " + matrix[2, 3] + "\n" +
               matrix[3, 0] + ", " + matrix[3, 1] + ", " + matrix[3, 2] + ", " + matrix[3, 3];
    }
}
