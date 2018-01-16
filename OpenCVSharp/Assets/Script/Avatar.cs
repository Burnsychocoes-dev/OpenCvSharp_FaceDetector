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
        public float distanceBetweenCenterLipAndButtomLip;
        public float distanceBetweenCenterLipAndTopLip;
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

    float secondCount = 0f;
    float lipHeightVoiceControl = 0f;
    float lipWidthVoiceControl = 0f;
    bool downCount = false;
    public float speakInterval = 0.5f;

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
        MakeSpeak();
    }

    // Use this for init the personnage 
    public void SetPerso()
    {
        this.transform.Translate(new Vector3(-5, 0, 0));
        avatarManager.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        SetDressed();
        // Partie gender
        if (landmarks.gender == "M")
            perso.gender = Gender.Male;
        else
            perso.gender = Gender.Femelle;

        Debug.Log(hair.yHairRoot);
        Debug.Log(hair.yHairTop);
        Debug.Log(Mathf.Abs(hair.yHairRoot - hair.yHairTop) / landmarks.faceHeight);
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
        perso.eye.eyeWidth = (float)landmarks.rightEyeWidth;

        if (perso.eye.eyeWidth <= 0.14f)
            perso.eye.width = Taille.Little;
        else if (perso.eye.eyeWidth > 0.16 && perso.eye.eyeWidth <= 0.18)
            perso.eye.width = Taille.Middle;
        else
            perso.eye.width = Taille.Big;


        // Partie nose
        perso.nose.noseHeight = (float)landmarks.noseHeight;
        perso.nose.noseWidth = (float)landmarks.noseWidth;
        perso.nose.nostrilThickness = (float)landmarks.nostrilThickness;

        if (perso.nose.noseHeight <= 0.25)
            perso.nose.height = Taille.Little;
        else if (perso.nose.noseHeight > 0.25 && perso.nose.noseHeight <= 0.30)
            perso.nose.height = Taille.Middle;
        else
            perso.nose.height = Taille.Big;

        if (perso.nose.noseWidth <= 0.27)
            perso.nose.width = Taille.Little;
        else if (perso.nose.noseWidth > 0.27 && perso.nose.noseWidth <= 0.31)
            perso.nose.width = Taille.Middle;
        else
            perso.nose.width = Taille.Big;


        // Partie mouth
        perso.mouth.distanceBetweenChinAndMouth = (float)landmarks.distanceBetweenLipAndChin;
        perso.mouth.distanceBetweenNoseTipAndMouth = (float)landmarks.distanceBetweenNoseTipAndLip;

        perso.mouth.distanceBetweenCenterLipAndButtomLip = (float)landmarks.buttomLipHeight;
        perso.mouth.distanceBetweenCenterLipAndTopLip = (float)landmarks.topLipHeight;
        Debug.Log(Math.Abs((float)perso.mouth.distanceBetweenCenterLipAndTopLip / (float)perso.mouth.distanceBetweenCenterLipAndButtomLip));
        if (Math.Abs((float)perso.mouth.distanceBetweenCenterLipAndTopLip / (float)perso.mouth.distanceBetweenCenterLipAndButtomLip) < 0.5)
            perso.mouth.proportionLevre = ProportionLevre.UnPourDeux;
        else if (Math.Abs((float)perso.mouth.distanceBetweenCenterLipAndTopLip / (float)perso.mouth.distanceBetweenCenterLipAndButtomLip) > 2)
            perso.mouth.proportionLevre = ProportionLevre.DeuxPourUn;
        else
            perso.mouth.proportionLevre = ProportionLevre.UnPourUn;
               
        perso.mouth.mouthWidth = (float)landmarks.lipWidth;

        if (perso.mouth.mouthWidth <= 0.59)
            perso.mouth.width = Taille.Little;
        else if (perso.mouth.mouthWidth > 0.64 && perso.mouth.mouthWidth <= 0.69)
            perso.mouth.width = Taille.Middle;
        else
            perso.mouth.width = Taille.Big;

    }


    public void ChangeNose()
    {
        // En fonction de noseHeight
        /*
         * petit : 0.23 < noseHeight < 0.25 -> 33 < neg < 100
         * moyen : 0.25 < noseHeight < 0.3 -> if < 0.29 0 < neg < 33 else 0 < pos < 33
         * grand : 0.3 < noseHeight < 0.33 -> 33 < pos < 100
         */
        switch(perso.nose.height)
        {
            case Taille.Little:
                float valeur_little = PercentageConvertorNeg(perso.nose.noseHeight, 0.23f, 0.25f, 33, 100);
                avatarManager.SetBlendshapeValue("PHMNoseHeight_NEGATIVE_", valeur_little);
                break;

            case Taille.Middle:
                if (perso.nose.noseHeight < 0.275)
                {
                    float valeur_middle = PercentageConvertorNeg(perso.nose.noseHeight, 0.25f, 0.275f, 0, 33);
                    avatarManager.SetBlendshapeValue("PHMNoseHeight_NEGATIVE_", valeur_middle);
                }
                else
                {
                    float valeur_middle = PercentageConvertor(perso.nose.noseHeight, 0.275f, 0.30f, 0, 33);
                    avatarManager.SetBlendshapeValue("PHMNoseHeight", valeur_middle);
                }
                break;

            case Taille.Big:
                float valeur_big = PercentageConvertor(perso.nose.noseHeight, 0.30f, 0.33f, 33, 100);
                avatarManager.SetBlendshapeValue("PHMNoseHeight", valeur_big);
                break;
        }

        // En fonction de noseWidth
        /*
         * petit : 0.24 < noseWidth < 0.27 -> 33 < neg < 100
         * moyen : 0.27 < noseWidth < 0.31 -> if < 0.29 0 < neg < 33 else 0 < pos < 33
         * grand : 0.31 < noseWidth < 0.34 -> 33 < pos < 100
         */
        switch (perso.nose.width)
        {
            case Taille.Little:
                float valeur_little = PercentageConvertorNeg(perso.nose.noseWidth, 0.24f, 0.27f, 33, 100);
                avatarManager.SetBlendshapeValue("PHMNoseWidth_NEGATIVE_", valeur_little);
                break;

            case Taille.Middle:
                if (perso.nose.noseWidth < 0.29)
                {
                    float valeur_middle = PercentageConvertorNeg(perso.nose.noseWidth, 0.27f, 0.29f, 0, 33);
                    avatarManager.SetBlendshapeValue("PHMNoseWidth_NEGATIVE_", valeur_middle);
                }
                else
                {
                    float valeur_middle = PercentageConvertor(perso.nose.noseWidth, 0.29f, 0.31f, 0, 33);
                    avatarManager.SetBlendshapeValue("PHMNoseWidth", valeur_middle);
                }
                break;

            case Taille.Big:
                float valeur_big = PercentageConvertor(perso.nose.noseWidth, 0.31f, 0.34f, 33, 100);
                avatarManager.SetBlendshapeValue("PHMNoseWidth", valeur_big);
                break;
        }


        // En fonction de nostrilThickness
        //avatarManager.SetBlendshapeValue("PHMNostrilWingWidth", 100);
        //avatarManager.SetBlendshapeValue("PHMNostrilWingWidth_NEGATIVE_", 100);
    }

    public void ChangeMouth()
    {
        switch(perso.mouth.proportionLevre)
        {
            case ProportionLevre.DeuxPourUn:
                avatarManager.SetBlendshapeValue("PHMLipLowerSize_NEGATIVE_", 100);
                avatarManager.SetBlendshapeValue("PHMLipUpperSize", 100);
                break;

            case ProportionLevre.UnPourUn:
                avatarManager.SetBlendshapeValue("PHMLipLowerSize_NEGATIVE_", 50);
                avatarManager.SetBlendshapeValue("PHMLipUpperSize", 100);
                break;

            case ProportionLevre.UnPourDeux:
                avatarManager.SetBlendshapeValue("PHMLipUpperSize", 50);
                break;
        }
        //// En fonction de distanceBetweenCenterLipAndButtomLip
        //avatarManager.SetBlendshapeValue("PHMLipLowerSize", 100);
        //avatarManager.SetBlendshapeValue("PHMLipLowerSize_NEGATIVE_", 100);

        //// En fonction de distanceBetweenCenterLipAndTopLip
        //avatarManager.SetBlendshapeValue("PHMLipUpperSize", 100);
        //avatarManager.SetBlendshapeValue("PHMLipUpperSize_NEGATIVE_", 100);

        //// En fonction de distanceBetweenChinAndMouth and distanceBetweenNoseTipAndMouth
        //avatarManager.SetBlendshapeValue("PHMMouthHeight", 100);
        //avatarManager.SetBlendshapeValue("PHMMouthHeight_NEGATIVE_", 100);

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
        switch (perso.mouth.width)
        {
            case Taille.Little:
                float valeur_little = PercentageConvertorNeg(perso.mouth.mouthWidth, 0.59f, 0.64f, 33, 100);
                avatarManager.SetBlendshapeValue("PHMMouthWidth_NEGATIVE_", valeur_little);
                break;

            case Taille.Middle:
                if (perso.mouth.mouthWidth < 0.41)
                {
                    float valeur_middle = PercentageConvertorNeg(perso.mouth.mouthWidth, 0.64f, 0.665f, 0, 33);
                    avatarManager.SetBlendshapeValue("PHMMouthWidth_NEGATIVE_", valeur_middle);
                }
                else
                {
                    float valeur_middle = PercentageConvertor(perso.mouth.mouthWidth, 0.665f, 0.69f, 0, 33);
                    avatarManager.SetBlendshapeValue("PHMMouthWidth", valeur_middle);
                }
                break;

            case Taille.Big:
                float valeur_big = PercentageConvertor(perso.mouth.mouthWidth, 0.69f, 0.74f, 33, 100);
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
         * petit : 0.14 < eyeWidth < 0.16 -> 33 < neg < 100
         * moyen : 0.16 < eyeWidth < 0.20 -> if < 0.23 -> 0 < neg < 33 else 0 < pos < 33
         * grand : 0.20 < eyeWidth < 0.24 -> 33 < pos < 100
         */

        switch (perso.eye.width)
        {
            case Taille.Little:
                float valeur_little = PercentageConvertorNeg(perso.eye.eyeWidth, 0.14f, 0.16f, 33, 100);
                avatarManager.SetBlendshapeValue("PHMEyesSize_NEGATIVE_", valeur_little);
                break;

            case Taille.Middle:
                if (perso.eye.eyeWidth < 0.18)
                {
                    float valeur_middle = PercentageConvertorNeg(perso.eye.eyeWidth, 0.16f, 0.18f, 0, 33);
                    avatarManager.SetBlendshapeValue("PHMEyesSize_NEGATIVE_", valeur_middle);
                }
                else
                {
                    float valeur_middle = PercentageConvertor(perso.eye.eyeWidth, 0.18f, 0.20f, 0, 33);
                    avatarManager.SetBlendshapeValue("PHMEyesSize", valeur_middle);
                }
                break;

            case Taille.Big:
                float valeur_big = PercentageConvertor(perso.eye.eyeWidth, 0.20f, 0.24f, 33, 100);
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
        Color color = new Color((float)perso.exactSkinColor.r / 255, (float)perso.exactSkinColor.g / 255, (float)perso.exactSkinColor.b / 255);
        avatarManager.GetBodyMaterial().SetColor("_Color", color);
        avatarManager.GetHairMaterial().SetColor("_Color", color);
    }


    public void MakeSpeak()
    {
        if(secondCount >= speakInterval)
        {
            secondCount = speakInterval - Time.deltaTime;
            lipHeightVoiceControl = 100f;
            lipWidthVoiceControl = 100f;
            downCount = true;
        }
        else if(secondCount <= 0.0f)
        {
            secondCount = 0f + Time.deltaTime;
            lipHeightVoiceControl = 0f;
            lipWidthVoiceControl = 0f;
            downCount = false;
        }
        else
        {
            if(downCount)
            {
                lipHeightVoiceControl -= 100 * Time.deltaTime;
                lipWidthVoiceControl -= 100 * Time.deltaTime;
                secondCount -= Time.deltaTime;
            }
            else
            {
                lipHeightVoiceControl += 100 * Time.deltaTime;
                lipWidthVoiceControl += 100 * Time.deltaTime;
                secondCount += Time.deltaTime;
            }
        }
        //Debug.Log(downCount);
        //Debug.Log(Time.deltaTime);
        //Debug.Log(secondCount);
        //Debug.Log(lipHeightVoiceControl);
        //Debug.Log(lipWidthVoiceControl);
        avatarManager.SetBlendshapeValue("eCTRLvAA", lipHeightVoiceControl);
        avatarManager.SetBlendshapeValue("eCTRLvT", lipWidthVoiceControl);
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
                hair.SetVisibility(true);
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
    public float PercentageConvertorNeg(float vToConvert, float srcIntervalMin, float srcIntervalMax, float destIntervalMin, float destIntervalMax)
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
