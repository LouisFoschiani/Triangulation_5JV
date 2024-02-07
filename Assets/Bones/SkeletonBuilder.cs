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

    public GameObject x_bot_v2;
    
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
                Vector3 barycentre = processor.GetWorldBarycenters()[0]; // Prendre le premier barycentre de la liste
               CreateAndPositionBone(processor.gameObject.name, barycentre);
                //Debug.Log("OBJ : " + processor.gameObject.name + " Barycentre : " + barycentre);
            }

        }
        foreach (var acp in acp)
        {
            acp.init();
        }
        void CreateAndPositionBone(string segmentName, Vector3 barycentre)
        {
            if (!bones.ContainsKey(segmentName))
            {
                GameObject bone = Instantiate(bonePrefab, barycentre, Quaternion.identity);
                bone.name = segmentName + "_Bone";
                bones[segmentName] = bone.transform;

            }
        }
        
        void SetInitialHierarchy()
        {
            // Définir x_bot_v2 comme parent initial de pelvis et buste
            bones["pelvis"].SetParent(x_bot_v2.transform, false);
            bones["buste"].SetParent(x_bot_v2.transform, false);
    
            // Définir les enfants de pelvis et buste
            SetChildrenOfBone("pelvis", new List<string> { "leg_upper_l", "leg_upper_r" });
            SetChildrenOfBone("buste", new List<string> { "head", "arm_l", "arm_r" });
    
            // Pour les autres segments, continuez à définir les enfants de manière similaire...
        }
        
        void SetChildrenOfBone(string parentBoneName, List<string> childrenSegments)
        {
            foreach (string child in childrenSegments)
            {
                // Trouvez le GameObject du segment enfant dans la scène
                GameObject childSegment = GameObject.Find(child);
                if(childSegment != null)
                {
                    // Créez un bone pour le segment enfant
                    CreateAndPositionBone(child + "_Bone", CalculateBarycentre(childSegment));
            
                    // Définissez le bone comme enfant du bone parent
                    bones[child + "_Bone"].SetParent(bones[parentBoneName], false);
            
                    // Définissez également le segment enfant comme enfant du bone parent
                    childSegment.transform.SetParent(bones[parentBoneName], false);
                }
                else
                {
                    Debug.LogError("Segment enfant introuvable pour : " + child);
                }
            }
        }
    }


    



}