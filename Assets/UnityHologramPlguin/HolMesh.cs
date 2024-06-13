using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[ExecuteInEditMode]
public class HolMesh : HolDataObject
{

    public enum GENTYPE
    {
        POINT_CLOUD,
        LAYER,
        MESH
    }
    public enum MESH_GENTYPE
    {
        MESH=2
    }
    public GENTYPE dataType;
          
    // Start is called before the first frame update
    void Start()
    {
        dataType = GENTYPE.MESH;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void SetFactor()
    {
        foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
        {
            foreach (var renderers in renderer.sharedMaterials)
            {
                if (renderers.shader.name == "Hol/PhongShader")
                {
                    renderers.SetFloat("_DiffuseFactor", DifusseFactor);
                    renderers.SetFloat("_AmbientFactor", AmbientFactor);
                    renderers.SetFloat("_SpecularFactor", SpecularFactor);
                    renderers.SetFloat("_Shininess", ShininessFactor);
                }
            }

        }
    }
    public override void GetFactor()
    {
        var renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer.sharedMaterial.shader.name == "Hol/PhongShader")
        {
            DifusseFactor = renderer.sharedMaterial.GetFloat("_DiffuseFactor");
            AmbientFactor = renderer.sharedMaterial.GetFloat("_AmbientFactor");
            SpecularFactor = renderer.sharedMaterial.GetFloat("_SpecularFactor");
            ShininessFactor = renderer.sharedMaterial.GetFloat("_Shininess");
        }
    }
}
