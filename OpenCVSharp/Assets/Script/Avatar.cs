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
        Little
    }

    public enum ProportionLevre
    {
        UnPourDeux,
        UnPourUn,
        DeuxPourUn
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
        public float distanceMiddleSourcilCenterEye;
        public float eyeWidth;
        public float distanceBetweenNoseTopAndEyes;
        public Taille width;
    }

    public struct Nose
    {
        public float noseHeight;
        public float noseWidth;
        public float nostrilThickness;
        public Taille width;
        public Taille height;
    }

    public struct Mouth
    {
        public float buttomLipHeight;
        public Taille buttomLipHeight_t;
        public float topLipHeight;
        public Taille topLipHeight_t;
        public ProportionLevre proportionLevre;
        public float distanceBetweenChinAndMouth;
        public float distanceBetweenNoseTipAndMouth;
        public float mouthWidth;
        public Taille width;
    }

    public struct Hair
    {
        public bool isHairless;
        public Taille height;
        public Taille length;
    }

    public struct Personnage
    {
        public Gender gender;
        public SkinColor skinColor;
        public Color32 exactSkinColor;
        public Eye eye;
        public Nose nose;
        public Mouth mouth;
        public Hair hair;
    }

    private Personnage perso;
    public Personnage Perso
    {
        get { return perso; }
    }

    FaceDetectionImage face;
    LandmarksRetriever landmarks;
    HairDetection hair;

    

    void Start()
    {
        avatarManager = GetComponent<MORPH3D.M3DCharacterManager>();
        landmarks = GetComponent<LandmarksRetriever>();
        face = GetComponent<FaceDetectionImage>();
        hair = GetComponent<HairDetection>();
        SetUndressed();
        SetHair(true);
        avatarManager.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        this.transform.Translate(new Vector3(5, 0, 0));
    }



    // Update is called once per frame
    void Update () {
        
	}

    // Use this for init the personnage 
    public void SetPerso()
    {
        this.transform.Translate(new Vector3(-5, 0, 0));
        avatarManager.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        SetDressed();
        // Partie gender
        //if (landmarks.gender == "M")
        //    perso.gender = Gender.Male;
        //else
        //    perso.gender = Gender.Femelle;

        //Debug.Log(hair.yHairRoot);
        //Debug.Log(hair.yHairTop);
        //Debug.Log(Mathf.Abs(hair.yHairRoot - hair.yHairTop) / landmarks.faceHeight);

        if(Mathf.Abs(hair.yHairRoot - hair.yHairTop)/landmarks.faceHeight < 0.05f || hair.yHairRoot == -1)
        {
            if(hair.longueur != HairDetection.Longueur.moyen && hair.longueur != HairDetection.Longueur.longs)
            {
                perso.hair.isHairless = true;
            }
                
        }

        // Partie skin color
        perso.exactSkinColor = hair.FromYCbCrToRGB(hair.SkinColorYCbCrExpectancy);
        Debug.Log(perso.exactSkinColor);

        if (face.CouleurPeauFront.Item0 > 170)
            perso.skinColor = SkinColor.Black;
        else
            perso.skinColor = SkinColor.White;


        // Partie eye
        perso.eye.distanceBetweenNoseTopAndEyes = (float)landmarks.distanceBetweenNoseTopAndEyes;
        perso.eye.distanceMiddleSourcilCenterEye = Mathf.Abs((float)landmarks.RightEyeBrowMiddle.Item1 - (float)landmarks.rightEyeCenter.Item1);
        perso.eye.eyeWidth = (float)landmarks.leftEyeWidth;

        if (perso.eye.eyeWidth <= 0.22f)
            perso.eye.width = Taille.Little;
        else
            perso.eye.width = Taille.Big;


        // Partie nose
        perso.nose.noseHeight = (float)landmarks.noseHeight;
        perso.nose.noseWidth = (float)landmarks.noseWidth;
        perso.nose.nostrilThickness = (float)landmarks.nostrilThickness;

        if (perso.nose.noseHeight <= 0.39)
            perso.nose.height = Taille.Little;
        else 
            perso.nose.height = Taille.Big;

        if (perso.nose.noseWidth <= 0.215)
            perso.nose.width = Taille.Little;
        else
            perso.nose.width = Taille.Big;


        // Partie mouth
        perso.mouth.distanceBetweenChinAndMouth = (float)landmarks.distanceBetweenLipAndChin;
        perso.mouth.distanceBetweenNoseTipAndMouth = (float)landmarks.distanceBetweenNoseTipAndLip;

        perso.mouth.buttomLipHeight = (float)landmarks.buttomLipHeight;
        perso.mouth.topLipHeight = (float)landmarks.topLipHeight;
        Debug.Log(Math.Abs((float)perso.mouth.topLipHeight / (float)perso.mouth.buttomLipHeight));
        if (Math.Abs((float)perso.mouth.topLipHeight / (float)perso.mouth.buttomLipHeight) < 0.8)
            perso.mouth.proportionLevre = ProportionLevre.UnPourDeux;
        else if (Math.Abs((float)perso.mouth.topLipHeight / (float)perso.mouth.buttomLipHeight) > 1.8)
            perso.mouth.proportionLevre = ProportionLevre.DeuxPourUn;
        else
            perso.mouth.proportionLevre = ProportionLevre.UnPourUn;

        if (perso.mouth.buttomLipHeight <= 0.09)
            perso.mouth.buttomLipHeight_t = Taille.Little;
        else
            perso.mouth.buttomLipHeight_t = Taille.Big;

        if (perso.mouth.topLipHeight <= 0.055)
            perso.mouth.topLipHeight_t = Taille.Little;
        else
            perso.mouth.topLipHeight_t = Taille.Big;

        perso.mouth.mouthWidth = (float)landmarks.lipWidth;

        if (perso.mouth.mouthWidth <= 0.40)
            perso.mouth.width = Taille.Little;
        else
            perso.mouth.width = Taille.Big;

    }


    public void ChangeNose()
    {
        // En fonction de noseHeight
        /*
         * Version finale : 
         * Avatar min : 0.32 -> 0.39
         * Avatar max : 0.39 -> 0.46
         * Avatar moy : 0.39
         * Si la valeur de noseHeight est entre 0.32 et 0.39 on va lui appliquer sa conversion en blendshape PHMNoseHeight_NEGATIVE_
         * Si la valeur de noseHeight est entre 0.39 et 0.46 on va lui appliquer sa conversion en blendshape PHMNoseHeight
         */
        switch (perso.nose.height)
        {
            case Taille.Little:
                float valeur_little = PercentageConvertorNeg(perso.nose.noseHeight, 0.31f, 0.39f, 0, 100);
                avatarManager.SetBlendshapeValue("PHMNoseHeight_NEGATIVE_", valeur_little);
                break;

            case Taille.Big:
                float valeur_big = PercentageConvertor(perso.nose.noseHeight, 0.39f, 0.47f, 0, 100);
                avatarManager.SetBlendshapeValue("PHMNoseHeight", valeur_big);
                break;
        }

        // En fonction de noseWidth
        /*
         * Version finale : 
         * Avatar min : 0.18 -> 0.215
         * Avatar max : 0.215 -> 0.25
         * Avatar moy : 0.215
         * Si la valeur de noseWidth est entre 0.18 et 0.215 on va lui appliquer sa conversion en blendshape PHMNoseHeight_NEGATIVE_
         * Si la valeur de noseWidth est entre 0.215 et 0.25 on va lui appliquer sa conversion en blendshape PHMNoseHeight
         */
        switch (perso.nose.width)
        {
            case Taille.Little:
                float valeur_little = PercentageConvertorNeg(perso.nose.noseWidth, 0.18f, 0.215f, 0, 100);
                avatarManager.SetBlendshapeValue("PHMNoseWidth_NEGATIVE_", valeur_little);
                break;

            case Taille.Big:
                float valeur_big = PercentageConvertor(perso.nose.noseWidth, 0.215f, 0.25f, 0, 100);
                avatarManager.SetBlendshapeValue("PHMNoseWidth", valeur_big);
                break;
        }


        // En fonction de nostrilThickness
        //avatarManager.SetBlendshapeValue("PHMNostrilWingWidth", 100);
        //avatarManager.SetBlendshapeValue("PHMNostrilWingWidth_NEGATIVE_", 100);
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
            case Taille.Little:
                float valeur_little = PercentageConvertorNeg(perso.mouth.buttomLipHeight, 0.07f, 0.09f, 0, 100);
                avatarManager.SetBlendshapeValue("PHMLipLowerSize_NEGATIVE_", valeur_little);
                break;

            case Taille.Big:
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
            case Taille.Little:
                float valeur_little = PercentageConvertorNeg(perso.mouth.topLipHeight, 0.03f, 0.055f, 0, 100);
                avatarManager.SetBlendshapeValue("PHMLipUpperSize_NEGATIVE_", valeur_little);
                break;

            case Taille.Big:
                float valeur_big = PercentageConvertor(perso.mouth.topLipHeight, 0.055f, 0.08f, 0, 100);
                avatarManager.SetBlendshapeValue("PHMLipUpperSize", valeur_big);
                break;
        }


        // En fonction de distanceBetweenChinAndMouth
        /*
         * Version finale : 
         * Avatar min : 0.20 -> 0.19
         * Avatar max : 0.27 -> 0.28
         * Avatar moy : 0.235
         * Si la valeur de distanceBetweenChinAndMouth est entre 0.19 et 0.235 on va lui appliquer sa conversion en blendshape PHMMouthHeight_NEGATIVE_
         * Si la valeur de distanceBetweenChinAndMouth est entre 0.235 et 0.28 on va lui appliquer sa conversion en blendshape PHMMouthHeight
         */
        if (perso.mouth.distanceBetweenChinAndMouth <= 0.18)
            avatarManager.SetBlendshapeValue("PHMMouthHeight_NEGATIVE_", 100);
        else if (perso.mouth.distanceBetweenChinAndMouth > 0.18 && perso.mouth.distanceBetweenChinAndMouth <= 0.235)
        {
            float valeur = PercentageConvertorNeg(perso.mouth.distanceBetweenChinAndMouth, 0.18f, 0.235f, 0, 100);
            avatarManager.SetBlendshapeValue("PHMMouthHeight_NEGATIVE_", valeur);
        }        
        else
        {
            float valeur = PercentageConvertor(perso.mouth.distanceBetweenChinAndMouth, 0.235f, 0.29f, 0, 100);
            avatarManager.SetBlendshapeValue("PHMMouthHeight", valeur);
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
            case Taille.Little:
                float valeur_little = PercentageConvertorNeg(perso.mouth.mouthWidth, 0.34f, 0.40f, 0, 100);
                avatarManager.SetBlendshapeValue("PHMMouthWidth_NEGATIVE_", valeur_little);
                break;

            case Taille.Big:
                float valeur_big = PercentageConvertor(perso.mouth.mouthWidth, 0.40f, 0.46f, 0, 100);
                avatarManager.SetBlendshapeValue("PHMMouthWidth", valeur_big);
                break;
        }
    }

    public void ChangeEyes()
    {
        //// En fonction de distanceMiddleSourcilCenterEye
        //avatarManager.SetBlendshapeValue("PHMEyesHeight", 100);
        //avatarManager.SetBlendshapeValue("PHMEyesHeight_NEGATIVE_", 100);

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
            case Taille.Little:
                float valeur_little = PercentageConvertorNeg(perso.eye.eyeWidth, 0.18f, 0.22f, 0, 50);
                avatarManager.SetBlendshapeValue("PHMEyesSize_NEGATIVE_", valeur_little);
                break;

            case Taille.Big:
                float valeur_big = PercentageConvertor(perso.eye.eyeWidth, 0.22f, 0.26f, 0, 50);
                avatarManager.SetBlendshapeValue("PHMEyesSize", valeur_big);
                break;
        }


        //// En fonction de distanceBetweenNoseTopAndEyes        
        //avatarManager.SetBlendshapeValue("PHMEyesWidth", 100);
        //avatarManager.SetBlendshapeValue("PHMEyesWidth_NEGATIVE_", 100);
    }

    public void ChangeSkinTexture(bool isWhite)
    {
        if(isWhite && perso.gender == Gender.Male)
        {
            avatarManager.GetHairMaterial().mainTexture = maleWhiteHeadSkinTexture;
            avatarManager.GetBodyMaterial().mainTexture = maleWhiteBodySkinTexture;
        }
        else if(isWhite && perso.gender == Gender.Femelle)
        {
            avatarManager.GetHairMaterial().mainTexture = femelleWhiteHeadSkinTexture;
            avatarManager.GetBodyMaterial().mainTexture = femelleWhiteBodySkinTexture;
        }
        else if(!isWhite && perso.gender == Gender.Male)
        {
            avatarManager.GetHairMaterial().mainTexture = maleBlackHeadSkinTexture;
            avatarManager.GetBodyMaterial().mainTexture = maleBlackBodySkinTexture;
        }
        else if(!isWhite && perso.gender == Gender.Femelle)
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

    }

    public void SetHair(bool init)
    {
        if(perso.hair.isHairless || init)
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
                if(hair.name == "FunkyHair")
                {
                    hair.SetVisibility(true);
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

    public static void MakeAvatarSpeak(MORPH3D.M3DCharacterManager avatar, bool mouthUp, float timeCount, float speechSpeed)
    {
        if(timeCount > speechSpeed)
        {
            timeCount = 0;
            if(mouthUp)
            {
                avatar.SetBlendshapeValue("eCTRLvAA", 100);
            }
            else
            {
                avatar.SetBlendshapeValue("eCTRLvAA", 0);
            }
            mouthUp = !mouthUp;
        }
        else
        {
            float value = 0;
            foreach (MORPH3D.FOUNDATIONS.Morph m in avatar.coreMorphs.morphs)
            {
                if(m.name == "eCTRLvAA")
                {
                    value = m.value;
                }
            }
            if(mouthUp)
            {
                value += PercentageConvertor(Time.deltaTime, 0, speechSpeed, 0, 100);
                avatar.SetBlendshapeValue("eCTRLvAA", value);
            }
            else
            {
                value -= PercentageConvertor(Time.deltaTime, 0, speechSpeed, 0, 100);
                avatar.SetBlendshapeValue("eCTRLvAA", value);
            }
            

        }
    }
}
