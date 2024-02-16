using System;
using UnityEngine;
using System.Collections.Generic;
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
    
    private struct LineRendererData
    {
        public LineRenderer lineRenderer;
        public Transform startJoint;
        public Transform endJoint;

        public LineRendererData(LineRenderer lineRenderer, Transform startJoint, Transform endJoint)
        {
            this.lineRenderer = lineRenderer;
            this.startJoint = startJoint;
            this.endJoint = endJoint;
        }
    }
    private List<LineRendererData> lineRendererDatas = new List<LineRendererData>();

    void Update()
    {
        foreach (var lineRendererData in lineRendererDatas)
        {
            UpdateLineRendererPosition(lineRendererData);
        }
    }
    
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
                    //CreateAndPositionBone(processor.gameObject.name, barycentre, orientation);
                    AddJointBone(processor.gameObject.name, processor.min_point, processor.max_point, orientation, barycentre);
                }
        }

        SetInitialHierarchy();
        //SetInitialHierarchyBaryCenter();
    }

    
    //Affichage des barycentres
    void CreateAndPositionBone(string segmentName, Vector3 barycentre, Quaternion orientation)
        {
            if (!bones.ContainsKey(segmentName))
            {
                GameObject bone = Instantiate(bonePrefab, barycentre, orientation); // Utilisez l'orientation ici
                bone.name = segmentName + "_Bone";
                bones[segmentName] = bone.transform;

            }
        }
        void AddJointBone(string parentSegmentName, Vector3 min_point, Vector3 max_point, Quaternion orientation, Vector3 barycentre)
        {
            List<string> allowedSegments2 = new List<string> { "pelvis","leg_upper_l","leg_lower_l","head","feet_r" };
            List<string> allowedSegments1 = new List<string> { "pelvis","head","arm_lower_r","arm_lower_l","arm_upper_r","arm_upper_l","leg_upper_r","leg_lower_r","feet_l" };
            
            //List<string> allowedSegments2 = new List<string> { "pelvis","head","arm_lower_r","arm_lower_l","arm_upper_r","arm_upper_l","leg_upper_r","leg_upper_l","leg_lower_r","leg_lower_l","feet" };
            //List<string> allowedSegments1 = new List<string> { "pelvis","head","arm_lower_r","arm_lower_l","arm_upper_r","arm_upper_l","leg_upper_r","leg_upper_l","leg_lower_r","leg_lower_l","feet" };

            
            if (!joints.ContainsKey(parentSegmentName + "_Joint_1") && allowedSegments1.Contains(parentSegmentName))
            {
                GameObject jointBone = Instantiate(jointPrefab, min_point, orientation);
                jointBone.name = parentSegmentName + "_Joint_1";
                joints[jointBone.name] = jointBone.transform;
            }
            
            if (!joints.ContainsKey(parentSegmentName + "_Joint_2") && allowedSegments2.Contains(parentSegmentName))
            {
                GameObject jointBone2 = Instantiate(jointPrefab, max_point, orientation);
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
            joints["head_Joint_2"].SetParent(GameObject.Find("head").transform, true);
        
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
            joints["feet_r_Joint_2"].SetParent(GameObject.Find("feet_r").transform, true);
            joints["feet_l_Joint_1"].SetParent(GameObject.Find("feet_l").transform, true);
            CreateAndConnectLine();
        }

        void ConfigureLineRenderer(LineRenderer lineRenderer, Vector3 startPosition, Vector3 endPosition)
        {
            lineRenderer.startWidth = 1.0f;
            lineRenderer.endWidth = 1.0f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPosition);
        }
        void UpdateLineRendererPosition(LineRendererData lineRendererData)
        {
            if (lineRendererData.startJoint != null && lineRendererData.endJoint != null)
            {
                lineRendererData.lineRenderer.SetPosition(0, lineRendererData.startJoint.position);
                lineRendererData.lineRenderer.SetPosition(1, lineRendererData.endJoint.position);
            }
        }
        void CreateAndConnectLine()
        {
            
            var jointPairs = new List<Tuple<string, string>>
            {
                Tuple.Create("head_Joint_1", "head_Joint_2"),
                Tuple.Create("arm_upper_r_Joint_1", "arm_lower_r_Joint_1"),
                Tuple.Create("arm_upper_l_Joint_1", "arm_lower_l_Joint_1"),
                Tuple.Create("pelvis_Joint_2", "leg_upper_r_Joint_1"),
                Tuple.Create("leg_upper_r_Joint_1", "leg_lower_r_Joint_1"),
                Tuple.Create("pelvis_Joint_1", "leg_upper_l_Joint_2"),
                Tuple.Create("leg_upper_l_Joint_2", "leg_lower_l_Joint_2"),
                Tuple.Create("feet_r_Joint_2", "leg_lower_r_Joint_1"),
                Tuple.Create("feet_l_Joint_1", "leg_lower_l_Joint_2"),
            };

            foreach (var jointPair in jointPairs)
            {
                CreateAndConfigureLineRenderer(jointPair.Item1, jointPair.Item2);
            }
        }
        
        void CreateAndConfigureLineRenderer(string startJointName, string endJointName)
        {

            GameObject lineObject = new GameObject(startJointName + "_" + endJointName + "_LineRenderer");
            LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

            ConfigureLineRenderer(lineRenderer, joints[startJointName].transform.position, joints[endJointName].transform.position);

            lineObject.transform.SetParent(joints[startJointName], true);
            if (joints.ContainsKey(startJointName) && joints.ContainsKey(endJointName))
            {
                LineRendererData lineRendererData = new LineRendererData(lineRenderer, joints[startJointName], joints[endJointName]);
                lineRendererDatas.Add(lineRendererData);
            }
        }

        
        void SetInitialHierarchyBaryCenter()
        {

            bones["pelvis"].SetParent(character.transform, true);
            bones["buste"].SetParent(character.transform, true);
            
            
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