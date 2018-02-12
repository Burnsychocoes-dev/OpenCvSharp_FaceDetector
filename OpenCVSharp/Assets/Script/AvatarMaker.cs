using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using System;

public class AvatarMaker : MonoBehaviour {

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




    [SerializeField]
    private AvatarScript.Gender prefabGender;
    public AvatarScript.Gender PrefabGender
    {
        get { return prefabGender; }
    }

    [SerializeField]
    private bool isDefinitivAvatar;

    private AvatarScript.Personnage perso;
    public AvatarScript.Personnage Perso
    {
        get { return perso;  }
        set { perso = value; }
    }


    // Variables centre de gravité
    float mouthHeightGravityCenter = 0;
    float chinHeightGravityCenter = 0;

    void Start()
    {
        avatarManager = GetComponent<MORPH3D.M3DCharacterManager>();
    }



    // Update is called once per frame
    void Update () {
        if(!isDefinitivAvatar)
        {
            // Switch case qui permet de recupérer les bonnes caractéristiques de l'avatar
            switch (AvatarScript.avatarSelectionId)
            {
                case 1:
                    perso = AvatarScript.avatar1;
                    break;

                case 2:
                    perso = AvatarScript.avatar2;
                    break;

                case 3:
                    perso = AvatarScript.avatar3;
                    break;

                case 4:
                    perso = AvatarScript.avatar4;
                    break;

                case 5:
                    perso = AvatarScript.avatar5;
                    break;

                default:
                    perso = AvatarScript.avatar1;
                    break;
            }
        }
        else
        {
            perso = AvatarScript.avatarDefinitif;
        }

        if (prefabGender == perso.gender)
        {
            SetDressed();
            SetHair(false);
            ChangeEyes();
            ChangeNose();
            ChangeMouth();
            ModelingFaceCurve();
        }
        else
        {
            SetUndressed();
            SetHair(true);
            avatarManager.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
    }


    public void ChangeNose()
    {
        // En fonction de noseHeight
        /*
         * Version finale : 
         * Avatar min : 0.33 -> 0.31
         * Avatar max : 0.45 -> 0.47
         * Avatar moy : 0.39
         * Si la valeur de noseHeight est entre 0.31 et 0.39 on va lui appliquer sa conversion en blendshape PHMNoseHeight_NEGATIVE_
         * Si la valeur de noseHeight est entre 0.39 et 0.47 on va lui appliquer sa conversion en blendshape PHMNoseHeight
         */
        switch (perso.nose.height)
        {
            case AvatarScript.Taille.Little:
                float valeur_little = PercentageConvertorNeg(perso.nose.noseHeight, 0.31f, 0.39f, 0, 100);
                mouthHeightGravityCenter = -valeur_little;
                avatarManager.SetBlendshapeValue("PHMNoseHeight_NEGATIVE_", valeur_little);
                break;

            case AvatarScript.Taille.Big:
                float valeur_big = PercentageConvertor(perso.nose.noseHeight, 0.39f, 0.47f, 0, 100);
                mouthHeightGravityCenter = valeur_big;
                avatarManager.SetBlendshapeValue("PHMNoseHeight", valeur_big);
                break;
        }

        // En fonction de noseWidth
        /*
         * Version finale : 
         * Avatar min : 0.18 -> 0.215
         * Avatar max : 0.215 -> 0.25
         * Avatar moy : 0.215
         * Si la valeur de noseWidth est entre 0.18 et 0.215 on va lui appliquer sa conversion en blendshape PHMNoseWidth_NEGATIVE_
         * Si la valeur de noseWidth est entre 0.215 et 0.25 on va lui appliquer sa conversion en blendshape PHMNoseWidth
         */
        switch (perso.nose.width)
        {
            case AvatarScript.Taille.Little:
                float valeur_little = PercentageConvertorNeg(perso.nose.noseWidth, 0.18f, 0.215f, 0, 100);
                avatarManager.SetBlendshapeValue("PHMNoseWidth_NEGATIVE_", valeur_little);
                break;

            case AvatarScript.Taille.Big:
                float valeur_big = PercentageConvertor(perso.nose.noseWidth, 0.215f, 0.25f, 0, 100);
                avatarManager.SetBlendshapeValue("PHMNoseWidth", valeur_big);
                break;
        }


        // Suite algo repartition des differents nez
        switch(AvatarScript.avatarSelectionId)
        {
            // avatar1
            case 1:
                switch(perso.nose.noseTipInclinaison)
                {
                    case AvatarScript.NoseTipInclinaison.NezRemonte:
                        avatarManager.SetBlendshapeValue("PHMNoseTipHeight", 100);
                        break;

                    case AvatarScript.NoseTipInclinaison.NezAbaisse:
                        avatarManager.SetBlendshapeValue("PHMNoseTipHeight_NEGATIVE_", 100);
                        break;

                    case AvatarScript.NoseTipInclinaison.NezNormal:
                        avatarManager.SetBlendshapeValue("PHMNoseTipHeight", 0);
                        avatarManager.SetBlendshapeValue("PHMNoseTipHeight_NEGATIVE_", 0);
                        break;
                }
                break;
            
            // avatar2
            case 2:
                switch(perso.nose.noseTipType)
                {
                    case AvatarScript.NoseTipType.NezRond:
                        avatarManager.SetBlendshapeValue("PHMNoseTipRound", 100);
                        break;

                    case AvatarScript.NoseTipType.NezPointue:
                        avatarManager.SetBlendshapeValue("PHMNoseTipRound_NEGATIVE_", 100);
                        break;
                }
                break;

            // avatar3
            case 3:
                switch (perso.nose.noseTipInclinaison)
                {
                    case AvatarScript.NoseTipInclinaison.NezRemonte:
                        avatarManager.SetBlendshapeValue("PHMNoseTipHeight", 50);
                        avatarManager.SetBlendshapeValue("PHMNoseTipRound", 50);
                        break;

                    case AvatarScript.NoseTipInclinaison.NezAbaisse:
                        avatarManager.SetBlendshapeValue("PHMNoseTipHeight_NEGATIVE_", 50);
                        avatarManager.SetBlendshapeValue("PHMNoseTipRound", 50);
                        break;

                    case AvatarScript.NoseTipInclinaison.NezNormal:
                        avatarManager.SetBlendshapeValue("PHMNoseTipRound", 50);
                        break;
                }
                break;

            // avatar4
            case 4:
                switch (perso.nose.noseTipInclinaison)
                {
                    case AvatarScript.NoseTipInclinaison.NezRemonte:
                        avatarManager.SetBlendshapeValue("PHMNoseTipHeight", 50);
                        avatarManager.SetBlendshapeValue("PHMNoseTipRound_NEGATIVE_", 50);
                        break;

                    case AvatarScript.NoseTipInclinaison.NezAbaisse:
                        avatarManager.SetBlendshapeValue("PHMNoseTipHeight_NEGATIVE_", 50);
                        avatarManager.SetBlendshapeValue("PHMNoseTipRound_NEGATIVE_", 50);
                        break;

                    case AvatarScript.NoseTipInclinaison.NezNormal:
                        avatarManager.SetBlendshapeValue("PHMNoseTipRound_NEGATIVE_", 50);
                        break;
                }
                break;

            // avatar5
            case 5:
                if(perso.nose.noseWidth > 0.25)
                {
                    avatarManager.SetBlendshapeValue("PHMNoseWidth", 50);
                    avatarManager.SetBlendshapeValue("PHMNosePinch_NEGATIVE_", 100);
                }
                else if(perso.nose.noseWidth < 0.18)
                {
                    avatarManager.SetBlendshapeValue("PHMNoseWidth_NEGATIVE_", 50);
                    avatarManager.SetBlendshapeValue("PHMNosePinch", 100);
                }
                else
                {
                    if(perso.nose.width == AvatarScript.Taille.Big)
                    {
                        float valeur_big = PercentageConvertor(perso.nose.noseWidth, 0.215f, 0.25f, 0, 100);
                        avatarManager.SetBlendshapeValue("PHMNoseWidth", 25);
                        avatarManager.SetBlendshapeValue("PHMNosePinch_NEGATIVE_", valeur_big);
                    }
                    else
                    {
                        float valeur_little = PercentageConvertorNeg(perso.nose.noseWidth, 0.18f, 0.215f, 0, 100);
                        avatarManager.SetBlendshapeValue("PHMNoseWidth_NEGATIVE_", 25);
                        avatarManager.SetBlendshapeValue("PHMNosePinch", valeur_little);
                    }
                }
                break;
        }
    }

    public void ChangeMouth()
    {
        // En fonction de la proportion levre haute et levre basse
        //switch(perso.mouth.proportionLevre)
        //{
        //    case ProportionLevre.DeuxPourUn:
        //        avatarManager.SetBlendshapeValue("PHMLipLowerSize_NEGATIVE_", 100);
        //        avatarManager.SetBlendshapeValue("PHMLipUpperSize", 100);
        //        break;

        //    case ProportionLevre.UnPourUn:
        //        avatarManager.SetBlendshapeValue("PHMLipLowerSize_NEGATIVE_", 50);
        //        avatarManager.SetBlendshapeValue("PHMLipUpperSize", 100);
        //        break;

        //    case ProportionLevre.UnPourDeux:
        //        avatarManager.SetBlendshapeValue("PHMLipUpperSize", 0);
        //        break;
        //}


        // En fonction de buttomLipHeight
        /*
         * Version finale : 
         * Avatar min : 0.08 -> 0.07
         * Avatar max : 0.10 -> 0.11
         * Avatar moy : 0.09
         * Si la valeur de buttomLipHeight est entre 0.07 et 0.09 on va lui appliquer sa conversion en blendshape PHMLipLowerSize_NEGATIVE_
         * Si la valeur de buttomLipHeight est entre 0.09 et 0.11 on va lui appliquer sa conversion en blendshape PHMLipLowerSize
         */
        switch (perso.mouth.buttomLipHeight_t)
        {
            case AvatarScript.Taille.Little:
                float valeur_little = PercentageConvertorNeg(perso.mouth.buttomLipHeight, 0.07f, 0.09f, 0, 100);
                avatarManager.SetBlendshapeValue("PHMLipLowerSize_NEGATIVE_", valeur_little);
                break;

            case AvatarScript.Taille.Big:
                float valeur_big = PercentageConvertor(perso.mouth.buttomLipHeight, 0.09f, 0.11f, 0, 100);
                avatarManager.SetBlendshapeValue("PHMLipLowerSize", valeur_big);
                break;
        }

        // En fonction de topLipHeight
        /*
         * Version finale : 
         * Avatar min : 0.04 -> 0.03
         * Avatar max : 0.07 -> 0.08
         * Avatar moy : 0.055
         * Si la valeur de topLipHeight est entre 0.04 et 0.055 on va lui appliquer sa conversion en blendshape PHMLipUpperSize_NEGATIVE_
         * Si la valeur de topLipHeight est entre 0.055 et 0.08 on va lui appliquer sa conversion en blendshape PHMLipUpperSize
         */
        switch (perso.mouth.topLipHeight_t)
        {
            case AvatarScript.Taille.Little:
                float valeur_little = PercentageConvertorNeg(perso.mouth.topLipHeight, 0.03f, 0.055f, 0, 100);
                avatarManager.SetBlendshapeValue("PHMLipUpperSize_NEGATIVE_", valeur_little);
                break;

            case AvatarScript.Taille.Big:
                float valeur_big = PercentageConvertor(perso.mouth.topLipHeight, 0.055f, 0.08f, 0, 100);
                avatarManager.SetBlendshapeValue("PHMLipUpperSize", valeur_big);
                break;
        }


        // En fonction de distanceBetweenNoseTipAndMouth
        /*
         * Version finale : 
         * Avatar min : 0.09 -> 0.08
         * Avatar max : 0.19 -> 0.20
         * Avatar moy : 0.14
         * Si la valeur de distanceBetweenChinAndMouth est entre 0.08 et 0.14 on va lui appliquer sa conversion en blendshape PHMMouthHeight_NEGATIVE_
         * Si la valeur de distanceBetweenChinAndMouth est entre 0.14 et 0.20 on va lui appliquer sa conversion en blendshape PHMMouthHeight
         */
        if (perso.mouth.distanceBetweenNoseTipAndMouth <= 0.14)
        {
            float valeur = PercentageConvertorNeg(perso.mouth.distanceBetweenNoseTipAndMouth, 0.08f, 0.14f, 0, 100);
            valeur -= mouthHeightGravityCenter;
            if (valeur > 100)
            {
                chinHeightGravityCenter = -100;
                avatarManager.SetBlendshapeValue("PHMMouthHeight_NEGATIVE_", 100);
            }
            else if (valeur < 0)
            {
                chinHeightGravityCenter = 0;
                avatarManager.SetBlendshapeValue("PHMMouthHeight_NEGATIVE_", 0);
            }
            else
            {
                chinHeightGravityCenter = -valeur;
                avatarManager.SetBlendshapeValue("PHMMouthHeight_NEGATIVE_", valeur);
            }
        }        
        else
        {
            float valeur = PercentageConvertor(perso.mouth.distanceBetweenNoseTipAndMouth, 0.14f, 0.20f, 0, 100);
            valeur += mouthHeightGravityCenter;
            if (valeur > 100)
            {
                chinHeightGravityCenter = 100;
                avatarManager.SetBlendshapeValue("PHMMouthHeight", 100);
            }
            else if (valeur < 0)
            {
                chinHeightGravityCenter = 0;
                avatarManager.SetBlendshapeValue("PHMMouthHeight", 0);
            }
            else
            {
                chinHeightGravityCenter = valeur;
                avatarManager.SetBlendshapeValue("PHMMouthHeight", valeur);
            }
        }


        // En fonction de MouthWidth
        /*
         * Version finale : 
         * Avatar min : 0.34 -> 0.40
         * Avatar max : 0.40 -> 0.46
         * Avatar moy : 0.40
         * Si la valeur de MouthWidth est entre 0.34 et 0.40 on va lui appliquer sa conversion en blendshape PHMMouthWidth_NEGATIVE_
         * Si la valeur de MouthWidth est entre 0.40 et 0.46 on va lui appliquer sa conversion en blendshape PHMMouthWidth
         */
        switch (perso.mouth.width)
        {
            case AvatarScript.Taille.Little:
                float valeur_little = PercentageConvertorNeg(perso.mouth.mouthWidth, 0.34f, 0.40f, 0, 100);
                avatarManager.SetBlendshapeValue("PHMMouthWidth_NEGATIVE_", valeur_little);
                break;

            case AvatarScript.Taille.Big:
                float valeur_big = PercentageConvertor(perso.mouth.mouthWidth, 0.40f, 0.46f, 0, 100);
                avatarManager.SetBlendshapeValue("PHMMouthWidth", valeur_big);
                break;
        }
    }

    public void ChangeEyes()
    {
        // En fonction de eyeWidth
        /*
         * Version finale : 
         * Avatar min : 0.18 -> 0.22
         * Avatar max : 0.22 -> 0.26
         * Avatar moy : 0.22
         * Si la valeur de eyeWidth est entre 0.19 et 0.22 on va lui appliquer sa conversion en blendshape PHMEyesSize_NEGATIVE_ (entre 0 et 50)
         * Si la valeur de eyeWidth est entre 0.22 et 0.25 on va lui appliquer sa conversion en blendshape PHMEyesSize (entre 0 et 50)
         * Les valeurs varient entre 0 et 50 car les extrêmes ne ressemble pas trop à ce qu'il existe en terme de proportion.
         */
        switch (perso.eye.width)
        {
            case AvatarScript.Taille.Little:
                float valeur_little = PercentageConvertorNeg(perso.eye.eyeWidth, 0.18f, 0.22f, 0, 50);
                avatarManager.SetBlendshapeValue("PHMEyesSize_NEGATIVE_", valeur_little);
                break;

            case AvatarScript.Taille.Big:
                float valeur_big = PercentageConvertor(perso.eye.eyeWidth, 0.22f, 0.26f, 0, 50);
                avatarManager.SetBlendshapeValue("PHMEyesSize", valeur_big);
                break;
        }


        // En fonction de distanceBrowEye
        /*
         * Version finale : 
         * Avatar min : 0.08 -> 0.15
         * Avatar max : 0.15 -> 0.22
         * Avatar moy : 0.15
         * Si la valeur de distanceBrowEye est entre 0.08 et 0.15 on va lui appliquer sa conversion en blendshape eCTRLBrowUp_DownL_NEGATIVE_ et eCTRLBrowUp_DownR_NEGATIVE_ 
         * Si la valeur de distanceBrowEye est entre 0.15 et 0.22 on va lui appliquer sa conversion en blendshape eCTRLBrowUp_Down 
         */
        if(perso.eye.distanceBrowEye < 0.15)
        {
            float valeur_little = PercentageConvertorNeg(perso.eye.distanceBrowEye, 0.08f, 0.15f, 0, 100);
            avatarManager.SetBlendshapeValue("eCTRLBrowUp_DownL_NEGATIVE_", valeur_little);
            avatarManager.SetBlendshapeValue("eCTRLBrowUp_DownR_NEGATIVE_", valeur_little);
        }
        else
        {
            float valeur_big = PercentageConvertor(perso.eye.distanceBrowEye, 0.15f, 0.22f, 0, 100);
            avatarManager.SetBlendshapeValue("eCTRLBrowUp_Down", valeur_big);
        }
    }

    public void ChangeSkinTexture(bool isWhite)
    {
        if(isWhite && perso.gender == AvatarScript.Gender.Male)
        {
            avatarManager.GetHairMaterial().mainTexture = maleWhiteHeadSkinTexture;
            avatarManager.GetBodyMaterial().mainTexture = maleWhiteBodySkinTexture;
        }
        else if(isWhite && perso.gender == AvatarScript.Gender.Femelle)
        {
            avatarManager.GetHairMaterial().mainTexture = femelleWhiteHeadSkinTexture;
            avatarManager.GetBodyMaterial().mainTexture = femelleWhiteBodySkinTexture;
        }
        else if(!isWhite && perso.gender == AvatarScript.Gender.Male)
        {
            avatarManager.GetHairMaterial().mainTexture = maleBlackHeadSkinTexture;
            avatarManager.GetBodyMaterial().mainTexture = maleBlackBodySkinTexture;
        }
        else if(!isWhite && perso.gender == AvatarScript.Gender.Femelle)
        {
            avatarManager.GetHairMaterial().mainTexture = femelleBlackHeadSkinTexture;
            avatarManager.GetBodyMaterial().mainTexture = femelleBlackBodySkinTexture;
        }
        //Color color = new Color((float)perso.exactSkinColor.r / 255, (float)perso.exactSkinColor.g / 255, (float)perso.exactSkinColor.b / 255);
        //avatarManager.GetBodyMaterial().SetColor("_Color", color);
        //avatarManager.GetHairMaterial().SetColor("_Color", color);
    }

    public void ModelingFaceCurve()
    {
        // En fonction de distanceBetweenChinAndMouth
        /*
         * Version finale : 
         * Avatar min : 0.18 -> 0.17
         * Avatar max : 0.30 -> 0.31
         * Avatar moy : 0.24
         * Si la valeur de distanceBetweenChinAndMouth est entre 0.17 et 0.24 on va lui appliquer sa conversion en blendshape PHMJawHeight_NEGATIVE_
         * Si la valeur de distanceBetweenChinAndMouth est entre 0.24 et 0.31 on va lui appliquer sa conversion en blendshape PHMJawHeight
         */
        if (perso.mouth.distanceBetweenChinAndMouth <= 0.24)
        {
            float valeur = PercentageConvertorNeg(perso.mouth.distanceBetweenChinAndMouth, 0.17f, 0.24f, 0, 100);
            valeur -= chinHeightGravityCenter;
            if (valeur > 100)
            {
                avatarManager.SetBlendshapeValue("PHMJawHeight_NEGATIVE_", 100);
            }
            else if (valeur < 0)
            {
                avatarManager.SetBlendshapeValue("PHMJawHeight_NEGATIVE_", 0);
            }
            else
            {
                avatarManager.SetBlendshapeValue("PHMJawHeight_NEGATIVE_", valeur);
            }
        }
        else
        {
            float valeur = PercentageConvertor(perso.mouth.distanceBetweenChinAndMouth, 0.24f, 0.31f, 0, 100);
            valeur += chinHeightGravityCenter;
            if (valeur > 100)
            {
                avatarManager.SetBlendshapeValue("PHMJawHeight", 100);
            }
            else if (valeur < 0)
            {
                avatarManager.SetBlendshapeValue("PHMJawHeight", 0);
            }
            else
            {
                avatarManager.SetBlendshapeValue("PHMJawHeight", valeur);
            }
        }


        // En fonction de CornerChinWidth
        /*
         * Version finale : 
         * Avatar min : 0.80 -> 0.79
         * Avatar max : 0.85 -> 0.86
         * Avatar moy : 0.825
         * Si la valeur de CornerChinWidth est entre 0.79 et 0.825 on va lui appliquer sa conversion en blendshape PHMJawCornerWidth
         * Si la valeur de CornerChinWidth est entre 0.825 et 0.89 on va lui appliquer sa conversion en blendshape PHMJawCornerWidth_NEGATIVE_
         */
        if (perso.visage.cornerChinWidth <= 0.825)
        {
            float valeur = PercentageConvertorNeg(perso.visage.cornerChinWidth, 0.79f, 0.825f, 0, 100);
            avatarManager.SetBlendshapeValue("PHMJawCornerWidth_NEGATIVE_", valeur);
        }
        else
        {
            float valeur = PercentageConvertor(perso.visage.cornerChinWidth, 0.825f, 0.86f, 0, 100);
            avatarManager.SetBlendshapeValue("PHMJawCornerWidth", valeur);
        }


        // En fonction de distanceButtomCurve
        /*
         * Version finale : 
         * Avatar min : 0.63 -> 0.62
         * Avatar max : 0.68 -> 0.69
         * Avatar moy : 0.655
         * Si la valeur de distanceButtomCurve est entre 0.62 et 0.655 on va lui appliquer sa conversion en blendshape PHMJawCurve
         * Si la valeur de distanceButtomCurve est entre 0.655 et 0.69 on va lui appliquer sa conversion en blendshape PHMJawCurve_NEGATIVE_
         */
        if (perso.visage.distanceButtomCurve <= 0.655)
        {
            float valeur = PercentageConvertorNeg(perso.visage.cornerChinWidth, 0.62f, 0.655f, 0, 100);
            avatarManager.SetBlendshapeValue("PHMJawCurve_NEGATIVE_", valeur);
        }
        else
        {
            float valeur = PercentageConvertor(perso.visage.cornerChinWidth, 0.655f, 0.69f, 0, 100);
            avatarManager.SetBlendshapeValue("PHMJawCurve", valeur);
        }

    }

    public void SetHair(bool init)
    {
        if(perso.haircut == AvatarScript.Haircut.Chauve)
        {
            foreach (var hair in avatarManager.GetAllHair())
            {
                hair.SetVisibility(false);
            }
        }
        else
        {
            foreach (var hair in avatarManager.GetAllHair())
            {
                if(hair.name == perso.haircut.ToString())
                {
                    hair.SetVisibility(true);
                }
                else
                {
                    hair.SetVisibility(false);
                }
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

    public static float PercentageConvertor(float vToConvert, float srcIntervalMin, float srcIntervalMax, float destIntervalMin, float destIntervalMax)
    {
        if(vToConvert < srcIntervalMin)
        {
            vToConvert = srcIntervalMin;
        }else if(vToConvert > srcIntervalMax)
        {
            vToConvert = srcIntervalMax;
        }
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
    public static float PercentageConvertorNeg(float vToConvert, float srcIntervalMin, float srcIntervalMax, float destIntervalMin, float destIntervalMax)
    {
        if (vToConvert < srcIntervalMin)
        {
            vToConvert = srcIntervalMin;
        }
        float destD = - PercentageConvertor(vToConvert, srcIntervalMin, srcIntervalMax, -destIntervalMax, -destIntervalMin);
        return destD;
    }

    public void Useless()
    {
        //avatarManager.coreMorphs.morphs;
        foreach(MORPH3D.FOUNDATIONS.Morph m in avatarManager.coreMorphs.morphs)
        {
            avatarManager.SetBlendshapeValue(m.name, UnityEngine.Random.value*100);
        }
        
    }

}
