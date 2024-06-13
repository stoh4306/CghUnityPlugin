#if UNITY_STANDALONE
#define IMPORT_GLENABLE
#endif


using B83.Image.BMP;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Jobs;
using UnityEditor.SceneManagement;
using Unity.Collections;


public class HolObjectLoader : Editor
{
    public static HolObjectLoader GetInstance()
    {
        if (instance == null)
        {
            instance = CreateInstance<HolObjectLoader>();
            instance.Init();
        }
        return instance;
    }


    static HolObjectLoader instance = null;

    [DllImport("cpp_plugin_holo")]
    private static extern bool GetCloudAndColorAndNormal([MarshalAs(UnmanagedType.LPWStr)]string fileName,
        ref int resultVertLength, ref IntPtr ptrResultVertsX, ref IntPtr ptrResultVertsY, ref IntPtr ptrResultVertsZ,
        ref int resultColorSize, ref IntPtr ptrResultColorR, ref IntPtr ptrResultColorG, ref IntPtr ptrResultColorB,
        ref int resultNormalSize, ref IntPtr ptrResultNormalX, ref IntPtr ptrResultNormalY, ref IntPtr ptrResultNroamlZ, ref float avgDist);

    [DllImport("cpp_plugin_holo")]
    private static extern bool LoadPointCloudAndNormalFromDepth(ref IntPtr ptrDepthImage, ref int depthImageSize, [MarshalAs(UnmanagedType.LPWStr)]string depthFileName, [MarshalAs(UnmanagedType.LPWStr)]string KFileName, [MarshalAs(UnmanagedType.LPWStr)]string camPoseFileName,
        ref IntPtr ptrResultVertsX, ref IntPtr ptrResultVertsY, ref IntPtr ptrResultVertsZ, ref IntPtr ptrResultNormalX, ref IntPtr ptrResultNormalY, ref IntPtr ptrResultNormalZ, ref int resultVertLength, int normalCalcSize, ref float avgDist, bool useMultiThread);

    [DllImport("cpp_plugin_holo")]
    private static extern bool LoadPointCloudAndColorNormalFromDepth(ref IntPtr ptrDepthImage, ref int depthImageSize, ref IntPtr ptrTextureImage, ref int textureImageSize, [MarshalAs(UnmanagedType.LPWStr)]string depthFileName, [MarshalAs(UnmanagedType.LPWStr)]string KFileName, [MarshalAs(UnmanagedType.LPWStr)]string camPoseFileName, [MarshalAs(UnmanagedType.LPWStr)]string textureFileName,
        ref IntPtr ptrResultVertsX, ref IntPtr ptrResultVertsY, ref IntPtr ptrResultVertsZ, ref IntPtr ptrResultColorR, ref IntPtr ptrResultColorG, ref IntPtr ptrResultColorB, ref IntPtr ptrResultNormalX, ref IntPtr ptrResultNormalY, ref IntPtr ptrResultNormalZ, ref int resultVertLength, int normalCalcSize, ref float avgDist, bool useMultiThread);
    [DllImport("cpp_plugin_holo")]
    private static extern bool LoadPointCloudAndColorNormalFromDepthUseMinMax(ref IntPtr ptrDepthImage, ref int depthImageSize, ref IntPtr ptrTextureImage, ref int textureImageSize, [MarshalAs(UnmanagedType.LPWStr)]string depthFileName, float zMin, float zMax, float scale, [MarshalAs(UnmanagedType.LPWStr)]string textureFileName,
        ref IntPtr ptrResultVertsX, ref IntPtr ptrResultVertsY, ref IntPtr ptrResultVertsZ, ref IntPtr ptrResultColorR, ref IntPtr ptrResultColorG, ref IntPtr ptrResultColorB, ref IntPtr ptrResultNormalX, ref IntPtr ptrResultNormalY, ref IntPtr ptrResultNormalZ, ref int resultVertLength, int normalCalcSize, ref float avgDist);

    [DllImport("cpp_plugin_holo")]
    private static extern float CalcAvgDistansePoints([MarshalAs(UnmanagedType.LPArray)]float[] vertsX, [MarshalAs(UnmanagedType.LPArray)]float[] vertsY, [MarshalAs(UnmanagedType.LPArray)]float[] vertsZ, int vertexSize);


    [DllImport("cpp_plugin_holo")]
    private static extern int CalcTextureUV(byte[] meshBuffer, [MarshalAs(UnmanagedType.LPWStr)]string kFileName, [MarshalAs(UnmanagedType.LPWStr)]string camPoseFile, int width, int height, ref IntPtr resultBuffer);


    [DllImport("cpp_plugin_holo")]
    private static extern bool SaveCloudFile([MarshalAs(UnmanagedType.LPWStr)]string fileName, bool saveBinary,
        int size, [MarshalAs(UnmanagedType.LPArray)]float[] x, [MarshalAs(UnmanagedType.LPArray)]float[] y, [MarshalAs(UnmanagedType.LPArray)]float[] z,
        int colorSize, [MarshalAs(UnmanagedType.LPArray)]float[] r, [MarshalAs(UnmanagedType.LPArray)]float[] g, [MarshalAs(UnmanagedType.LPArray)]float[] b,
        int normalSize, [MarshalAs(UnmanagedType.LPArray)]float[] nx, [MarshalAs(UnmanagedType.LPArray)]float[] ny, [MarshalAs(UnmanagedType.LPArray)]float[] nz);

    [DllImport("cpp_plugin_holo")]
    private static extern bool SaveBothDepth([MarshalAs(UnmanagedType.LPArray)]float[] data, int width, int height, [MarshalAs(UnmanagedType.LPWStr)]string file8bitName, [MarshalAs(UnmanagedType.LPWStr)]string file16bitName);

    public enum FileType { ASCII, BINARY }
    Mesh mesh;
    public string fileName { get; set; }
    public string saveFileName { get; set; }
    public double filterSize { get; set; }
    public double scaleSize { get; set; }
    public float normalCalcRadius { get; set; }
    public FileType fileType { get; set; }
    public bool useFilter { get; set; }
    public bool useColor { get; set; }
    public Color pointColor { get; set; }



    public string KFileName { get; set; }
    public string depthFileName { get; set; }
    public string camPoseFileName { get; set; }
    public string depthPointsTextureFIleName { get; set; }

    public int normalCalcSize { get; set; }




    public string sourceObjectPath { get; set; }
    public string loadObjectPath { get; set; }
    public string sourceTexturePath { get; set; }
    public string loadTexturePath { get; set; }

    public int previewResolution;

    HolDataObject targetData;
    public HolDataObject TargetData
    {
        get { return targetData; }
        set { targetData = value; }
    }

    public void CameraSetting()
    {
        var trans = Camera.main.transform;
        trans.position = Vector3.zero;
        trans.rotation = Quaternion.identity;

    }

    public HolMesh targetObject
    {
        get
        {
            return targetData as HolMesh;
        }
    }
    public HolPointCloud targetCloud
    {
        get
        {
            return targetData as HolPointCloud;
        }
    }

    public Color meshColor { get; set; }
    const UInt32 GL_VERTEX_PROGRAM_POINT_SIZE = 0x8642;
    public float zMin { get; set; }
    public float zMax { get; set; }
    public float xyScale { get; set; }

    const string LibGLPath =
#if UNITY_STANDALONE_WIN
         "opengl32.dll";
#elif UNITY_STANDALONE_OSX
         "/System/Library/Frameworks/OpenGL.framework/OpenGL";
#elif UNITY_STANDALONE_LINUX
         "libGL";    // Untested on Linux, this may not be correct
#else
         null;   // OpenGL ES platforms don't require this feature
#endif

#if IMPORT_GLENABLE
    [DllImport(LibGLPath)]
    public static extern void glEnable(UInt32 cap);

    private bool mIsOpenGL;
#endif

    public void traceSelect()
    {

        GameObject selectObj = (Selection.activeObject as GameObject);
        if (selectObj != null)
        {
            var mGPointCloud = selectObj.GetComponent<HolPointCloud>();
            if (mGPointCloud != null)
            {
                targetData = mGPointCloud;
                if (targetCloud != null && targetCloud.data != null)
                {
                    normalCalcRadius = targetCloud.data.avgDiff * 5.5f;
                }
            }
            else
            {
                var meshFilter = selectObj.GetComponentInChildren<MeshFilter>();
                var meshRenderer = selectObj.GetComponentInChildren<MeshRenderer>();
                if (meshFilter != null && meshFilter.gameObject != selectObj)
                {
                    var hpMesh = selectObj.GetComponent<HolMesh>();
                    if (hpMesh == null)
                    {
                        targetData = selectObj.AddComponent<HolMesh>();
                        meshZInsideOut(targetData as HolMesh, "");

                    }
                    else
                    {
                        targetData = hpMesh;
                    }
                }
            }


        }

    }
    private void OnHierarchyChange()
    {
        var objs = FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];

        foreach (var obj in objs)
        {
            var parent = obj.transform.parent;
            if (parent != null)
            {
                var dataObj = parent.GetComponent<HolDataObject>();
                if (dataObj == null)
                {
                    dataObj = parent.gameObject.AddComponent<HolMesh>();

                    meshZInsideOut(dataObj as HolMesh, "");
                }
            }
        }

    }
    float[] depthData;

    bool useMultiThread = false;

    public void Init()
    {
        Camera.main.orthographic = true;
        filterSize = 0.1;
        scaleSize = 0.01;
        xyScale = 0.01f;
        normalCalcSize = 1;
        previewResolution = 1;
        mIsOpenGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
        if (mIsOpenGL)
        {
            glEnable(GL_VERTEX_PROGRAM_POINT_SIZE);
        }

        var objs = FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];

        foreach (var obj in objs)
        {
            var parent = obj.transform.parent;
            if (parent != null)
            {
                var dataObj = parent.GetComponent<HolMesh>();
                if (dataObj != null)
                    meshZInsideOut(dataObj as HolMesh, "");
            }
        }
    }

    public void normalZchange()
    {
        var meshfilters = targetCloud.GetComponentsInChildren<MeshFilter>();
        foreach (var meshfilter in meshfilters)
        {
            var mesh = meshfilter.sharedMesh;
            Vector3[] normals = new Vector3[mesh.normals.Length];
            mesh.normals.CopyTo(normals, 0);
            int normalsize = normals.Length;
            for (int i = 0; i < normalsize; ++i)
                normals[i] = -normals[i];
            mesh.normals = normals;
        }
    }
    GameObject MakeCloudObject(string fileName)
    {
        IntPtr ptrResultVertsX = IntPtr.Zero;
        IntPtr ptrResultVertsY = IntPtr.Zero;
        IntPtr ptrResultVertsZ = IntPtr.Zero;

        IntPtr ptrResultColorR = IntPtr.Zero;
        IntPtr ptrResultColorG = IntPtr.Zero;
        IntPtr ptrResultColorB = IntPtr.Zero;

        IntPtr ptrResultNormalX = IntPtr.Zero;
        IntPtr ptrResultNormalY = IntPtr.Zero;
        IntPtr ptrResultNormalZ = IntPtr.Zero;
        int resultVertLength = 0;
        float avgDist = 1;
        GameObject rootObj;
        int colorSize = 0;
        int normalSize = 0;
        bool success = GetCloudAndColorAndNormal(fileName, ref colorSize, ref ptrResultVertsX, ref ptrResultVertsY, ref ptrResultVertsZ, ref resultVertLength,
            ref ptrResultColorR, ref ptrResultColorG, ref ptrResultColorB,
            ref normalSize, ref ptrResultNormalX, ref ptrResultNormalY, ref ptrResultNormalZ, ref avgDist);
        if (success)
        {

            rootObj = new GameObject(Path.GetFileName(fileName));
            HolPointCloud data = rootObj.AddComponent<HolPointCloud>();
            GameObject dataObj = new GameObject(Path.GetFileName(fileName).Split('.')[0] + "data");
            dataObj.transform.parent = rootObj.transform;
            data.data = dataObj.AddComponent<HolData>();
            // Load the results into a managed array.
            data.data.vertexSize = resultVertLength;

            data.malloc(resultVertLength);

            Marshal.Copy(ptrResultVertsX, data.data.verticesX, 0, resultVertLength);
            Marshal.Copy(ptrResultVertsY, data.data.verticesY, 0, resultVertLength);
            Marshal.Copy(ptrResultVertsZ, data.data.verticesZ, 0, resultVertLength);

            if (colorSize > 0)
            {
                data.mallocColor(colorSize);
                Marshal.Copy(ptrResultColorR, data.data.colorsR, 0, colorSize);
                Marshal.Copy(ptrResultColorG, data.data.colorsG, 0, colorSize);
                Marshal.Copy(ptrResultColorB, data.data.colorsB, 0, colorSize);
            }
            if (normalSize > 0)
            {
                data.mallocNormal(normalSize);
                Marshal.Copy(ptrResultNormalX, data.data.normalsX, 0, normalSize);
                Marshal.Copy(ptrResultNormalY, data.data.normalsY, 0, normalSize);
                Marshal.Copy(ptrResultNormalZ, data.data.normalsZ, 0, normalSize);
            }
            int current = 0;

            int part = 0;
            while (current < resultVertLength)
            {
                part++;
                int num = Math.Min(65536, resultVertLength - current);
                int colorNum = Math.Min(65536, colorSize - current);
                int normalNum = Math.Min(65536, normalSize - current);

                GameObject targetObj = new GameObject(Path.GetFileName(fileName) + "_part_" + part);// + "mesh_" + current + "_to_" + (current + num));

                targetObj.transform.parent = rootObj.transform;
                var meshFilter = targetObj.GetComponent<MeshFilter>();
                if (meshFilter == null)
                    meshFilter = targetObj.AddComponent<MeshFilter>();
                var renderer = targetObj.GetComponent<MeshRenderer>();
                if (renderer == null)
                    renderer = targetObj.AddComponent<MeshRenderer>();
                if (renderer.sharedMaterial == null)
                    renderer.sharedMaterial = new Material(Shader.Find("Hol/PointRenderer"));
                else
                    renderer.sharedMaterial.shader = Shader.Find("Hol/PointRenderer");

                renderer.sharedMaterial.SetFloat("_pointSize", 5.0f);
                mesh = new Mesh();
                meshFilter.mesh = mesh;

                Vector3[] points = new Vector3[num];
                Vector3[] normals = new Vector3[num];
                int[] indecies = new int[num];
                Color[] colors = new Color[num];

                for (int i = 0; i < num; i++)
                {
                    if (i < colorNum)
                        colors[i] = new Color(data.data.colorsR[current + i] / 255.0f, data.data.colorsG[current + i] / 255.0f, data.data.colorsB[current + i] / 255.0f);
                    else
                        colors[i] = Color.black;
                    Vector3 newPts = new Vector3(data.data.verticesX[current + i], data.data.verticesY[current + i], -data.data.verticesZ[current + i]);
                    points[i] = newPts;
                    if (i < normalNum)
                        normals[i] = new Vector3(data.data.normalsX[current + i], data.data.normalsY[current + i], -data.data.normalsZ[current + i]);
                    else
                        normals[i] = Vector3.zero;
                    indecies[i] = i;
                }
                mesh.vertices = points;
                mesh.colors = colors;
                mesh.normals = normals;
                mesh.SetIndices(indecies, MeshTopology.Points, 0);

                mesh.RecalculateBounds();
                current += num;
            }
            data.Stats();
            data.data.avgDiff = avgDist;
            return rootObj;
        }
        else
        {
            Debug.Log("Ended not sucessfully.");
            return null;
        }
    }
    public GameObject LoadPointCloud()
    {
        if (System.IO.File.Exists(fileName))
        {
            return MakeCloudObject(fileName);
        }
        else
        {
            Debug.Log("Can't find File : " + fileName);
            return null;
        }
    }
    public GameObject LoadPointCloud(string _fileName)
    {
        return MakeCloudObject(_fileName);
    }


    public void SetColor()
    {
        foreach (var c in targetCloud.GetComponentsInChildren<MeshFilter>())
        {
            Color[] newColors = new Color[c.sharedMesh.vertices.Length];
            for (int i = 0; i < newColors.Length; ++i)
            {
                newColors[i] = pointColor;
            }
            c.sharedMesh.colors = newColors;

        }
        for (int i = 0; i < targetCloud.data.vertexSize; ++i)
        {
            targetCloud.data.colorsR[i] = pointColor.r * 255;
            targetCloud.data.colorsG[i] = pointColor.g * 255;
            targetCloud.data.colorsB[i] = pointColor.b * 255;
        }
    }

    public void SavePointCloudFIle(HolPointCloud targetCloud)
    {

        bool isBinary = false;
        switch (fileType)
        {
            case FileType.ASCII:
                isBinary = false;
                break;
            case FileType.BINARY:
                isBinary = true;
                break;
        }
        int resultVertLength = targetCloud.data.vertexSize;

        float[] verX = new float[resultVertLength];
        float[] verY = new float[resultVertLength];
        float[] verZ = new float[resultVertLength];

        for (int i = 0; i < resultVertLength; ++i)
        {
            Vector3 pts = new Vector3(targetCloud.data.verticesX[i], targetCloud.data.verticesY[i], targetCloud.data.verticesZ[i]);
            verX[i] = pts.x;
            verY[i] = pts.y;
            verZ[i] = pts.z;
        }

        SaveCloudFile(saveFileName, isBinary, targetCloud.data.vertexSize,
            verX, verY, verZ,
            targetCloud.data.colorsR.Length, targetCloud.data.colorsR, targetCloud.data.colorsG, targetCloud.data.colorsB,
            targetCloud.data.normalsX.Length, targetCloud.data.normalsX, targetCloud.data.normalsY, targetCloud.data.normalsZ
            );
    }
    public float GetAvgDistanse(HolPointCloud cloud)
    {
        Transform trs = cloud.transform;
        Matrix4x4 mtx = new Matrix4x4();
        mtx[0] = trs.localScale.x;
        mtx[5] = trs.localScale.y;
        mtx[10] = trs.localScale.z;
        float[] verX = new float[cloud.data.vertexSize];
        float[] verY = new float[cloud.data.vertexSize];
        float[] verZ = new float[cloud.data.vertexSize];

        for (int i = 0; i < cloud.data.vertexSize; ++i)
        {
            Vector3 pts = new Vector3(cloud.data.verticesX[i], cloud.data.verticesY[i], cloud.data.verticesZ[i]);
            pts = mtx * pts;
            verX[i] = pts.x;
            verY[i] = pts.y;
            verZ[i] = pts.z;
        }

        return CalcAvgDistansePoints(verX, verY, verZ, cloud.data.vertexSize);
    }
    public bool reCalcNormal { get; set; }
    public void SavePointCloud()
    {
        SavePointCloudFIle(targetCloud);

    }
    public void SaveCloudFile(string fileName, bool saveBinary,
        int size, float[] x, float[] y, float[] z,
        float[] r, float[] g, float[] b,
        float[] nx, float[] ny, float[] nz)
    {
        SaveCloudFile(fileName, saveBinary, size, x, y, z,
            r.Length, r, g, b,
            nx.Length, nx, ny, nz);
    }
    public void SaveCloudFile(string fileName, bool saveBinary,
        HolPointCloud cloud)
    {
        SaveCloudFile(fileName, saveBinary, cloud.data.vertexSize, cloud.data.verticesX, cloud.data.verticesY, cloud.data.verticesZ,
            cloud.data.colorsR.Length, cloud.data.colorsR, cloud.data.colorsG, cloud.data.colorsB,
            cloud.data.normalsX.Length, cloud.data.normalsX, cloud.data.normalsY, cloud.data.normalsZ);
    }

    public void SaveDepthImage(string fileName, HolPointCloud cloud)
    {
        string depthFileName = fileName + ".png";
        string textureFileName = fileName + "_Texture.png";
        FileStream sw = new FileStream(depthFileName, FileMode.OpenOrCreate);
        sw.Write(cloud.data.DepthImage, 0, cloud.data.DepthImageSize);

        sw.Close();
        sw = new FileStream(textureFileName, FileMode.OpenOrCreate);
        sw.Write(cloud.data.TextureImage, 0, cloud.data.TextureImageSize);
        sw.Close();
    }
    public void LoadDepthPoints()
    {
        LoadDepthPoints(depthFileName, KFileName, camPoseFileName, depthPointsTextureFIleName);
    }

    public GameObject LoadDepthPoints(string depthFileName, string KFileName, string camPoseFileName, string depthPointsTextureFIleName = "")
    {
        int resultVertLength = 0;
        IntPtr ptrResultVertsX = IntPtr.Zero;
        IntPtr ptrResultVertsY = IntPtr.Zero;
        IntPtr ptrResultVertsZ = IntPtr.Zero;

        IntPtr ptrResultColorR = IntPtr.Zero;
        IntPtr ptrResultColorG = IntPtr.Zero;
        IntPtr ptrResultColorB = IntPtr.Zero;

        IntPtr ptrResultNormalX = IntPtr.Zero;
        IntPtr ptrResultNormalY = IntPtr.Zero;
        IntPtr ptrResultNormalZ = IntPtr.Zero;
        bool success;
        bool textureLoad = (depthPointsTextureFIleName != "" && System.IO.File.Exists(depthPointsTextureFIleName));
        float avgDist = 0;
        IntPtr ptrDepthImage = IntPtr.Zero;
        IntPtr ptrTextureImage = IntPtr.Zero;
        int depthImageSize = 0;
        int textureImageSize = 0;
        GameObject rootObj = null;

        if (textureLoad)
        {
            success = LoadPointCloudAndColorNormalFromDepth(ref ptrDepthImage, ref depthImageSize, ref ptrTextureImage, ref textureImageSize, depthFileName, KFileName, camPoseFileName, depthPointsTextureFIleName, ref ptrResultVertsX, ref ptrResultVertsY, ref ptrResultVertsZ, ref ptrResultColorR, ref ptrResultColorG, ref ptrResultColorB, ref ptrResultNormalX, ref ptrResultNormalY, ref ptrResultNormalZ, ref resultVertLength, normalCalcSize, ref avgDist, useMultiThread);
        }
        else
        {
            success = LoadPointCloudAndNormalFromDepth(ref ptrDepthImage, ref depthImageSize, depthFileName, KFileName, camPoseFileName, ref ptrResultVertsX, ref ptrResultVertsY, ref ptrResultVertsZ, ref ptrResultNormalX, ref ptrResultNormalY, ref ptrResultNormalZ, ref resultVertLength, normalCalcSize, ref avgDist, useMultiThread);
        }

        var objName = Path.GetFileName(depthFileName).Split('.');
        if (success)
        {
            rootObj = new GameObject(objName[0]);
            HolPointCloud data = rootObj.AddComponent<HolPointCloud>();
            GameObject dataObj = new GameObject(objName[0] + "data");
            dataObj.transform.parent = rootObj.transform;
            data.data = dataObj.AddComponent<HolData>();
            //dataObj.SetActive(false);
            data.data.vertexSize = resultVertLength;
            data.dataType = HolPointCloud.DATA_TYPE.DEPTH_IMAGE;
            data.malloc(resultVertLength);
            data.mallocColor(resultVertLength);
            data.mallocNormal(resultVertLength);
            data.data.K = HolStringParser.ReadMat3x3FromFile(KFileName);
            data.data.CamPose = HolStringParser.ReadMat4x4FromFile(camPoseFileName);

            data.data.DepthImage = new byte[depthImageSize];
            data.data.DepthImageSize = depthImageSize;
            Marshal.Copy(ptrDepthImage, data.data.DepthImage, 0, depthImageSize);

            Marshal.Copy(ptrResultVertsX, data.data.verticesX, 0, resultVertLength);
            Marshal.Copy(ptrResultVertsY, data.data.verticesY, 0, resultVertLength);
            Marshal.Copy(ptrResultVertsZ, data.data.verticesZ, 0, resultVertLength);
            if (textureLoad)
            {
                data.data.TextureImage = new byte[textureImageSize];
                data.data.TextureImageSize = textureImageSize;
                Marshal.Copy(ptrTextureImage, data.data.TextureImage, 0, textureImageSize);
                Marshal.Copy(ptrResultColorR, data.data.colorsR, 0, resultVertLength);
                Marshal.Copy(ptrResultColorG, data.data.colorsG, 0, resultVertLength);
                Marshal.Copy(ptrResultColorB, data.data.colorsB, 0, resultVertLength);


            }
            else
            {
                for (int i = 0; i < resultVertLength; ++i)
                {
                    data.data.TextureImageSize = 0;
                    data.data.TextureImage = null;
                    data.data.colorsR[i] = 255.0f;
                    data.data.colorsG[i] = 255.0f;
                    data.data.colorsB[i] = 255.0f;

                }

            }
            Marshal.Copy(ptrResultNormalX, data.data.normalsX, 0, resultVertLength);
            Marshal.Copy(ptrResultNormalY, data.data.normalsY, 0, resultVertLength);
            Marshal.Copy(ptrResultNormalZ, data.data.normalsZ, 0, resultVertLength);

            DataToObject(resultVertLength, objName[0], rootObj, data, avgDist);

        }
        return rootObj;
    }
    public void LoadDepthPointsUseZValue()
    {


        int resultVertLength = 0;
        IntPtr ptrResultVertsX = IntPtr.Zero;
        IntPtr ptrResultVertsY = IntPtr.Zero;
        IntPtr ptrResultVertsZ = IntPtr.Zero;

        IntPtr ptrResultColorR = IntPtr.Zero;
        IntPtr ptrResultColorG = IntPtr.Zero;
        IntPtr ptrResultColorB = IntPtr.Zero;

        IntPtr ptrResultNormalX = IntPtr.Zero;
        IntPtr ptrResultNormalY = IntPtr.Zero;
        IntPtr ptrResultNormalZ = IntPtr.Zero;
        bool success = false;
        bool textureLoad = (depthPointsTextureFIleName != "" && System.IO.File.Exists(depthPointsTextureFIleName));
        float avgDist = 0;

        IntPtr ptrDepthImage = IntPtr.Zero;
        IntPtr ptrTextureImage = IntPtr.Zero;
        int depthImageSize = 0;
        int textureImageSize = 0;

        if (textureLoad)
        {
            success = LoadPointCloudAndColorNormalFromDepthUseMinMax(ref ptrDepthImage, ref depthImageSize, ref ptrTextureImage, ref textureImageSize, depthFileName, zMin, zMax, xyScale, depthPointsTextureFIleName, ref ptrResultVertsX, ref ptrResultVertsY, ref ptrResultVertsZ, ref ptrResultColorR, ref ptrResultColorG, ref ptrResultColorB, ref ptrResultNormalX, ref ptrResultNormalY, ref ptrResultNormalZ, ref resultVertLength, normalCalcSize, ref avgDist);
        }
        else
        {
            success = LoadPointCloudAndColorNormalFromDepthUseMinMax(ref ptrDepthImage, ref depthImageSize, ref ptrTextureImage, ref textureImageSize, depthFileName, zMin, zMax, xyScale, null, ref ptrResultVertsX, ref ptrResultVertsY, ref ptrResultVertsZ, ref ptrResultColorR, ref ptrResultColorG, ref ptrResultColorB, ref ptrResultNormalX, ref ptrResultNormalY, ref ptrResultNormalZ, ref resultVertLength, normalCalcSize, ref avgDist);

        }
        var objName = Path.GetFileName(depthFileName).Split('.');
        if (success)
        {
            GameObject rootObj = new GameObject(objName[0]);
            HolPointCloud data = rootObj.AddComponent<HolPointCloud>();
            GameObject dataObj = new GameObject(objName[0] + "data");
            dataObj.transform.parent = rootObj.transform;
            data.data = dataObj.AddComponent<HolData>();
            //dataObj.SetActive(false);
            data.data.vertexSize = resultVertLength;

            data.dataType = HolPointCloud.DATA_TYPE.POINT_CLOUD;
            data.malloc(resultVertLength);
            data.mallocColor(resultVertLength);
            data.mallocNormal(resultVertLength);
            data.data.DepthImage = new byte[depthImageSize];
            data.data.DepthImageSize = depthImageSize;
            Marshal.Copy(ptrDepthImage, data.data.DepthImage, 0, depthImageSize);



            Marshal.Copy(ptrResultVertsX, data.data.verticesX, 0, resultVertLength);
            Marshal.Copy(ptrResultVertsY, data.data.verticesY, 0, resultVertLength);
            Marshal.Copy(ptrResultVertsZ, data.data.verticesZ, 0, resultVertLength);
            if (textureLoad)
            {
                data.data.TextureImage = new byte[textureImageSize];
                data.data.TextureImageSize = textureImageSize;


                Marshal.Copy(ptrTextureImage, data.data.TextureImage, 0, textureImageSize);

                Marshal.Copy(ptrResultColorR, data.data.colorsR, 0, resultVertLength);
                Marshal.Copy(ptrResultColorG, data.data.colorsG, 0, resultVertLength);
                Marshal.Copy(ptrResultColorB, data.data.colorsB, 0, resultVertLength);


            }
            else
            {
                data.data.TextureImage = null;
                data.data.TextureImageSize = 0;
                for (int i = 0; i < resultVertLength; ++i)
                {
                    data.data.colorsR[i] = 255;
                    data.data.colorsG[i] = 255;
                    data.data.colorsB[i] = 255;

                }

            }
            Marshal.Copy(ptrResultNormalX, data.data.normalsX, 0, resultVertLength);
            Marshal.Copy(ptrResultNormalY, data.data.normalsY, 0, resultVertLength);
            Marshal.Copy(ptrResultNormalZ, data.data.normalsZ, 0, resultVertLength);
            DataToObject(resultVertLength, objName[0], rootObj, data, avgDist);
        }

    }

    void DataToObject(int resultVertLength, string rootName, GameObject rootObj, HolPointCloud data, float avgDist)
    {
        int current = 0;

        int part = 0;

        while (current < resultVertLength)
        {
            part++;
            int num = Math.Min(65536, resultVertLength - current);

            GameObject targetObj = new GameObject(rootName + "_part_" + part);

            targetObj.transform.parent = rootObj.transform;
            var meshFilter = targetObj.GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = targetObj.AddComponent<MeshFilter>();
            var renderer = targetObj.GetComponent<MeshRenderer>();
            if (renderer == null)
                renderer = targetObj.AddComponent<MeshRenderer>();
            if (renderer.sharedMaterial == null)
                renderer.sharedMaterial = new Material(Shader.Find("Hol/PointRenderer"));
            else
                renderer.sharedMaterial.shader = Shader.Find("Hol/PointRenderer");
            mesh = new Mesh();
            meshFilter.mesh = mesh;

            Vector3[] points = new Vector3[num];
            int[] indecies = new int[num];
            Color[] colors = new Color[num];

            Vector3[] normals = new Vector3[num];
            for (int i = 0, j = current; (i < num) && (j < resultVertLength); i++, j += previewResolution)
            {
                colors[i] = new Color(data.data.colorsR[j] / 255.0f, data.data.colorsG[j] / 255.0f, data.data.colorsB[j] / 255.0f);
                points[i] = new Vector3(data.data.verticesX[j], data.data.verticesY[j], -data.data.verticesZ[j]);
                normals[i] = new Vector3(data.data.normalsX[j], data.data.normalsY[j], -data.data.normalsZ[j]);

                indecies[i] = i;
            }
            mesh.vertices = points;
            mesh.colors = colors;
            mesh.normals = normals;
            mesh.SetIndices(indecies, MeshTopology.Points, 0);
            mesh.RecalculateBounds();
            current += num * previewResolution;

        }


        rootObj.transform.position = Camera.main.transform.position;
        rootObj.transform.localScale = Camera.main.transform.localScale;
        rootObj.transform.rotation = Camera.main.transform.rotation;


        data.Stats();
        data.data.avgDiff = avgDist;

        EditorUtility.SetDirty(rootObj);
        EditorSceneManager.MarkSceneDirty(rootObj.gameObject.scene);
    }

    public string CopyObject()
    {

        return CopyFIle(sourceObjectPath);

        // Use Path class to manipulate file and directory paths.

    }
    public GameObject CopyObject(string fileName)
    {


        return LoadObject(CopyFIle(fileName));
        // Use Path class to manipulate file and directory paths.

    }
    public void SetMeshColor()
    {
        foreach (var mate in targetObject.GetComponentInChildren<MeshRenderer>().sharedMaterials)
        {
            mate.mainTexture = null;
            mate.color = meshColor;
        }



    }

    public Texture2D CopyTexture()
    {
        Texture2D newTexture = CopyTexture(sourceTexturePath);
        var renderers = targetObject.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            foreach (var mate in renderer.sharedMaterials)
            {
                mate.mainTexture = newTexture;
                mate.mainTexture.name = Path.GetFileNameWithoutExtension(sourceTexturePath);
                mate.name = mate.name.Replace(" (Instance)", "");
            }

        }
        return newTexture;
    }

    public Texture2D CopyTexture(string textureFileName)
    {
        Texture2D newTexture = new Texture2D(10, 10);
        if (File.Exists(textureFileName))
        {
            if (Path.GetExtension(textureFileName) == ".bmp")
            {
                BMPLoader loader = new BMPLoader();
                BMPImage img = loader.LoadBMP(textureFileName);
                newTexture = img.ToTexture2D();

                newTexture.Apply();

            }
            else
            {
                byte[] fileData;

                fileData = File.ReadAllBytes(textureFileName);

                newTexture.LoadImage(fileData);
                newTexture.name = Path.GetFileNameWithoutExtension(textureFileName);
                newTexture.Apply();
            }

        }
        return newTexture;

    }
    public string CopyFIle(string sourceFilePath)
    {
        string sourceFile = sourceFilePath;
        string resourcePath = Application.dataPath + "/Resources/";
        string destFile = resourcePath + sourceFilePath.Substring(3);

        string ext = Path.GetExtension(destFile);

        string renamePath = Path.GetDirectoryName(destFile) + "\\";
        string renameFile = renamePath + Path.GetFileNameWithoutExtension(destFile);
        destFile = renameFile + ext;
        string sourcePath = System.IO.Path.GetDirectoryName(sourceFilePath);
        string targetPath = System.IO.Path.GetDirectoryName(destFile);
        // To copy a folder's contents to a new location:
        // Create a new target folder, if necessary.
        if (!System.IO.Directory.Exists(targetPath))
        {
            System.IO.Directory.CreateDirectory(targetPath);
        }

        // To copy a file to another location and 
        // overwrite the destination file if it already exists.


        //mtl file
        string mtlFile = sourcePath + "\\" + Path.GetFileNameWithoutExtension(sourceFilePath) + ".mtl";
        if (System.IO.File.Exists(mtlFile))
        {
            string mtlTarget = renameFile + ".mtl";
            System.IO.File.Copy(mtlFile, mtlTarget, true);
            //read mtl
            byte[] fileData;
            fileData = File.ReadAllBytes(mtlTarget);

            //texture copy
            string mtlData = Encoding.UTF8.GetString(fileData);
            int textureIndex = mtlData.IndexOf("map_Kd");
            if (textureIndex > 0)
            {
                string subMtl = mtlData.Substring(textureIndex + 7);
                int lineEnd = subMtl.IndexOf("\r\n");
                if (lineEnd <= 0)
                {
                    lineEnd = subMtl.IndexOf("\n");
                }

                string textureName = subMtl.Substring(0, lineEnd);

                //Path.GetFileName(texture)
                string fullPath;
                if (textureName.IndexOf(":") == 1)
                {
                    fullPath = textureName;
                    textureName = Path.GetFileName(textureName);
                }
                else
                {
                    fullPath = sourcePath + "\\" + textureName;
                }

                System.IO.File.Copy(fullPath, targetPath + "\\" + textureName, true);

                string str = "Assets/Resources/" + System.IO.Path.GetDirectoryName(sourceFilePath.Substring(3)) + "/" + textureName;
                AssetDatabase.ImportAsset(str);
                TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(str);
                importer.isReadable = true;
                AssetDatabase.ImportAsset(str, ImportAssetOptions.ForceUpdate);

            }

            //
        }
        System.IO.File.Copy(sourceFile, destFile, true);
        AssetDatabase.ImportAsset(destFile.Substring(Application.dataPath.Length - 6));
        // To copy all the files in one directory to another directory.
        // Get the files in the source folder. (To recursively iterate through
        // all subfolders under the current directory, see
        // "How to: Iterate Through a Directory Tree.")
        // Note: Check for target path was performed previously
        //       in this code example.
        if (System.IO.Directory.Exists(sourceFile))
        {
            string[] files = System.IO.Directory.GetFiles(sourceFile);

            // Copy the files and overwrite destination files if they already exist.
            foreach (string s in files)
            {
                // Use static Path methods to extract only the file name from the path.
                fileName = System.IO.Path.GetFileName(s);
                //destFile = System.IO.Path.Combine(targetPath, fileName);
                System.IO.File.Copy(s, destFile, true);
                //Debug.Log(destFile.Substring(Application.dataPath.Length - 6));
                AssetDatabase.ImportAsset(destFile.Substring(Application.dataPath.Length - 6));
            }
        }
        else
        {
            Console.WriteLine("Source path does not exist!");
        }
        return destFile;
    }

    public void LoadObject()
    {
        LoadObject(loadObjectPath);
    }

    public GameObject LoadObject(string fileName)
    {
        string resourcePath = Application.dataPath + "/Resources/";
        string objPath = fileName.Substring(resourcePath.Length);
        objPath = objPath.Substring(0, objPath.Length - Path.GetExtension(objPath).Length);
        GameObject loadedObejct = Instantiate(Resources.Load(objPath, typeof(GameObject)) as GameObject);
        if (loadedObejct != null)
        {
            loadedObejct.name = Path.GetFileName(objPath);
            foreach (var aa in loadedObejct.GetComponentsInChildren<MeshFilter>())
            {
                aa.name = Path.GetFileName(objPath);
            }
            targetData = loadedObejct.AddComponent<HolMesh>();
            targetData.OriginFileName = fileName;
            meshZInsideOut(targetData as HolMesh, Path.ChangeExtension(resourcePath + objPath, "mtl"));
        }
        return loadedObejct;
    }
    void meshZInsideOut(HolMesh hpMesh, string mtlFIleName)
    {

        mtlFIleName = mtlFIleName.Replace("\\\\", "/");


        float ambient = 0.6f;
        float diffuse = 1;
        float spacular = 1;
        float sharpness = 10;
        if (File.Exists(mtlFIleName))
        {
            StringReader fs = new StringReader(mtlFIleName);
            string line;
            char[] token = { ' ', '\n' };
            while ((line = fs.ReadLine()) != null)
            {
                string[] datas = line.Split(token, StringSplitOptions.RemoveEmptyEntries);
                switch (datas[0])
                {
                    case "Kd":
                        diffuse = float.Parse(datas[1]);
                        break;
                    case "Ka":
                        ambient = float.Parse(datas[1]);
                        break;
                    case "Ks":
                        spacular = float.Parse(datas[1]);
                        break;
                    case "sharpness":
                        sharpness = float.Parse(datas[1]);
                        break;
                }
            }
        }
        MeshFilter[] meshFilters = hpMesh.GetComponentsInChildren<MeshFilter>();
        foreach (var meshFilter in meshFilters)
        {
            var meshRenderer = meshFilter.GetComponent<MeshRenderer>();
            var mesh = meshFilter.sharedMesh;
            foreach (var mater in meshRenderer.sharedMaterials)
            {
                mater.shader = Shader.Find("Hol/PhongShader");

                mater.SetFloat("_AmbientFactor", ambient);
                mater.SetFloat("_DiffuseFactor", diffuse);
                mater.SetFloat("_SpecularFactor", spacular);
                mater.SetFloat("_Sharpness", sharpness);
            }
            Vector3[] newVertices = new Vector3[mesh.vertices.Length];
            Vector3[] newNormals = new Vector3[mesh.vertices.Length];
            int submeshCount = mesh.subMeshCount;
            int[][] newIndices = new int[submeshCount][];
            for (int i = 0; i < submeshCount; ++i)
            {
                newIndices[i] = new int[mesh.GetIndexCount(i)];
                uint submeshindicies = mesh.GetIndexCount(i);
                int[] indices = mesh.GetIndices(i);
                for (int j = 0; j < submeshindicies / 3; ++j)
                {
                    newIndices[i][j * 3] = indices[j * 3];
                    newIndices[i][j * 3 + 1] = indices[j * 3 + 1];
                    newIndices[i][j * 3 + 2] = indices[j * 3 + 2];
                }

            }
            int len = mesh.vertices.Length;
            var vertices = mesh.vertices;
            var normals = mesh.normals;


            for (int i = 0; i < len; ++i)
            {
                newVertices[i].x = -vertices[i].x;
                newVertices[i].y = vertices[i].y;
                newVertices[i].z = -vertices[i].z;

                newNormals[i].x = -normals[i].x;
                newNormals[i].y = normals[i].y;
                newNormals[i].z = -normals[i].z;

            }


            mesh.vertices = newVertices;
            mesh.normals = newNormals;

            for (int i = 0; i < submeshCount; ++i)
            {
                mesh.SetIndices(newIndices[i], mesh.GetTopology(i), i);
            }

        }
    }
    public void CalcNewUV()
    {
        Texture2D texture2D = CopyTexture();
        MeshFilter meshFilter = targetObject.GetComponentInChildren<MeshFilter>();
        Mesh targetMesh = meshFilter.sharedMesh;
        byte[] buffer = MeshToBuffer(targetMesh);
        IntPtr resultPtr = IntPtr.Zero;
        int buffersize = CalcTextureUV(buffer, KFileName, camPoseFileName, texture2D.width, texture2D.height, ref resultPtr);
        byte[] resultBuffer = new byte[buffersize];
        Marshal.Copy(resultPtr, resultBuffer, 0, buffersize);

        MeshFromBuffer(targetMesh, resultBuffer, buffersize);

    }


    public byte[] MeshToBuffer(Mesh targetMesh)
    {

        //vertices
        Vector3[] vertices = targetMesh.vertices;
        Vector3[] normals = targetMesh.normals;
        //indices
        int[] triangles = targetMesh.triangles;

        UInt32 pointNum = (UInt32)vertices.Length;
        UInt32 polygonNum = (UInt32)triangles.Length;
        polygonNum = (UInt32)(polygonNum / 3);
        var stream = new MemoryStream();
        var buf = new BinaryWriter(stream);
        buf.Write(pointNum);
        buf.Write(polygonNum);
        for (int i = 0; i < pointNum; ++i)
        {

            buf.Write(-vertices[i].x);
            buf.Write(vertices[i].y);
            buf.Write(vertices[i].z);
        }
        for (int i = 0; i < pointNum; ++i)
        {

            buf.Write(normals[i].x);
            buf.Write(normals[i].y);
            buf.Write(normals[i].z);
        }
        for (int i = 0; i < polygonNum * 3; ++i)
        {
            buf.Write(triangles[i]);
        }

        return stream.ToArray();



    }
    public void MeshFromBuffer(Mesh newMesh, byte[] buffer, int resultSize)
    {
        UInt32 pointSize = 0;
        UInt32 triangleSize = 0;
        Stream meshStream = new MemoryStream(buffer);

        var buf = new BinaryReader(meshStream);
        pointSize = buf.ReadUInt32();
        triangleSize = buf.ReadUInt32();
        Vector3[] points = new Vector3[pointSize];
        Vector3[] normals = new Vector3[pointSize];
        Vector2[] uvs = new Vector2[pointSize];
        int[] triangles = new int[triangleSize * 3];
        for (int i = 0; i < pointSize; ++i)
        {
            points[i].x = buf.ReadSingle();
            points[i].y = buf.ReadSingle();
            points[i].z = buf.ReadSingle();

        }
        for (int i = 0; i < pointSize; ++i)
        {
            normals[i].x = buf.ReadSingle();
            normals[i].y = buf.ReadSingle();
            normals[i].z = buf.ReadSingle();

        }
        for (int i = 0; i < pointSize; ++i)
        {
            uvs[i].x = buf.ReadSingle();
            uvs[i].y = buf.ReadSingle();

        }
        for (int i = 0; i < triangleSize * 3; ++i)
        {
            int val = buf.ReadInt32();
            triangles[i] = val;
        }


        newMesh.uv = uvs;
        int max = triangles.Max();

        int idx = Array.IndexOf(triangles, max);
    }
    public string saveObjName { get; set; }

    public void SaveMesh()
    {
        OBJExporter exporter = CreateInstance<OBJExporter>();
        exporter.Export(targetObject.GetComponentInChildren<MeshFilter>().gameObject, saveObjName);
    }


    public string saveDepthFolder { get; set; }
    public void MeshToPointCloud(bool toDepth, bool toPointCloud, int width, int height, bool applyTransform = false)
    {
        if (targetObject == null)
            return;
        RaycastHit hit;
        GameObject camObj = null;
        Camera camScript;


        Vector3 pos = new Vector3();

        ArrayList pointsList = new ArrayList();
        ArrayList colorsList = new ArrayList();
        ArrayList normalsList = new ArrayList();

        camScript = Camera.main;
        camObj = Camera.main.gameObject;


        camScript.orthographic = false;
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        GameViewUtils.ModCustomSize(GameViewUtils.GameViewSizeType.FixedResolution, GameViewSizeGroupType.Standalone, width, height, "Hologram");
        //repaint 된 다음 호출
        EditorApplication.delayCall += () =>
        {

            bool isOK = EditorUtility.DisplayDialog("Check GameView", "Is the GameView screen the same as the desired output?", "Convert", "Cancel");
            if (isOK == false)
                return;




            StreamWriter sw = new StreamWriter(saveDepthFolder + "/K.txt");
            Matrix4x4 mtx = camScript.projectionMatrix;

            mtx.m00 *= (camScript.pixelWidth / 2);
            mtx.m11 *= (camScript.pixelHeight / 2);

            mtx.m02 = camScript.pixelWidth / 2;
            mtx.m12 = camScript.pixelHeight / 2;
            mtx.m22 = 1;


            sw.Write(mtx.m00);
            sw.Write("\t");
            sw.Write(mtx.m01);
            sw.Write("\t");
            sw.Write(mtx.m02);
            sw.WriteLine();
            sw.Write(mtx.m10);
            sw.Write("\t");
            sw.Write(mtx.m11);
            sw.Write("\t");
            sw.Write(mtx.m12);
            sw.WriteLine();
            sw.Write(mtx.m20);
            sw.Write("\t");
            sw.Write(mtx.m21);
            sw.Write("\t");
            sw.Write(mtx.m22);
            sw.WriteLine();


            sw.Close();

            sw = new StreamWriter(saveDepthFolder + "/Cam_Pose.txt");
            mtx = Matrix4x4.identity;
            mtx = camScript.transform.localToWorldMatrix;
            sw.Write(mtx.ToString());
            sw.Close();
            //List<MeshCollider> meshCol = null;
            foreach(var meshFilter in targetObject.GetComponentsInChildren<MeshFilter>())
            {
                //meshCol.Add(meshFilter.gameObject.AddComponent<MeshCollider>());
                meshFilter.gameObject.AddComponent<MeshCollider>();

            }
            //if (targetObject.GetComponentInChildren<Collider>() == null)
            //{
            //    meshCol = targetObject.GetComponentInChildren<MeshFilter>().gameObject.AddComponent<MeshCollider>();
            //}
            GameObject backupTransform = null;
            if (applyTransform == false)
            {
                backupTransform = new GameObject();
                backupTransform.transform.position = targetObject.transform.position;
                backupTransform.transform.rotation = targetObject.transform.rotation;
                backupTransform.transform.localScale = targetObject.transform.localScale;

                targetObject.transform.position = Vector3.zero;
                targetObject.transform.rotation = Quaternion.identity;
                targetObject.transform.localScale = Vector3.one;
            }
            Texture2D colorTxt = new Texture2D(camScript.pixelWidth, camScript.pixelHeight, TextureFormat.RGBAFloat, false);
            int per = camScript.pixelHeight * camScript.pixelWidth;
            int count = 0;

            float[] depthImage = new float[camScript.pixelWidth * camScript.pixelHeight];

            //모든 픽셀에 대해 공통값을 가져야 함
            Transform objectHit = null;
            Mesh mesh = null;
            MeshCollider collider = null;
            MeshRenderer renderer = null;
            Texture2D txt2 = null;
            int subMeshCount = 0;
            for (int j = 0; j < camScript.pixelHeight; ++j)
            {
                if (j % (camScript.pixelHeight / 50) == 0)
                    EditorUtility.DisplayProgressBar("Converting", "Raycasting...", count / (per * 2.0f));
                for (int i = 0; i < camScript.pixelWidth; ++i)
                {
                    count++;
                    pos.x = i;
                    pos.y = j;
                    Ray ray = camScript.ScreenPointToRay(pos);

                    if (Physics.Raycast(ray, out hit))
                    {
                        if (objectHit == null || objectHit != hit.transform)
                        {
                            objectHit = hit.transform;
                            renderer = objectHit.gameObject.GetComponent<MeshRenderer>();
                            txt2 = renderer.sharedMaterial.mainTexture as Texture2D;
                            collider = hit.collider as MeshCollider;

                            // Remember to handle case where collider is null because you hit a non-mesh primitive...
                            if (collider != null)
                                mesh = collider.sharedMesh;
                            else
                                mesh = objectHit.GetComponent<MeshFilter>().sharedMesh;
                        }
                        if (hit.textureCoord != null)
                        {

                            pointsList.Add(hit.point);

                            Vector4 pts = camScript.worldToCameraMatrix * new Vector4(hit.point.x, hit.point.y, hit.point.z, 1);
                            depthImage[i + j * camScript.pixelWidth] = (-pts.z);

                            if (txt2 != null && renderer.sharedMaterials.Length == 1)
                            {
                                colorsList.Add(txt2.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y));
                                colorTxt.SetPixel(i, j, txt2.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y));

                            }
                            else
                            {
                                // There are 3 indices stored per triangle
                                int limit = hit.triangleIndex * 3;
                                int submesh=0;
                                if (subMeshCount == 0)
                                    subMeshCount = mesh.subMeshCount;
                                for (submesh = 0; submesh < subMeshCount; ++submesh)
                                {
                                    int numIndices = mesh.GetTriangles(submesh).Length;
                                    if (numIndices > limit)
                                        break;

                                    limit -= numIndices;
                                }

                                Material material = renderer.sharedMaterials[submesh];
                                Texture2D txt = material.mainTexture as Texture2D;
                                if (txt != null)
                                {

                                    Color col = txt.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y) * material.color;
                                    colorsList.Add(col);
                                    colorTxt.SetPixel(i, j, col);
                                }
                                else
                                {
                                    Color col = material.color;
                                    if (col != null)
                                    {
                                        colorsList.Add(col);
                                        colorTxt.SetPixel(i, j, col);
                                    }
                                    else
                                    {
                                        colorsList.Add(Color.white);
                                        colorTxt.SetPixel(i, j, Color.white);
                                    }
                                }
                            }
                            normalsList.Add(hit.normal);
                        }
                    }
                    else
                    {
                        depthImage[i + j * camScript.pixelWidth] = float.PositiveInfinity;
                        colorTxt.SetPixel(i, j, new Color(0, 0, 0, 0));
                    }
                }
            }
            if (pointsList.Count == 0)
            {
                EditorUtility.ClearProgressBar();
                Debug.Log("Can not find Mesh");
                return;
            }
            colorTxt.Apply();
            if (toDepth == true)
            {
                string saveDepth8bitFileName = saveDepthFolder + "/depth_8bit.png";
                string saveDepthFileName = saveDepthFolder + "/depth_16bit.png";
                string saveTextureFileName = saveDepthFolder + "/Texture.png";


                File.WriteAllBytes(saveTextureFileName, colorTxt.EncodeToPNG());
                SaveBothDepth(depthImage, camScript.pixelWidth, camScript.pixelHeight, saveDepth8bitFileName, saveDepthFileName);
                ///
            }
            if (toPointCloud == true)
            {
                GameObject rootObj = new GameObject("MeshCaptureCloud");
                if (applyTransform == false)
                {
                    rootObj.transform.position = backupTransform.transform.position;
                    rootObj.transform.rotation = backupTransform.transform.rotation;
                    rootObj.transform.localScale = backupTransform.transform.localScale;
                }
                HolPointCloud data = rootObj.AddComponent<HolPointCloud>();
                GameObject dataObj = new GameObject("MeshCaptureCloud" + "data");
                dataObj.transform.parent = rootObj.transform;
                data.data = dataObj.AddComponent<HolData>();
                //Mesh mesh;
                int resultVertLength = pointsList.Count;
                data.data.vertexSize = resultVertLength;

                data.malloc(resultVertLength);
                data.mallocColor(resultVertLength);
                data.mallocNormal(resultVertLength);


                int current = 0;

                int part = 0;

                while (current < resultVertLength)
                {
                    EditorUtility.DisplayProgressBar("Converting", "Convert To GameObject...", 50 + (current / resultVertLength * 2.0f));
                    part++;
                    int num = Math.Min(65536, resultVertLength - current);

                    GameObject targetObj = new GameObject("_part_" + part);

                    targetObj.transform.parent = rootObj.transform;
                    var meshFilter = targetObj.GetComponent<MeshFilter>();
                    if (meshFilter == null)
                        meshFilter = targetObj.AddComponent<MeshFilter>();
                    renderer = targetObj.GetComponent<MeshRenderer>();
                    if (renderer == null)
                        renderer = targetObj.AddComponent<MeshRenderer>();
                    if (renderer.sharedMaterial == null)
                        renderer.sharedMaterial = new Material(Shader.Find("Hol/PointRenderer"));
                    else
                        renderer.sharedMaterial.shader = Shader.Find("Hol/PointRenderer");

                    renderer.sharedMaterial.SetFloat("_pointSize", 5.0f);
                    mesh = new Mesh();
                    meshFilter.mesh = mesh;

                    Vector3[] points = new Vector3[num];
                    Vector3[] normals = new Vector3[num];
                    int[] indecies = new int[num];
                    Color[] colors = new Color[num];
                    for (int i = 0; i < num; i++)
                    {
                        colors[i] = (Color)colorsList[i + current];
                        points[i] = (Vector3)pointsList[i + current];
                        normals[i] = (Vector3)normalsList[i + current];
                        data.data.colorsR[i + current] = colors[i].r * 255;
                        data.data.colorsG[i + current] = colors[i].g * 255;
                        data.data.colorsB[i + current] = colors[i].b * 255;
                        data.data.verticesX[i + current] = points[i].x;
                        data.data.verticesY[i + current] = points[i].y;
                        data.data.verticesZ[i + current] = -points[i].z;

                        data.data.normalsX[i + current] = normals[i].x;
                        data.data.normalsY[i + current] = normals[i].y;
                        data.data.normalsZ[i + current] = -normals[i].z;


                        indecies[i] = i;
                    }
                    mesh.vertices = points;
                    mesh.colors = colors;
                    mesh.normals = normals;
                    mesh.SetIndices(indecies, MeshTopology.Points, 0);

                    mesh.RecalculateBounds();
                    current += num;
                }
                data.Stats();
                SaveCloudFile(saveDepthFolder + "/pointCloud.ply", true, resultVertLength, data.data.verticesX, data.data.verticesY, data.data.verticesZ,
                    data.data.colorsR.Length, data.data.colorsR, data.data.colorsG, data.data.colorsB,
                    data.data.normalsX.Length, data.data.normalsX, data.data.normalsY, data.data.normalsZ);

            }
            EditorUtility.ClearProgressBar();
            if (applyTransform == false)
            {
                targetObject.transform.position = backupTransform.transform.position;
                targetObject.transform.rotation = backupTransform.transform.rotation;
                targetObject.transform.localScale = backupTransform.transform.localScale;

            }
            foreach (var collider1 in targetObject.GetComponentsInChildren<Collider>())
            {
                DestroyImmediate(collider1);
            }
                //if (meshCol != null)
                //DestroyImmediate(meshCol);
                if (applyTransform == false)
            {
                DestroyImmediate(backupTransform);
                DestroyImmediate(camObj);
            }
            camScript.orthographic = true;
            //depthImageNA.Dispose();
            return;
        };

    }
}
