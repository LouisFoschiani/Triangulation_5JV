using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ACP : MonoBehaviour
{
    public MeshDataProcessor meshDataProcessor; // Référence à MeshDataProcessor
    private List<Vector3> eigenvectors = new List<Vector3>();
    public bool IsInitialized { get; private set; }
    
    public void init()
    {
        IsInitialized = true;
        if (meshDataProcessor != null)
        {
            Matrix4x4 covarianceMatrix = meshDataProcessor.GetCovarianceMatrix();
            (float eigenvalue, Vector3 eigenvector) = PowerIteration(covarianceMatrix, 10000, 0.00001f); // Augmentation du nombre d'itérations et réduction de la tolérance
            //Debug.Log("Valeur propre dominante: " + eigenvalue);
            //Debug.Log("Vecteur propre associé: " + eigenvector);
            
            List<Vector3> vertices = meshDataProcessor.GetVertices();
            List<Vector3> projectedPoints = ProjectVertices(vertices, eigenvector);
            SaveProjectedPoints(projectedPoints);
            eigenvectors.Add(eigenvector);
        }
        else
        {
            Debug.LogError("MeshDataProcessor non attaché!");
        }
        
    }
    public int GetSegmentIndex(string segmentName)
    {
        return meshDataProcessor.GetSegmentIndex(segmentName);
        
    }
    (float, Vector3) PowerIteration(Matrix4x4 matrix, int maxIterations, float tolerance)
    {
        Vector3 b_k = Vector3.right;
        Vector3 b_k1;

        for (int i = 0; i < maxIterations; i++)
        {
            b_k1 = MultiplyMatrixVector(matrix, b_k);
            b_k1.Normalize();

            if (Vector3.Distance(b_k, b_k1) < tolerance)
            {
                break;
            }

            b_k = b_k1;
        }
        float eigenvalue = Vector3.Dot(MultiplyMatrixVector(matrix, b_k), b_k) / Vector3.Dot(b_k, b_k);
        return (eigenvalue, b_k);
    }

    Vector3 MultiplyMatrixVector(Matrix4x4 matrix, Vector3 vector)
    {
        Vector4 temp = new Vector4(vector.x, vector.y, vector.z, 1);
        temp = matrix * temp;
        return new Vector3(temp.x, temp.y, temp.z);
    }
    List<Vector3> ProjectVertices(List<Vector3> vertices, Vector3 eigenvector)
    {
        List<Vector3> projectedPoints = new List<Vector3>();
        foreach (Vector3 vertex in vertices)
        {
            projectedPoints.Add(ProjectPointOntoEigenvector(vertex, eigenvector));
        }
        return projectedPoints;
    }
    public List<Vector3> GetEigenvectors()
    {
        return eigenvectors;
    }
    Vector3 ProjectPointOntoEigenvector(Vector3 point, Vector3 eigenvector)
    {
        float scalarProjection = Vector3.Dot(point, eigenvector);
        return scalarProjection * eigenvector;
    }

    void SaveProjectedPoints(List<Vector3> projectedPoints)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "projected_points.txt");
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (Vector3 point in projectedPoints)
            {
                writer.WriteLine(point.x + "," + point.y + "," + point.z);
            }
        }
    }
}