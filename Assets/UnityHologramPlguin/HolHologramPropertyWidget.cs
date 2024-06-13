using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;


//game view와 hologram number of pixel 연동
public static class GameViewUtils
{
    static object gameViewSizesInstance;
    static MethodInfo getGroup;
    public static void SetSize(int index)
    {
         var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView,UnityEditor");
         var gvWnd = EditorWindow.GetWindow(gvWndType);
         var SizeSelectionCallback = gvWndType.GetMethod("SizeSelectionCallback",
                     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
          SizeSelectionCallback.Invoke(gvWnd, new object[] {index,null});
    }

    static GameViewUtils()
    {
        var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
        var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var instanceProp = singleType.GetProperty("instance");
        getGroup = sizesType.GetMethod("GetGroup");
        gameViewSizesInstance = instanceProp.GetValue(null, null);
    }

    public enum GameViewSizeType
    {
        AspectRatio, FixedResolution
    }

    public static void AddCustomSize(GameViewSizeType viewSizeType, GameViewSizeGroupType sizeGroupType, int width, int height, string text)
    {
        var group = GetGroup(sizeGroupType);
        var addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize");
        var gameViewSize = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");

#if UNITY_2017_4_OR_NEWER
        var gameViewSizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
        var ctor = gameViewSize.GetConstructor(new Type[] { gameViewSizeType, typeof(int), typeof(int), typeof(string) });
#else
                    var constructor = gameViewSize.GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(string) });
#endif
        var newSize = ctor.Invoke(new object[] { (int)viewSizeType, width, height, text });
        addCustomSize.Invoke(group, new object[] { newSize });
    }

    public static bool SizeExists(GameViewSizeGroupType sizeGroupType, string text)
    {
        return FindSize(sizeGroupType, text) != -1;
    }

    public static int FindSize(GameViewSizeGroupType sizeGroupType, string text)
    {
        // for loop...

        var group = GetGroup(sizeGroupType);
        var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
        var displayTexts = getDisplayTexts.Invoke(group, null) as string[];
        for (int i = 0; i < displayTexts.Length; i++)
        {
            string display = displayTexts[i];
            // the text we get is "Name (W:H)" if the size has a name, or just "W:H" e.g. 16:9
            // so if we're querying a custom size text we substring to only get the name
            // You could see the outputs by just logging
            int pren = display.IndexOf('(');
            if (pren != -1)
                display = display.Substring(0, pren - 1); // -1 to remove the space that's before the prens. This is very implementation-depdenent
            if (display == text)
                return i;
        }
        return -1;
    }

    public static bool SizeExists(GameViewSizeGroupType sizeGroupType, int width, int height)
    {
        return FindSize(sizeGroupType, width, height) != -1;
    }

    public static int FindSize(GameViewSizeGroupType sizeGroupType, int width, int height)
    {
        var group = GetGroup(sizeGroupType);
        var groupType = group.GetType();
        var getBuiltinCount = groupType.GetMethod("GetBuiltinCount");
        var getCustomCount = groupType.GetMethod("GetCustomCount");
        int sizesCount = (int)getBuiltinCount.Invoke(group, null) + (int)getCustomCount.Invoke(group, null);
        var getGameViewSize = groupType.GetMethod("GetGameViewSize");
        var gvsType = getGameViewSize.ReturnType;
        var widthProp = gvsType.GetProperty("width");
        var heightProp = gvsType.GetProperty("height");
        var indexValue = new object[1];
        for (int i = 0; i < sizesCount; i++)
        {
            indexValue[0] = i;
            var size = getGameViewSize.Invoke(group, indexValue);
            int sizeWidth = (int)widthProp.GetValue(size, null);
            int sizeHeight = (int)heightProp.GetValue(size, null);
            if (sizeWidth == width && sizeHeight == height)
                return i;
        }
        return -1;
    }

    public static int GetSize(GameViewSizeGroupType sizeGroupType)
    {
        var group = GetGroup(sizeGroupType);
        var groupType = group.GetType();
        var getBuiltinCount = groupType.GetMethod("GetBuiltinCount");
        var getCustomCount = groupType.GetMethod("GetCustomCount");
        return ((int)getBuiltinCount.Invoke(group, null) + (int)getCustomCount.Invoke(group, null));  
    }

    static object GetGroup(GameViewSizeGroupType type)
    {
        return getGroup.Invoke(gameViewSizesInstance, new object[] { (int)type });
    }
    public static void RemoveCustomSize(GameViewSizeGroupType sizeGroupType, int index)
    {
        var group = GetGroup(sizeGroupType);
        var addCustomSize = getGroup.ReturnType.GetMethod("RemoveCustomSize");
        addCustomSize.Invoke(group, new object[] { index });
    }


    public static void ModCustomSize(GameViewSizeType viewSizeType, GameViewSizeGroupType sizeGroupType, int width, int height, string text)
    {
        int index = FindSize(sizeGroupType,text);
        var group = GetGroup(sizeGroupType);
        var removeCustomSize = getGroup.ReturnType.GetMethod("RemoveCustomSize");
        if(index>=0)
            removeCustomSize.Invoke(group, new object[] { index });
        var addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize");
        var gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");

#if UNITY_2017_4_OR_NEWER
        var gameViewSizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
        var ctor = gvsType.GetConstructor(new Type[] { gameViewSizeType, typeof(int), typeof(int), typeof(string) });
#else
        var ctor = gvsType.GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(string) });
#endif
        var newSize = ctor.Invoke(new object[] { (int)viewSizeType, width, height, text });
        addCustomSize.Invoke(group, new object[] { newSize });
    }

}

//view
public class HolHologramPropertyWidget : EditorWindow
{

    static HolHologramPlugin data;
    bool informationGroup;
    bool sampleParametesrGroup;
    bool generationGroup;
    bool objectInfoGroup;
    bool visualizationGroup;
    bool reconstructionGroup;
    bool betchGenerationGroup;
    bool XMLFileGroup;
    Vector2 scrollPosition;

    public static Vector2Int GetMainGameViewSize()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
        Vector2 vec2 = (Vector2)Res;
        Vector2Int vec2i = new Vector2Int((int)vec2.x, (int)vec2.y);
        return vec2i;
    }

    //Menu 
    [MenuItem("Window/Hologram Plugin/Hologram Property Setting")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<HolHologramPropertyWidget>("Hologram Property Setting");

    }


    Vector2Int resolVec = Vector2Int.zero;
    private void OnSelectionChange()
    {
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj != null)
            data.referenceMesh = selectedObj.GetComponent<HolDataObject>();
        HolPointCloud refCloud = data.referenceMesh as HolPointCloud;
        if (refCloud != null)
        {
            var zRange = data.layerZRange;
            if (refCloud.data != null)
            {
                zRange.x = refCloud.data.min.z;
                zRange.y = refCloud.data.max.z;
                data.layerZRange = zRange;
            }
        }
        if (data != null)
            data.CameraSetting();

    }
    private void OnInspectorUpdate()
    {
        if (data != null)
        {
            data.CameraSetting();
            //GameViewUtils.ModCustomSize(GameViewUtils.GameViewSizeType.FixedResolution, GameViewSizeGroupType.Standalone, resolVec.x, resolVec.y, "Hologram");
        }
        if(focusedWindow== this)
        {
            EditorApplication.delayCall += () =>
            {
                GameViewUtils.ModCustomSize(GameViewUtils.GameViewSizeType.FixedResolution, GameViewSizeGroupType.Standalone, data.numOfPixels.x, data.numOfPixels.y, "Hologram");
            };
        }

        resolVec = GetMainGameViewSize();
    }
    private void OnGUI()
    {

        if (data == null)
        {
            data = HolHologramPlugin.GetInstance();
            resolVec = data.numOfPixels;
            //GameViewUtils.ModCustomSize(GameViewUtils.GameViewSizeType.FixedResolution, GameViewSizeGroupType.Standalone, resolVec.x, resolVec.y, "Hologram");
        }
        if (data != null)
        {
            data.CameraSetting();
        }
        SetLayOut();
        if (GUI.changed)
        {
            data.SaveToScene();
        }
        if (focusedWindow == this)
        {
            EditorApplication.delayCall += () =>
            {
                GameViewUtils.ModCustomSize(GameViewUtils.GameViewSizeType.FixedResolution, GameViewSizeGroupType.Standalone, data.numOfPixels.x, data.numOfPixels.y, "Hologram");
            };
        }
    }
    void SetLayOut()
    {
        EditorGUIUtility.labelWidth = 230;
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
        informationGroup = EditorGUILayout.Foldout(informationGroup, "CGH Information");
        if (informationGroup)
        {
            InformationLayout();
        }
        XMLFileGroup = EditorGUILayout.Foldout(XMLFileGroup, "Export");
        if (XMLFileGroup)
        {
            FileLayout();
        }
        EditorGUILayout.Space();
        GUILayout.EndScrollView();
    }
    void InformationLayout()
    {
        EditorGUI.indentLevel++;
        sampleParametesrGroup = EditorGUILayout.Foldout(sampleParametesrGroup, "Sampling Parameters");
        if (sampleParametesrGroup)
        {
            EditorGUI.indentLevel++;

            if (data.numOfPixels.x == 0 || data.numOfPixels.y == 0)
                data.numOfPixels = resolVec;
            data.numOfPixels = VectorXYField("Number of Pixels", data.numOfPixels);
            resolVec = data.numOfPixels;
            
            data.pixelPitch = VectorXYField("Sampling Pitch(um)", data.pixelPitch);
            EditorGUI.indentLevel--;
        }
        generationGroup = EditorGUILayout.Foldout(generationGroup, "Generation");
        if (generationGroup)
        {
            EditorGUI.indentLevel++;
            data.fullColor = EditorGUILayout.Toggle("Full Color", data.fullColor);
            data.monoWaveLength = EditorGUILayout.FloatField("Mono Wavelength(nm)", data.monoWaveLength);
            data.RGBWaveLength = VectorRGBField("RGB Wavelength(nm)", data.RGBWaveLength);

            if (data.referenceMesh != null && data.referenceMesh.GetComponent<HolMesh>() == null)
            {
                //depth or pointCloud
                var script = data.referenceMesh.GetComponent<HolPointCloud>();
                if (script.dataType == HolPointCloud.DATA_TYPE.POINT_CLOUD)
                {
                    HolPointCloud.POINT_CLOUD_DATA_TYPE pointCloudDataType = HolPointCloud.POINT_CLOUD_DATA_TYPE.POINT_CLOUD;
                    EditorGUILayout.EnumPopup("CGH Generation Method", pointCloudDataType);
                    data.GenMethod = HolMesh.GENTYPE.POINT_CLOUD;
                }
                else
                {

                    HolPointCloud.GEN_TYPE dataType = HolPointCloud.GEN_TYPE.LAYER;
                    switch (data.GenMethod)
                    {
                        case HolMesh.GENTYPE.LAYER:
                        case HolMesh.GENTYPE.POINT_CLOUD:
                            dataType = (HolPointCloud.GEN_TYPE)data.GenMethod;
                            break;
                        case HolMesh.GENTYPE.MESH:
                            dataType = HolPointCloud.GEN_TYPE.POINT_CLOUD;
                            break;
                    }
                    data.GenMethod = (HolMesh.GENTYPE)EditorGUILayout.EnumPopup("CGH Generation Method", dataType);
                }
            }
            else
            {
                HolMesh.MESH_GENTYPE dataType = HolMesh.MESH_GENTYPE.MESH;
                data.GenMethod = (HolMesh.GENTYPE)EditorGUILayout.EnumPopup("CGH Generation Method", dataType);
            }


            data.zDepthScaleFactor = EditorGUILayout.FloatField("Object Location(cm)", data.zDepthScaleFactor);
            data.useGPU = EditorGUILayout.Toggle("Use GPU", data.useGPU);
            data.randomPhase = EditorGUILayout.Toggle("Random Phase", data.randomPhase);

            switch (data.GenMethod)
            {
                case HolMesh.GENTYPE.LAYER:
                    data.numOfLayers = EditorGUILayout.IntField("Number of Layers", data.numOfLayers);
                    string[] caption = new string[2] { " ", " " };
                    data.layerZRange = VectorCustomField("Layer Z Range", data.layerZRange, caption);
                    break;
                case HolMesh.GENTYPE.MESH:
                    data.carrierWave = VectorXYZField("Carrier Wave (Mesh)", data.carrierWave);
                    data.meshShadingOption = (HolHologramPlugin.MESH_SHADING_OPTION)EditorGUILayout.EnumPopup("Mesh Shading Option", data.meshShadingOption);
                    data.applyTexture = EditorGUILayout.Toggle("Apply Texture(Mesh)", data.applyTexture);
                    break;
            }




            EditorGUI.indentLevel--;
        }

        data.referenceMesh = EditorGUILayout.ObjectField("Target Object", data.referenceMesh, typeof(HolDataObject), true) as HolDataObject;
        EditorGUI.indentLevel--;
    }
    public static Vector3 VectorRGBField(string label, Vector3 value, params GUILayoutOption[] options)
    {
        using (var j = new EditorGUILayout.HorizontalScope())
        {
            EditorGUIUtility.labelWidth = 200;
            EditorGUIUtility.fieldWidth = 1;
            EditorGUILayout.LabelField(label);
            EditorGUIUtility.labelWidth = 50;
            EditorGUIUtility.fieldWidth = 50;
            {
                value.x = EditorGUILayout.FloatField("R", value.x);
                value.y = EditorGUILayout.FloatField("G", value.y);
                value.z = EditorGUILayout.FloatField("B", value.z);
            }
            EditorGUIUtility.labelWidth = 230;
        }
        return value;
    }

    public static Vector3 VectorXYZField(string label, Vector3 value, params GUILayoutOption[] options)
    {
        using (var j = new EditorGUILayout.HorizontalScope())
        {
            EditorGUIUtility.labelWidth = 200;
            EditorGUIUtility.fieldWidth = 1;
            EditorGUILayout.LabelField(label);
            EditorGUIUtility.fieldWidth = 50;
            EditorGUIUtility.labelWidth = 50;
            {
                value.x = EditorGUILayout.FloatField("X", value.x);
                value.y = EditorGUILayout.FloatField("Y", value.y);
                value.z = EditorGUILayout.FloatField("Z", value.z);
            }
            EditorGUIUtility.labelWidth = 230;
        }
        return value;
    }

    public static Vector2 VectorXYField(string label, Vector2 value, params GUILayoutOption[] options)
    {
        using (var j = new EditorGUILayout.HorizontalScope())
        {
            EditorGUIUtility.labelWidth = 50;
            EditorGUIUtility.fieldWidth = 1;
            EditorGUILayout.LabelField(label);
            EditorGUIUtility.fieldWidth = 50;
            {
                value.x = EditorGUILayout.FloatField("X", value.x);
                value.y = EditorGUILayout.FloatField("Y", value.y);
            }
            EditorGUIUtility.labelWidth = 230;
        }
        return value;
    }

    public static Vector2 VectorCustomField(string label, Vector2 value, string[] captions, params GUILayoutOption[] options)
    {
        using (var j = new EditorGUILayout.HorizontalScope())
        {
            EditorGUIUtility.labelWidth = 200;
            EditorGUIUtility.fieldWidth = 1;
            EditorGUILayout.LabelField(label);
            EditorGUIUtility.fieldWidth = 1;
            EditorGUIUtility.labelWidth = 1;
            {
                value.x = EditorGUILayout.FloatField(captions[0], value.x);
                value.y = EditorGUILayout.FloatField(captions[1], value.y);
            }
            EditorGUIUtility.fieldWidth = 50;
            EditorGUIUtility.labelWidth = 230;
        }
        return value;
    }

    public static Vector2Int VectorXYField(string label, Vector2Int value, params GUILayoutOption[] options)
    {
        using (var j = new EditorGUILayout.HorizontalScope())
        {
            EditorGUIUtility.labelWidth = 50;
            EditorGUIUtility.fieldWidth = 1;
            EditorGUILayout.LabelField(label);
            EditorGUIUtility.fieldWidth = 50;
            {
                value.x = EditorGUILayout.IntField("X", value.x);
                value.y = EditorGUILayout.IntField("Y", value.y);
            }
            EditorGUIUtility.labelWidth = 230;
        }
        return value;
    }
    public void FileLayout()
    {
        EditorGUI.indentLevel++;
        SaveXML();
        EditorGUI.indentLevel--;
    }

    public void SaveXML()
    {
        data.cghOutputFIlePrefix = EditorGUILayout.TextField("CGH Output File Prefix", data.cghOutputFIlePrefix);
        using (var j = new EditorGUILayout.HorizontalScope())
        {
            data.saveXMLFileName = EditorGUILayout.TextField("XML Path : ", data.saveXMLFileName);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string str;
                try
                {
                    str = Path.GetDirectoryName(data.saveXMLFileName);
                }
                catch (Exception e)
                {
                    str = e.ToString();
                    str = "";
                }
                string path = EditorUtility.SaveFilePanel("Export XML File", str, data.saveXMLFileName, "xml");
                if (path.Length != 0)
                {
                    data.saveXMLFileName = path;
                }
            }
        }
        using (var j = new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Export XML", GUILayout.Width(200)))
            {

                data.SaveHologramInputXML(data.saveXMLFileName);
            }
        }

    }
}