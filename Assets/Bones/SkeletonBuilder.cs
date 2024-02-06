using UnityEngine;
using System.Collections.Generic;

public class SkeletonBuilder : MonoBehaviour
{
    public GameObject bonePrefab;
    private Dictionary<string, Transform> bones = new Dictionary<string, Transform>();
    private MeshDataProcessor[] meshDataProcessor;
    private MeshDataProcessor processor;       
    private ACP[] acp;

    public GameObject x_bot_v2;
    void Awake()
    {
        meshDataProcessor = FindObjectsOfType<MeshDataProcessor>();
        acp = FindObjectsOfType<ACP>();
    }

    void Start()
    {
        foreach (var processor in meshDataProcessor)
        {
            processor.init();
        }
        foreach (var acpData in acp)
        {
            acpData.init();
        }
        // Create the base bones which do not depend on other bones
        CreateBone("pelvis", x_bot_v2.transform);
        CreateBone("buste", x_bot_v2.transform);

        // Now create the rest of the bones
        CreateBonesAndSetHierarchy();
    
        // After all bones are created, set up their hierarchy
        SetBoneHierarchy();
    }

    void CreateBonesAndSetHierarchy()
    {
        foreach (var processor in meshDataProcessor)
        {
            foreach (string segmentName in processor.GetSegmentNames())
            {
                if (!bones.ContainsKey(segmentName))
                {
                    Transform boneParent = DetermineBoneParent(segmentName);
                    CreateBone(segmentName, boneParent);
                }
            }
        }

        // Ajustez la hiérarchie des bones pour connecter les segments inférieurs aux supérieurs
        foreach (var boneKVP in bones)
        {
            Transform segmentMeshTransform = FindMeshSegment(boneKVP.Key);
            if(segmentMeshTransform != null)
            {
                segmentMeshTransform.SetParent(boneKVP.Value, false); // Parentez le segment de mesh au bone
            }
        }
    }


    void CreateBone(string segmentName, Transform meshSegmentTransform)
    {
        Vector3 position = CalculateBonePosition(segmentName);
        Quaternion orientation = CalculateBoneOrientation(segmentName);

        // Crée le bone à la position et avec l'orientation calculées
        GameObject bone = Instantiate(bonePrefab, position, orientation);
        bone.name = segmentName + "_Bone";

        // Si meshSegmentTransform n'est pas null, parentez-le au bone
        if (meshSegmentTransform != null)
        {
            // Mettez à jour la hiérarchie pour que le mesh soit l'enfant du bone
            meshSegmentTransform.SetParent(bone.transform, false);
            meshSegmentTransform.localPosition = Vector3.zero;
            meshSegmentTransform.localRotation = Quaternion.identity;
        
            // Optionnellement, si le mesh doit être repositionné pour s'adapter à la position du bone
            // meshSegmentTransform.position = position;
            // meshSegmentTransform.rotation = orientation;
        }
        else
        {
            Debug.LogError("Le GameObject pour le segment de mesh est introuvable: " + segmentName);
        }

        bones[segmentName] = bone.transform;
    }

    Transform FindMeshSegment(string segmentName)
    {
        // Trouvez le GameObject qui représente le segment de mesh dans la scène
        GameObject meshSegment = GameObject.Find(segmentName); // Assurez-vous que le nom correspond exactement
        if (meshSegment != null)
        {
            return meshSegment.transform;
        }
        else
        {
            Debug.LogError("Segment de mesh introuvable pour: " + segmentName);
            return null;
        }
    }
    Transform DetermineBoneParent(string segmentName)
    {
        // Logique simplifiée pour déterminer le parent du bone
        switch (segmentName)
        {
            case "pelvis":
                return bones["pelvis_Bone"];
            case "buste":                   
                return bones["buste_Bone"]; 
            case "leg_upper_l":
                return bones["leg_upper_l_Bone"];
            case "leg_upper_r":
                return bones["leg_upper_r_Bone"];
            case "leg_lower_l":                     
                return bones["leg_lower_l_Bone"];   
            case "leg_lower_r":                     
                return bones["leg_lower_r_Bone"];        
            case "feet_l":                     
                return bones["feet_l_Bone"];   
            case "feet_r":                     
                return bones["feet_r_Bone"];
            case "arm_l":                    
                return bones["arm_l_Bone"];  
            case "arm_r":                    
                return bones["arm_r_Bone"];  
            case "head":                     
                return bones["head_Bone"];   
            default:
                return x_bot_v2.transform; // Retournez le transform racine si aucun autre parent n'est approprié
          
        }
    }

    Vector3 CalculateBonePosition(string boneName)
    {
        foreach (var processor in meshDataProcessor)
        {
            int index = processor.GetSegmentIndex(boneName);
            //Debug.Log(processor.gameObject.name + " Index: " + index);
            if (index != -1 && index < processor.worldBarycenters.Count)
            {
                return processor.worldBarycenters[index]; // Utilisez directement le barycentre mondial
            } 
        }
        return Vector3.zero;
    }

    Quaternion CalculateBoneOrientation(string boneName)
    {
        // Implémentez la logique pour calculer l'orientation du bone
        // Exemple basique :
        if (acp != null && acp.Length > 0)
        {
            int index = acp[0].GetSegmentIndex(boneName);
            if (index >= 0 && index < acp[0].GetEigenvectors().Count)
            {
                Vector3 eigenvector = acp[0].GetEigenvectors()[index];
                return Quaternion.FromToRotation(Vector3.up, eigenvector);
            }
        }
        return Quaternion.identity;
    }

    void SetBoneHierarchy()
    {
        // Set up the hierarchy for the rest of the bones now that they all should exist
        foreach (var boneName in bones.Keys)
        {
            Transform boneParent = DetermineBoneParent(boneName);
            if(boneParent != null)
            {
                bones[boneName].SetParent(boneParent, false);
            }
        }
    }
}