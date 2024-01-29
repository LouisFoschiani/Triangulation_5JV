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

        foreach (var meshData in meshDataProcessor)
        {
            meshData.init();
        }
        foreach (var acpData in acp)
        {
            acpData.init();
        }
        
        if (meshDataProcessor != null && meshDataProcessor.Length > 0 && acp != null && acp.Length > 0)
        {
            foreach (var meshData in meshDataProcessor)
            {
                foreach (string segmentName in meshData.GetSegmentNames())
                {
                    CreateAndRepositionBone(segmentName);
                }
            }
 
            SetBoneHierarchy();
        }
    }
    void CreateAndRepositionBone(string segmentName)
    {
        Vector3 position = CalculateBonePosition(segmentName);
        Debug.Log("Position du barycentre pour " + segmentName + ": " + position);
        Quaternion orientation = CalculateBoneOrientation(segmentName);
        GameObject bone = Instantiate(bonePrefab, position, orientation, transform);
        if (bone != null)
        {
            bone.name = segmentName + "_Bone";
            bones[segmentName] = bone.transform;
            //RepositionBone(segmentName);
        }
        else
        {
            Debug.LogError("Failed to instantiate bone prefab for: " + segmentName);
        }
    }
    
    void RepositionSegment(string segmentName, Vector3 newPosition, Quaternion newOrientation)
    {
        if (bones.TryGetValue(segmentName, out Transform boneTransform))
        {
            boneTransform.position = newPosition;
            boneTransform.rotation = newOrientation;
        }
        else
        {
            Debug.LogError("Segment not found: " + segmentName);
        }
    }

    Vector3 CalculateBonePosition(string boneName)
    {
        if (meshDataProcessor != null && meshDataProcessor.Length > 0)
        {
            int index = meshDataProcessor[0].GetSegmentIndex(boneName);
            if (index != -1 && index < meshDataProcessor[0].GetBarycenters().Count)
            {
                return meshDataProcessor[0].GetBarycenters()[index];
            }
            else
            {
                //Debug.LogError("Invalid index for bone: " + boneName + ", Index: " + index + ", Barycenters count: " + meshDataProcessor[0].GetBarycenters().Count);
                return Vector3.zero; // Retourne une position par défaut en cas d'erreur
            }
        }
        else
        {
            Debug.LogError("MeshDataProcessor is not properly initialized.");
            return Vector3.zero; // Retourne une position par défaut si MeshDataProcessor n'est pas initialisé
        }
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
    
    /*void RepositionBone(string segmentName)
    {
        int index = meshDataProcessor[0].GetSegmentIndex(segmentName);
        if (index != -1 && index < acp[0].GetEigenvectors().Count)
        {
            Vector3 newPosition = meshDataProcessor[0].GetBarycenters()[index];
            Vector3 eigenvector = acp[0].GetEigenvectors()[index];
            Quaternion newOrientation = Quaternion.FromToRotation(Vector3.up, eigenvector);
            RepositionSegment(segmentName, newPosition, newOrientation);
        }
        else
        {
            Debug.LogError("Invalid index for repositioning bone: " + segmentName);
        }
    }
    
    void CreateBone(string name)
    {
        Vector3 position = CalculateBonePosition(name, name);
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
       
    }*/


}
