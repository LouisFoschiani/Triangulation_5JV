using UnityEngine;
using System.Collections.Generic;

public class SkeletonBuilder : MonoBehaviour
{
    public GameObject bonePrefab;
    private Dictionary<string, Transform> bones = new Dictionary<string, Transform>();
    private MeshDataProcessor meshDataProcessor;
    private ACP acp;

    void Awake()
    {
        meshDataProcessor = FindObjectOfType<MeshDataProcessor>();
        acp = FindObjectOfType<ACP>();
    }

    void Start()
    {
        // Supposons que les noms des bones correspondent aux noms des GameObjects de segments
        foreach (string boneName in meshDataProcessor.GetSegmentNames())
        {
            CreateBone(boneName);
        }

        // Parentez les bones pour refléter la structure anatomique du personnage
        SetBoneHierarchy();
    }

    void CreateBone(string name)
    {
        Vector3 position = CalculateBonePosition(name);
        Quaternion orientation = CalculateBoneOrientation(name);

        GameObject bone = Instantiate(bonePrefab, position, orientation, transform);
        bone.name = name + "_Bone";
        bones[name] = bone.transform;
    }

    Vector3 CalculateBonePosition(string boneName)
    {
        int index = meshDataProcessor.GetSegmentIndex(boneName);
        return meshDataProcessor.GetBarycenters()[index];
    }

    Quaternion CalculateBoneOrientation(string boneName)
    {
        int index = acp.GetSegmentIndex(boneName);
        Vector3 eigenvector = acp.GetEigenvectors()[index];
        return Quaternion.FromToRotation(Vector3.up, eigenvector);
    }

    void SetBoneHierarchy()
    {
        // Exemple de parentage des bones
        bones["leg_upper_l"].parent = bones["Pelvis"];                    
        bones["leg_lower_l"].parent = bones["leg_upper_l"];               
        bones["feet_l"].parent = bones["leg_lower_l"];                    
                                                                          
        bones["leg_upper_r"].parent = bones["Pelvis"];                    
        bones["leg_lower_r"].parent = bones["leg_upper_r"];               
        bones["feet_r"].parent = bones["leg_lower_r"];                    
                                                                          
        bones["Buste"].parent = bones["Pelvis"];                          
        bones["arm_l"].parent = bones["Buste"];                           
        bones["arm_r"].parent = bones["Buste"];                           
        bones["head"].parent = bones["Buste"];

        // Continuez avec d'autres bones en fonction de leur relation anatomique
    }

    // ... Vous devrez peut-être ajouter des méthodes supplémentaires pour calculer les positions et les orientations ...
}