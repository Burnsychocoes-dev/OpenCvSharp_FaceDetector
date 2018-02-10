using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;


// Parallel computation support
using System;

public class HairDetection : MonoBehaviour
{
    [SerializeField]
    //private int fixThreshold = 10;
    private int colorSampleListSize = 300;
    private double skinColorCbCrThreshold;
    private Vec3f skinColorYCbCrExpectancy;
    public Vec3f SkinColorYCbCrExpectancy
    {
        get { return skinColorYCbCrExpectancy; }
    }



    private Vec3f hairColorYCbCrExpectancy;
    public Vec3f HairColorYCbCrExpectancy
    {
        get { return hairColorYCbCrExpectancy; }
    }


    public int yHairRoot = -1;
    //yHairTop représente le plus haut point de la tête
    public int yHairTop = -1;
    private int yHairMax;
    private int hairHeight;
    private int j_min = -1;
    /*public int J_min
    {
        get { return j_min; }
    }
    private int j_max=-1;
    public int J_max
    {
        get { return j_max; }
    }*/
    /*public enum Epaisseur
    {
        aucune,
        peu_epais,
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
    public Longueur longueur = Longueur.aucune;*/

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
        RangeHair
    }

    public Haircut haircut = Haircut.Chauve;

    private PhotoAnalysingScript photo;

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

        photo = GetComponent<PhotoAnalysingScript>();
    }

    // Update is called once per frame
    void Update()
    {

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
        //ClearNonHair();
        //On va chercher le dernier Y où on apperçoit des cheveux
        Debug.Log("Finding Hair YMax");
        FindHairMax();
        //On décide
        Debug.Log("Guessing Hair Length");
        //GuessHairLength();
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
        //photo = GetComponent<photo>();
        //Mat result = new Mat(photo.VideoSourceImage.Size(), photo.VideoSourceImage.Type());
        Mat result = new Mat(photo.VideoSourceImage.Size(), MatType.CV_8UC3);

        //Mat result = photo.VideoSourceImage;
        Mat bgModel = new Mat(); //background model
        Mat fgModel = new Mat(); //foreground model


        //draw a rectangle 
        //OpenCvSharp.Rect rectangle = new OpenCvSharp.Rect(1, 1, photo.VideoSourceImage.Cols - 1, photo.VideoSourceImage.Rows - 1);
        OpenCvSharp.Rect rectangle = new OpenCvSharp.Rect(photo.Face.X - 100, photo.Face.Y - 100, photo.Face.Width + 200, photo.Face.Height + 200);

        Cv2.GrabCut(photo.VideoSourceImage, result, rectangle, bgModel, fgModel, 1, GrabCutModes.InitWithRect);
        Cv2.Compare(result, new Scalar(3, 3, 3), result, CmpTypes.EQ);
        matrix2_grabcut = new Mat(photo.ImHeight, photo.ImWidth, MatType.CV_8UC3, new Scalar(255, 255, 255));

        photo.VideoSourceImage.CopyTo(matrix2_grabcut, result);

        matrix2_grabcut.CopyTo(photo.VideoSourceImage);
    }



    public void GetSkinColor()
    {
        Debug.Log("Get Skin Color");
        Vec3f[] skinColorSampleYCbCr = new Vec3f[colorSampleListSize];
        int skinColorCounter = GetColorFromRect(skinColorSampleYCbCr, photo.RectFront); // récupération d'un échantillon de couleurs dans le rectangle donné en entrée
        //>>>Calcul de l'espérance skinColorYCbCrExpectancy
        skinColorYCbCrExpectancy = ComputeVec3fExpectancy(skinColorSampleYCbCr, skinColorCounter);
        Debug.Log("Skin Color YCbCrExpectancy");
        Debug.Log(skinColorYCbCrExpectancy.Item1);
        Debug.Log(skinColorYCbCrExpectancy.Item2);



        //Affichage de la couleur de peau sur un carré rempli en haut à gauche de l'image

        int abscisse = photo.VideoSourceImage.Width / 10;
        int ordonnee = photo.VideoSourceImage.Height / 10;

        Color32 couleurRGB = FromYCbCrToRGB(skinColorYCbCrExpectancy);
        Scalar couleurRectangle = Scalar.FromRgb(couleurRGB.r, couleurRGB.g, couleurRGB.b);


        OpenCvSharp.Rect rectCouleur = new OpenCvSharp.Rect(abscisse, ordonnee, photo.VideoSourceImage.Width / 20, photo.VideoSourceImage.Height / 20);
        Cv2.Rectangle(photo.VideoSourceImage, rectCouleur, couleurRectangle, -5);



        //>>>Calcul des Thresholds skinColorYCbCrThresholds
        skinColorCbCrThreshold = Math.Ceiling(ComputeVec3fThresholds(skinColorSampleYCbCr, skinColorCounter, skinColorYCbCrExpectancy));

        //skinColorCbCrThreshold = fixThreshold;
        Debug.Log("Skin Color YCbCrThresholds");
        Debug.Log(skinColorCbCrThreshold);
    }


    public void GetHairColor()
    {
        Debug.Log("Get Hair Color");

        if (yHairRoot == -1)
        {
            Debug.Log("No hair here");

        }
        else
        {
            Vec3f[] hairColorSampleYCbCr = new Vec3f[colorSampleListSize];

            //Création du carré dans lequel sera effectué le prélèvement
            float abscisse_landmark_22 = photo.localLandmarks[2 * 21];
            float abscisse_landmark_23 = photo.localLandmarks[2 * 22];

            int distance = (int)(abscisse_landmark_23 - abscisse_landmark_22);

            int ordonnee_rect = yHairRoot - 3 * (yHairRoot - yHairTop) / 10;

            OpenCvSharp.Rect rect = new OpenCvSharp.Rect((int)abscisse_landmark_22, ordonnee_rect, distance, (yHairRoot - yHairTop) / 10);

            int hairColorCounter = GetColorFromRect(hairColorSampleYCbCr, rect);

            //>>>Calcul de l'espérance hairColorYCbCrExpectancy
            hairColorYCbCrExpectancy = ComputeVec3fExpectancy(hairColorSampleYCbCr, hairColorCounter);
            Debug.Log("Hair Color YCbCrExpectancy");
            Debug.Log(hairColorYCbCrExpectancy);


            //Affichage de la couleur des cheveux sur un carré rempli en bas à gauche de l'image

            int abscisse = photo.VideoSourceImage.Width / 10;  //à gauche de l'image
            int ordonnee = 9 * photo.VideoSourceImage.Height / 10; // en bas de l'image

            Color32 couleurRGB = FromYCbCrToRGB(hairColorYCbCrExpectancy);
            Scalar couleurRectangle = Scalar.FromRgb(couleurRGB.r, couleurRGB.g, couleurRGB.b);


            OpenCvSharp.Rect rectCouleur = new OpenCvSharp.Rect(abscisse, ordonnee, photo.VideoSourceImage.Width / 20, photo.VideoSourceImage.Height / 20);
            Cv2.Rectangle(photo.VideoSourceImage, rectCouleur, couleurRectangle, -5);

            Scalar couleur = Scalar.FromRgb(255, 0, 0);
            Cv2.Rectangle(photo.VideoSourceImage, rect, couleur, 3);



            /*//>>>Calcul des Thresholds hairColorYCbCrThresholds
            double hairColorCbCrThreshold = ComputeVec3fThresholds(hairColorSampleYCbCr, hairColorCounter, hairColorYCbCrExpectancy);
            Debug.Log("Hair Color YCbCrThresholds");
            Debug.Log(hairColorCbCrThreshold);*/
        }
    }



    private int GetColorFromRect(Vec3f[] colorSampleYcbcr, OpenCvSharp.Rect rect)
    {

        Debug.Log("Get Skin Color");
        int skinColorCounter = 0;
        //you can Pick if it's 0
        int youCanPick = 0;
        //On va pick tous les 10 pixels
        int youCanPickEveryXPixels = 10;
        System.Random rand = new System.Random();


        for (var i = rect.Y; i < rect.Y + rect.Height; i++)
        {
            for (var j = rect.X; j < rect.X + rect.Width; j++)
            {
                Vec3b vec = photo.VideoSourceImage.At<Vec3b>(i, j);


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
                    colorSampleYcbcr[skinColorCounter] = FromRGBToYCbCr(color);
                    skinColorCounter++;
                }


                youCanPick = (youCanPick + 1) % youCanPickEveryXPixels;
                if (youCanPick == 1)
                {
                    youCanPickEveryXPixels = rand.Next(5, 15);
                }

            }
        }

        return skinColorCounter;

    }

    public void getEyeColor()
    {
        // Récupération des coordonnées des point 44 et 45 des landmarks (voir schéma des landmarks)
        /* Vec2f point_44 = new Vec2f(photo.localLandmarks[2 * 43], photo.localLandmarks[2 * 43 + 1]);
         Vec2f point_45 = new Vec2f(photo.localLandmarks[2 * 44], photo.localLandmarks[2 * 44 + 1]);

         // Récupération des coordonnées des point 47 et 48 des landmarks (voir schéma des landmarks)
         Vec2f point_47 = new Vec2f(photo.localLandmarks[2 * 46], photo.localLandmarks[2 * 46 + 1]);
         Vec2f point_48 = new Vec2f(photo.localLandmarks[2 * 47], photo.localLandmarks[2 * 47 + 1]);

         float moyenne_abscisses_44_45 = (point_44.Item0 + point_45.Item0) / 2;
         float moyenne_ordonnees_44_45 = (point_44.Item1 + point_45.Item1) / 2;

         float moyenne_abscisses_47_48 = (point_47.Item0 + point_48.Item0) / 2;
         float moyenne_ordonnees_47_48 = (point_47.Item1 + point_48.Item1) / 2;

         //Point en lequel nous prélevons la couleur des yeux
         float abscisse_point_choisit = (moyenne_abscisses_44_45 + moyenne_abscisses_47_48)/2;
         float ordonnee_point_choisit = (( float ) 3 / 10) * moyenne_ordonnees_44_45 + ((float) 7 / 10) * moyenne_ordonnees_47_48;

         */

        Vec2f point_43 = new Vec2f(photo.localLandmarks[2 * 42], photo.localLandmarks[2 * 42 + 1]);
        Vec2f point_46 = new Vec2f(photo.localLandmarks[2 * 45], photo.localLandmarks[2 * 45 + 1]);

        //Point en lequel nous prélevons la couleur des yeux

        float abscisse_point_choisit = ((float)4 / 10) * point_43.Item0 + ((float)6 / 10) * point_46.Item0;
        float ordonnee_point_choisit = ((float)4 / 10) * point_43.Item1 + ((float)6 / 10) * point_46.Item1;

        // Couleur b, v et r du pixel associé à l'isobarycentre de ces landmarks
        Vec3b vec = photo.VideoSourceImage.At<Vec3b>((int)Math.Floor(ordonnee_point_choisit), (int)Math.Floor(abscisse_point_choisit));

        //Affichage de la couleur des yeux sur un carré rempli en haut à droite de l'image
        int abscisse = 9 * photo.VideoSourceImage.Width / 10;
        int ordonnee = photo.VideoSourceImage.Height / 10;

        Scalar couleurRectangle = Scalar.FromRgb(vec.Item2, vec.Item1, vec.Item0);

        OpenCvSharp.Rect rectCouleur = new OpenCvSharp.Rect(abscisse, ordonnee, photo.VideoSourceImage.Width / 20, photo.VideoSourceImage.Height / 20);
        Cv2.Rectangle(photo.VideoSourceImage, rectCouleur, couleurRectangle, -5);

        /*for (int i = (int)Math.Floor(ordonnee_point_choisit)-1; i<= (int)Math.Floor(ordonnee_point_choisit) + 1; i++)
        {
            for (int j = (int)Math.Floor(abscisse_point_choisit) - 1; j <= (int)Math.Floor(abscisse_point_choisit) + 1; j++)
            {
                photo.VideoSourceImage.Set<Vec3b>(i, j, new Vec3b
                {
                    Item0 = 0,
                    Item1 = 255,
                    Item2 = 0
                });
            }
        }*/

        photo.VideoSourceImage.Set<Vec3b>((int)Math.Floor(ordonnee_point_choisit), (int)Math.Floor(abscisse_point_choisit), new Vec3b
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

        int j = (int)photo.localLandmarks[2 * 8];

        for (var i = photo.RectFront.Y; i > 0; i--)
        {

            Vec3b vec = photo.VideoSourceImage.At<Vec3b>(i, j);
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
            }
            else
            {
                pixelBlancCounter = 0;
            }

            Vec3f sample = FromRGBToYCbCr(color);
            //Tant qu'on ne trouve pas les cheveux (le pixel courant est de la peau), on remonte vers le haut (donc on varie le y)
            if (yHairRoot == -1)
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
                }
                else
                {
                    pixelNonSkin = 0;
                }

            }
            else
            { // yHairRoot a déjà été défini, on se contente d'afficher le pixel courant
                photo.VideoSourceImage.Set<Vec3b>(i, j, new Vec3b
                {
                    Item0 = 0,
                    Item1 = 255,
                    Item2 = 0
                });
            }
        }

    }



    public void ClearSkin()
    {
        Debug.Log("Clear Skin");
        //Si on clear juste le skin
        for (var i = 0; i < photo.ImHeight; i++)
        {
            for (var j = 0; j < photo.ImWidth; j++)
            {
                //Vec3b vec = photo.VideoSourceImageData[j + i * photo.ImWidth];
                Vec3b vec = photo.VideoSourceImage.At<Vec3b>(i, j);
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

                    photo.VideoSourceImage.Set<Vec3b>(i, j, new Vec3b
                    {
                        Item0 = 255,
                        Item1 = 255,
                        Item2 = 255
                    });
                }
            }
        }

    }



    public void FindHairMax()
    {
        int nbOfLineNonHairThreshold = 4;
        int lineNonHairCounter = 0;

        //Calcul de j_min et j_max
        int j_min = (int)Math.Floor(photo.localLandmarks[0]);
        int j_max = (int)Math.Floor(photo.localLandmarks[2 * 16]);

        //Parcours de toutes les lignes à partir du landmarks 15 (extrémité basse de l'oreille)
        int i0 = (int)photo.localLandmarks[2 * 14 + 1];
        yHairMax = i0;

        for (var i = i0; i < photo.ImHeight; i++)
        {
            bool gaucheValide = false;
            bool droiteValide = false;

            for (var j = 0; j < photo.ImWidth; j++)
            {
                if (!gaucheValide)
                {
                    if (j <= j_min)
                    {
                        //Verification de la nature du pixel (i,j)
                        Vec3b vec = photo.VideoSourceImage.At<Vec3b>(i, j);
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


                    }
                    else
                    {
                        //aucun pixel à gauche de j_min représente des cheveux, donc la ligne i est invalide !
                        break;
                    }
                }
                else
                {
                    if (!droiteValide)
                    {
                        if (j >= j_max)
                        {
                            //Verification de la nature du pixel (i,j)
                            Vec3b vec = photo.VideoSourceImage.At<Vec3b>(i, j);
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
            }
            else
            {
                lineNonHairCounter++;
                if (lineNonHairCounter >= nbOfLineNonHairThreshold)
                {
                    break;
                }
            }
        }

        for (var j = 0; j < photo.ImWidth; j++)
        {
            photo.VideoSourceImage.Set<Vec3b>(i0, j, new Vec3b
            {
                Item0 = 0,
                Item1 = 255,
                Item2 = 0
            });
        }

        for (var i = 0; i < photo.ImHeight; i++)
        {
            photo.VideoSourceImage.Set<Vec3b>(i, j_min, new Vec3b
            {
                Item0 = 0,
                Item1 = 255,
                Item2 = 0
            });

            photo.VideoSourceImage.Set<Vec3b>(i, j_max, new Vec3b
            {
                Item0 = 0,
                Item1 = 255,
                Item2 = 0
            });
        }

        Debug.Log("yhairmax : " + yHairMax);
    }




    //Cette fonction permet de déterminer la longueur des cheveux pour les hommes
    //En sortie : un entier compris entre 1 et 3 :
    //           - 1 => les cheveux sont courts
    //           - 3 => les cheveux sont longs
    //           - 2 => à mis chemin entre 1 et 2
    private int GuessHairLength_men()
    {
        int ordonnee_landmarks_14 = (int)photo.localLandmarks[2 * 13 + 1];
        int ordonnee_landmarks_9 = (int)photo.localLandmarks[2 * 8 + 1];

        int resultat = 0;

        if (yHairMax < ordonnee_landmarks_14)
        {
            //Les Cheveux sont courts
            resultat = 1;
        }
        else if (yHairMax < ordonnee_landmarks_9)
        {
            //Les Cheveux ne sont ni court, ni long
            resultat = 2;
        }
        else
        {
            //Les Cheveux sont long
            resultat = 3;
        }
        return resultat;
    }


    //Cette fonction permet de déterminer la longueur des cheveux pour les femmes
    //En sortie : un entier compris entre 1 et 3 :
    //           - 1 => les cheveux sont très courts
    //           - 2 => les cheveux sont courts
    //           - 3 => les cheveux sont entre court et long
    //           - 4 => les cheveux sont longs

    private int GuessHairLength_women()
    {
        int ordonnee_landmarks_14 = (int)photo.localLandmarks[2 * 13 + 1];
        int ordonnee_landmarks_12 = (int)photo.localLandmarks[2 * 11 + 1];
        int ordonnee_landmarks_9 = (int)photo.localLandmarks[2 * 8 + 1];

        int ordonnee_landmarks_11 = (int)photo.localLandmarks[2 * 11 + 1];
        int dy = ordonnee_landmarks_9 - ordonnee_landmarks_11;


        int resultat = 0;

        if (yHairMax < ordonnee_landmarks_14)
        {
            //Les Cheveux sont très courts
            resultat = 1;
        }
        else if (yHairMax < ordonnee_landmarks_12)
        {
            //Les Cheveux sont courts
            resultat = 2;
        }
        else if (yHairMax < ordonnee_landmarks_9 + dy)
        {
            //les cheveux sont entre court et long
            resultat = 3;
        }
        else
        {
            //les cheveux sont long
            resultat = 4;
        }

        return resultat;
    }


    //Cette fonction permet de déterminer la proportion de l'épaisseur des cheveux par rapport au visage
    //En sortie : un entier compris entre 0 et 4 :
    //           - 1 => l'épaisseur est faible
    //           - 2 => l'épaisseur est moyenne
    //           - 3 => l'épaisseur est forte
    //           - 4 => l'épaisseur est très forte

    private int GuessHairHeight()
    {
        int resultat = 0;
        int epaisseur = yHairRoot - yHairTop;

        float ordonnee_landmark_20 = photo.localLandmarks[2 * 19 + 1];
        float ordonnee_landmark_25 = photo.localLandmarks[2 * 24 + 1];

        int ordonnee_haut_visage = (int)(ordonnee_landmark_20 + ordonnee_landmark_25) / 2;
        int ordonnee_bas_visage = (int)photo.localLandmarks[2 * 8];
        int taille_visage = ordonnee_bas_visage - ordonnee_haut_visage;

        double proportion = (double)epaisseur / (double)(taille_visage);
        double proportion_faible = 0.05;
        double proportion_moyenne = 0.25;
        double proportion_forte = 0.45;
        double proportion_tres_forte = 0.55;


        double distance_1 = Math.Abs(proportion - proportion_faible);
        double distance_2 = Math.Abs(proportion - proportion_moyenne);
        double distance_3 = Math.Abs(proportion - proportion_forte);
        double distance_4 = Math.Abs(proportion - proportion_tres_forte);

        if (distance_1 <= distance_2 && distance_1 <= distance_3 && distance_1 <= distance_4)
        {
            //le front est grand
            resultat = 1;
        }
        else if (distance_2 <= distance_1 && distance_2 <= distance_3 && distance_2 <= distance_4)
        {
            //le front est moyen
            resultat = 2;
        }
        else if (distance_3 <= distance_1 && distance_3 <= distance_2 && distance_3 <= distance_4)
        {
            //le front est petit
            resultat = 3;
        }
        else
        {
            resultat = 4;
        }
        return resultat;
    }



    //Cette fonction permet de déterminer la proportion du front par rapport au visage
    //           - 1 => le front est grand
    //           - 3 => le front est petit 
    //           - 2 => à mis chemin entre 1 et 2
    private int TailleFront()
    {
        int resultat = 0;

        float ordonnee_landmark_20 = photo.localLandmarks[2 * 19 + 1];
        float ordonnee_landmark_25 = photo.localLandmarks[2 * 24 + 1];

        int ordonnee_haut_visage = (int)(ordonnee_landmark_20 + ordonnee_landmark_25) / 2;
        int ordonnee_bas_visage = (int)photo.localLandmarks[2 * 8];

        int taille_visage = ordonnee_bas_visage - ordonnee_haut_visage;

        int taille_front = ordonnee_haut_visage - yHairRoot;

        double proportion = ((double)taille_front / (double)taille_visage);
        Debug.Log("Proportion obtenue : " + proportion);

        double proportion_forte = 0.3;
        double proportion_moyenne = 0.15;

        double proportion_faible = 0.05;
        //double proportion_moyenne = (proportion_forte + proportion_faible) / 2;

        double distance_1 = Math.Abs(proportion - proportion_forte);
        double distance_2 = Math.Abs(proportion - proportion_moyenne);
        double distance_3 = Math.Abs(proportion - proportion_faible);

        if (distance_1 <= distance_2 && distance_1 <= distance_3)
        {
            //le front est grand
            resultat = 1;
        }
        else if (distance_2 <= distance_1 && distance_2 <= distance_3)
        {
            //le front est moyen
            resultat = 2;
        }
        else
        {
            //le front est petit
            resultat = 3;
        }


        return resultat;
    }


    //Cette fonction permet de savoir si les cheveux sont disparate (les cheveux débordent considérablement au dela des oreilles)
    public Boolean EstDisparate(double proportion)
    {
        //Calcul de l'abscisse du point à la frontière entre les cheveux et le fond en parcourant la ligne d'ordonnée un landmark du sourcil
        // à gauche et à droite
        int iDroit = (int)photo.localLandmarks[2 * 24 + 1];
        int jDroit = (int)photo.localLandmarks[2 * 24];
        int xFrontiereDroite = -1;

        int iGauche = (int)photo.localLandmarks[2 * 19 + 1];
        int jGauche = (int)photo.localLandmarks[2 * 19];
        int xFrontiereGauche = -1;

        //Calcul de la frontière à droite
        for (int j = jDroit; j < photo.ImWidth; j++)
        {
            Vec3b vec = photo.VideoSourceImage.At<Vec3b>(iDroit, j); //Récupération du vecteur (b,v,r) du pixel (i,j)
            if (vec.Item2 == 0 && vec.Item1 == 0 && vec.Item0 == 0)
            {
                xFrontiereDroite = j;
                break;
            }

            photo.VideoSourceImage.Set<Vec3b>(iDroit, j, new Vec3b
            {
                Item0 = 0,
                Item1 = 255,
                Item2 = 0
            });
        }

        //Calcul de la frontière à gauche
        for (int j = jGauche; j >= 0; j--)
        {
            Vec3b vec = photo.VideoSourceImage.At<Vec3b>(iGauche, j); //Récupération du vecteur (b,v,r) du pixel (i,j)
            if (vec.Item2 == 0 && vec.Item1 == 0 && vec.Item0 == 0)
            {
                xFrontiereGauche = j;
                break;
            }

            photo.VideoSourceImage.Set<Vec3b>(iGauche, j, new Vec3b
            {
                Item0 = 0,
                Item1 = 255,
                Item2 = 0
            });
        }


        int j_min = (int)Math.Floor(photo.localLandmarks[0]);
        int j_max = (int)Math.Floor(photo.localLandmarks[2 * 16]);

        int largeurVisage = j_max - j_min;
        double proportion_1 = ((double)(xFrontiereDroite - j_max)) / ((double)largeurVisage); // décrit à quel point les cheveux débordent à droite 
        double proportion_2 = ((double)(j_min - xFrontiereGauche)) / ((double)largeurVisage); // décrit à quel point les cheveux débordent à gauche 

        return (proportion_1 >= proportion || proportion_2 >= proportion);
    }

    //Cette méthode permet de déterminer parmis les coupes existantes dans MORPH3D celle qui est la + appropriée
    public void GuessHairCut()
    {
        if (AvatarScript.avatar1.gender == AvatarScript.Gender.Male)
        {
            //L'utilisateur est un homme, le choix des coupes s'effectue parmis les coupes d'hommes

            /*yHairMax = (int)photo.localLandmarks[2 * 14 + 1];
            yHairTop = 75;*/


            int longueurCheveux = GuessHairLength_men();
            int tailleFront = TailleFront();
            int epaisseur = GuessHairHeight();

            if (epaisseur == 1)
            {
                //L'épaisseur des cheveux est trop faible
                haircut = Haircut.Chauve;
            }
            else
            {
                if (longueurCheveux == 2)
                {
                    //Les cheveux ne sont ni court, ni long
                    haircut = Haircut.DrifterHair;

                }
                else if (longueurCheveux == 3)
                {
                    //Les cheveux sont long
                    haircut = Haircut.CasualLongHair;
                }
                else
                {
                    //Les cheveux sont courts

                    if (tailleFront == 1)
                    {
                        //Le front est grand
                        if (epaisseur == 2)
                        {
                            //l'épaisseur des cheveux est moyenne
                            haircut = Haircut.BoldHair;
                        }
                        else
                        {
                            //l'épaisseur des cheveux est élevé, voir très élevé, il reste 2 candidats qu'on va distinguer aléatoirement (forte ressemblance)
                            System.Random rand = new System.Random();
                            int aleatoire = rand.Next(2);
                            if (aleatoire == 0)
                            {
                                haircut = Haircut.ScottHair;
                            }
                            if (aleatoire == 1)
                            {
                                haircut = Haircut.FunkyHair;
                            }
                        }

                    }
                    else if (tailleFront == 2)
                    {
                        //le front est moyen
                        if (epaisseur == 2 || epaisseur == 3)
                        {
                            //épaisseur moyenne ou élevée
                            haircut = Haircut.JakeHair;
                        }
                        else
                        {
                            //épaisseur très élevée, il reste 2 candidats à départager par rapport au caractère disparate ou compacte des cheveux
                            if (EstDisparate(0.17))
                            {
                                haircut = Haircut.MicahMaleHair;
                            }
                            else
                            {
                                haircut = Haircut.KamiHair;
                            }
                        }
                    }
                    else
                    {
                        //Le front est casiment inexistant
                        haircut = Haircut.KungFuHair;
                    }
                }
            }
        }
        else
        {
            //L'utilisateur est une femme, le choix des coupes s'effectue parmis les coupes de femmes
            int longueurCheveux = GuessHairLength_women();

            if (longueurCheveux == 1)
            {
                //Les cheveux sont très courts, MicahFemaleHair est le seul candidat possible
                haircut = Haircut.MicahFemaleHair;
            }
            else if (longueurCheveux == 2)
            {
                //Les cheveux sont courts, 2 candidats : ToulouseHair et NordicHair qu'on va distinguer par le caractère disparate ou non des cheveux
                if (EstDisparate(0.23))
                {
                    haircut = Haircut.NordicHair;
                }
                else
                {
                    haircut = Haircut.ToulouseHair;
                }

            }
            else if (longueurCheveux == 3)
            {
                //Les cheveux sont ni court ni long, FashionHair est le seul candidat possible
                haircut = Haircut.FashionHair;
            }
            else
            {
                //Les cheveux sont long, RangerHair est le seul candidat possible
                haircut = Haircut.RangeHair;
            }

        }

        Debug.Log("Coupe de cheveux : " + haircut);

    }



    //met à jour l'espérance à partir d'un tableau d'échantillons
    Vec3f ComputeVec3fExpectancy(Vec3f[] tab, int taille_tab)
    {
        Vec3f expectancy = new Vec3f();
        Debug.Log("Compute Expectancy");
        float yExpectancy = 0f;
        float cbExpectancy = 0f;
        float crExpectancy = 0f;
        for (int i = 0; i < taille_tab; i++)
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
    double ComputeVec3fThresholds(Vec3f[] tab, int taille_tab, Vec3f expectancy)
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
            Item0 = 0.299f * RGB.r + 0.587f * RGB.g + 0.114f * RGB.b,
            //Cb
            Item1 = -0.1687f * RGB.r - 0.3313f * RGB.g + 0.5f * RGB.b + 128,
            //Cr
            Item2 = 0.5f * RGB.r - 0.4187f * RGB.g - 0.0813f * RGB.b + 128
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
