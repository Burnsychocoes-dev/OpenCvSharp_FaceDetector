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
    private int colorSampleListSize = 300;

    private Vec3f[] skinColorSampleYCbCr;    
    private Vec3f skinColorYCbCrExpectancy;
    private float skinColorCbCrThreshold;

    private Vec3f[] hairColorSampleYCbCr;
    private Vec3f hairColorYCbCrExpectancy;
    private float hairColorCbCrThreshold;

    private int yHairRoot =-1;
    //yHairTop représente le plus haut point de la tête
    private int yHairTop =-1;
    private int yHairMax;
    private int hairHeight;

    private FaceDetectionImage faceDetectionImage;
    private LandmarksRetriever landMarksRetriever;

    private Mat matrix2_grabcut;
    public Mat Matrix2_grabcut
    {
        get { return matrix2_grabcut; }
    }

    //Il me faut l'accès à l'image, ainsi que les coordonnées des joues et du front + les landmarks des coins des yeux et du menton
    // Use this for initialization
    void Start () {
        skinColorSampleYCbCr = new Vec3f[colorSampleListSize];
        hairColorSampleYCbCr = new Vec3f[colorSampleListSize];
        faceDetectionImage = GetComponent<FaceDetectionImage>();
        landMarksRetriever = GetComponent<LandmarksRetriever>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void DetectHairAndGuess()
    {
        //Prétraitement à définir, comme par exemple, augmenter le contraste
        Debug.Log("Pretreating");
        Pretraitement();
        //On récupère les infos sur la peau (les infos YCbCr)
        Debug.Log("Getting Skin Color");
        GetSkinColor();
        //Grace à ces infos, on peut déterminer où sont les cheveux
        Debug.Log("Finding Hair YRoots");
        FindHairRoots();
        //On va chercher la partie supérieure des cheveux
        Debug.Log("Finding Hair YTop");
        FindHairTop();
        //Une fois qu'on a les cheveux du haut, on récupère les infos sur les cheveux (YCbCr)
        Debug.Log("Getting Hair Color");
        GetHairColor();        
        //On va clear la partie peau, on peut tout enlever à la place si on veut
        Debug.Log("Clearing Skin");
        ClearSkin();
        //On va clear tout ce qui n'est pas cheveu
        Debug.Log("Clearing Non Hair");
        ClearNonHair();
        //On va chercher le dernier Y où on apperçoit des cheveux
        Debug.Log("Finding Hair YMax");
        FindHairYMax();
        //On décide
        Debug.Log("Guessing Hair Length");
        GuessHairLength();

        Debug.Log("Guessing Hair Height");
        GuessHairHeight();
    }

    public void Pretraitement()
    {
        Debug.Log("Pretreat");
        //Augmentation du contraste ou non
        Debug.Log("GrabCutting");
        GrabCut();
    }

    void GrabCut()
    {
        Debug.Log("GrabCut");
        Mat result = faceDetectionImage.VideoSourceImage;
        Mat bgModel = new Mat(); //background model
        Mat fgModel = new Mat(); //foreground model

        //draw a rectangle 
        OpenCvSharp.Rect rectangle = new OpenCvSharp.Rect(1, 1, faceDetectionImage.VideoSourceImage.Cols - 1, faceDetectionImage.VideoSourceImage.Rows - 1);
        Cv2.GrabCut(faceDetectionImage.VideoSourceImage, result, rectangle, bgModel, fgModel, 10, GrabCutModes.InitWithRect);
        Cv2.Compare(result, new Scalar(3, 3, 3), result,CmpTypes.EQ);
        matrix2_grabcut = new Mat(faceDetectionImage.VideoSourceImage.Size(),MatType.CV_8UC3, new Scalar(255, 255, 255));
        faceDetectionImage.VideoSourceImage.CopyTo(matrix2_grabcut, result);
        

    }

    void GetSkinColor()
    {
        Debug.Log("Get Skin Color");
        int skinColorCounter = 0;
        //you can Pick if it's 0
        int youCanPick = 0;
        //On va pick tous les 10 pixels
        int youCanPickEveryXPixels = 10;
        for(var i=0; i < faceDetectionImage.ImHeight; i++)
        {
            for (var j = 0; j < faceDetectionImage.ImWidth; j++)
            {
                float coordX = j;
                float coordY = faceDetectionImage.ImHeight - i;
                //Vec3b vec = faceDetectionImage.VideoSourceImageData[j + i * faceDetectionImage.ImWidth];
                Vec3b vec = matrix2_grabcut.At<Vec3b>(i * faceDetectionImage.ImWidth, j);
                //>>>Récupérations d'échantillons de couleur du front
                if (coordX > faceDetectionImage.RectFront.X && coordX < faceDetectionImage.RectFront.X + faceDetectionImage.RectFront.Width &&
                    coordY > faceDetectionImage.RectFront.Y && coordY < faceDetectionImage.RectFront.Y + faceDetectionImage.RectFront.Height && youCanPick == 0 && skinColorCounter < colorSampleListSize/2)
                {
                    //Récupérer un échantillon de couleur du front
                    Color32 color = new Color32
                    {
                        r = vec.Item2,
                        g = vec.Item1,
                        b = vec.Item0
                    };
                    //Le transformer en YCbCr
                    //L'ajouter au tableau skinColorSampleYCbCr
                    skinColorSampleYCbCr[skinColorCounter] = FromRGBToYCbCr(color);
                    skinColorCounter++;
                } 
                //>>>Peut-être ajouter des échantillons des joues ?
                else if (coordX > faceDetectionImage.RectEyeLeft.X && coordX < faceDetectionImage.RectEyeLeft.X + faceDetectionImage.RectEyeLeft.Width &&
                   coordY > faceDetectionImage.RectEyeLeft.Y && coordY < faceDetectionImage.RectEyeLeft.Y + faceDetectionImage.RectEyeLeft.Height && youCanPick == 0 && skinColorCounter < colorSampleListSize )
                {
                    Color32 color = new Color32
                    {
                        r = vec.Item2,
                        g = vec.Item1,
                        b = vec.Item0
                    };
                    //Le transformer en YCbCr
                    //L'ajouter au tableau skinColorSampleYCbCr
                    skinColorSampleYCbCr[skinColorCounter] = FromRGBToYCbCr(color);
                    skinColorCounter++;
                }
                else if (coordX > faceDetectionImage.RectEyeRight.X && coordX < faceDetectionImage.RectEyeRight.X + faceDetectionImage.RectEyeRight.Width &&
                   coordY > faceDetectionImage.RectEyeRight.Y && coordY < faceDetectionImage.RectEyeRight.Y + faceDetectionImage.RectEyeRight.Height && youCanPick == 0 && skinColorCounter < colorSampleListSize)
                {
                    Color32 color = new Color32
                    {
                        r = vec.Item2,
                        g = vec.Item1,
                        b = vec.Item0
                    };
                    //Le transformer en YCbCr
                    //L'ajouter au tableau skinColorSampleYCbCr
                    skinColorSampleYCbCr[skinColorCounter] = FromRGBToYCbCr(color);
                    skinColorCounter++;
                }


                youCanPick = (youCanPick + 1) % youCanPickEveryXPixels;
            }
        }


        //>>>Calcul de l'espérance skinColorYCbCrExpectancy
        ComputeVec3fExpectancy(skinColorSampleYCbCr, skinColorYCbCrExpectancy);
        Debug.Log("Skin Color YCbCrExpectancy");
        Debug.Log(skinColorYCbCrExpectancy);

        //>>>Calcul des Thresholds skinColorYCbCrThresholds
        ComputeVec3fThresholds(skinColorSampleYCbCr, skinColorCbCrThreshold,skinColorYCbCrExpectancy);
        Debug.Log("Skin Color YCbCrThresholds");
        Debug.Log(skinColorCbCrThreshold);

    }

    void FindHairRoots()
    {
        Debug.Log("Find Hair Roots");
        //On part du haut du front
        int j = faceDetectionImage.RectFront.X+faceDetectionImage.RectFront.Width/2;
        for(var i = faceDetectionImage.RectFront.Y; i>0; i--)
        {
            //Vec3b vec = faceDetectionImage.VideoSourceImageData[j + i * faceDetectionImage.ImWidth];
            Vec3b vec = matrix2_grabcut.At<Vec3b>(i * faceDetectionImage.ImWidth, j);
            Color32 color = new Color32
            {
                r = vec.Item2,
                g = vec.Item1,
                b = vec.Item0
            };
            //Si on est sur un pixel blanc, on s'arrête
            if (color.r == 255 && color.g == 255 && color.b == 255)
            {
                //on est arrivé au haut du crane
                yHairTop = i;
                Debug.Log("No Hair roots found, Top found at y : ");
                Debug.Log(yHairTop);
                return;
            }

            Vec3f sample = FromRGBToYCbCr(color);
            //Tant qu'on ne trouve pas les cheveux (condition à redéfinir précisement), on remonte vers le haut (donc on varie le y)
            if (EuclidianDistance(sample.Item1,sample.Item2,skinColorYCbCrExpectancy)> skinColorCbCrThreshold)
            {
                yHairRoot = i;
                Debug.Log("Hair roots found, y : ");
                Debug.Log(yHairRoot);
                return;
            }
            //Dés qu'on valide la condition, on s'arrête et on set le yHairRoot
        }

    }

    void GetHairColor()
    {
        Debug.Log("Get Hair Color");

        if (yHairRoot == -1)
        {
            Debug.Log("No hair here");
            return;
        }

        int hairColorCounter = 0;
        //you can Pick if it's 0
        int youCanPick = 0;
        //On va pick tous les X pixels
        int youCanPickEveryXPixels = 10;

        for (var i = yHairRoot; i > 0; i--)
        {
            //Pour ne pas prendre des pixels en dehors de la tête
            for (var j = faceDetectionImage.Face.X+faceDetectionImage.Face.Width/4; j < faceDetectionImage.Face.X + 3*faceDetectionImage.Face.Width / 4; j++)
            {
                //Vec3b vec = faceDetectionImage.VideoSourceImageData[j + i * faceDetectionImage.ImWidth];
                Vec3b vec = matrix2_grabcut.At<Vec3b>(i * faceDetectionImage.ImWidth, j);
                Color32 color = new Color32
                {
                    r = vec.Item2,
                    g = vec.Item1,
                    b = vec.Item0
                };
                //>>>Récupérations d'échantillons de couleur des cheveux
                if(youCanPick==0 && hairColorCounter < colorSampleListSize)
                {
                    //Récupérer un échantillon de couleur des cheveux
                    //Le transformer en YCbCr
                    hairColorSampleYCbCr[hairColorCounter] = FromRGBToYCbCr(color);
                    hairColorCounter++;
                }                              
                youCanPick = (youCanPick + 1) % youCanPickEveryXPixels;
            }
        }
        //>>>Calcul de l'espérance hairColorYCbCrExpectancy
        ComputeVec3fExpectancy(hairColorSampleYCbCr, hairColorYCbCrExpectancy);
        Debug.Log("Hair Color YCbCrExpectancy");
        Debug.Log(hairColorYCbCrExpectancy);

        //>>>Calcul des Thresholds hairColorYCbCrThresholds
        ComputeVec3fThresholds(hairColorSampleYCbCr, hairColorCbCrThreshold,hairColorYCbCrExpectancy);
        Debug.Log("Hair Color YCbCrThresholds");
        Debug.Log(hairColorCbCrThreshold);
    }

    void FindHairTop()
    {
        Debug.Log("Find Hair Top");
        if (yHairTop != -1)
        {
            Debug.Log("No hair here ");
            return;
        }
        int nbOfPixelBlancThreshold = 4;
        int pixelBlancCounter = 0;
        int lastPixelBlancY = -1;
        int j = faceDetectionImage.RectFront.X + faceDetectionImage.RectFront.Width / 2;
        //On part de yRoot
        for (var i = yHairRoot; i > 0; i--)
        {
            //Vec3b vec = faceDetectionImage.VideoSourceImageData[j + i * faceDetectionImage.ImWidth];
            Vec3b vec = matrix2_grabcut.At<Vec3b>(i * faceDetectionImage.ImWidth, j);
            Color32 color = new Color32
            {
                r = vec.Item2,
                g = vec.Item1,
                b = vec.Item0
            };
            //Tant qu'on ne trouve pas 4 pixels blancs d'affilée (condition à redéfinir précisement), on remonte vers le haut (donc on varie le y)
            //Dés qu'on valide la condition, on s'arrête et on set le yHairTop à y+4 (puisqu'on redescend)
            if (color.r == 255 && color.g == 255 && color.b == 255)
            {
                //On vérifie si on a bien une suite de pixels blancs
                if(lastPixelBlancY == -1)
                {
                    pixelBlancCounter++;
                    lastPixelBlancY = i;
                }
                else if(lastPixelBlancY == i+1)
                {
                    pixelBlancCounter++;
                    lastPixelBlancY = i;
                }
                else
                {
                    lastPixelBlancY = -1;
                    pixelBlancCounter = 0;
                }
                
                if(pixelBlancCounter == nbOfPixelBlancThreshold)
                {
                    //on est arrivé au haut du crane
                    yHairTop = i + nbOfPixelBlancThreshold;
                    Debug.Log("Hair top found, y : ");
                    Debug.Log(yHairTop);
                    return;
                }
                
            }
        }
        
    }

    void ClearSkin()
    {
        Debug.Log("Clear Skin");
        //Si on clear juste le skin
        for (var i = 0; i < faceDetectionImage.ImHeight; i++)
        {
            for (var j = 0; j < faceDetectionImage.ImWidth; j++)
            {
                //Vec3b vec = faceDetectionImage.VideoSourceImageData[j + i * faceDetectionImage.ImWidth];
                Vec3b vec = matrix2_grabcut.At<Vec3b>(i * faceDetectionImage.ImWidth, j);
                Color32 color = new Color32
                {
                    r = vec.Item2,
                    g = vec.Item1,
                    b = vec.Item0
                };

                Vec3f sample = FromRGBToYCbCr(color);
                //Si c'est de la peau, on enlève
                if (EuclidianDistance(sample.Item1, sample.Item2, skinColorYCbCrExpectancy) < skinColorCbCrThreshold)
                {
                    //faceDetectionImage.VideoSourceImageData[j + i * faceDetectionImage.ImWidth] = new Vec3b
                    //{
                    //    Item0 = 255,
                    //    Item1 = 255,
                    //    Item2 = 255
                    //};

                    matrix2_grabcut.Set<Vec3b>(i * faceDetectionImage.ImWidth + j, new Vec3b
                    {
                        Item0 = 255,
                        Item1 = 255,
                        Item2 = 255
                    });
                }
            }
        }



    }

    void ClearNonHair()
    {
        Debug.Log("Clear Non Hair");
        //Si on clear juste le skin
        for (var i = 0; i < faceDetectionImage.ImHeight; i++)
        {
            for (var j = 0; j < faceDetectionImage.ImWidth; j++)
            {
                //Vec3b vec = faceDetectionImage.VideoSourceImageData[j + i * faceDetectionImage.ImWidth];
                Vec3b vec = matrix2_grabcut.At<Vec3b>(i * faceDetectionImage.ImWidth, j);
                Color32 color = new Color32
                {
                    r = vec.Item2,
                    g = vec.Item1,
                    b = vec.Item0
                };

                Vec3f sample = FromRGBToYCbCr(color);
                //Si ce n'est pas des cheveux
                if (EuclidianDistance(sample.Item1, sample.Item2, hairColorYCbCrExpectancy) > hairColorCbCrThreshold)
                {
                    //faceDetectionImage.VideoSourceImageData[j + i * faceDetectionImage.ImWidth] = new Vec3b
                    //{
                    //    Item0 = 255,
                    //    Item1 = 255,
                    //    Item2 = 255
                    //};
                    matrix2_grabcut.Set<Vec3b>(i * faceDetectionImage.ImWidth + j,new Vec3b
                    {
                        Item0 = 255,
                        Item1 = 255,
                        Item2 = 255
                    });
                }
            }
        }
    }

    void ClearFace()
    {
        Debug.Log("Clear Face");
        //Si on veut clear toute la face, donc les yeux et les moustaches avec
        //On part du yHairRoot jusqu'au yMenton
        for (var i = yHairRoot; i < landMarksRetriever.Chin.Item1; i++)
        {
            //On part du xMinLeftEye jusqu'au xMaxRightEye
            for (var j = (int)(landMarksRetriever.LeftEyeBrowLeft.Item0); j < landMarksRetriever.RightEyeBrowRight.Item0; j++)
            {
                //On met en blanc tout ce qu'il y a dedans
                //faceDetectionImage.VideoSourceImageData[j + i * faceDetectionImage.ImWidth] = new Vec3b
                //{
                //    Item0 = 255,
                //    Item1 = 255,
                //    Item2 = 255
                //};

                matrix2_grabcut.Set<Vec3b>(i * faceDetectionImage.ImWidth + j, new Vec3b
                {
                    Item0 = 255,
                    Item1 = 255,
                    Item2 = 255
                });
            }
        }
    }

    void FindHairYMax()
    {
        Debug.Log("Find Hair Y Max");

        int nbOfPixelNonHairThreshold = 4;
        int pixelNonHairCounter = 0;
        int lastPixelNonHairY = -1;
        int lastPixelHairY = -1;

        //On part de yRoot
        for (var i = yHairRoot; i < faceDetectionImage.ImHeight; i++)
        {
            //Pour ne pas prendre des pixels en dehors de la tête
            for (var j = 0; j < faceDetectionImage.ImWidth; j++)
            {

                //On va balayer l'image vers le bas à la recherche de cheveux -> condition = on trouve au moins 4 pixels correspondant à des cheveux pour une ligne (ou bien on met une autre condition)s
                //Vec3b vec = faceDetectionImage.VideoSourceImageData[j + i * faceDetectionImage.ImWidth];
                Vec3b vec = matrix2_grabcut.At<Vec3b>(i * faceDetectionImage.ImWidth, j);
                Color32 color = new Color32
                {
                    r = vec.Item2,
                    g = vec.Item1,
                    b = vec.Item0
                };
                Vec3f sample = FromRGBToYCbCr(color);
                //Tant qu'on trouve des cheveux, on continue vers le bas
                if (EuclidianDistance(sample.Item1, sample.Item2, hairColorYCbCrExpectancy) < hairColorCbCrThreshold)
                {
                    lastPixelHairY = i;
                }
                
            }
            //Si il n'y a pas eu de cheveu
            if(lastPixelHairY != i)
            {
                if (lastPixelNonHairY == -1)
                {
                    lastPixelNonHairY = i;
                    pixelNonHairCounter++;
                }
                //Si le dernier endroit où il n'y a pas de cheveux était juste au dessus
                else if(lastPixelNonHairY == i - 1)
                {
                    lastPixelNonHairY = i;
                    pixelNonHairCounter++;
                }
                else
                {
                    lastPixelNonHairY = -1;
                    pixelNonHairCounter= 0;
                }

                if (pixelNonHairCounter == nbOfPixelNonHairThreshold)
                {
                    //si on ne trouve plus de cheveux sur threshold pixels, c'est qu'on a trouvé la fin des cheveux, on met à jour yHairMax
                    yHairMax = i - nbOfPixelNonHairThreshold;
                    Debug.Log("Hair max found, y : ");
                    Debug.Log(yHairMax);
                    return;
                }
            }

        }
    }

    void GuessHairLength()
    {
        Debug.Log("Guess Hair Length");
        //on fait yHairMax - yHairRoot et on compare à la longueur du visage
        if(yHairRoot == -1)
        {
            Debug.Log("Cette personne est chauve !");
            return;
        }

        //On va comparer yHairMax par rapport aux landmarks : le nez et le menton
        if (yHairMax <= landMarksRetriever.Nose.Item1)
        {
            Debug.Log("Cette personne a les cheveux court !");
            return;
        } else if (yHairMax >= landMarksRetriever.Chin.Item1)
        {
            Debug.Log("Cette personne a les cheveux longs !");
            return;
        }
        else
        {
            Debug.Log("Cette personne a les cheveux moyens !");
            return;
        }
        
    }

    void GuessHairHeight()
    {
        Debug.Log("Guess Hair Height");
        hairHeight = yHairRoot - yHairTop;
        if (hairHeight >= faceDetectionImage.Face.Height / 3)
        {
            Debug.Log("Cette personne a les cheveux épais !");
            return;
        }else if(hairHeight <= faceDetectionImage.Face.Height / 6)
        {
            Debug.Log("Cette personne a les cheveux non épais !");
            return;
        }
        else
        {
            Debug.Log("Cette personne a les cheveux moyen épais !");
            return;
        }
    }

    //met à jour l'espérance à partir d'un tableau d'échantillons
    void ComputeVec3fExpectancy(Vec3f[] tab, Vec3f expectancy)
    {
        Debug.Log("Compute Expectancy");
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

    //met à jour les tresholds à partir d'un tableau d'échantillons
    void ComputeVec3fThresholds(Vec3f[] tab, float threshold, Vec3f expectancy)
    {
        Debug.Log("Compute Thresholds");
        float dmin = Mathf.Sqrt(Mathf.Pow(tab[0].Item1 - expectancy.Item1, 2) + Mathf.Pow(tab[0].Item2 - expectancy.Item2, 2));
        

        for (int i = 0; i < tab.Length; i++)
        {
            float d = Mathf.Sqrt(Mathf.Pow(tab[i].Item1 - expectancy.Item1, 2) + Mathf.Pow(tab[i].Item2 - expectancy.Item2, 2));
            if (dmin < d)
            {
                dmin = d;
            }
        }

        threshold = dmin;
    }

    float EuclidianDistance(float Cb, float Cr, Vec3f expectancy)
    {
        return (Mathf.Sqrt(Mathf.Pow(Cb - expectancy.Item1, 2) + Mathf.Pow(Cr - expectancy.Item2, 2)));
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
