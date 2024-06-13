using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml;
public class HolLoaderWidget : EditorWindow
{
    static HolObjectLoader loader;

    Vector2 scrollPosition;

    bool pointCloudGroup;
    bool loadPointCloudGroup;
    bool loadDepthGroup;
    bool pointCloudColorGroup;
    bool calcNoramlGroup;
    bool fileSaveGroup;
    bool meshGroup;
    bool loadMeshGroup;
    bool meshColorGroup;
    bool calcUVGroup;
    bool textureGroup;
    bool saveObjGroup;


    
    bool saveSceneGroup;
    bool loadSceneGroup;

    bool meshToDepthGroup;
    public enum PNG_BIT
    {
        PNG_DEPTH_8BIT,
        PNG_DEPTH_16BIT
    }
    PNG_BIT useZMinZFMax=PNG_BIT.PNG_DEPTH_8BIT;
    string directory="";

    bool mIsOpenGL;

    // Start is called before the first frame update
    [MenuItem("Window/Hologram Plugin/Object Loader")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<HolLoaderWidget>("Object Loader");

    }
    private void OnSelectionChange()
    {
        if (loader != null)
            loader.traceSelect();
        else
            loader=HolObjectLoader.GetInstance();

        loader.CameraSetting();
    }
    private void OnInspectorUpdate()
    {

    }
    private void OnGUI()
    {
        if (loader == null)
        {
            mIsOpenGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
            loader = HolObjectLoader.GetInstance();
        }
        SetLayout();
        
    }
    void SetLayout()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
        loader.TargetData = EditorGUILayout.ObjectField("Target Object", loader.TargetData, typeof(HolDataObject), true) as HolDataObject;
        if (loader.targetCloud != null)
        {
            loader.targetCloud.GetFactor();
            loader.targetCloud.AmbientFactor = EditorGUILayout.FloatField("Ambient Factor", loader.targetCloud.AmbientFactor);
            loader.targetCloud.DifusseFactor = EditorGUILayout.FloatField("Diffuse Factor", loader.targetCloud.DifusseFactor);
            loader.targetCloud.SpecularFactor = EditorGUILayout.FloatField("Specular Factor", loader.targetCloud.SpecularFactor);
            loader.targetCloud.ShininessFactor = EditorGUILayout.FloatField("Specular Exponent", loader.targetCloud.ShininessFactor);
            loader.targetCloud.SetFactor();
        }
        else if(loader.targetObject != null)
        {
            loader.targetObject.GetFactor();
            loader.targetObject.AmbientFactor = EditorGUILayout.FloatField("Ambient Factor", loader.targetObject.AmbientFactor);
            loader.targetObject.DifusseFactor = EditorGUILayout.FloatField("Diffuse Factor", loader.targetObject.DifusseFactor);
            loader.targetObject.SpecularFactor = EditorGUILayout.FloatField("Specular Factor", loader.targetObject.SpecularFactor);
            loader.targetObject.ShininessFactor = EditorGUILayout.FloatField("Specular Exponent", loader.targetObject.ShininessFactor);
            loader.targetObject.SetFactor();
        }else
        {
            EditorGUILayout.FloatField("Ambient Factor", 0);
            EditorGUILayout.FloatField("Diffuse Factor", 0);
            EditorGUILayout.FloatField("Specular Factor", 0);
            EditorGUILayout.FloatField("Specular Exponent", 0);
        }

        EditorGUIUtility.labelWidth = 230;
        pointCloudGroup = EditorGUILayout.Foldout(pointCloudGroup, "Point Cloud");
        if (pointCloudGroup)
        {
            EditorGUI.indentLevel++;

            if (mIsOpenGL && loader.targetCloud != null)
            {
                loader.targetCloud.PointSize = EditorGUILayout.FloatField("Preview Point Size", loader.targetCloud.PointSize);
                loader.targetCloud.SetFactor();

            }


            loadPointCloudGroup = EditorGUILayout.Foldout(loadPointCloudGroup, "Load Point Cloud");
            if (loadPointCloudGroup)
            {
                EditorGUI.indentLevel++;
                LoadPointCloudLayout();
                EditorGUI.indentLevel--;
            }
            loadDepthGroup= EditorGUILayout.Foldout(loadDepthGroup, "Load Depth Image");
            if (loadDepthGroup)
            {
                EditorGUI.indentLevel++;
                LoadDepthImageLayout();
                EditorGUI.indentLevel--;
            }
            pointCloudColorGroup = EditorGUILayout.Foldout(pointCloudColorGroup, "Point Cloud Color");
            if (pointCloudColorGroup)
            {
                EditorGUI.indentLevel++;
                PointCloudColorLayout();
                EditorGUI.indentLevel--;
            }

            fileSaveGroup = EditorGUILayout.Foldout(fileSaveGroup, "Save Point Cloud");
            if (fileSaveGroup)
            {
                EditorGUI.indentLevel++;
                FileSaveLayout();
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
        meshGroup = EditorGUILayout.Foldout(meshGroup, "Mesh");
        if (meshGroup) {
            EditorGUI.indentLevel++;

            loadMeshGroup = EditorGUILayout.Foldout(loadMeshGroup, "Load Mesh");
            if (loadMeshGroup)
            {
                EditorGUI.indentLevel++;
                LoadObjLayout();
                EditorGUI.indentLevel--;
            }

            meshColorGroup = EditorGUILayout.Foldout(meshColorGroup, "Mesh Color");
            if (meshColorGroup)
            {
                EditorGUI.indentLevel++;
                SetObjColorLayout();
                EditorGUI.indentLevel--;
            }
            

            textureGroup = EditorGUILayout.Foldout(textureGroup, "Load Texture");
            if (textureGroup)
            {
                EditorGUI.indentLevel++;
                LoadTextureLayout();
                EditorGUI.indentLevel--;
            }

            meshToDepthGroup = EditorGUILayout.Foldout(meshToDepthGroup, "Conversion");
            if (meshToDepthGroup)
            {
                EditorGUI.indentLevel++;
                MeshToDepthLayout();
                EditorGUI.indentLevel--;
            }


            EditorGUI.indentLevel--;
            
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        GUILayout.EndScrollView();
    }
    void LoadPointCloudLayout()
    {
        
        using (var j = new EditorGUILayout.HorizontalScope())
        {
            loader.fileName = EditorGUILayout.TextField("Point Cloud File Name ", loader.fileName);

            //file Explorer Button
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFilePanel("Open Point Cloud", loader.fileName, "ply,pcd");
                if (path.Length != 0)
                {
                    loader.fileName = path;
                }
            }
        }

        using (var j = new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Load", GUILayout.Width(200) ))
            {
                loader.LoadPointCloud();
            }
        }
    }

    void LoadDepthImageLayout()
    {

        useZMinZFMax = (PNG_BIT)EditorGUILayout.EnumPopup("Depth Image Type", useZMinZFMax);
        if (useZMinZFMax == PNG_BIT.PNG_DEPTH_8BIT)
        {
            loader.zMin = EditorGUILayout.FloatField("zMin",loader.zMin);
            loader.zMax = EditorGUILayout.FloatField("zMax", loader.zMax);
            loader.xyScale = EditorGUILayout.FloatField("XYScale", loader.xyScale);
        }
        else
        {
            using (var j = new EditorGUILayout.HorizontalScope())
            {
                loader.KFileName = EditorGUILayout.TextField("Camera Intrinsic Parameter File", loader.KFileName);

                //file Explorer Button
                if (GUILayout.Button("...", GUILayout.Width(30)))
                {
                    string path = EditorUtility.OpenFilePanel("Open Intrinsic File", loader.KFileName, "txt");
                    if (path.Length != 0)
                    {
                        loader.KFileName = path;
                    }
                }
            }

            using (var j = new EditorGUILayout.HorizontalScope())
            {
                loader.camPoseFileName = EditorGUILayout.TextField("Camera Extrinsic Parameter File", loader.camPoseFileName);

                //file Explorer Button
                if (GUILayout.Button("...", GUILayout.Width(30)))
                {
                    string path = EditorUtility.OpenFilePanel("Open Extrinsic File", loader.camPoseFileName, "txt");
                    if (path.Length != 0)
                    {
                        loader.camPoseFileName = path;
                    }
                }
            }
        }

        loader.previewResolution = EditorGUILayout.IntField("Preview Resolution", loader.previewResolution);

        using (var j = new EditorGUILayout.HorizontalScope())
        {
            
            loader.depthFileName = EditorGUILayout.TextField("Depth Image File", loader.depthFileName);

            //file Explorer Button
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFilePanel("Open Depth Image", loader.depthFileName, "png");
                if (path.Length != 0)
                {
                    loader.depthFileName = path;
                }
            }
        }

        using (var j = new EditorGUILayout.HorizontalScope())
        {
            loader.depthPointsTextureFIleName = EditorGUILayout.TextField("Texture(Optional)", loader.depthPointsTextureFIleName);


            //file Explorer Button
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFilePanel("Open Intrinsic File", loader.depthPointsTextureFIleName, "jpg,bmp,png");
                if (path.Length != 0)
                {
                    loader.depthPointsTextureFIleName = path;
                }
            }
        }

        loader.normalCalcSize = EditorGUILayout.IntField("Normal Calculation Radius", loader.normalCalcSize);
        using (var j = new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Load", GUILayout.Width(100)))
            {
                if (useZMinZFMax == PNG_BIT.PNG_DEPTH_8BIT)
                {
                    loader.LoadDepthPointsUseZValue();
                }
                else
                {
                    loader.LoadDepthPoints();
                }
            }
        }
    }

    void PointCloudColorLayout()
    {
        loader.pointColor = EditorGUILayout.ColorField("Color", loader.pointColor);
        using (var j = new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Set Color", GUILayout.Width(200)))
            {
                loader.SetColor();
            }
        }
    }    
    void FileSaveLayout()
    {
        using (var j = new EditorGUILayout.HorizontalScope())
        {
            loader.saveFileName = EditorGUILayout.TextField("File Path ", loader.saveFileName);

            //file Explorer Button
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.SaveFilePanel("Save Point Cloud", directory, loader.saveFileName, "ply,pcd");
                if (path.Length != 0)
                {
                    loader.saveFileName = path;
                }
            }
        }

        loader.fileType = (HolObjectLoader.FileType)EditorGUILayout.EnumPopup("File Type",loader.fileType);
        using (var j = new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Save", GUILayout.Width(200)))
            {
                loader.SavePointCloud();
            }
        }
    }





    void LoadObjLayout()
    {
        using (var j = new EditorGUILayout.HorizontalScope())
        {
            loader.sourceObjectPath = EditorGUILayout.TextField("File Path", loader.sourceObjectPath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFilePanel("Load File", loader.sourceObjectPath, "obj,fbx");
                if (path.Length != 0)
                {
                    loader.sourceObjectPath = path;
                }
            }
        }
        using (var j = new EditorGUILayout.HorizontalScope())
        {

            EditorGUILayout.Space();
            if (GUILayout.Button("Load", GUILayout.Width(200)))
            {
                loader.loadObjectPath =  loader.CopyObject();
                loader.LoadObject();
            }
        }
    }
    void SetObjColorLayout()
    {

        loader.meshColor = EditorGUILayout.ColorField("Mesh Color", loader.meshColor);
        using (var j = new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Set Color", GUILayout.Width(200)))
            {
                loader.SetMeshColor();
            }
        }
    }
    void LoadTextureLayout()
    {
        using (var j = new EditorGUILayout.HorizontalScope())
        {
            loader.sourceTexturePath = EditorGUILayout.TextField("File Path", loader.sourceTexturePath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFilePanel("Load File", loader.sourceTexturePath, "bmp,jpg,png");
                if (path.Length != 0)
                {
                    loader.sourceTexturePath = path;
                }
            }
        }

        using (var j = new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Load", GUILayout.Width(200)))
            {

                loader.CopyTexture();
            }
        }
        
    }

    bool savePointCloud;
    int imageWidth;
    int imageHeight;
    bool applyTransformation;
    public enum CONVERT_TYPE { BOTH,DEPTH,POINT_CLOUD}
    CONVERT_TYPE convertType;
    public void MeshToDepthLayout()
    {

        convertType = (CONVERT_TYPE)EditorGUILayout.EnumPopup("Export Type",convertType);
        applyTransformation = true;
        switch (convertType)
        {
            case CONVERT_TYPE.BOTH:
                imageWidth = EditorGUILayout.IntField("Image Width", imageWidth);
                imageHeight = EditorGUILayout.IntField("Image Height", imageHeight);
                using (var j = new EditorGUILayout.HorizontalScope())
                {
                    loader.saveDepthFolder = EditorGUILayout.TextField("Folder Path", loader.saveDepthFolder);
                    if (GUILayout.Button("...", GUILayout.Width(30)))
                    {
                        string path = EditorUtility.SaveFolderPanel("Save Folder Path", loader.saveDepthFolder, loader.saveDepthFolder);
                        if (path.Length != 0)
                        {
                            loader.saveDepthFolder = path;
                        }
                    }
                }
                using (var j = new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Convert", GUILayout.Width(200)))
                    {
                        loader.MeshToPointCloud(true, true, imageWidth, imageHeight, applyTransformation);
                    }
                }
                return;
            case CONVERT_TYPE.DEPTH:
                savePointCloud = false;
                break;
            case CONVERT_TYPE.POINT_CLOUD:
                savePointCloud = true;
                break;
        }
        if (savePointCloud == false)
        {
            imageWidth = EditorGUILayout.IntField("Image Width", imageWidth);
            imageHeight = EditorGUILayout.IntField("Image Height", imageHeight);
        }

        using (var j = new EditorGUILayout.HorizontalScope())
        {
            loader.saveDepthFolder = EditorGUILayout.TextField("Folder Path", loader.saveDepthFolder);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.SaveFolderPanel("Save Folder Path", loader.saveDepthFolder, loader.saveDepthFolder);
                if (path.Length != 0)
                {
                    loader.saveDepthFolder = path;
                }
            }
        }
        using (var j = new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Convert", GUILayout.Width(200)))
            {
                loader.MeshToPointCloud(!savePointCloud,savePointCloud, imageWidth, imageHeight, applyTransformation);
            }
        }
    }

}
