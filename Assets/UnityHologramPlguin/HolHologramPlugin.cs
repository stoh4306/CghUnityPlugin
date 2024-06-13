using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor.SceneManagement;

delegate void DelegateNoReturn(string a);

[ExecuteInEditMode]
public class HolHologramPlugin : Editor
{
    public enum MESH_SHADING_OPTION
    {
        NoShading=0,
        FlatShading,
        SmoothShading
    }
    ///singleton 
    ///
    static HolHologramPlugin instance = null;
    public static HolHologramPlugin GetInstance()
    {
        if (instance == null)
        {
            instance = CreateInstance<HolHologramPlugin>();
            instance.Init();
        }
        return instance;
    }

    void Init()
    {
        Camera.main.orthographic = true;
        monoWaveLength = 633;
        RGBWaveLength = new Vector3Int(633, 532, 414);
        pixelPitch = new Vector2(25, 25);
        numOfPixels = new Vector2Int(1024, 1024);
        cghOutputFIlePrefix = "CGH";
        
        carrierWave = new Vector3(0, 0, 1);
        applyTexture = true;
        zDepthScaleFactor = 100;
        numOfLayers = 8;
        layerZRange = new Vector2(0, 1);

        useGPU = true;
        randomPhase = false;

        saveDataOnScene = Camera.main.GetComponent<HologramData>();
        if (saveDataOnScene == null)
        {
            saveDataOnScene = Camera.main.gameObject.AddComponent<HologramData>();
            SaveToScene();
        }
        else
        {
            pixelPitch = saveDataOnScene.pixelPitch;
            numOfPixels = saveDataOnScene.numOfPixels;
            cameraNode = saveDataOnScene.cameraNode;
            fullColor = saveDataOnScene.fullColor;
            monoWaveLength = saveDataOnScene.monoWaveLength;
            RGBWaveLength = saveDataOnScene.RGBWaveLength;
            generationMethod = saveDataOnScene.generationMethod;
            zDepthScaleFactor = saveDataOnScene.zDepthScaleFactor;


            cghOutputFIlePrefix = saveDataOnScene.cghOutputFIlePrefix;
            randomPhase = saveDataOnScene.randomPhase;

            numOfLayers = saveDataOnScene.numOfLayers;
            layerZRange = saveDataOnScene.layerZRange;

            carrierWave = saveDataOnScene.carrierWave;
            meshShadingOption = saveDataOnScene.meshShadingOption;
            applyTexture = saveDataOnScene.applyTexture;

            useGPU = saveDataOnScene.useGPU;

            GenMethod = saveDataOnScene.GenMethod;


            referenceMesh = saveDataOnScene.referenceMesh;
            scaledDistanseOfObject = saveDataOnScene.scaledDistanseOfObject;
            saveXMLFileName = saveDataOnScene.saveXMLFileName;
        }

    }
    ///property



    //Hologram information
    ///Sample Parameters
    public Vector2 pixelPitch;
    public Vector2Int numOfPixels;
    public Camera cameraNode;

    ///Generation
    public bool fullColor;
    public float monoWaveLength;
    public Vector3 RGBWaveLength;
    public int generationMethod;
    public float zDepthScaleFactor;


    public string cghOutputFIlePrefix;
    public bool randomPhase;

    public int numOfLayers;
    public Vector2 layerZRange;

    public Vector3 carrierWave;
    public MESH_SHADING_OPTION meshShadingOption;
    public bool applyTexture;

    public bool useGPU;

    public HolMesh.GENTYPE GenMethod;

    ///Object Info
    public HolDataObject referenceMesh;
    public float scaledDistanseOfObject;

    public HologramData saveDataOnScene;
    public string saveXMLFileName;
    public void CameraSetting()
    {
        var trans = Camera.main.transform;
        trans.position = Vector3.zero;
        trans.rotation = Quaternion.identity;
        
    }

    /// <summary>
    /// method
    /// </summary>
    public void SavePropertyToXML(XmlWriter xmlWriter)
    {
        SaveToScene();
        xmlWriter.WriteStartElement("Hologram");
        xmlWriter.WriteStartElement("FullColor");
        xmlWriter.WriteValue(fullColor.ToString());
        xmlWriter.WriteEndElement();
        xmlWriter.WriteStartElement("RGBWaveLength");
        xmlWriter.WriteValue(HolStringParser.VectorToString(RGBWaveLength));
        xmlWriter.WriteEndElement();
        xmlWriter.WriteStartElement("MonoWaveLength");
        xmlWriter.WriteValue(monoWaveLength.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("PixelPitch");
        xmlWriter.WriteValue(HolStringParser.VectorToString(pixelPitch));
        xmlWriter.WriteEndElement();
        xmlWriter.WriteStartElement("NumOfPixels");
        xmlWriter.WriteValue(HolStringParser.VectorToString(numOfPixels));
        xmlWriter.WriteEndElement();
        xmlWriter.WriteStartElement("CGHOutputFilePrefix");
        xmlWriter.WriteValue(cghOutputFIlePrefix);
        xmlWriter.WriteEndElement();

        xmlWriter.WriteComment("0:point cloud, 1: layer, 2: mesh");
        xmlWriter.WriteStartElement("GenMethod");
        if (GenMethod == HolMesh.GENTYPE.POINT_CLOUD)
        {
            xmlWriter.WriteValue(0);

        }
        else
        {
            if (GenMethod == HolMesh.GENTYPE.LAYER)
            {
                xmlWriter.WriteValue(1);
            }
            else
            {
                xmlWriter.WriteValue(2);
            }
        }

        //xmlWriter.WriteValue(GenMethod.ToString());
        
        xmlWriter.WriteEndElement();
        
        xmlWriter.WriteStartElement("ZDepthScaleFactor");
        xmlWriter.WriteValue(zDepthScaleFactor.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("UseGPU");
        xmlWriter.WriteValue(useGPU.ToString());
        xmlWriter.WriteEndElement();
        xmlWriter.WriteStartElement("RandomPhase");
        xmlWriter.WriteValue(randomPhase.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("NumOfLayers");
        xmlWriter.WriteValue(numOfLayers.ToString());
        xmlWriter.WriteEndElement();
        xmlWriter.WriteStartElement("LayerZRange");
        Vector2 vec2 = layerZRange;
        vec2.x = -layerZRange.y;
        vec2.y = -layerZRange.x;
        xmlWriter.WriteValue(HolStringParser.VectorToString(vec2));
        xmlWriter.WriteEndElement();



        xmlWriter.WriteStartElement("CarrierWave");
        xmlWriter.WriteValue(HolStringParser.VectorToString(carrierWave));
        xmlWriter.WriteEndElement();
        xmlWriter.WriteStartElement("MeshShadingOption");
        xmlWriter.WriteValue((int)meshShadingOption);
        xmlWriter.WriteEndElement();
        xmlWriter.WriteStartElement("ApplyTexture");
        xmlWriter.WriteValue(applyTexture.ToString());
        xmlWriter.WriteEndElement();


        xmlWriter.WriteEndElement();

    }
    public void SaveHologramInputXML(string saveXMLPath)
    {
        XmlWriterSettings setting = new XmlWriterSettings();
        
        setting.Indent = true;
        setting.IndentChars = " ";
        setting.NewLineOnAttributes = true;
        setting.NewLineHandling = NewLineHandling.Replace;
        saveXMLPath = Path.GetFullPath(saveXMLPath);
        CreateDir(saveXMLPath);


        string ssaveXMLFolder = Path.GetDirectoryName(saveXMLPath);
        using (HolXmlWriter xmlWriter = new HolXmlWriter(saveXMLPath, setting))
        {
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Scene");
            xmlWriter.WriteStartElement("Object");
            xmlWriter.WriteComment("0:point cloud, 1:depth image, 2:triangle mesh");
            xmlWriter.WriteStartElement("ObjectType");

            HolPointCloud script = referenceMesh.GetComponent<HolPointCloud>();
            if (script == null)
            {
                xmlWriter.WriteValue(2);

            }
            else
            {
                if (script.dataType == HolPointCloud.DATA_TYPE.POINT_CLOUD)
                {
                    xmlWriter.WriteValue(0);
                }
                else
                {
                    xmlWriter.WriteValue(1);
                }
            }

            xmlWriter.WriteEndElement();

            
            xmlWriter.WriteStartElement("FileName");
            string subFileName = "";
            if (script == null)
            {
                MeshFilter meshFilter = referenceMesh.GetComponentInChildren<MeshFilter>();
                HolMesh meshScript = referenceMesh.GetComponent<HolMesh>();

                if (!Directory.Exists(ssaveXMLFolder + "\\Mesh"))
                {
                    Directory.CreateDirectory(ssaveXMLFolder + "\\Mesh");
                }
                subFileName = ".\\Mesh\\" + referenceMesh.name + ".obj";

                string originFileName = meshScript.OriginFileName;
                OBJExporter exporter = CreateInstance<OBJExporter>();

                string textureFileName = exporter.Export(meshFilter.gameObject, ssaveXMLFolder+subFileName);
                if (originFileName != null && originFileName != "")
                {
                    Debug.Log(originFileName + " -> " + ssaveXMLFolder + subFileName);
                    System.IO.File.Copy(originFileName, ssaveXMLFolder + subFileName, true);
                }

                xmlWriter.WriteValue(subFileName);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Texture");
                if (textureFileName == null || textureFileName.Length<4)
                    xmlWriter.WriteValue("null");
                else
                    xmlWriter.WriteValue(".\\Mesh\\" + textureFileName);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("PhongFactor");
                xmlWriter.WriteStartElement("Ambient");
                xmlWriter.WriteValue(meshScript.AmbientFactor);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Diffuse");
                xmlWriter.WriteValue(meshScript.DifusseFactor);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Specular");
                xmlWriter.WriteValue(meshScript.SpecularFactor);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Shininess");
                xmlWriter.WriteValue(meshScript.ShininessFactor);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }
            else
            {
                if (script.dataType == HolPointCloud.DATA_TYPE.POINT_CLOUD)
                {
                    if (!Directory.Exists(ssaveXMLFolder + "\\PointCloud"))
                    {
                        Directory.CreateDirectory(ssaveXMLFolder + "\\PointCloud");
                    }


                    subFileName = ".\\PointCloud\\" + script.name + ".ply";
                    HolObjectLoader.GetInstance().SaveCloudFile(ssaveXMLFolder + subFileName, true, script);
                    xmlWriter.WriteValue(subFileName);
                    xmlWriter.WriteEndElement();

                    

                }
                else
                {
                    if (!Directory.Exists(ssaveXMLFolder + "\\DepthImage"))
                    {
                        Directory.CreateDirectory(ssaveXMLFolder + "\\DepthImage");
                    }
                    subFileName = ".\\DepthImage\\" + script.name;
                    HolObjectLoader.GetInstance().SaveDepthImage(ssaveXMLFolder + subFileName, script);
                    string depthFileName = subFileName + ".png";
                    string textureFileName = subFileName + "_Texture.png";
                    xmlWriter.WriteValue(depthFileName);
                    xmlWriter.WriteEndElement();

                    HolStringParser.WriteMatrix3x3ToFile(ssaveXMLFolder + "\\DepthImage\\K.txt", script.data.K);
                    HolStringParser.WriteMatrix4x4ToFile(ssaveXMLFolder + "\\DepthImage\\Cam_Pose.txt", script.data.CamPose);
                    xmlWriter.WriteStartElement("CameraMatrix");
                    xmlWriter.WriteValue(".\\DepthImage\\K.txt");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("CameraPose");
                    xmlWriter.WriteValue(".\\DepthImage\\Cam_Pose.txt");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Texture");
                    xmlWriter.WriteValue(textureFileName);
                    xmlWriter.WriteEndElement();

                }
                xmlWriter.WriteStartElement("PhongFactor");
                xmlWriter.WriteStartElement("Ambient");
                xmlWriter.WriteValue(script.AmbientFactor);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Diffuse");
                xmlWriter.WriteValue(script.DifusseFactor);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Specular");
                xmlWriter.WriteValue(script.SpecularFactor);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Shininess");
                xmlWriter.WriteValue(script.ShininessFactor);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }
            Matrix4x4 mat = referenceMesh.transform.localToWorldMatrix;
            var mat1 = HolStringParser.UnityMatToMayaMat(mat);
            HolStringParser.WriteMatrixToString(xmlWriter, mat1);

            //Object end
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("HologramCamera");
            
            mat = Camera.main.transform.localToWorldMatrix;
            mat1 = HolStringParser.UnityMatToMayaMat(mat);
            HolStringParser.WriteMatrixToString(xmlWriter, mat1);

            xmlWriter.WriteStartElement("CameraOrthographic");
            xmlWriter.WriteValue(Camera.main.orthographic.ToString());
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("CameraOrthoSize");
            xmlWriter.WriteValue(Camera.main.orthographicSize);
            xmlWriter.WriteEndElement();
            //Camera end
            xmlWriter.WriteEndElement();

            var lightObj = FindObjectOfType(typeof(Light)) as Light;
            xmlWriter.WriteStartElement("Light");
            xmlWriter.WriteStartElement("LightDirection");
            var liMat = HolStringParser.UnityMatToMayaMat(lightObj.transform.localToWorldMatrix);
            Vector3 bb = liMat * Vector3.back;
            xmlWriter.WriteValue(HolStringParser.VectorToString(bb));
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("LightMagnitude");
            xmlWriter.WriteValue(lightObj.intensity);
            xmlWriter.WriteEndElement();




            //Light end
            xmlWriter.WriteEndElement();

            SavePropertyToXML(xmlWriter);

            //Hologram end
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();

        }
        Debug.Log("Save XML :" + saveXMLPath);
    }
    void CreateDir(string path)
    {
        path = Path.GetFullPath(path);
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            CreateDir(Path.GetDirectoryName(path));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
    }

    
    public void SaveToScene()
    {
        EditorUtility.SetDirty(saveDataOnScene);
        EditorSceneManager.MarkSceneDirty(saveDataOnScene.gameObject.scene);
        saveDataOnScene.pixelPitch = pixelPitch;
        saveDataOnScene.numOfPixels =numOfPixels;
        saveDataOnScene.cameraNode =cameraNode;
        saveDataOnScene.fullColor =fullColor;
        saveDataOnScene.monoWaveLength =monoWaveLength;
        saveDataOnScene.RGBWaveLength =RGBWaveLength;
        saveDataOnScene.generationMethod =generationMethod;
        saveDataOnScene.zDepthScaleFactor =zDepthScaleFactor;


        saveDataOnScene.cghOutputFIlePrefix =cghOutputFIlePrefix;
        saveDataOnScene.randomPhase =randomPhase;

        saveDataOnScene.numOfLayers =numOfLayers;
        saveDataOnScene.layerZRange =layerZRange;

        saveDataOnScene.carrierWave =carrierWave;
        saveDataOnScene.meshShadingOption =meshShadingOption;
        saveDataOnScene.applyTexture =applyTexture;

        saveDataOnScene.useGPU =useGPU;

        saveDataOnScene.GenMethod =GenMethod;


        saveDataOnScene.referenceMesh =referenceMesh;
        saveDataOnScene.scaledDistanseOfObject = scaledDistanseOfObject;
        saveDataOnScene.saveXMLFileName = saveXMLFileName;
    }

}
