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
    private int colorSampleListSize = 20;

    private Vec3f[] skinColorSampleYCbCr;
    private Vec3f skinColorYCbCrExpectancy;
    private Vec3f skinColorYCbCrThresholds;

    private Vec3f[] hairColorSampleYCbCr;
    private Vec3f hairColorYCbCrExpectancy;
    private Vec3f hairColorYCbCrThresholds;

    private int yHairRoot;
    private int yHairTop;
    private int yMaxHair;
    private FaceDetectionImage faceDetectionImage;

    //Il me faut l'accès à l'image, ainsi que les coordonnées des joues et du front + les landmarks des coins des yeux et du menton
    // Use this for initialization
    void Start () {
        skinColorSampleYCbCr = new Vec3f[colorSampleListSize];
        hairColorSampleYCbCr = new Vec3f[colorSampleListSize];
        faceDetectionImage = GetComponent<FaceDetectionImage>();
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
        //On va chercher la partie supérieure des cheveux
        FindHairTop();
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
        //>>>Récupérations d'échantillons de couleur des cheveux
        //Récupérer un échantillon de couleur des cheveux

        //Le transformer en YCbCr

        //>>>Calcul de l'espérance hairColorYCbCrExpectancy

        //>>>Calcul des Thresholds hairColorYCbCrThresholds
    }

    void FindHairTop()
    {
        //On part de yRoot
        //Tant qu'on ne trouve pas 4 pixels blancs d'affilée (condition à redéfinir précisement), on remonte vers le haut (donc on varie le y)
        //Dés qu'on valide la condition, on s'arrête et on set le yHairTop à y+4 (puisqu'on redescend)
    }

    void FindHairYMax()
    {
        //On part de yRoot

        //On va balayer l'image vers le bas à la recherche de cheveux -> condition = on trouve au moins 4 pixels correspondant à des cheveux pour une ligne (ou bien on met une autre condition)s

        //Tant qu'on trouve des cheveux, on continue vers le bas

        //si on ne trouve plus de cheveux, c'est qu'on a trouvé la fin des cheveux, on met à jour yHairMax
    }

    void GuessHairLength()
    {
        //on fait yHairMax - yHairRoot et on compare à la longueur du visage
    }

    //met à jour l'espérance à partir d'un tableau d'échantillons
    void ComputeVec3fExpectancy(Vec3f[] tab, Vec3f expectancy)
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

        expectancy = new Vec3f
        {
            //Y
            Item0 = yExpectancy,
            //Cb
            Item1 = cbExpectancy,
            //Cr
            Item2 = crExpectancy
        };
    }

    //met à jour les tresholds à partir d'un tableau d'échantillons
    void ComputeVec3fThresholds(Vec3f[] tab, Vec3f threshold)
    {
        float yMin = tab[0].Item0;
        float cbMin = tab[0].Item1;
        float crMin = tab[0].Item2;

        float yMax = tab[0].Item0;
        float cbMax = tab[0].Item1;
        float crMax = tab[0].Item2;

        for (int i = 0; i < tab.Length; i++)
        {
            if (tab[i].Item0 < yMin)
            {
                yMin = tab[i].Item0;
            }else if(tab[i].Item0 > yMax)
            {
                yMax = tab[i].Item0;
            }

            if (tab[i].Item1 < cbMin)
            {
                cbMin = tab[i].Item1;
            }
            else if (tab[i].Item1 > cbMax)
            {
                cbMax = tab[i].Item1;
            }

            if (tab[i].Item2 < crMin)
            {
                crMin = tab[i].Item2;
            }
            else if (tab[i].Item2 > crMax)
            {
                crMax = tab[i].Item2;
            }
        }

        threshold = new Vec3f
        {
            Item0 = yMax - yMin,

            Item1 = cbMax - cbMin,

            Item2 = crMax - crMin

        };
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
