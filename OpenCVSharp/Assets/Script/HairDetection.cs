using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;


// Parallel computation support
using Uk.Org.Adcock.Parallel;
using System;
using System.Web;
using System.Runtime.InteropServices;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HairDetection : MonoBehaviour {
    private Vec3f[] skinColorSampleYCbCr;
    private Vec3f skinColorYCbCrExpectancy;
    private Vec3f skinColorYCbCrThresholds;

    private Vec3f[] hairColorSampleYCbCr;
    private Vec3f hairColorYCbCrExpectancy;
    private Vec3f hairColorYCbCrThresholds;

    private int yHairRoot;
    private int yMaxHair;

    //Il me faut l'accès à l'image, ainsi que les coordonnées des joues et du front + les landmarks des coins des yeux et du menton
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void DetectHairAndGuess()
    {
        //Prétraitement à définir, comme par exemple, augmenter le contraste
        Pretraitement();
        //On récupère les infos sur la peau (les infos YCbCr)
        GetSkinColor();
        //Grace à ces infos, on peut déterminer où sont les cheveux
        FindHairRoots();
        //Une fois qu'on a les cheveux du haut, on récupère les infos sur les cheveux (YCbCr)
        GetHairColor();
        //On va chercher le dernier Y où on apperçoit des cheveux
        FindHairYMax();
        //On décide
        GuessHairLength();
    }

    void Pretraitement()
    {
        //Augmentation du contraste ou non
    }

    void GetSkinColor()
    {
        //>>>Récupérations d'échantillons de couleur du front
        //Récupérer un échantillon de couleur du front

        //Le transformer en YCbCr

        //L'ajouter au tableau skinColorSampleYCbCr

        //>>>Peut-être ajouter des échantillons des joues ?

        //>>>Calcul de l'espérance skinColorYCbCrExpectancy

        //>>>Calcul des Thresholds skinColorYCbCrThresholds

    }

    void FindHairRoots()
    {
        //On part du haut du front
            //Tant qu'on ne trouve pas les cheveux (condition à redéfinir précisement), on remonte vers le haut (donc on varie le y)
            //Dés qu'on valide la condition, on s'arrête et on set le yHairRoot
    }

    void GetHairColor()
    {

    }

    void FindHairYMax()
    {

    }

    void GuessHairLength()
    {

    }

    void ComputeVec3fExpectancy(Vec3f[] tab)
    {
        float yExpectancy = 0;
        float cbExpectancy = 0;
        float crExpectancy = 0;
        for(int i=0; i<tab.Length; i++)
        {
            yExpectancy += tab[i].Item0;
            cbExpectancy += tab[i].Item1;
            crExpectancy += tab[i].Item2;
        }

        yExpectancy /= tab.Length;
        cbExpectancy /= tab.Length;
        crExpectancy /= tab.Length;
        // a changer pour les cheveux
        skinColorYCbCrExpectancy = new Vec3f
        {
            //Y
            Item0 = yExpectancy,
            //Cb
            Item1 = cbExpectancy,
            //Cr
            Item2 = crExpectancy
        };
    }

    void ComputeVec3fThresholds(Vec3f[] tab)
    {

    }

    Vec3f FromRGBToYCbCr(Color32 RGB)
    {
        var vec3 = new Vec3f
        {
            //Y
            Item0 = 0.299f* RGB.r +  0.587f *RGB.g + 0.114f*RGB.b,
            //Cb
            Item1 = -0.1687f*RGB.r - 0.3313f*RGB.g + 0.5f*RGB.b+ 128,
            //Cr
            Item2 = 0.5f*RGB.r - 0.4187f*RGB.g - 0.0813f*RGB.b + 128
        };

        return vec3;
    }
}
