using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;


// Parallel computation support
using System;

public class HairDetection : MonoBehaviour {
    [SerializeField]
    //private int fixThreshold = 10;
    private int colorSampleListSize = 300;

    private Vec3f[] skinColorSampleYCbCr;    
    private Vec3f skinColorYCbCrExpectancy;
    public Vec3f SkinColorYCbCrExpectancy
    {
        get { return skinColorYCbCrExpectancy; }
    }
    private double skinColorCbCrThreshold;

    private Vec3f[] hairColorSampleYCbCr;
    private Vec3f hairColorYCbCrExpectancy;
    private double hairColorCbCrThreshold;

    public int yHairRoot =-1;
    //yHairTop représente le plus haut point de la tête
    public int yHairTop =-1;
    private int yHairMax;
    private int hairHeight;
    private int j_min=-1;
    public int J_min
    {
        get { return j_min; }
    }
    private int j_max=-1;
    public int J_max
    {
        get { return j_max; }
    }
    public enum Epaisseur
    {
        aucune,
        non_epais,
        epais,
        tres_epais
    }
    public Epaisseur epaisseur = Epaisseur.aucune;

    public enum Longueur
    {
        aucune,
        tres_court,
        moyen,
        longs
    }
    public Longueur longueur = Longueur.aucune;

    private FaceDetectionImage faceDetectionImage;
    private LandmarksRetriever landMarksRetriever;

    private Mat matrix2_grabcut;
    public Mat Matrix2_grabcut
    {
        get { return matrix2_grabcut; }
    }

    //Il me faut l'accès à l'image, ainsi que les coordonnées des joues et du front + les landmarks des coins des yeux et du menton
    // Use this for initialization
    public void Init()
    {
        skinColorYCbCrExpectancy = new Vec3f();
        hairColorYCbCrExpectancy = new Vec3f();
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
        //FindHairTop();
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

    public void GrabCut()
    {
        Debug.Log("GrabCut");
        //faceDetectionImage = GetComponent<FaceDetectionImage>();
        //Mat result = new Mat(faceDetectionImage.VideoSourceImage.Size(), faceDetectionImage.VideoSourceImage.Type());
        Mat result = new Mat(faceDetectionImage.VideoSourceImage.Size(), MatType.CV_8UC3);
        
        //Mat result = faceDetectionImage.VideoSourceImage;
        Mat bgModel = new Mat(); //background model
        Mat fgModel = new Mat(); //foreground model
        

        //draw a rectangle 
        //OpenCvSharp.Rect rectangle = new OpenCvSharp.Rect(1, 1, faceDetectionImage.VideoSourceImage.Cols - 1, faceDetectionImage.VideoSourceImage.Rows - 1);
        OpenCvSharp.Rect rectangle = new OpenCvSharp.Rect(faceDetectionImage.Face.X - 100, faceDetectionImage.Face.Y - 100, faceDetectionImage.Face.Width + 200, faceDetectionImage.Face.Height + 200);

        Cv2.GrabCut(faceDetectionImage.VideoSourceImage, result, rectangle, bgModel, fgModel, 1, GrabCutModes.InitWithRect);
        Cv2.Compare(result, new Scalar(3, 3, 3), result, CmpTypes.EQ);
        matrix2_grabcut = new Mat(faceDetectionImage.ImHeight, faceDetectionImage.ImWidth, MatType.CV_8UC3, new Scalar(255, 255, 255));

        faceDetectionImage.VideoSourceImage.CopyTo(matrix2_grabcut, result);
        
        matrix2_grabcut.CopyTo(faceDetectionImage.VideoSourceImage);
    }

    public void GetSkinColor()
    {

        Debug.Log("Get Skin Color");
        int skinColorCounter = 0;
        //you can Pick if it's 0
        int youCanPick = 0;
        //On va pick tous les 10 pixels
        int youCanPickEveryXPixels = 10;
        System.Random rand = new System.Random();


        for (var i= faceDetectionImage.RectFront.Y; i < faceDetectionImage.RectFront.Y + faceDetectionImage.RectFront.Height; i++)
        {
            for (var j = faceDetectionImage.RectFront.X; j < faceDetectionImage.RectFront.X + faceDetectionImage.RectFront.Width; j++)
            {
                //Vec3b vec = faceDetectionImage.VideoSourceImageData[j + i * faceDetectionImage.ImWidth];
                Vec3b vec = faceDetectionImage.VideoSourceImage.At<Vec3b>(i, j);


                /* Code pour changer la couleur du pixel (i,j)
                 * faceDetectionImage.VideoSourceImage.Set<Vec3b>(i, j, new Vec3b
                {
                    Item0 = 255,
                    Item1 = 0,
                    Item2 = 0
                });*/

                //>>>Récupérations d'échantillons de couleur du front
                if (youCanPick == 0 && skinColorCounter < colorSampleListSize)
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
                /*else if (j > faceDetectionImage.RectEyeLeft.X && j < faceDetectionImage.RectEyeLeft.X + (faceDetectionImage.RectEyeLeft.Width - 10) &&
                   i > faceDetectionImage.RectEyeLeft.Y && i < faceDetectionImage.RectEyeLeft.Y + faceDetectionImage.RectEyeLeft.Height && youCanPick == 0 && skinColorCounter < colorSampleListSize )
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
                else if (j > faceDetectionImage.RectEyeRight.X && j < faceDetectionImage.RectEyeRight.X + faceDetectionImage.RectEyeRight.Width &&
                   i > faceDetectionImage.RectEyeRight.Y && i < faceDetectionImage.RectEyeRight.Y + faceDetectionImage.RectEyeRight.Height && youCanPick == 0 && skinColorCounter < colorSampleListSize)
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
                }*/


                youCanPick = (youCanPick + 1) % youCanPickEveryXPixels;
                if (youCanPick == 1)
                {
                    youCanPickEveryXPixels = rand.Next(5, 15);
                }
                
            }
        }


        //>>>Calcul de l'espérance skinColorYCbCrExpectancy
        skinColorYCbCrExpectancy = ComputeVec3fExpectancy(skinColorSampleYCbCr, skinColorCounter);
        Debug.Log("Skin Color YCbCrExpectancy");
        Debug.Log(skinColorYCbCrExpectancy.Item1);
        Debug.Log(skinColorYCbCrExpectancy.Item2);



        //skinColorYCbCrExpectancy.Item0 = 128; //simule un éclairage normal

        //Affichage de la couleur de peau sur un carré rempli en haut à gauche de l'image

        int abscisse = faceDetectionImage.VideoSourceImage.Width / 10;
        int ordonnee = faceDetectionImage.VideoSourceImage.Height / 10;

        Color32 couleurRGB = FromYCbCrToRGB(skinColorYCbCrExpectancy);
        Scalar couleurRectangle = Scalar.FromRgb(couleurRGB.r, couleurRGB.g, couleurRGB.b);


        OpenCvSharp.Rect rectCouleur = new OpenCvSharp.Rect(abscisse, ordonnee, faceDetectionImage.VideoSourceImage.Width / 20, faceDetectionImage.VideoSourceImage.Height / 20);
        Cv2.Rectangle(faceDetectionImage.VideoSourceImage, rectCouleur, couleurRectangle, -5);



        //>>>Calcul des Thresholds skinColorYCbCrThresholds
        skinColorCbCrThreshold = Math.Ceiling(ComputeVec3fThresholds(skinColorSampleYCbCr, skinColorCounter, skinColorYCbCrExpectancy));
        
        //skinColorCbCrThreshold = fixThreshold;
        Debug.Log("Seuil calculé : " + skinColorCbCrThreshold);
        Debug.Log("Skin Color YCbCrThresholds");
        Debug.Log(skinColorCbCrThreshold);

    }

    public void getEyeColor()
    {
        // Récupération des coordonnées des point 44 et 45 des landmarks (voir schéma des landmarks)
        /* Vec2f point_44 = new Vec2f(faceDetectionImage.localLandmarks[2 * 43], faceDetectionImage.localLandmarks[2 * 43 + 1]);
         Vec2f point_45 = new Vec2f(faceDetectionImage.localLandmarks[2 * 44], faceDetectionImage.localLandmarks[2 * 44 + 1]);

         // Récupération des coordonnées des point 47 et 48 des landmarks (voir schéma des landmarks)
         Vec2f point_47 = new Vec2f(faceDetectionImage.localLandmarks[2 * 46], faceDetectionImage.localLandmarks[2 * 46 + 1]);
         Vec2f point_48 = new Vec2f(faceDetectionImage.localLandmarks[2 * 47], faceDetectionImage.localLandmarks[2 * 47 + 1]);

         float moyenne_abscisses_44_45 = (point_44.Item0 + point_45.Item0) / 2;
         float moyenne_ordonnees_44_45 = (point_44.Item1 + point_45.Item1) / 2;

         float moyenne_abscisses_47_48 = (point_47.Item0 + point_48.Item0) / 2;
         float moyenne_ordonnees_47_48 = (point_47.Item1 + point_48.Item1) / 2;

         //Point en lequel nous prélevons la couleur des yeux
         float abscisse_point_choisit = (moyenne_abscisses_44_45 + moyenne_abscisses_47_48)/2;
         float ordonnee_point_choisit = (( float ) 3 / 10) * moyenne_ordonnees_44_45 + ((float) 7 / 10) * moyenne_ordonnees_47_48;

         */

        Vec2f point_43 = new Vec2f(faceDetectionImage.localLandmarks[2 * 42], faceDetectionImage.localLandmarks[2 * 42 + 1]);
        Vec2f point_46 = new Vec2f(faceDetectionImage.localLandmarks[2 * 45], faceDetectionImage.localLandmarks[2 * 45 + 1]);

        //Point en lequel nous prélevons la couleur des yeux

        float abscisse_point_choisit = ((float)4 / 10)*point_43.Item0 + ((float)6 / 10)*point_46.Item0;
        float ordonnee_point_choisit = ((float)4 / 10) * point_43.Item1 + ((float)6 / 10) * point_46.Item1;

        // Couleur b, v et r du pixel associé à l'isobarycentre de ces landmarks
        Vec3b vec = faceDetectionImage.VideoSourceImage.At<Vec3b>((int) Math.Floor(ordonnee_point_choisit), (int)Math.Floor(abscisse_point_choisit));

        //Affichage de la couleur des yeux sur un carré rempli en haut à droite de l'image
        int abscisse = 9*faceDetectionImage.VideoSourceImage.Width / 10;
        int ordonnee = faceDetectionImage.VideoSourceImage.Height / 10;

        Scalar couleurRectangle = Scalar.FromRgb(vec.Item2, vec.Item1, vec.Item0);

        OpenCvSharp.Rect rectCouleur = new OpenCvSharp.Rect(abscisse, ordonnee, faceDetectionImage.VideoSourceImage.Width / 20, faceDetectionImage.VideoSourceImage.Height / 20);
        Cv2.Rectangle(faceDetectionImage.VideoSourceImage, rectCouleur, couleurRectangle, -5);

        /*for (int i = (int)Math.Floor(ordonnee_point_choisit)-1; i<= (int)Math.Floor(ordonnee_point_choisit) + 1; i++)
        {
            for (int j = (int)Math.Floor(abscisse_point_choisit) - 1; j <= (int)Math.Floor(abscisse_point_choisit) + 1; j++)
            {
                faceDetectionImage.VideoSourceImage.Set<Vec3b>(i, j, new Vec3b
                {
                    Item0 = 0,
                    Item1 = 255,
                    Item2 = 0
                });
            }
        }*/

        faceDetectionImage.VideoSourceImage.Set<Vec3b>((int)Math.Floor(ordonnee_point_choisit) , (int)Math.Floor(abscisse_point_choisit), new Vec3b
        {
            Item0 = 0,
            Item1 = 255,
            Item2 = 0
        });



    }



    public void FindHairRoots()
    {
        Debug.Log("Find Hair Roots");
        
        int nbOfPixelBlancThreshold = 4;
        int pixelBlancCounter = 0;
        int pixelNonSkin = 0;

        //On part du haut du front
        //int j = faceDetectionImage.RectFront.X+faceDetectionImage.RectFront.Width/2;
        int j = (int)faceDetectionImage.localLandmarks[2 * 8];

        for (var i = faceDetectionImage.RectFront.Y; i>0; i--)
        {
            
            Vec3b vec = faceDetectionImage.VideoSourceImage.At<Vec3b>(i , j);
            Color32 color = new Color32
            {
                r = vec.Item2,
                g = vec.Item1,
                b = vec.Item0
            };
            
            if (color.r == 255 && color.g == 255 && color.b == 255)
            {
                pixelBlancCounter++;
                if (pixelBlancCounter >= nbOfPixelBlancThreshold)
                {
                    yHairTop = i + nbOfPixelBlancThreshold;
                    if (yHairRoot == -1)
                    {
                        yHairRoot = yHairTop;
                    }
                    break;
                }
            } else
            {
                pixelBlancCounter = 0;
            }

            Vec3f sample = FromRGBToYCbCr(color);
            //Tant qu'on ne trouve pas les cheveux (le pixel courant est de la peau), on remonte vers le haut (donc on varie le y)
            if (yHairRoot==-1)
            {
                if (EuclidianDistance(sample.Item1, sample.Item2, skinColorYCbCrExpectancy) > skinColorCbCrThreshold)
                {
                    pixelNonSkin++;
                    if (pixelNonSkin >= nbOfPixelBlancThreshold)
                    {
                        yHairRoot = i + pixelNonSkin;
                        Debug.Log("Hair roots found, y : ");
                        Debug.Log(yHairRoot);
                    }
                } else
                {
                    pixelNonSkin = 0;
                }
                    
            } else
            { // yHairRoot a déjà été défini, on se contente d'afficher le pixel courant
                faceDetectionImage.VideoSourceImage.Set<Vec3b>(i, j, new Vec3b
                {
                    Item0 = 0,
                    Item1 = 255,
                    Item2 = 0
                });
            }
        }

    }


    void FindJminJmax()
    {
        j_min = (int) Math.Floor(faceDetectionImage.localLandmarks[0]);
        j_max = (int) Math.Floor(faceDetectionImage.localLandmarks[2*16]);
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
                Vec3b vec = faceDetectionImage.VideoSourceImage.At<Vec3b>(i * faceDetectionImage.ImWidth, j);
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
        hairColorYCbCrExpectancy = ComputeVec3fExpectancy(hairColorSampleYCbCr, hairColorCounter);
        Debug.Log("Hair Color YCbCrExpectancy");
        Debug.Log(hairColorYCbCrExpectancy);

        //>>>Calcul des Thresholds hairColorYCbCrThresholds
        hairColorCbCrThreshold = ComputeVec3fThresholds(hairColorSampleYCbCr, hairColorCounter, hairColorYCbCrExpectancy);
        Debug.Log("Hair Color YCbCrThresholds");
        Debug.Log(hairColorCbCrThreshold);
    }

   /* void FindHairTop()
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
            Vec3b vec = faceDetectionImage.VideoSourceImage.At<Vec3b>(i * faceDetectionImage.ImWidth, j);
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
        
    }*/

    public void ClearSkin()
    {
        Debug.Log("Clear Skin");
        //Si on clear juste le skin
        for (var i = 0; i < faceDetectionImage.ImHeight; i++)
        {
            for (var j = 0; j < faceDetectionImage.ImWidth; j++)
            {
                //Vec3b vec = faceDetectionImage.VideoSourceImageData[j + i * faceDetectionImage.ImWidth];
                Vec3b vec = faceDetectionImage.VideoSourceImage.At<Vec3b>(i, j);
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

                    faceDetectionImage.VideoSourceImage.Set<Vec3b>(i,j, new Vec3b
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
                Vec3b vec = faceDetectionImage.VideoSourceImage.At<Vec3b>(i * faceDetectionImage.ImWidth, j);
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

                faceDetectionImage.VideoSourceImage.Set<Vec3b>(i * faceDetectionImage.ImWidth + j, new Vec3b
                {
                    Item0 = 255,
                    Item1 = 255,
                    Item2 = 255
                });
            }
        }
    }

    public void FindHairMax()
    {
        int nbOfLineNonHairThreshold = 4;
        int lineNonHairCounter = 0;

        //Calcul de j_min et j_max
        FindJminJmax();

        //Parcours de toutes les lignes à partir du carré des yeux pour déterminer la longueur des cheveux
        int i0 = faceDetectionImage.RectEyeRight.Y + faceDetectionImage.RectEyeRight.Height;
        yHairMax = i0;

        for (var i = i0; i < faceDetectionImage.ImHeight; i++)
        {
            bool gaucheValide = false;
            bool droiteValide = false;

            for (var j = 0; j < faceDetectionImage.ImWidth; j++)
            {
                if (!gaucheValide)
                {
                    if (j <= j_min)
                    {
                        //Verification de la nature du pixel (i,j)
                        Vec3b vec = faceDetectionImage.VideoSourceImage.At<Vec3b>(i, j);
                        Color32 color = new Color32
                        {
                            r = vec.Item2,
                            g = vec.Item1,
                            b = vec.Item0
                        };

                        Vec3f sample = FromRGBToYCbCr(color);
                        /*if (EuclidianDistance(sample.Item1, sample.Item2, skinColorYCbCrExpectancy) > skinColorCbCrThreshold && !(color.r == 255 && color.g == 255 && color.b == 255))
                        {
                            gaucheValide = true;
                        }*/
                        gaucheValide = !(color.r == 255 && color.g == 255 && color.b == 255);


                    } else
                    {
                        //aucun pixel à gauche de j_min représente des cheveux, donc la ligne i est invalide !
                        break;
                    }
                }
                else {
                    if (!droiteValide)
                    {
                        if (j >= j_max)
                        {
                            //Verification de la nature du pixel (i,j)
                            Vec3b vec = faceDetectionImage.VideoSourceImage.At<Vec3b>(i, j);
                            Color32 color = new Color32
                            {
                                r = vec.Item2,
                                g = vec.Item1,
                                b = vec.Item0
                            };

                            Vec3f sample = FromRGBToYCbCr(color);
                            if (EuclidianDistance(sample.Item1, sample.Item2, skinColorYCbCrExpectancy) > skinColorCbCrThreshold && !(color.r == 255 && color.g == 255 && color.b == 255))
                            {
                                droiteValide = true;
                                break; // plus besoin de continuer
                            }
                        }
                    } 
                }
            }

            //Conclure sur la validité de la ligne i
            if (gaucheValide && droiteValide)
            {
                yHairMax = i;
                lineNonHairCounter = 0;
            } else
            {
                lineNonHairCounter++;
                if (lineNonHairCounter >= nbOfLineNonHairThreshold)
                {
                    break;
                }
            }
        }

        for (var j = 0; j < faceDetectionImage.ImWidth; j++)
        {
            faceDetectionImage.VideoSourceImage.Set<Vec3b>(yHairMax, j, new Vec3b
            {
                Item0 = 0,
                Item1 = 255,
                Item2 = 0
            });
        }

        for (var i = 0; i < faceDetectionImage.ImHeight; i++)
        {
            faceDetectionImage.VideoSourceImage.Set<Vec3b>(i, j_min, new Vec3b
            {
                Item0 = 0,
                Item1 = 255,
                Item2 = 0
            });

            faceDetectionImage.VideoSourceImage.Set<Vec3b>(i, j_max, new Vec3b
            {
                Item0 = 0,
                Item1 = 255,
                Item2 = 0
            });
        }

        Debug.Log("yhairmax : " + yHairMax);
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
                Vec3b vec = faceDetectionImage.VideoSourceImage.At<Vec3b>(i, j);
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

    public void GuessHairLength()
    {
        Debug.Log("Guess Hair Length");
        //on fait yHairMax - yHairRoot et on compare à la longueur du visage
        if(yHairRoot == yHairTop)
        {
            Debug.Log("Cette personne est chauve !");
            
            return;
        }

        //On va comparer yHairMax par rapport aux landmarks : le nez et le menton
        if (yHairMax <= landMarksRetriever.Nose.Item1)
        {
            Debug.Log("Cette personne a les cheveux court !");
            longueur = Longueur.tres_court;
            return;
        } else if (yHairMax >= landMarksRetriever.Chin.Item1)
        {
            Debug.Log("Cette personne a les cheveux longs !");
            longueur = Longueur.longs;
            return;
        }
        else
        {
            Debug.Log("Cette personne a les cheveux moyens !");
            longueur = Longueur.moyen;
            return;
        }
        
    }

    /*void GuessHairLength_sans_landmarks()
    {
        if (yHairMax < faceDetectionImage.rectMouth.Y)
        {
            //Cheveux très court
            longueur = Longueur.tres_court;
        }
        else if (yHairMax > faceDetectionImage.Face.Y + faceDetectionImage.Face.Height)
        {
            //Cheveux long
            longueur = Longueur.longs;
        } else
        {
            //Cheveux court
            longueur = Longueur.court;
        }
    }*/

    public void GuessHairHeight()
    {
        Debug.Log("Guess Hair Height");
        hairHeight = yHairRoot - yHairTop;
        if (hairHeight >= faceDetectionImage.Face.Height / 3)
        {
            Debug.Log("Cette personne a les cheveux très épais !");
            epaisseur = Epaisseur.tres_epais;
            return;
        }else if(hairHeight <= faceDetectionImage.Face.Height / 6)
        {
            Debug.Log("Cette personne a les cheveux non épais !");
            epaisseur = Epaisseur.non_epais;
            return;
        }
        else
        {
            Debug.Log("Cette personne a les cheveux épais !");
            epaisseur = Epaisseur.epais;
            return;
        }
    }

    //met à jour l'espérance à partir d'un tableau d'échantillons
    Vec3f ComputeVec3fExpectancy(Vec3f[] tab, int taille_tab)
    {
        Vec3f expectancy = new Vec3f();
        Debug.Log("Compute Expectancy");
        float yExpectancy = 0f;
        float cbExpectancy = 0f;
        float crExpectancy = 0f;
        for(int i=0; i < taille_tab; i++)
        {
            yExpectancy += tab[i].Item0;
            cbExpectancy += tab[i].Item1;
            crExpectancy += tab[i].Item2;
        }
        //Debug.Log(yExpectancy);
        yExpectancy /= taille_tab;
        cbExpectancy /= taille_tab;
        crExpectancy /= taille_tab;
       
        expectancy.Item0 = yExpectancy;
        expectancy.Item1 = cbExpectancy;
        expectancy.Item2 = crExpectancy;

        return expectancy;
        

    }

    //permet d'obtenir le treshold à partir d'un tableau d'échantillons et de son espérance
    double ComputeVec3fThresholds(Vec3f[] tab, int taille_tab , Vec3f expectancy)
    {
        Debug.Log("Compute Thresholds");
        double dmin = EuclidianDistance(tab[0].Item1, tab[0].Item2, expectancy);        

        for (int i = 0; i < taille_tab; i++)
        {
            double d = EuclidianDistance(tab[i].Item1, tab[i].Item2, expectancy);

            if (dmin < d)
            {
                dmin = d;
            }
        }

        return dmin;
    }

    double EuclidianDistance(float Cb, float Cr, Vec3f expectancy)
    {
        return (Mathf.Sqrt(Mathf.Pow(Cb - expectancy.Item1, 2) + Mathf.Pow(Cr - expectancy.Item2, 2)));
    }

    public Vec3f FromRGBToYCbCr(Color32 RGB)
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

    public Color32 FromYCbCrToRGB(Vec3f YCbCr)
    {
        var rgb = new Color32
        {
            //r
            r = (Byte)(YCbCr.Item0 + 1.402f * (YCbCr.Item2 - 128)),
            //g
            g = (Byte)(YCbCr.Item0 - 0.34414f * (YCbCr.Item1 - 128) - 0.71414f * (YCbCr.Item2 - 128)),
            //b
            b = (Byte)(YCbCr.Item0 + 1.772f * (YCbCr.Item1 - 128))
        };
        return rgb;
    }
}
