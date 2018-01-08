using UnityEngine;
using System.Collections;
using UnityEditor;

public class MorphSkinDeferredGUI : ShaderGUI
{
	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        base.OnGUI(materialEditor,props);

		Material material = materialEditor.target as Material;
        SetMaterialKeywords(material);

    }
	static void SetMaterialKeywords(Material material)
    {
        Color eyeTintColor = material.GetColor("_EyeTint");
        bool ShouldTintBeEnabled = eyeTintColor.a > 0.05f;

        SetKeyword(material, "_OVERLAY", material.GetTexture("_Overlay"));
        SetKeyword(material, "_EYETINT", ShouldTintBeEnabled);
        SetKeyword(material, "_EYETEX", material.GetTexture("_EyeTex"));

        //legacy fix for male eye ring in the white area
        bool ShouldRingCheckBeIncluded = false;
        Texture mainTex = material.GetTexture("_MainTex");
        if(mainTex != null && mainTex.name.Contains("M3DMale") || mainTex.name.Contains("M3DFemale"))
        {
            ShouldRingCheckBeIncluded = true;
        }
            ShouldRingCheckBeIncluded = true;
        SetKeyword(material, "_INCLUDERINGCHECK",ShouldRingCheckBeIncluded);
    }
	static void SetKeyword(Material m, string keyword, bool state)
	{
		if (state)
			m.EnableKeyword (keyword);
		else
			m.DisableKeyword (keyword);
	}
}
