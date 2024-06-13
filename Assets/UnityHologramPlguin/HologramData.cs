using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[InitializeOnLoad]
//[ExecuteInEditMode]
[System.Serializable]
public class HologramData : MonoBehaviour
{
    [HideInInspector]
    public Vector2 pixelPitch;// { get; set; }
    [HideInInspector]
    public Vector2Int numOfPixels;// { get; set; }
    [HideInInspector]
    public Camera cameraNode;// { get; set; }

    ///Generation
    [HideInInspector]
    public bool fullColor;// { get; set; }
    [HideInInspector]
    public float monoWaveLength;// { get; set; }
    [HideInInspector]
    public Vector3 RGBWaveLength;// { get; set; }
    [HideInInspector]
    public int generationMethod;// { get; set; }
    [HideInInspector]
    public float zDepthScaleFactor;// { get; set; }

    [HideInInspector]
    public string cghOutputFIlePrefix;// { get; set; }
    [HideInInspector]
    public bool randomPhase;// { get; set; }

    [HideInInspector]
    public int numOfLayers;// { get; set; }
    [HideInInspector]
    public Vector2 layerZRange;// { set; get; }

    [HideInInspector]
    public Vector3 carrierWave;// { get; set; }
    [HideInInspector]
    public HolHologramPlugin.MESH_SHADING_OPTION meshShadingOption;//{get;set;}
    [HideInInspector]
    public bool applyTexture;// { get; set; }

    [HideInInspector]
    public bool useGPU;// { get; set; }

    //public MGPointCloud.GENTYPE pointCloudGenMethod;
    [HideInInspector]
    public HolMesh.GENTYPE GenMethod;

    ///Object Info
    [HideInInspector]
    public HolDataObject referenceMesh;// { get; set; }
    [HideInInspector]
    public float scaledDistanseOfObject;// { get; set; }
    [HideInInspector]
    public string saveXMLFileName;
    //static HologramData()
    //{
    //    Debug.Log("LoadHologramData");
    //}
    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("StartHologramData");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public void Reset()
    //{
    //    Debug.Log("ResetLoadHologramData");
    //}

    //public void Awake()
    //{
    //    Debug.Log("AwakeLoadHologramData");
    //}
    //public void OnEnable()
    //{
    //    Debug.Log("OnEnableLoadHologramData");
    //}
}
