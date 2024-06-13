using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;



public class HolData: MonoBehaviour
{
    public int vertexSize;
    [HideInInspector]
    public float[] verticesX;
    [HideInInspector]
    public float[] verticesY;
    [HideInInspector]
    public float[] verticesZ;
    [HideInInspector]
    public float[] normalsX;
    [HideInInspector]
    public float[] normalsY;
    [HideInInspector]
    public float[] normalsZ;
    [HideInInspector]
    public float[] colorsR;
    [HideInInspector]
    public float[] colorsG;
    [HideInInspector]
    public float[] colorsB;
    [HideInInspector]
    public byte[] DepthImage;
    [HideInInspector]
    public byte[] TextureImage;

    public Vector3 max;
    public Vector3 min;

    public float avgDiff;



    public Matrix4x4 K;
    public Matrix4x4 CamPose;

    public int DepthImageSize;
    public int TextureImageSize;
    public void Stats()
    {
        max = new Vector3(verticesX.Max(), verticesY.Max(), -verticesZ.Min());
        min = new Vector3(verticesX.Min(), verticesY.Min(), -verticesZ.Max());

    }
    private void Start()
    {
        
    }
}
