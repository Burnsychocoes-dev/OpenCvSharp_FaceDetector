using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using System;

public class Avatar : MonoBehaviour {

    private MORPH3D.M3DCharacterManager avatarManager;
    [SerializeField]
    private Texture maleBlackHeadSkinTexture;
    [SerializeField]
    private Texture maleWhiteHeadSkinTexture;
    [SerializeField]
    private Texture maleBlackBodySkinTexture;
    [SerializeField]
    private Texture maleWhiteBodySkinTexture;
    [SerializeField]
    private Texture femelleBlackHeadSkinTexture;
    [SerializeField]
    private Texture femelleWhiteHeadSkinTexture;
    [SerializeField]
    private Texture femelleBlackBodySkinTexture;
    [SerializeField]
    private Texture femelleWhiteBodySkinTexture;

    public enum Taille
    {
        Big,
        Middle,
        little
    }

    public enum SkinColor
    {
        Black,
        White
    }

    public enum Gender
    {
        Male,
        Femelle
    }

    public struct Eye
    {
        Taille width;
    }

    public struct Nose
    {
        Taille width;
        Taille height;
    }

    public struct Mouth
    {
        Taille height;
    }

    public struct Hair
    {
        bool isHairless;
        Taille height;
        Taille length;
    }

    public struct Person
    {
        Gender gender;
        SkinColor skinColor;
        Color exactSkinColor;
        Eye eye;
        Nose nose;
        Mouth mouth;
        Hair hair;
    }

    private Person perso;
    public Person Perso
    {
        get { return perso; }
    }

    void Start()
    {
        avatarManager = GetComponent<MORPH3D.M3DCharacterManager>();
        avatarManager.SetBlendshapeValue("PHMEyesSize", 100);
        ChangeSkinTexture(new Color(0.56f, 0.27f, 0.27f), true, false);
    }



    // Update is called once per frame
    void Update () {
		
	}

    // Use this for initialization
    public void Create()
    {

    }


    public void ChangeNose(float noseHeight, float noseWidth, float nostrilThickness)
    {
        // En fonction de noseHeight
        avatarManager.SetBlendshapeValue("PHMNoseHeight", 100);
        avatarManager.SetBlendshapeValue("PHMNoseHeight_NEGATIVE_", 100);

        // En fonction de noseWidth
        avatarManager.SetBlendshapeValue("PHMNoseWidth", 100);
        avatarManager.SetBlendshapeValue("PHMNoseWidth_NEGATIVE_", 100);

        // En fonction de nostrilThickness
        avatarManager.SetBlendshapeValue("PHMNostrilWingWidth", 100);
        avatarManager.SetBlendshapeValue("PHMNostrilWingWidth_NEGATIVE_", 100);
    }

    public void ChangeMouth(float distanceBetweenCenterLipAndButtomLip, float distanceBetweenCenterLipAndTopLip, float distanceBetweenChinAndMouth, float distanceBetweenNoseTipAndMouth, float MouthWidth)
    {
        // En fonction de distanceBetweenCenterLipAndButtomLip
        avatarManager.SetBlendshapeValue("PHMLipLowerSize", 100);
        avatarManager.SetBlendshapeValue("PHMLipLowerSize_NEGATIVE_", 100);

        // En fonction de distanceBetweenCenterLipAndTopLip
        avatarManager.SetBlendshapeValue("PHMLipUpperSize", 100);
        avatarManager.SetBlendshapeValue("PHMLipUpperSize_NEGATIVE_", 100);

        // En fonction de distanceBetweenChinAndMouth and distanceBetweenNoseTipAndMouth
        avatarManager.SetBlendshapeValue("PHMMouthHeight", 100);
        avatarManager.SetBlendshapeValue("PHMMouthHeight_NEGATIVE_", 100);

        // En fonction de MouthWidth
        avatarManager.SetBlendshapeValue("PHMMouthWidth", 100);
        avatarManager.SetBlendshapeValue("PHMMouthWidth_NEGATIVE_", 100);
    }

    public void ChangeEyes(float distanceMiddleSourcilCenterEye, float eyeWidth, float distanceBetweenNoseTopAndEyes)
    {
        // En fonction de distanceMiddleSourcilCenterEye
        avatarManager.SetBlendshapeValue("PHMEyesHeight", 100);
        avatarManager.SetBlendshapeValue("PHMEyesHeight_NEGATIVE_", 100);

        // En fonction de eyeWidth
        avatarManager.SetBlendshapeValue("PHMEyesSize", 100);
        avatarManager.SetBlendshapeValue("PHMEyesHeight_NEGATIVE_", 100);

        // En fonction de distanceBetweenNoseTopAndEyes
        avatarManager.SetBlendshapeValue("PHMEyesWidth", 100);
        avatarManager.SetBlendshapeValue("PHMEyesWidth_NEGATIVE_", 100);
    }

    public void ChangeSkinTexture(Color color, bool isMale, bool isWhite)
    {
        if(isWhite && isMale)
        {
            avatarManager.GetHairMaterial().mainTexture = maleWhiteHeadSkinTexture;
            avatarManager.GetBodyMaterial().mainTexture = maleWhiteBodySkinTexture;
        }
        else if(isWhite && !isMale)
        {
            avatarManager.GetHairMaterial().mainTexture = femelleWhiteHeadSkinTexture;
            avatarManager.GetBodyMaterial().mainTexture = femelleWhiteBodySkinTexture;
        }
        else if(!isWhite && isMale)
        {
            avatarManager.GetHairMaterial().mainTexture = maleBlackHeadSkinTexture;
            avatarManager.GetBodyMaterial().mainTexture = maleBlackBodySkinTexture;
        }
        else if(!isWhite && !isMale)
        {
            avatarManager.GetHairMaterial().mainTexture = femelleBlackHeadSkinTexture;
            avatarManager.GetBodyMaterial().mainTexture = femelleBlackBodySkinTexture;
        }
        avatarManager.GetBodyMaterial().SetColor("_Color", color);
        avatarManager.GetHairMaterial().SetColor("_Color", color);
    }

    public void SetHair(bool isHairless)
    {
        if(isHairless)
        {
            foreach (var hair in avatarManager.GetAllHair())
            {
                hair.SetVisibility(false);
            }
        }
    }

    public void SetDressed()
    {
        foreach(var dress in avatarManager.GetAllClothing())
        {
            dress.SetVisibility(true);
        }
    }

    public void SetUndressed()
    {
        foreach (var dress in avatarManager.GetAllClothing())
        {
            dress.SetVisibility(false);
        }
    }

}
