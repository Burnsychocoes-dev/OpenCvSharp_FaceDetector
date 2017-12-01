using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;

public class Avatar : MonoBehaviour {

    MORPH3D.M3DCharacterManager avatar;
    List<MORPH3D.FOUNDATIONS.Morph> morphs;

    enum Taille
    {
        Big,
        Middle,
        little
    }

    struct Eye
    {
        Taille width;
    }

    struct Nose
    {
        Taille width;
        Taille height;
    }

    struct Mouth
    {
        Taille height;
    }

    struct Hair
    {
        Taille height;
        Taille length;
    }

    void Start()
    {
        avatar = GetComponent<MORPH3D.M3DCharacterManager>();
        morphs = avatar.coreMorphs.morphs;
        //foreach(var morph in morphs)
        //{
        //    Debug.Log(morph.name);
        //}
        avatar.SetBlendshapeValue("PHMEyesSize", 100);
    }

    // Update is called once per frame
    void Update () {
		
	}

    // Use this for initialization
    public void Create()
    {

    }


    public void ChangeColor(Vec3f color)
    {
        //MORPH3D.FOUNDATIONS.Morph morph = new MORPH3D.FOUNDATIONS.Morph();
        //System.Predicate<MORPH3D.FOUNDATIONS.Morph> PHMChinWidth = new System.Predicate<MORPH3D.FOUNDATIONS.Morph>();
        //morphs.Find(PHMChinWidth);
    }

    //private static bool FindMorph(MORPH3D.FOUNDATIONS.Morph obj)
    //{
    //    return obj.;
    //}
}
