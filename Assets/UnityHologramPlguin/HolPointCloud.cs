using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
public abstract class HolDataObject : MonoBehaviour
{
    public string OriginFileName;
    [HideInInspector]
    public float AmbientFactor = 1.0f;
    [HideInInspector]
    public float DifusseFactor = 1.0f;
    [HideInInspector]
    public float SpecularFactor = 1.0f;

    [HideInInspector]
    public float ShininessFactor = 1.0f;

    [HideInInspector]
    public float PointSize = 1.0f;

    public abstract void SetFactor();
    public abstract void GetFactor();
}
//[Serializable]

[ExecuteInEditMode]
public class HolPointCloud : HolDataObject
{
    public HolData data;


    public HolPointCloud()
    {
        if (data == null)
        {
            //data = new HolData();
        }
    }
    public enum POINT_CLOUD_DATA_TYPE
    {
        POINT_CLOUD
    }
    public enum DATA_TYPE
    {
        POINT_CLOUD,
        DEPTH_IMAGE
    }
    public enum GEN_TYPE
    {
        POINT_CLOUD,
        LAYER
    }


    public DATA_TYPE dataType;

    public void CopyFromVector3Array(Vector3[] vec, Vector3[] nor = null, Color[] col = null)
    {
        malloc(vec.Length);
        for (int i = 0; i < vec.Length; ++i)
        {
            data.verticesX[i] = vec[i].x;
            data.verticesY[i] = vec[i].y;
            data.verticesZ[i] = vec[i].z;
        }
        if (nor != null)
        {
            if (data.normalsX.Length < vec.Length)
            {
                data.normalsX = new float[vec.Length];
                data.normalsY = new float[vec.Length];
                data.normalsZ = new float[vec.Length];
            }
            for (int i = 0; i < vec.Length; ++i)
            {
                data.normalsX[i] = nor[i].x;
                data.normalsY[i] = nor[i].y;
                data.normalsZ[i] = nor[i].z;
            }
        }
        if (col != null)
        {
            if (data.normalsX.Length < vec.Length)
            {
                data.colorsR = new float[vec.Length];
                data.colorsG = new float[vec.Length];
                data.colorsB = new float[vec.Length];
            }
            for (int i = 0; i < vec.Length; ++i)
            {
                data.colorsR[i] = col[i].r;
                data.colorsG[i] = col[i].g;
                data.colorsB[i] = col[i].b;
            }

        }

    }

    public void malloc(int size)
    {
        data.verticesX = new float[size];
        data.verticesY = new float[size];
        data.verticesZ = new float[size];
        data.normalsX = new float[0];
        data.normalsY = new float[0];
        data.normalsZ = new float[0];
        data.colorsR = new float[0];
        data.colorsG = new float[0];
        data.colorsB = new float[0];
    }
    public void mallocColor(int size)
    {
        data.colorsR = new float[size];
        data.colorsG = new float[size];
        data.colorsB = new float[size];
    }
    public void mallocNormal(int size)
    {
        data.normalsX = new float[size];
        data.normalsY = new float[size];
        data.normalsZ = new float[size];
    }

    public void Stats()
    {
        data.Stats();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    float deltaTime;
    private HolPointCloud targetCloud;

    public void CopyFrom(HolPointCloud targetCloud)
    {
        malloc(targetCloud.data.vertexSize);
        targetCloud.data.verticesX.CopyTo(data.verticesX, 0);
        targetCloud.data.verticesY.CopyTo(data.verticesY, 0);
        targetCloud.data.verticesZ.CopyTo(data.verticesZ, 0);
        targetCloud.data.normalsX.CopyTo(data.normalsX, 0);
        targetCloud.data.normalsY.CopyTo(data.normalsY, 0);
        targetCloud.data.normalsZ.CopyTo(data.normalsZ, 0);
        targetCloud.data.colorsB.CopyTo(data.colorsB, 0);
        targetCloud.data.colorsG.CopyTo(data.colorsG, 0);
        targetCloud.data.colorsR.CopyTo(data.colorsR, 0);

        data.max = targetCloud.data.max;
        data.min = targetCloud.data.min;
        data.avgDiff = targetCloud.data.avgDiff;
        data.vertexSize = targetCloud.data.vertexSize;
    }

    void Update()
    {

    }
    public override void SetFactor()
    {
        foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
        {
            renderer.sharedMaterial.SetFloat("_DiffuseFactor", DifusseFactor);
            renderer.sharedMaterial.SetFloat("_AmbientFactor", AmbientFactor);
            renderer.sharedMaterial.SetFloat("_SpecularFactor", SpecularFactor);
            renderer.sharedMaterial.SetFloat("_Shininess", ShininessFactor);
            renderer.sharedMaterial.SetFloat("_PointSize", PointSize);

        }
    }
    public override void GetFactor()
    {
        var renderer = GetComponentInChildren<MeshRenderer>();
        DifusseFactor = renderer.sharedMaterial.GetFloat("_DiffuseFactor");
        AmbientFactor = renderer.sharedMaterial.GetFloat("_AmbientFactor");
        SpecularFactor = renderer.sharedMaterial.GetFloat("_SpecularFactor");
        ShininessFactor = renderer.sharedMaterial.GetFloat("_Shininess");
        PointSize = renderer.sharedMaterial.GetFloat("_PointSize");
    }

}
