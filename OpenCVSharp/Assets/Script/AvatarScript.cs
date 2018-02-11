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
        public float bigAngleNoseTopTip;
        public float littleAngleNoseTopTip;
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

    public struct Visage
    {
        public float cornerChinWidth;
        public float distanceButtomCurve;      
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
        public Visage visage;
    }

    public static Personnage avatar1;
    //public Personnage Avatar1
    //{
    //    get { return avatar1; }
    //}
    public static Personnage avatar2;
    //public Personnage Avatar2
    //{
    //    get { return avatar1; }
    //}
    public static Personnage avatar3;
    //public Personnage Avatar3
    //{
    //    get { return avatar1; }
    //}
    public static Personnage avatarDefinitif;

    public static int avatarSelectionId = 1;

    private static int avatarGenerateNumber = 3;
    public static int AvatarGenerateNumber
    {
        get { return avatarGenerateNumber; }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
