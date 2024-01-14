using UnityEngine;
using System.Collections.Generic;

public class SkeletonBuilder : MonoBehaviour
{
    public GameObject bonePrefab;
    private Dictionary<string, Transform> bones = new Dictionary<string, Transform>();
    private MeshDataProcessor[] meshDataProcessor;
    private ACP[] acp;
    private HashSet<string> processedSegmentNames = new HashSet<string>(); 

    void Awake()
    {
        meshDataProcessor = FindObjectsOfType<MeshDataProcessor>();
        acp = FindObjectsOfType<ACP>();
    }

    void Start()
    {
        foreach (var processor in meshDataProcessor)
        {
            foreach (string segmentName in processor.GetSegmentNames())
            {
               //Debug.Log("Creating bone for segment: " + segmentName);
                if (!processedSegmentNames.Contains(segmentName))
                {
                    CreateBone(segmentName);
                    processedSegmentNames.Add(segmentName);
                }
            }
        }
        SetBoneHierarchy();
    }


    void CreateBone(string name)
    {
        Vector3 position = CalculateBonePosition(name);
        Quaternion orientation = CalculateBoneOrientation(name);

        GameObject bone = Instantiate(bonePrefab, position, orientation, transform);
        if (bone == null)
        {
            Debug.LogError("Failed to instantiate bone prefab for: " + name);
        }
        else
        {
            //Debug.Log("Creating bone: " + name);
        }  
        bone.name = name + "_Bone";
        bones[name] = bone.transform;   
       
    }
    
    Vector3 CalculateBonePosition(string boneName)
    {
        foreach (var processor in meshDataProcessor)
        {
            int index = processor.GetSegmentIndex(boneName);
            Debug.Log("Processing bone: " + boneName + ", Index: " + index);

            if (index != -1 && index < processor.GetBarycenters().Count)
            {
                return processor.GetBarycenters()[index];
            }
            else
            {
                Debug.LogError("Invalid index for bone: " + boneName + ", Index: " + index + ", Barycenters count: " + processor.GetBarycenters().Count);
            }
        }
        Debug.LogError("Bone name not found in any MeshDataProcessor: " + boneName);
        return Vector3.zero;
    }



    Quaternion CalculateBoneOrientation(string boneName)
    {
        if (acp != null && acp.Length > 0)
        {
            int index = acp[0].GetSegmentIndex(boneName); // Utilisez acp[0] pour accéder au premier élément du tableau

            // Vérifier si l'index est valide avant d'accéder à la liste des vecteurs propres
            if (index >= 0 && index < acp[0].GetEigenvectors().Count) 
            {
                Vector3 eigenvector = acp[0].GetEigenvectors()[index]; 
                return Quaternion.FromToRotation(Vector3.up, eigenvector);
            }
            else
            {
                //Debug.LogError("Invalid index for bone: " + boneName);
                return Quaternion.identity; // Retourner une valeur par défaut ou une rotation nulle
            }
        }
        else
        {
            Debug.LogError("ACP is not properly initialized.");
            return Quaternion.identity; // Retourner une valeur par défaut ou une rotation nulle
        }
    }



    void SetBoneHierarchy()
    {
        //Debug.Log("Liste des os créés :");
    
        /*foreach (var pair in bones)
        {
            Debug.Log(pair.Key);
        }*/

        // Exemple de parentage des bones
        bones["leg_upper_l"].parent = bones["pelvis"];
        bones["leg_lower_l"].parent = bones["leg_upper_l"];
        bones["feet_l"].parent = bones["leg_lower_l"];
    
        bones["leg_upper_r"].parent = bones["pelvis"];
        bones["leg_lower_r"].parent = bones["leg_upper_r"];
        bones["feet_r"].parent = bones["leg_lower_r"];
    
        bones["buste"].parent = bones["pelvis"];
        bones["arm_l"].parent = bones["buste"];
        bones["arm_r"].parent = bones["buste"];
        bones["head"].parent = bones["buste"];
    }


}