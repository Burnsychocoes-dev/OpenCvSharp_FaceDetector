using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarScript : MonoBehaviour {

    public enum Taille
    {
        Big,
        Middle,
        Little
    }

    public enum NoseTipInclinaison
    {
        NezRemonte,
        NezAbaisse,
        NezNormal
    }

    public enum NoseTipType
    {
        NezRond,
        NezPointue
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

    public enum Haircut
    {
        Chauve,
        BoldHair,
        CasualLongHair,
        DrifterHair,
        FunkyHair,
        JakeHair,
        KamiHair,
        ScottHair,
        MicahMaleHair,
        KungFuHair,
        MicahFemaleHair,
        ToulouseHair,
        NordicHair,
        FashionHair,
        RangerHair,
        KeikoPonytailHair,
        SparkleHair,
        ShortPonytailHair,
        SultryHair,
        RevHair
    }

    public struct Eye
    {
        public float eyeWidth;
        public float distanceBetweenNoseTopAndEyes;
        public float distanceBrowEye;
        public Taille width;
    }

    public struct Nose
    {
        public float noseHeight;
        public float noseWidth;
        public float noseTipHeight;
        public Taille width;
        public Taille height;
        public NoseTipInclinaison noseTipInclinaison;
        public NoseTipType noseTipType;
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

    public struct Visage
    {
        public float cornerChinWidth;
        public float distanceButtomCurve;      
    }


    public struct Personnage
    {
        public Gender gender;
        public SkinColor skinColor;
        public Color32 exactSkinColor;
        public Eye eye;
        public Nose nose;
        public Mouth mouth;
        public Haircut haircut;
        public Visage visage;
    }

    public static Personnage avatar1;
 
    public static Personnage avatar2;

    public static Personnage avatar3;

    public static Personnage avatar4;

    public static Personnage avatar5;
 
    public static Personnage avatarDefinitif;
    

    public static int avatarSelectionId = 1;

    private static int avatarGenerateNumber = 5;
    public static int AvatarGenerateNumber
    {
        get { return avatarGenerateNumber; }
    }

    public static int avatarHaircutSelectionId = 1;
    private static int avatarHaircutAvailableNumber = 10;
    public static int AvatarHaircutAvailableNumber
    {
        get { return avatarHaircutAvailableNumber; }
    }


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
