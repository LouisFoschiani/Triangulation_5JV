using UnityEngine;
using System.Collections.Generic;

public class SkeletonBuilder : MonoBehaviour
{
    public GameObject bonePrefab;
    private Dictionary<string, Transform> bones = new Dictionary<string, Transform>();
    private MeshDataProcessor[] meshDataProcessor;
    private ACP[] acp;

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
            foreach (var barycenter in processor.worldBarycenters)
            {
                Debug.Log(processor.gameObject.name + " Barycentre mondial: " + barycenter);
            }
        }
        foreach (var acpData in acp)
        {
            acpData.init();
        }
        CreateBonesAndSetHierarchy();
        
    }

    void CreateBonesAndSetHierarchy()
    {
        CreateBone("pelvis");
        CreateBone("buste");
        foreach (var processor in meshDataProcessor)
        {
            foreach (string segmentName in processor.GetSegmentNames())
            {
                if (!bones.ContainsKey(segmentName) && segmentName != "pelvis" && segmentName != "buste")
                {
                    CreateBone(segmentName);
                }
            }
        }

        SetBoneHierarchy();
    }

    void CreateBone(string segmentName)
    {
        // Obtenez le barycentre mondial pour la position du bone
        Vector3 position = CalculateBonePosition(segmentName);

        // Calculez l'orientation du bone en utilisant les données ACP
        Quaternion orientation = CalculateBoneOrientation(segmentName);
        Debug.Log("Indice de " + segmentName + " : " + meshDataProcessor[0].GetSegmentIndex(segmentName));

        // Instanciez le bone avec la position et l'orientation correctes
        GameObject bone = Instantiate(bonePrefab, position, orientation);
        if (bone != null)
        {
            bone.name = segmentName + "_Bone";
            bones[segmentName] = bone.transform;
        }
        else
        {
            Debug.LogError("Échec de l'instanciation du bone pour : " + segmentName);
        }
    }


    Transform DetermineBoneParent(string segmentName) {
        switch (segmentName) {
            case "leg_upper_l":
            case "leg_upper_r":
                return GameObject.Find("pelvis").transform; // Retrouvez le GameObject "pelvis" dans la scène et utilisez son transform
            case "leg_lower_l":
                return GameObject.Find("leg_upper_l_Bone").transform; // Le bone "leg_upper_l_Bone" est le parent
            case "leg_lower_r":
                return GameObject.Find("leg_upper_r_Bone").transform; // Le bone "leg_upper_r_Bone" est le parent
            case "feet_l":
                return GameObject.Find("leg_lower_l_Bone").transform; // Le bone "leg_lower_l_Bone" est le parent
            case "feet_r":
                return GameObject.Find("leg_lower_r_Bone").transform; // Le bone "leg_lower_r_Bone" est le parent
            case "buste":
                return GameObject.Find("pelvis").transform; // Le GameObject "pelvis" est le parent
            case "arm_l":
            case "arm_r":
                return GameObject.Find("buste_Bone").transform; // Le bone "buste_Bone" est le parent
            case "head":
                return GameObject.Find("buste_Bone").transform; // Le bone "buste_Bone" est le parent
            default:
                return GameObject.Find("x_bot_v2").transform; // Si le segment n'est pas listé, utilisez un parent par défaut
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
        // Configurer la hiérarchie des bones
        // Exemple de configuration basique :
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
