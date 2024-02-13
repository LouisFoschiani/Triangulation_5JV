using System;
using UnityEngine;
using System.Collections.Generic;

public class MeshDataProcessor : MonoBehaviour
{
    private Matrix4x4 covarianceMatrix;
    private List<Vector3> vertices;
    public List<Vector3> worldBarycenters = new List<Vector3>();
    
    private List<Vector3> eigenvectors = new List<Vector3>();
    public Vector3 BMin;
    public Vector3 CMax;
    public Vector3 worldBarycenter;
    public void init()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.mesh == null)
        {
            Debug.LogError("MeshFilter ou Mesh est null.");
            return;
        }
        Mesh mesh = meshFilter.mesh;
        vertices = new List<Vector3>(mesh.vertices);
        worldBarycenter = Vector3.zero;

        for (int i = 0; i < vertices.Count; i++)
        {
            worldBarycenter += vertices[i];
        }
        worldBarycenter /= vertices.Count;

        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] -= worldBarycenter;
        }
        
        
        worldBarycenter = transform.TransformPoint(worldBarycenter);
        worldBarycenters.Add(worldBarycenter);

        Matrix4x4 covarMat = CalculateCovarianceMatrix(vertices); 

        Vector3 properVec = PowerIteration(covarMat).normalized; 

        eigenvectors.Add(properVec);
        
        BMin = Vector3.Dot(vertices[0], properVec) * properVec;
        CMax = BMin;
        for (int i = 1; i < vertices.Count; i++)
        {
            Vector3 pp = Vector3.Dot(vertices[i], properVec) * properVec;
            if (Vector3.Dot(pp, properVec) < 0)
            {
                if (Vector3.Distance(BMin, Vector3.zero) < Vector3.Distance(pp, Vector3.zero))
                {
                    BMin = pp;
                }
            }
            else
            {
                if (Vector3.Distance(CMax, Vector3.zero) < Vector3.Distance(pp, Vector3.zero))
                {
                    CMax = pp;
                }
            }
        }
        BMin = transform.TransformPoint(BMin);
        CMax = transform.TransformPoint(CMax);
    }


    Vector3 PowerIteration(Matrix4x4 covarMat)
    {
        Vector3 properVector = new Vector3(1.0f, 0.0f, 0.0f);
        float error = 1.0f;
        const float tolerance = 1e-6f;
        int maxIter = 200;
        float lambda = 0.0f;
        int iter = 0;
        while (error > tolerance && iter < maxIter)
        {
            Vector3 y = covarMat.MultiplyVector(properVector);
            Vector3 z = y.normalized;
            lambda = Vector3.Dot(y, z);
            error = Vector3.Distance(properVector, z);
            properVector = z;
            iter++;
        }
        return properVector;
    }
  
    public List<Vector3> GetEigenvectors()
    {
        return eigenvectors;
    }
   
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

            Gizmos.DrawSphere(worldBarycenter, 0.1f); // Ajustez la taille si nécessaire
        
    }

    public List<Vector3> GetVertices()
    {
        return vertices;
    }
    public List<Vector3> GetWorldBarycenters()
    {
        return worldBarycenters;
    }


    Matrix4x4 CalculateCovarianceMatrix(List<Vector3> vertices)
    {
        Vector3 mean = Vector3.zero;
        foreach (var point in vertices)
        {
            mean += point;
        }
        mean /= vertices.Count;

        float xx = 0, xy = 0, xz = 0, yy = 0, yz = 0, zz = 0;
        foreach (var point in vertices)
        {
            Vector3 diff = point - mean;
            xx += diff.x * diff.x;
            xy += diff.x * diff.y;
            xz += diff.x * diff.z;
            yy += diff.y * diff.y;
            yz += diff.y * diff.z;
            zz += diff.z * diff.z;
        }
        int n = vertices.Count;
        Matrix4x4 covarMat = new Matrix4x4();
        covarMat[0, 0] = xx / n; covarMat[0, 1] = xy / n; covarMat[0, 2] = xz / n; covarMat[0, 3] = 0;
        covarMat[1, 0] = xy / n; covarMat[1, 1] = yy / n; covarMat[1, 2] = yz / n; covarMat[1, 3] = 0;
        covarMat[2, 0] = xz / n; covarMat[2, 1] = yz / n; covarMat[2, 2] = zz / n; covarMat[2, 3] = 0;
        covarMat[3, 0] = 0;     covarMat[3, 1] = 0;     covarMat[3, 2] = 0;     covarMat[3, 3] = 1;

        return covarMat;
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