using System;
using UnityEngine;
using System.Collections.Generic;

public class SkeletonBuilder : MonoBehaviour
{
    public GameObject bonePrefab;
    private Dictionary<string, Transform> bones = new Dictionary<string, Transform>();
    private MeshDataProcessor[] meshDataProcessor;
    private MeshDataProcessor processor;       
    private ACP[] acp;

    public GameObject character;
    
    private List<string> segmentNames = new List<string>
    {
        "pelvis",
        "buste",
        "head",
        "leg_upper_l",
        "leg_lower_l",
        "feet_l",
        "leg_upper_r",
        "leg_lower_r",
        "feet_r",
        "arm_l",
        "arm_r"
    };
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
                if (processor.GetWorldBarycenters().Count > 0)
                {
                    Vector3 barycentre = processor.GetWorldBarycenters()[0];
                    Vector3 eigenvector = processor.GetEigenvectors()[0];
                    Quaternion orientation = Quaternion.LookRotation(eigenvector);
                    
                    CreateAndPositionBone(processor.gameObject.name, barycentre, orientation);
                }
        }

        SetInitialHierarchy();
    }


    void CreateAndPositionBone(string segmentName, Vector3 barycentre, Quaternion orientation)
        {
            if (!bones.ContainsKey(segmentName))
            {
                GameObject bone = Instantiate(bonePrefab, barycentre, orientation); // Utilisez l'orientation ici
                bone.name = segmentName + "_Bone";
                bones[segmentName] = bone.transform;

            }
        }
        void AddJointBone(string parentSegmentName, string jointName, Vector3 jointPosition)
        {
            if (!bones.ContainsKey(jointName))
            {
                // Créez le joint (pivot) au niveau du genou par exemple
                GameObject jointBone = new GameObject(jointName + "_Bone");
                jointBone.transform.position = jointPosition;
                jointBone.transform.SetParent(bones[parentSegmentName], true); // Parenté à l'os de la cuisse
                bones[jointName] = jointBone.transform;
            }
        }
        void SetInitialHierarchy()
        {
            // x_bot_v2 est le parent de tous les premiers bones
            bones["pelvis"].SetParent(character.transform, true);
            bones["buste"].SetParent(character.transform, true);
            
            // PELVIS
            GameObject.Find("pelvis").transform.SetParent(bones["pelvis"], true);
            bones["leg_upper_l"].SetParent(GameObject.Find("pelvis").transform, true);
            bones["leg_upper_r"].SetParent(GameObject.Find("pelvis").transform, true);
            
            GameObject.Find("leg_upper_l").transform.SetParent(bones["leg_upper_l"], true);
            GameObject.Find("leg_upper_r").transform.SetParent(bones["leg_upper_r"], true);
            bones["leg_lower_l"].SetParent(GameObject.Find("leg_upper_l").transform, true);
            bones["leg_lower_r"].SetParent(GameObject.Find("leg_upper_r").transform, true);
            
            GameObject.Find("leg_lower_l").transform.SetParent(bones["leg_lower_l"], true);
            GameObject.Find("leg_lower_r").transform.SetParent(bones["leg_lower_r"], true);
            
            bones["feet_l"].SetParent(GameObject.Find("leg_lower_l").transform, true);
            bones["feet_r"].SetParent(GameObject.Find("leg_lower_r").transform, true);
            GameObject.Find("feet_l").transform.SetParent(bones["feet_l"], true);
            GameObject.Find("feet_r").transform.SetParent(bones["feet_r"], true);
            
            // BUSTE
            GameObject.Find("buste").transform.SetParent(bones["buste"], true);
            GameObject.Find("arm_upper_l").transform.SetParent(bones["arm_upper_l"], true);
            GameObject.Find("arm_upper_r").transform.SetParent(bones["arm_upper_r"], true);
            GameObject.Find("head").transform.SetParent(bones["head"], true);
            
            bones["arm_upper_l"].SetParent(GameObject.Find("buste").transform, true);
            bones["arm_upper_r"].SetParent(GameObject.Find("buste").transform, true);
            
            bones["arm_lower_l"].SetParent(GameObject.Find("arm_upper_l").transform, true);
            bones["arm_lower_r"].SetParent(GameObject.Find("arm_upper_r").transform, true);
            
            GameObject.Find("arm_lower_l").transform.SetParent(bones["arm_lower_l"], true);
            GameObject.Find("arm_lower_r").transform.SetParent(bones["arm_lower_r"], true);
            
            bones["head"].SetParent(GameObject.Find("buste").transform, true);
        }
}