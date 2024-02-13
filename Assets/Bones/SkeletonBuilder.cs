using System;
using UnityEngine;
using System.Collections.Generic;
using System.Numerics;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class SkeletonBuilder : MonoBehaviour
{
    public GameObject bonePrefab;
    public GameObject jointPrefab;
    private Dictionary<string, Transform> bones = new Dictionary<string, Transform>();
    private Dictionary<string, Transform> joints = new Dictionary<string, Transform>();
    private MeshDataProcessor[] meshDataProcessor;
    private MeshDataProcessor processor;       


    public GameObject character;
   
    void Awake()
    {
        meshDataProcessor = FindObjectsOfType<MeshDataProcessor>();
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
                    AddJointBone(processor.gameObject.name, processor.BMin, processor.CMax, orientation, barycentre);
                }
        }

        SetInitialHierarchy();
        SetInitialHierarchyBaryCenter();
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
        void AddJointBone(string parentSegmentName, Vector3 BMin, Vector3 CMax, Quaternion orientation, Vector3 barycentre)
        {
            //Debug.Log("Mesh : "+parentSegmentName+" Bmin : "+ BMin);
            //Debug.Log("Mesh : "+parentSegmentName+" Cmax : "+ CMax);
            List<string> allowedSegments2 = new List<string> { "pelvis","leg_upper_l","leg_lower_l" };
            List<string> allowedSegments1 = new List<string> { "pelvis","head","arm_lower_r","arm_lower_l","arm_upper_r","arm_upper_l","leg_upper_r","leg_lower_r" };
            
            if (!joints.ContainsKey(parentSegmentName + "_Joint_1") && allowedSegments1.Contains(parentSegmentName))
            {
                GameObject jointBone = Instantiate(jointPrefab, BMin + barycentre, orientation);
                jointBone.name = parentSegmentName + "_Joint_1";
                joints[jointBone.name] = jointBone.transform; 
            }

            // Créer le deuxième joint
            if (!joints.ContainsKey(parentSegmentName + "_Joint_2") && allowedSegments2.Contains(parentSegmentName))
            {
                GameObject jointBone2 = Instantiate(jointPrefab, CMax + barycentre, orientation);
                jointBone2.name = parentSegmentName + "_Joint_2";
                joints[jointBone2.name] = jointBone2.transform;
            }  
        }
        void SetInitialHierarchy()
        {
            GameObject.Find("pelvis").transform.SetParent(character.transform, true);
   
            joints["head_Joint_1"].SetParent(GameObject.Find("buste").transform, true);
            GameObject.Find("head").transform.SetParent(joints["head_Joint_1"], true);
            GameObject.Find("buste").transform.SetParent(character.transform, true);
            //joints["buste_Joint_1"].SetParent(GameObject.Find("buste").transform, true);
            //joints["buste_Joint_2"].SetParent(GameObject.Find("buste").transform, true);

            joints["arm_upper_r_Joint_1"].SetParent(GameObject.Find("buste").transform, true);
            GameObject.Find("arm_upper_r").transform.SetParent(joints["arm_upper_r_Joint_1"], true);
            joints["arm_lower_r_Joint_1"].SetParent(GameObject.Find("arm_upper_r").transform, true);
            GameObject.Find("arm_lower_r").transform.SetParent(joints["arm_lower_r_Joint_1"], true);
            
            joints["arm_upper_l_Joint_1"].SetParent(GameObject.Find("buste").transform, true);
            GameObject.Find("arm_upper_l").transform.SetParent(joints["arm_upper_l_Joint_1"], true);
            joints["arm_lower_l_Joint_1"].SetParent(GameObject.Find("arm_upper_l").transform, true);
            GameObject.Find("arm_lower_l").transform.SetParent(joints["arm_lower_l_Joint_1"], true);
            
            GameObject.Find("pelvis").transform.SetParent(character.transform, true);
            joints["pelvis_Joint_1"].SetParent(GameObject.Find("pelvis").transform, true);
            joints["pelvis_Joint_2"].SetParent(GameObject.Find("pelvis").transform, true);
            
            GameObject.Find("leg_upper_l").transform.SetParent(joints["pelvis_Joint_1"], true);
            GameObject.Find("leg_upper_r").transform.SetParent(joints["pelvis_Joint_2"], true);
            joints["leg_upper_r_Joint_1"].SetParent(GameObject.Find("leg_upper_r").transform, true);
            joints["leg_upper_l_Joint_2"].SetParent(GameObject.Find("leg_upper_l").transform, true);
            GameObject.Find("leg_lower_l").transform.SetParent(joints["leg_upper_l_Joint_2"], true);
            GameObject.Find("leg_lower_r").transform.SetParent(joints["leg_upper_r_Joint_1"], true);
            
            
            joints["leg_lower_r_Joint_1"].SetParent(GameObject.Find("leg_lower_r").transform, true);
            joints["leg_lower_l_Joint_2"].SetParent(GameObject.Find("leg_lower_l").transform, true);
            GameObject.Find("feet_l").transform.SetParent(joints["leg_lower_l_Joint_2"], true);
            GameObject.Find("feet_r").transform.SetParent(joints["leg_lower_r_Joint_1"], true);    
            
        }

        void SetInitialHierarchyBaryCenter()
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