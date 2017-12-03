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
        /*
         * petit : 0.23 < noseHeight < 0.25 -> 33 < neg < 100
         * moyen : 0.25 < noseHeight < 0.3 -> if < 0.29 0 < neg < 33 else 0 < pos < 33
         * grand : 0.3 < noseHeight < 0.33 -> 33 < pos < 100
         */
        avatarManager.SetBlendshapeValue("PHMNoseHeight", 100);
        avatarManager.SetBlendshapeValue("PHMNoseHeight_NEGATIVE_", 100);

        // En fonction de noseWidth
        /*
         * petit : 0.24 < noseWidth < 0.27 -> 33 < neg < 100
         * moyen : 0.27 < noseWidth < 0.31 -> if < 0.29 0 < neg < 33 else 0 < pos < 33
         * grand : 0.31 < noseWidth < 0.34 -> 33 < pos < 100
         */
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
        /*
         * Version grossière : 
         * Tout est comparé à facewidth ex : 40% de facewidth
         *   petit : 0.35 <mouthwidth < 0.4 -> mouthWidth,0 ; mouthwidth_negative -> 33 < x < 100
         *   ex : float valueToSet = PercentageConvertorNeg(mouthwidth, 0.35, 0.4, 33, 100);
         *   moyen : 0.4 < mouthwidth < 0.42 -> mouthWidth 0 < x < 33 ou mouthwidth_neg -> 0 < x < 33
         *   ex : if mouthWidth < 0.41 -> valueToSet = PercentageConvertorNeg(mouthwidth, 0.4, 0.41, 0, 33);
         *          else if > 0.41 && < 0.42 -> valueToSet = PercentageConvertor(mouthwidth, 0.41, 0.42, 0, 33);
         *   grand : 0.42< mouthwidth < 0.46 -> mouthwidth 33 < x < 100 
         *   ex : valueToSet = PercentageConvertor(mouthwidth, 0.42, 0.46, 33, 100);
         *   autre valeur : A AFFICHER POUR DEBUGUER
         */
        avatarManager.SetBlendshapeValue("PHMMouthWidth", 100);
        avatarManager.SetBlendshapeValue("PHMMouthWidth_NEGATIVE_", 100);
    }

    public void ChangeEyes(float distanceMiddleSourcilCenterEye, float eyeWidth, float distanceBetweenNoseTopAndEyes)
    {
        // En fonction de distanceMiddleSourcilCenterEye
        avatarManager.SetBlendshapeValue("PHMEyesHeight", 100);
        avatarManager.SetBlendshapeValue("PHMEyesHeight_NEGATIVE_", 100);

        // En fonction de eyeWidth
        /*
         * petit : 0.18 < eyeWidth < 0.22 -> 33 < neg < 100
         * moyen : 0.22 < eyeWidth < 0.24 -> if < 0.21 -> 0 < neg < 33 else 0 < pos < 33
         * grand : 0.24 < eyeWidth < 0.26 -> 33 < pos < 100
         */
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

    public float PercentageConvertor(float vToConvert, float srcIntervalMin, float srcIntervalMax, float destIntervalMin, float destIntervalMax)
    {
        float dSrc = srcIntervalMax - srcIntervalMin;
        float dDest = destIntervalMax - destIntervalMin;
        float d = vToConvert - srcIntervalMin;
        //On commence par chercher la proportion de d par rapport à dSrc
        float prop = d / dSrc;
        //On cherche l'équivalent par rapport à dDest
        float propEqDDest = prop * dDest;
        //Une fois qu'on a l'équivalent, il suffit de l'ajouter à destIntervalMin pour obtenir notre valeur souhaitée
        float destD = destIntervalMin + propEqDDest;
        return destD;

    }

    //pour le destInterval Min, on met le Min mathématique, et pas le min sémantique, ex : mouth_wide_neg, entre 33 et 100, on met 33
    public float PercentageConvertorNeg(float vToConvert, float srcIntervalMin, float srcIntervalMax, float destIntervalMin, float destIntervalMax)
    {
        float destD = - PercentageConvertor(vToConvert, srcIntervalMin, srcIntervalMax, -destIntervalMax, -destIntervalMin);
        return destD;
    }
}
