// Copyright (c) 2016, Long Qian
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, are
// permitted provided that the following conditions are met:
//
//    * Redistributions of source code must retain the above copyright notice, this list
//      of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this
//      list of conditions and the following disclaimer in the documentation and/or other
//      materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
// OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
// SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
// THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using UnityEngine;
using OpenCvSharp;


// Parallel computation support
using Uk.Org.Adcock.Parallel;
using System;
using System.Web;
using System.Runtime.InteropServices;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


// class for video display and processed video display
// current process is canny edge detection
public class PhotoAnalysingScript : MonoBehaviour
{
    SoundEffectsHelper soundEffects;
    private int frameCount = 0;
    [SerializeField]
    private int maxCount = 3;

    [SerializeField]
    private string imagePath;

    [SerializeField]
    private Image image;

    private bool waitSoundEffect = false;

    // Video parameters
    private Texture2D imageTexture;
    public MeshRenderer ProcessedTextureRenderer;

    // Variables des rectangles de decoupe
    float rectShapeHeight = 50;    
    OpenCvSharp.Rect face;
    public OpenCvSharp.Rect Face
    {
        get { return face; }
    }
    
    // Video size
    private const int imWidth = 1280;
    public int ImWidth{
        get { return imWidth; }
    }
    private const int imHeight = 720;
    public int ImHeight
    {
        get { return imHeight; }
    }
    private int imFrameRate;

    // OpenCVSharp parameters
    private Mat videoSourceImage;
    public Mat VideoSourceImage
    {
        get { return videoSourceImage; }
    }
    private Mat cannyImage;
    private Texture2D processedTexture;
    private Vec3b[] videoSourceImageData;
    public Vec3b[] VideoSourceImageData
    {
        get { return videoSourceImageData; }
        set { videoSourceImageData = value; }

    }
    private byte[] cannyImageData;

    // Frame rate parameter
    private int updateFrameCount = 0;
    private int textureCount = 0;
    private int displayCount = 0;

    // Données utiles
    private Vec3f couleurPeauBasOeilGauche;
    private Vec3f couleurPeauBasOeilDroit;
    private Vec3f couleurPeauFront;
    public Vec3f CouleurPeauFront
    {
        get { return couleurPeauFront; }
    }
    private Vec3f couleurCheveux;
    [SerializeField]
    private float margeErreurCouleurPeau = 20f;
    private float maxCoordY = 0f;
    static bool isFind = false;
    private float lipHeight;
    public float LipHeight
    {
        get { return lipHeight; }
        set { lipHeight = value; }
    }

    HairDetection hair;


    private enum Etape {
        GetLandmarks,
        AnalysingLandmarks,
        AnalysingHaircut,
        Idle
    }
    private Etape etape;


    public float[] localLandmarks;
    // The imported function
    [DllImport("face_landmark_detection_ex", EntryPoint = "FaceLandmarkDetection")] public static extern int GetLocalLandmarks(String datPath, String filePath, float[] landmarks);


    void Start() {
        hair = GetComponent<HairDetection>();


        byte[] photoFile = File.ReadAllBytes(imagePath);
        imageTexture = new Texture2D(2, 2);
        imageTexture.LoadImage(photoFile);

        // initialize video / image with given size
        videoSourceImage = new Mat(imHeight, imWidth, MatType.CV_8UC3);
        videoSourceImageData = new Vec3b[imHeight * imWidth];
        cannyImage = new Mat(imHeight, imWidth, MatType.CV_8UC3);
        cannyImageData = new byte[imHeight * imWidth];

        // create processed video texture as Texture2D object
        processedTexture = new Texture2D(imWidth, imHeight, TextureFormat.RGBA32, true, true);

        // assign the processedTexture to the meshrenderer for display
        ProcessedTextureRenderer.material.mainTexture = processedTexture;

        updateFrameCount++;

        // convert texture of original video to OpenCVSharp Mat object
        TextureToMat();

        MatToTexture(videoSourceImage);

        etape = Etape.GetLandmarks;
    }
    
    void Update()
    {
        switch(etape)
        {
            case Etape.GetLandmarks:
                GetLandmarks();
                etape = Etape.AnalysingLandmarks;
                break;

            case Etape.AnalysingLandmarks:
                LandmarkAnalyserAvatar1();
                LandmarkAnalyserAvatar2();
                LandmarkAnalyserAvatar3();
                LandmarkAnalyserAvatar4();
                LandmarkAnalyserAvatar5();
                etape = Etape.AnalysingHaircut;
                break;

            case Etape.AnalysingHaircut:
                TextureToMat();
                ProcessImage(videoSourceImage, false);
                // Hair analyse
                hair.Init();
                Cv2.Flip(videoSourceImage, videoSourceImage, FlipMode.X);
                if (hair.GrabCut())
                {
                    //le grabCut s'est bien déroulé, on ooursuit les autres analyses                 
                    hair.GetSkinColor();
                    hair.getEyeColor();
                    hair.GetHairColor();
                    hair.FindHairMax();
                    hair.GuessHairCut();
                    AvatarScript.avatar1.haircut = hair.Haircut;
                    AvatarScript.avatar2.haircut = hair.Haircut;
                    AvatarScript.avatar3.haircut = hair.Haircut;
                    AvatarScript.avatar4.haircut = hair.Haircut;
                    AvatarScript.avatar5.haircut = hair.Haircut;               

                } else
                {
                    hair.GetSkinColor();
                    hair.getEyeColor();
                    //le grabCut s'est mal déroulé, on assigne une coupe de cheveux par défaut
                    if (AvatarScript.avatar1.gender == AvatarScript.Gender.Male)
                    {
                        AvatarScript.avatar1.haircut = AvatarScript.Haircut.ScottHair;
                        AvatarScript.avatar2.haircut = AvatarScript.Haircut.ScottHair;
                        AvatarScript.avatar3.haircut = AvatarScript.Haircut.ScottHair;
                        AvatarScript.avatar4.haircut = AvatarScript.Haircut.ScottHair;
                        AvatarScript.avatar5.haircut = AvatarScript.Haircut.ScottHair;
                    }
                    else
                    {
                        AvatarScript.avatar1.haircut = AvatarScript.Haircut.RangerHair;
                        AvatarScript.avatar2.haircut = AvatarScript.Haircut.RangerHair;
                        AvatarScript.avatar3.haircut = AvatarScript.Haircut.RangerHair;
                        AvatarScript.avatar4.haircut = AvatarScript.Haircut.RangerHair;
                        AvatarScript.avatar5.haircut = AvatarScript.Haircut.RangerHair;
                    }

                }
                Cv2.Flip(videoSourceImage, videoSourceImage, FlipMode.X);


                SceneManager.LoadScene("Scene5");
                break;

            default:
                break;
        }
    }

    // Convert Unity Texture2D object to OpenCVSharp Mat object
    void TextureToMat()
    {
        // Color32 array : r, g, b, a
        Color32[] c = imageTexture.GetPixels32();

        // Parallel for loop
        // convert Color32 object to Vec3b object
        // Vec3b is the representation of pixel for Mat
        Parallel.For(0, imHeight, i =>
        {
            for (var j = 0; j < imWidth; j++)
            {
                var col = c[j + i * imWidth];
                var vec3 = new Vec3b
                {
                    Item0 = col.b,
                    Item1 = col.g,
                    Item2 = col.r
                };
                // set pixel to an array
                videoSourceImageData[j + i * imWidth] = vec3;
            }
        });
        // assign the Vec3b array to Mat
        videoSourceImage.SetArray(0, 0, videoSourceImageData);
    }
    
    // Convert OpenCVSharp Mat object to Unity Texture2D object
    void MatToTexture(Mat _image)
    {
        // cannyImageData is byte array, because canny image is grayscale

        //cannyImage.GetArray(0, 0, cannyImageData);

        //cannyImage.GetArray(0, 0, cannyImageData);
        _image.GetArray(0, 0, videoSourceImageData);

        // create Color32 array that can be assigned to Texture2D directly
        Color32[] c = new Color32[imHeight * imWidth];


        // parallel for loop
        Parallel.For(0, imHeight, i =>
        {
            for (var j = 0; j < imWidth; j++)
            {
                float coordX = j; 
                float coordY = imHeight - i;

                Vec3b vec = videoSourceImageData[j + i * imWidth];

                //if (coordX > rectFront.X && coordX < rectFront.X + rectFront.Width &&
                //    coordY > rectFront.Y && coordY < rectFront.Y + rectFront.Height)
                //{
                //    var color32 = new Color32
                //    {
                //        r = 255,
                //        g = 255,
                //        b = 255,


                //        a = 0
                //    };
                //    c[j + i * imWidth] = color32;
                //}
                //else if (coordX > rectEyeLeft.X && coordX < rectEyeLeft.X + rectEyeLeft.Width &&
                //    coordY > rectEyeLeft.Y && coordY < rectEyeLeft.Y + rectEyeLeft.Height)
                //{
                //    var color32 = new Color32
                //    {
                //        r = 255,
                //        g = 255,
                //        b = 255,


                //        a = 0
                //    };
                //    c[j + i * imWidth] = color32;
                //}
                //else if (coordX > rectEyeRight.X && coordX < rectEyeRight.X + rectEyeRight.Width &&
                //    coordY > rectEyeRight.Y && coordY < rectEyeRight.Y + rectEyeRight.Height)
                //{
                //    var color32 = new Color32
                //    {
                //        r = 255,
                //        g = 255,
                //        b = 255,


                //        a = 0
                //    };
                //    c[j + i * imWidth] = color32;
                //}
                //else if (coordX > rectCheveux.X && coordX < rectCheveux.X + rectCheveux.Width &&
                //     coordY > rectCheveux.Y && coordY < rectCheveux.Y + rectCheveux.Height)
                //{
                //    var color32 = new Color32
                //    {
                //        r = 255,
                //        g = 255,
                //        b = 255,


                //        a = 0
                //    };
                //    c[j + i * imWidth] = color32;
                //}
                //else
                //{
                    var color32 = new Color32
                    {
                        r = vec.Item2,
                        g = vec.Item1,
                        b = vec.Item0,


                        a = 0
                    };
                    c[j + i * imWidth] = color32;
                //}
            }
        });

        processedTexture.SetPixels32(c);
        // to update the texture, OpenGL manner
        processedTexture.Apply();
    }

    void CleanScreen()
    {
        // create Color32 array that can be assigned to Texture2D directly
        Color32[] c = new Color32[imHeight * imWidth];


        // parallel for loop
        Parallel.For(0, imHeight, i =>
        {
            for (var j = 0; j < imWidth; j++)
            {
                float coordX = j;
                float coordY = imHeight - i;

                Vec3b vec = videoSourceImageData[j + i * imWidth];

                var color32 = new Color32
                {
                    r = 255,
                    g = 255,
                    b = 255,

                    a = 0
                };
                c[j + i * imWidth] = color32;               
            }
        });
        processedTexture.SetPixels32(c);
        // to update the texture, OpenGL manner
        processedTexture.Apply();
    }

    

    // Simple example of canny edge detect
    void ProcessImage(Mat _image, bool draw)
    {
        Cv2.Flip(_image, _image, FlipMode.X);
        Cv2.Canny(_image, cannyImage, 100, 100);
        var grayImage = new Mat();
        Cv2.CvtColor(_image, grayImage, ColorConversionCodes.BGR2GRAY);
        Cv2.EqualizeHist(grayImage, grayImage);

        var face_cascade = new CascadeClassifier();
        face_cascade.Load(Application.dataPath + "/Plugins/Classifiers/haarcascade_frontalface_default.xml");
        var eye_cascade = new CascadeClassifier();
        eye_cascade.Load(Application.dataPath + "/Plugins/Classifiers/haarcascade_eye_tree_eyeglasses.xml");
        var mouth_cascade = new CascadeClassifier();
        mouth_cascade.Load(Application.dataPath + "/Plugins/Classifiers/haarcascade_mcs_mouth.xml");
        //Debug.Log(" ");

        var faces = face_cascade.DetectMultiScale(
                image: grayImage,
                scaleFactor: 1.3,
                minNeighbors: 5,
                flags: HaarDetectionType.DoRoughSearch | HaarDetectionType.ScaleImage,
                minSize: new Size(100, 100)
        );

        //Bounds meshRendererBounds = GetComponentInChildren<MeshRenderer>().bounds;
        //Vector3 meshRendererCenter = meshRendererBounds.center;
        //Vector3 maxBound = meshRendererBounds.max;
        //Vector3 minBound = meshRendererBounds.min;
        //OpenCvSharp.Rect rect = new OpenCvSharp.Rect((int)meshRendererCenter.x + 350,(int)meshRendererCenter.y + 50, 600,600);
        var global_rectangle_color = Scalar.FromRgb(0, 0, 255);
        //Cv2.Rectangle(_image, rect, global_rectangle_color, 3);
        //Console.WriteLine("Detected faces: {0}", faces.Length);
        //Debug.Log(faces.Length);

        //var rnd = new System.Random();

        var face_count = 0;
        foreach (var faceRect in faces)
        {
            var detectedFaceImage = new Mat(_image, faceRect);
            //Cv2.ImShow(string.Format("Face {0}", face_count), detectedFaceImage);
            //Cv2.WaitKey(1); // do events

            var facec_rectangle_color = Scalar.FromRgb(255, 0, 0);
            face = faceRect;
            if(draw)
                //Cv2.Rectangle(_image, faceRect, facec_rectangle_color, 3);


            face_count++;
        }

        Cv2.Flip(_image, _image, FlipMode.X);
    }


    void GetLandmarks()
    {
        GetLocalLandmarks("Assets/Plugins/Dlib/shape_predictor_68_face_landmarks.dat", imagePath, localLandmarks);
    }

    void DrawLandmarks(Mat _image)
    {
        var landmark_color = Scalar.FromRgb(0, 255, 0);
        // Head
        for (int i = 0; i < 16; i++)
        {
            Point point1 = new Point(localLandmarks[2 * i], localLandmarks[2 * i + 1]);
            Point point2 = new Point(localLandmarks[2 * (i + 1)], localLandmarks[2 * (i + 1) + 1]);
            Cv2.Line(_image, point1, point2, landmark_color, 2);
        }
        // Right brow
        for (int i = 17; i < 21; i++)
        {
            Point point1 = new Point(localLandmarks[2 * i], localLandmarks[2 * i + 1]);
            Point point2 = new Point(localLandmarks[2 * (i + 1)], localLandmarks[2 * (i + 1) + 1]);
            Cv2.Line(_image, point1, point2, landmark_color, 2);
        }
        // Left brow
        for (int i = 22; i < 26; i++)
        {
            Point point1 = new Point(localLandmarks[2 * i], localLandmarks[2 * i + 1]);
            Point point2 = new Point(localLandmarks[2 * (i + 1)], localLandmarks[2 * (i + 1) + 1]);
            Cv2.Line(_image, point1, point2, landmark_color, 2);
        }
        // Nose
        for (int i = 27; i < 36; i++)
        {
            if (i == 35)
            {
                Point point1 = new Point(localLandmarks[2 * i], localLandmarks[2 * i + 1]);
                Point point2 = new Point(localLandmarks[2 * 30], localLandmarks[2 * 30 + 1]);
                Cv2.Line(_image, point1, point2, landmark_color, 2);
            }
            else
            {
                Point point1 = new Point(localLandmarks[2 * i], localLandmarks[2 * i + 1]);
                Point point2 = new Point(localLandmarks[2 * (i + 1)], localLandmarks[2 * (i + 1) + 1]);
                Cv2.Line(_image, point1, point2, landmark_color, 2);
            }
        }
        // Left Eye
        for (int i = 36; i < 42; i++)
        {
            if (i == 41)
            {
                Point point1 = new Point(localLandmarks[2 * i], localLandmarks[2 * i + 1]);
                Point point2 = new Point(localLandmarks[2 * 36], localLandmarks[2 * 36 + 1]);
                Cv2.Line(_image, point1, point2, landmark_color, 2);
            }
            else
            {
                Point point1 = new Point(localLandmarks[2 * i], localLandmarks[2 * i + 1]);
                Point point2 = new Point(localLandmarks[2 * (i + 1)], localLandmarks[2 * (i + 1) + 1]);
                Cv2.Line(_image, point1, point2, landmark_color, 2);
            }
        }
        // Right Eye
        for (int i = 42; i < 48; i++)
        {
            if (i == 47)
            {
                Point point1 = new Point(localLandmarks[2 * i], localLandmarks[2 * i + 1]);
                Point point2 = new Point(localLandmarks[2 * 42], localLandmarks[2 * 42 + 1]);
                Cv2.Line(_image, point1, point2, landmark_color, 2);
            }
            else
            {
                Point point1 = new Point(localLandmarks[2 * i], localLandmarks[2 * i + 1]);
                Point point2 = new Point(localLandmarks[2 * (i + 1)], localLandmarks[2 * (i + 1) + 1]);
                Cv2.Line(_image, point1, point2, landmark_color, 2);
            }
        }
        // Extern lip
        for (int i = 48; i < 60; i++)
        {
            if (i == 59)
            {
                Point point1 = new Point(localLandmarks[2 * i], localLandmarks[2 * i + 1]);
                Point point2 = new Point(localLandmarks[2 * 48], localLandmarks[2 * 48 + 1]);
                Cv2.Line(_image, point1, point2, landmark_color, 2);
            }
            else
            {
                Point point1 = new Point(localLandmarks[2 * i], localLandmarks[2 * i + 1]);
                Point point2 = new Point(localLandmarks[2 * (i + 1)], localLandmarks[2 * (i + 1) + 1]);
                Cv2.Line(_image, point1, point2, landmark_color, 2);
            }
        }
        // Intern lip
        for (int i = 60; i < 68; i++)
        {
            if (i == 67)
            {
                Point point1 = new Point(localLandmarks[2 * i], localLandmarks[2 * i + 1]);
                Point point2 = new Point(localLandmarks[2 * 60], localLandmarks[2 * 60 + 1]);
                Cv2.Line(_image, point1, point2, landmark_color, 2);
            }
            else
            {
                Point point1 = new Point(localLandmarks[2 * i], localLandmarks[2 * i + 1]);
                Point point2 = new Point(localLandmarks[2 * (i + 1)], localLandmarks[2 * (i + 1) + 1]);
                Cv2.Line(_image, point1, point2, landmark_color, 2);
            }
        }
        //Triangle nez
        Point point3 = new Point(localLandmarks[2 * 30], localLandmarks[2 * 30 + 1]);
        Point point4 = new Point(localLandmarks[2 * 32], localLandmarks[2 * 32 + 1]);
        Cv2.Line(_image, point3, point4, landmark_color, 2);
        Point point5 = new Point(localLandmarks[2 * 34], localLandmarks[2 * 34 + 1]);
        Cv2.Line(_image, point3, point5, landmark_color, 2);
    }

    public static float DistanceEuclidienne(float x1, float y1, float x2, float y2)
    {
        return Mathf.Sqrt( Mathf.Pow( y2-y1, 2 ) + Mathf.Pow( x2-x1, 2 ) );
    }

    public static float AirTriangle(float x1, float y1, float x2, float y2, float x3, float y3)
    {
        float a = DistanceEuclidienne(x1, y1, x2, y2); //longueur coté a
        float b = DistanceEuclidienne(x2, y2, x3, y3); //longueur coté b
        float c = DistanceEuclidienne(x1, y1, x3, y3); //longueur coté c
        float p = (a + b + c) / 2; //demi périmètre du triangle
        float air = Mathf.Sqrt(p*(p-a)*(p-b)*(p-c)); //formule de calcule d'air
        return air;
    }

    public static float CoefficientDirecteurDroite(float x1, float y1, float x2, float y2)
    {
        float a = (y2 - y1) / (x2 - x1);
        return a;
    }

    /*
     * Retourne l'angle formé par 2 droites en degré
    */
    public static float Angle2Droites(float x1, float y1, float x2, float y2)
    {
        float normeVecteur1 = Mathf.Sqrt(Mathf.Pow(x1, 2) + Mathf.Pow(y1, 2));
        float normeVecteur2 = Mathf.Sqrt(Mathf.Pow(x2, 2) + Mathf.Pow(y2, 2));
        float produitScalaire = x1 * x2 + y1 * y2;
        float angleRadian = Mathf.Acos(produitScalaire / (normeVecteur1 * normeVecteur2));
        float angleDegree = angleRadian * 180 / Mathf.PI;
        return angleDegree;
    }

    // Display the original video in a opencv window
    void UpdateWindow(Mat _image)
    {
        Cv2.Flip(_image, _image, FlipMode.X);
        //Cv2.ImShow("Copy video", _image);
        displayCount++;
    }

    public void LandmarkAnalyserAvatar1()
    {
        // Dimension visage
        float faceHeight = DistanceEuclidienne(localLandmarks[2 * 8], localLandmarks[2 * 8 + 1],
                                                               localLandmarks[2 * 27], localLandmarks[2 * 27 + 1]);
        Debug.Log("face height : " + faceHeight);

        float faceWidth = DistanceEuclidienne(localLandmarks[2 * 1], localLandmarks[2 * 1 + 1],
                                                           localLandmarks[2 * 15], localLandmarks[2 * 15 + 1]);
        Debug.Log("face width : " + faceWidth);


        // Partie eye
        AvatarScript.avatar1.eye.distanceBrowEye = DistanceEuclidienne(localLandmarks[2 * 19], localLandmarks[2 * 19 + 1],
                                                          localLandmarks[2 * 37], localLandmarks[2 * 37 + 1]) / faceHeight;
        AvatarScript.avatar1.eye.eyeWidth = DistanceEuclidienne(localLandmarks[2 * 36], localLandmarks[2 * 36 + 1],
                                                                localLandmarks[2 * 39], localLandmarks[2 * 39 + 1]) / faceWidth;

        if (AvatarScript.avatar1.eye.eyeWidth <= 0.22f)
            AvatarScript.avatar1.eye.width = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar1.eye.width = AvatarScript.Taille.Big;


        // Partie nose
        AvatarScript.avatar1.nose.noseHeight = DistanceEuclidienne(localLandmarks[2 * 27], localLandmarks[2 * 27 + 1],
                                                               localLandmarks[2 * 30], localLandmarks[2 * 30 + 1]) / faceHeight;
        AvatarScript.avatar1.nose.noseWidth = DistanceEuclidienne(localLandmarks[2 * 31], localLandmarks[2 * 31 + 1],
                                                               localLandmarks[2 * 35], localLandmarks[2 * 35 + 1]) / faceWidth;


        if (AvatarScript.avatar1.nose.noseHeight <= 0.39)
            AvatarScript.avatar1.nose.height = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar1.nose.height = AvatarScript.Taille.Big;

        if (AvatarScript.avatar1.nose.noseWidth <= 0.215)
            AvatarScript.avatar1.nose.width = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar1.nose.width = AvatarScript.Taille.Big;

        float noseHeight = DistanceEuclidienne(localLandmarks[2 * 27], localLandmarks[2 * 27 + 1],
                                                               localLandmarks[2 * 30], localLandmarks[2 * 30 + 1]);
        AvatarScript.avatar1.nose.noseTipHeight = DistanceEuclidienne(localLandmarks[2 * 30], localLandmarks[2 * 30 + 1],
                                                  localLandmarks[2 * 33], localLandmarks[2 * 33 + 1]) / noseHeight;
        Debug.Log("noseTipHeight : " + AvatarScript.avatar1.nose.noseTipHeight);
        // Debut algo de prise de decision pour les differents nez
        if (AvatarScript.avatar1.nose.noseTipHeight > 0.38)
        {
            AvatarScript.avatar1.nose.noseTipInclinaison = AvatarScript.NoseTipInclinaison.NezRemonte;
        }
        else if(AvatarScript.avatar1.nose.noseTipHeight < 0.23)
        {
            AvatarScript.avatar1.nose.noseTipInclinaison = AvatarScript.NoseTipInclinaison.NezAbaisse;
        }
        else
        {
            AvatarScript.avatar1.nose.noseTipInclinaison = AvatarScript.NoseTipInclinaison.NezNormal;
        }
        if (AvatarScript.avatar1.nose.noseTipHeight <= 0.305)
        {
            AvatarScript.avatar1.nose.noseTipType = AvatarScript.NoseTipType.NezRond;
        }
        else
        {
            AvatarScript.avatar1.nose.noseTipType = AvatarScript.NoseTipType.NezPointue;
        }

        // Partie mouth
        AvatarScript.avatar1.mouth.distanceBetweenChinAndMouth = DistanceEuclidienne(localLandmarks[2 * 8], localLandmarks[2 * 8 + 1],
                                                                                     localLandmarks[2 * 57], localLandmarks[2 * 57 + 1]) / faceHeight;
        AvatarScript.avatar1.mouth.distanceBetweenNoseTipAndMouth = DistanceEuclidienne(localLandmarks[2 * 33], localLandmarks[2 * 33 + 1],
                                                               localLandmarks[2 * 51], localLandmarks[2 * 51 + 1]) / faceHeight;

        AvatarScript.avatar1.mouth.buttomLipHeight = DistanceEuclidienne(localLandmarks[2 * 66], localLandmarks[2 * 66 + 1],
                                                               localLandmarks[2 * 57], localLandmarks[2 * 57 + 1]) / faceHeight;
        AvatarScript.avatar1.mouth.topLipHeight = DistanceEuclidienne(localLandmarks[2 * 52], localLandmarks[2 * 52 + 1],
                                                               localLandmarks[2 * 63], localLandmarks[2 * 63 + 1]) / faceHeight;


        if (AvatarScript.avatar1.mouth.buttomLipHeight <= 0.09)
            AvatarScript.avatar1.mouth.buttomLipHeight_t = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar1.mouth.buttomLipHeight_t = AvatarScript.Taille.Big;

        if (AvatarScript.avatar1.mouth.topLipHeight <= 0.055)
            AvatarScript.avatar1.mouth.topLipHeight_t = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar1.mouth.topLipHeight_t = AvatarScript.Taille.Big;

        AvatarScript.avatar1.mouth.mouthWidth = DistanceEuclidienne(localLandmarks[2 * 48], localLandmarks[2 * 48 + 1],
                                                               localLandmarks[2 * 54], localLandmarks[2 * 54 + 1]) / faceWidth;

        if (AvatarScript.avatar1.mouth.mouthWidth <= 0.40)
            AvatarScript.avatar1.mouth.width = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar1.mouth.width = AvatarScript.Taille.Big;

        // Partie visage curve
        AvatarScript.avatar1.visage.cornerChinWidth = DistanceEuclidienne(localLandmarks[2 * 4], localLandmarks[2 * 4 + 1],
                                                               localLandmarks[2 * 12], localLandmarks[2 * 12 + 1]) / faceWidth;
        AvatarScript.avatar1.visage.distanceButtomCurve = DistanceEuclidienne(localLandmarks[2 * 5], localLandmarks[2 * 5 + 1],
                                                               localLandmarks[2 * 11], localLandmarks[2 * 11 + 1]) / faceWidth;
    }

    public void LandmarkAnalyserAvatar2()
    {
        // Dimension visage
        float faceHeight = FaceDetectionImage.DistanceEuclidienne(localLandmarks[2 * 8], localLandmarks[2 * 8 + 1],
                                                               localLandmarks[2 * 27], localLandmarks[2 * 27 + 1]);
        Debug.Log("face height : " + faceHeight);

        float faceWidth = FaceDetectionImage.DistanceEuclidienne(localLandmarks[2 * 1], localLandmarks[2 * 1 + 1],
                                                           localLandmarks[2 * 15], localLandmarks[2 * 15 + 1]);
        Debug.Log("face width : " + faceWidth);


        // Partie eye
        AvatarScript.avatar2.eye.distanceBrowEye = DistanceEuclidienne(localLandmarks[2 * 19], localLandmarks[2 * 19 + 1],
                                                          localLandmarks[2 * 37], localLandmarks[2 * 37 + 1]) / faceHeight;
        AvatarScript.avatar2.eye.eyeWidth = DistanceEuclidienne(localLandmarks[2 * 36], localLandmarks[2 * 36 + 1],
                                                                localLandmarks[2 * 39], localLandmarks[2 * 39 + 1]) / faceWidth;

        if (AvatarScript.avatar2.eye.eyeWidth <= 0.22f)
            AvatarScript.avatar2.eye.width = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar2.eye.width = AvatarScript.Taille.Big;


        // Partie nose
        AvatarScript.avatar2.nose.noseHeight = DistanceEuclidienne(localLandmarks[2 * 27], localLandmarks[2 * 27 + 1],
                                                               localLandmarks[2 * 30], localLandmarks[2 * 30 + 1]) / faceHeight;
        AvatarScript.avatar2.nose.noseWidth = DistanceEuclidienne(localLandmarks[2 * 31], localLandmarks[2 * 31 + 1],
                                                               localLandmarks[2 * 35], localLandmarks[2 * 35 + 1]) / faceWidth;

        if (AvatarScript.avatar2.nose.noseHeight <= 0.39)
            AvatarScript.avatar2.nose.height = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar2.nose.height = AvatarScript.Taille.Big;

        if (AvatarScript.avatar2.nose.noseWidth <= 0.215)
            AvatarScript.avatar2.nose.width = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar2.nose.width = AvatarScript.Taille.Big;

        AvatarScript.avatar2.nose.noseTipHeight = DistanceEuclidienne(localLandmarks[2 * 30], localLandmarks[2 * 30 + 1],
                                          localLandmarks[2 * 33], localLandmarks[2 * 33 + 1]) / AvatarScript.avatar1.nose.noseHeight;
        // Debut algo de prise de decision pour les differents nez
        if (AvatarScript.avatar2.nose.noseTipHeight > 0.38)
        {
            AvatarScript.avatar2.nose.noseTipInclinaison = AvatarScript.NoseTipInclinaison.NezRemonte;
        }
        else if (AvatarScript.avatar2.nose.noseTipHeight < 0.23)
        {
            AvatarScript.avatar2.nose.noseTipInclinaison = AvatarScript.NoseTipInclinaison.NezAbaisse;
        }
        else
        {
            AvatarScript.avatar2.nose.noseTipInclinaison = AvatarScript.NoseTipInclinaison.NezNormal;
        }
        if (AvatarScript.avatar2.nose.noseTipHeight <= 0.305)
        {
            AvatarScript.avatar2.nose.noseTipType = AvatarScript.NoseTipType.NezRond;
        }
        else
        {
            AvatarScript.avatar2.nose.noseTipType = AvatarScript.NoseTipType.NezPointue;
        }


        // Partie mouth
        AvatarScript.avatar2.mouth.distanceBetweenChinAndMouth = DistanceEuclidienne(localLandmarks[2 * 8], localLandmarks[2 * 8 + 1],
                                                                                     localLandmarks[2 * 57], localLandmarks[2 * 57 + 1]) / faceHeight;
        AvatarScript.avatar2.mouth.distanceBetweenNoseTipAndMouth = DistanceEuclidienne(localLandmarks[2 * 33], localLandmarks[2 * 33 + 1],
                                                               localLandmarks[2 * 51], localLandmarks[2 * 51 + 1]) / faceHeight;

        AvatarScript.avatar2.mouth.buttomLipHeight = DistanceEuclidienne(localLandmarks[2 * 66], localLandmarks[2 * 66 + 1],
                                                               localLandmarks[2 * 57], localLandmarks[2 * 57 + 1]) / faceHeight;
        AvatarScript.avatar2.mouth.topLipHeight = DistanceEuclidienne(localLandmarks[2 * 52], localLandmarks[2 * 52 + 1],
                                                               localLandmarks[2 * 63], localLandmarks[2 * 63 + 1]) / faceHeight;


        if (AvatarScript.avatar2.mouth.buttomLipHeight <= 0.09)
            AvatarScript.avatar2.mouth.buttomLipHeight_t = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar2.mouth.buttomLipHeight_t = AvatarScript.Taille.Big;

        if (AvatarScript.avatar2.mouth.topLipHeight <= 0.055)
            AvatarScript.avatar2.mouth.topLipHeight_t = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar2.mouth.topLipHeight_t = AvatarScript.Taille.Big;

        AvatarScript.avatar2.mouth.mouthWidth = DistanceEuclidienne(localLandmarks[2 * 48], localLandmarks[2 * 48 + 1],
                                                               localLandmarks[2 * 54], localLandmarks[2 * 54 + 1]) / faceWidth;

        if (AvatarScript.avatar2.mouth.mouthWidth <= 0.40)
            AvatarScript.avatar2.mouth.width = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar2.mouth.width = AvatarScript.Taille.Big;

        // Partie visage curve
        AvatarScript.avatar2.visage.cornerChinWidth = DistanceEuclidienne(localLandmarks[2 * 4], localLandmarks[2 * 4 + 1],
                                                               localLandmarks[2 * 12], localLandmarks[2 * 12 + 1]) / faceWidth;
        AvatarScript.avatar2.visage.distanceButtomCurve = DistanceEuclidienne(localLandmarks[2 * 5], localLandmarks[2 * 5 + 1],
                                                               localLandmarks[2 * 11], localLandmarks[2 * 11 + 1]) / faceWidth;
    }

    public void LandmarkAnalyserAvatar3()
    {
        // Dimension visage
        float faceHeight = FaceDetectionImage.DistanceEuclidienne(localLandmarks[2 * 8], localLandmarks[2 * 8 + 1],
                                                               localLandmarks[2 * 27], localLandmarks[2 * 27 + 1]);
        Debug.Log("face height : " + faceHeight);

        float faceWidth = FaceDetectionImage.DistanceEuclidienne(localLandmarks[2 * 1], localLandmarks[2 * 1 + 1],
                                                           localLandmarks[2 * 15], localLandmarks[2 * 15 + 1]);
        Debug.Log("face width : " + faceWidth);


        // Partie eye
        AvatarScript.avatar3.eye.distanceBrowEye = DistanceEuclidienne(localLandmarks[2 * 19], localLandmarks[2 * 19 + 1],
                                                          localLandmarks[2 * 37], localLandmarks[2 * 37 + 1]) / faceHeight;
        AvatarScript.avatar3.eye.eyeWidth = DistanceEuclidienne(localLandmarks[2 * 36], localLandmarks[2 * 36 + 1],
                                                                localLandmarks[2 * 39], localLandmarks[2 * 39 + 1]) / faceWidth;

        if (AvatarScript.avatar3.eye.eyeWidth <= 0.22f)
            AvatarScript.avatar3.eye.width = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar3.eye.width = AvatarScript.Taille.Big;


        // Partie nose
        AvatarScript.avatar3.nose.noseHeight = DistanceEuclidienne(localLandmarks[2 * 27], localLandmarks[2 * 27 + 1],
                                                               localLandmarks[2 * 30], localLandmarks[2 * 30 + 1]) / faceHeight;
        AvatarScript.avatar3.nose.noseWidth = DistanceEuclidienne(localLandmarks[2 * 31], localLandmarks[2 * 31 + 1],
                                                               localLandmarks[2 * 35], localLandmarks[2 * 35 + 1]) / faceWidth;

        if (AvatarScript.avatar3.nose.noseHeight <= 0.39)
            AvatarScript.avatar3.nose.height = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar3.nose.height = AvatarScript.Taille.Big;

        if (AvatarScript.avatar3.nose.noseWidth <= 0.215)
            AvatarScript.avatar3.nose.width = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar3.nose.width = AvatarScript.Taille.Big;

        AvatarScript.avatar3.nose.noseTipHeight = DistanceEuclidienne(localLandmarks[2 * 30], localLandmarks[2 * 30 + 1],
                                         localLandmarks[2 * 33], localLandmarks[2 * 33 + 1]) / AvatarScript.avatar1.nose.noseHeight;
        // Debut algo de prise de decision pour les differents nez
        if (AvatarScript.avatar3.nose.noseTipHeight > 0.38)
        {
            AvatarScript.avatar3.nose.noseTipInclinaison = AvatarScript.NoseTipInclinaison.NezRemonte;
        }
        else if (AvatarScript.avatar3.nose.noseTipHeight < 0.23)
        {
            AvatarScript.avatar3.nose.noseTipInclinaison = AvatarScript.NoseTipInclinaison.NezAbaisse;
        }
        else
        {
            AvatarScript.avatar3.nose.noseTipInclinaison = AvatarScript.NoseTipInclinaison.NezNormal;
        }
        if (AvatarScript.avatar3.nose.noseTipHeight <= 0.305)
        {
            AvatarScript.avatar3.nose.noseTipType = AvatarScript.NoseTipType.NezRond;
        }
        else
        {
            AvatarScript.avatar3.nose.noseTipType = AvatarScript.NoseTipType.NezPointue;
        }


        // Partie mouth
        AvatarScript.avatar3.mouth.distanceBetweenChinAndMouth = DistanceEuclidienne(localLandmarks[2 * 8], localLandmarks[2 * 8 + 1],
                                                                                     localLandmarks[2 * 57], localLandmarks[2 * 57 + 1]) / faceHeight;
        AvatarScript.avatar3.mouth.distanceBetweenNoseTipAndMouth = DistanceEuclidienne(localLandmarks[2 * 33], localLandmarks[2 * 33 + 1],
                                                               localLandmarks[2 * 51], localLandmarks[2 * 51 + 1]) / faceHeight;

        AvatarScript.avatar3.mouth.buttomLipHeight = DistanceEuclidienne(localLandmarks[2 * 66], localLandmarks[2 * 66 + 1],
                                                               localLandmarks[2 * 57], localLandmarks[2 * 57 + 1]) / faceHeight;
        AvatarScript.avatar3.mouth.topLipHeight = DistanceEuclidienne(localLandmarks[2 * 52], localLandmarks[2 * 52 + 1],
                                                               localLandmarks[2 * 63], localLandmarks[2 * 63 + 1]) / faceHeight;



        if (AvatarScript.avatar3.mouth.buttomLipHeight <= 0.09)
            AvatarScript.avatar3.mouth.buttomLipHeight_t = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar3.mouth.buttomLipHeight_t = AvatarScript.Taille.Big;

        if (AvatarScript.avatar3.mouth.topLipHeight <= 0.055)
            AvatarScript.avatar3.mouth.topLipHeight_t = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar3.mouth.topLipHeight_t = AvatarScript.Taille.Big;

        AvatarScript.avatar3.mouth.mouthWidth = DistanceEuclidienne(localLandmarks[2 * 48], localLandmarks[2 * 48 + 1],
                                                               localLandmarks[2 * 54], localLandmarks[2 * 54 + 1]) / faceWidth;

        if (AvatarScript.avatar3.mouth.mouthWidth <= 0.40)
            AvatarScript.avatar3.mouth.width = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar3.mouth.width = AvatarScript.Taille.Big;

        // Partie visage curve
        AvatarScript.avatar3.visage.cornerChinWidth = DistanceEuclidienne(localLandmarks[2 * 4], localLandmarks[2 * 4 + 1],
                                                               localLandmarks[2 * 12], localLandmarks[2 * 12 + 1]) / faceWidth;
        AvatarScript.avatar3.visage.distanceButtomCurve = DistanceEuclidienne(localLandmarks[2 * 5], localLandmarks[2 * 5 + 1],
                                                               localLandmarks[2 * 11], localLandmarks[2 * 11 + 1]) / faceWidth;
    }

    public void LandmarkAnalyserAvatar4()
    {
        // Dimension visage
        float faceHeight = FaceDetectionImage.DistanceEuclidienne(localLandmarks[2 * 8], localLandmarks[2 * 8 + 1],
                                                               localLandmarks[2 * 27], localLandmarks[2 * 27 + 1]);
        Debug.Log("face height : " + faceHeight);

        float faceWidth = FaceDetectionImage.DistanceEuclidienne(localLandmarks[2 * 1], localLandmarks[2 * 1 + 1],
                                                           localLandmarks[2 * 15], localLandmarks[2 * 15 + 1]);
        Debug.Log("face width : " + faceWidth);


        // Partie eye
        AvatarScript.avatar4.eye.distanceBrowEye = DistanceEuclidienne(localLandmarks[2 * 19], localLandmarks[2 * 19 + 1],
                                                          localLandmarks[2 * 37], localLandmarks[2 * 37 + 1]) / faceHeight;
        AvatarScript.avatar4.eye.eyeWidth = DistanceEuclidienne(localLandmarks[2 * 36], localLandmarks[2 * 36 + 1],
                                                                localLandmarks[2 * 39], localLandmarks[2 * 39 + 1]) / faceWidth;

        if (AvatarScript.avatar4.eye.eyeWidth <= 0.22f)
            AvatarScript.avatar4.eye.width = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar4.eye.width = AvatarScript.Taille.Big;


        // Partie nose
        AvatarScript.avatar4.nose.noseHeight = DistanceEuclidienne(localLandmarks[2 * 27], localLandmarks[2 * 27 + 1],
                                                               localLandmarks[2 * 30], localLandmarks[2 * 30 + 1]) / faceHeight;
        AvatarScript.avatar4.nose.noseWidth = DistanceEuclidienne(localLandmarks[2 * 31], localLandmarks[2 * 31 + 1],
                                                               localLandmarks[2 * 35], localLandmarks[2 * 35 + 1]) / faceWidth;

        if (AvatarScript.avatar4.nose.noseHeight <= 0.39)
            AvatarScript.avatar4.nose.height = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar4.nose.height = AvatarScript.Taille.Big;

        if (AvatarScript.avatar4.nose.noseWidth <= 0.215)
            AvatarScript.avatar4.nose.width = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar4.nose.width = AvatarScript.Taille.Big;

        AvatarScript.avatar4.nose.noseTipHeight = DistanceEuclidienne(localLandmarks[2 * 30], localLandmarks[2 * 30 + 1],
                                         localLandmarks[2 * 33], localLandmarks[2 * 33 + 1]) / AvatarScript.avatar1.nose.noseHeight;
        // Debut algo de prise de decision pour les differents nez
        if (AvatarScript.avatar4.nose.noseTipHeight > 0.38)
        {
            AvatarScript.avatar4.nose.noseTipInclinaison = AvatarScript.NoseTipInclinaison.NezRemonte;
        }
        else if (AvatarScript.avatar4.nose.noseTipHeight < 0.23)
        {
            AvatarScript.avatar4.nose.noseTipInclinaison = AvatarScript.NoseTipInclinaison.NezAbaisse;
        }
        else
        {
            AvatarScript.avatar4.nose.noseTipInclinaison = AvatarScript.NoseTipInclinaison.NezNormal;
        }
        if (AvatarScript.avatar4.nose.noseTipHeight <= 0.305)
        {
            AvatarScript.avatar4.nose.noseTipType = AvatarScript.NoseTipType.NezRond;
        }
        else
        {
            AvatarScript.avatar4.nose.noseTipType = AvatarScript.NoseTipType.NezPointue;
        }


        // Partie mouth
        AvatarScript.avatar4.mouth.distanceBetweenChinAndMouth = DistanceEuclidienne(localLandmarks[2 * 8], localLandmarks[2 * 8 + 1],
                                                                                     localLandmarks[2 * 57], localLandmarks[2 * 57 + 1]) / faceHeight;
        AvatarScript.avatar4.mouth.distanceBetweenNoseTipAndMouth = DistanceEuclidienne(localLandmarks[2 * 33], localLandmarks[2 * 33 + 1],
                                                               localLandmarks[2 * 51], localLandmarks[2 * 51 + 1]) / faceHeight;

        AvatarScript.avatar4.mouth.buttomLipHeight = DistanceEuclidienne(localLandmarks[2 * 66], localLandmarks[2 * 66 + 1],
                                                               localLandmarks[2 * 57], localLandmarks[2 * 57 + 1]) / faceHeight;
        AvatarScript.avatar4.mouth.topLipHeight = DistanceEuclidienne(localLandmarks[2 * 52], localLandmarks[2 * 52 + 1],
                                                               localLandmarks[2 * 63], localLandmarks[2 * 63 + 1]) / faceHeight;



        if (AvatarScript.avatar4.mouth.buttomLipHeight <= 0.09)
            AvatarScript.avatar4.mouth.buttomLipHeight_t = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar4.mouth.buttomLipHeight_t = AvatarScript.Taille.Big;

        if (AvatarScript.avatar4.mouth.topLipHeight <= 0.055)
            AvatarScript.avatar4.mouth.topLipHeight_t = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar4.mouth.topLipHeight_t = AvatarScript.Taille.Big;

        AvatarScript.avatar4.mouth.mouthWidth = DistanceEuclidienne(localLandmarks[2 * 48], localLandmarks[2 * 48 + 1],
                                                               localLandmarks[2 * 54], localLandmarks[2 * 54 + 1]) / faceWidth;

        if (AvatarScript.avatar4.mouth.mouthWidth <= 0.40)
            AvatarScript.avatar4.mouth.width = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar4.mouth.width = AvatarScript.Taille.Big;

        // Partie visage curve
        AvatarScript.avatar4.visage.cornerChinWidth = DistanceEuclidienne(localLandmarks[2 * 4], localLandmarks[2 * 4 + 1],
                                                               localLandmarks[2 * 12], localLandmarks[2 * 12 + 1]) / faceWidth;
        AvatarScript.avatar4.visage.distanceButtomCurve = DistanceEuclidienne(localLandmarks[2 * 5], localLandmarks[2 * 5 + 1],
                                                               localLandmarks[2 * 11], localLandmarks[2 * 11 + 1]) / faceWidth;
    }

    public void LandmarkAnalyserAvatar5()
    {
        // Dimension visage
        float faceHeight = FaceDetectionImage.DistanceEuclidienne(localLandmarks[2 * 8], localLandmarks[2 * 8 + 1],
                                                               localLandmarks[2 * 27], localLandmarks[2 * 27 + 1]);
        Debug.Log("face height : " + faceHeight);

        float faceWidth = FaceDetectionImage.DistanceEuclidienne(localLandmarks[2 * 1], localLandmarks[2 * 1 + 1],
                                                           localLandmarks[2 * 15], localLandmarks[2 * 15 + 1]);
        Debug.Log("face width : " + faceWidth);


        // Partie eye
        AvatarScript.avatar5.eye.distanceBrowEye = DistanceEuclidienne(localLandmarks[2 * 19], localLandmarks[2 * 19 + 1],
                                                          localLandmarks[2 * 37], localLandmarks[2 * 37 + 1]) / faceHeight;
        AvatarScript.avatar5.eye.eyeWidth = DistanceEuclidienne(localLandmarks[2 * 36], localLandmarks[2 * 36 + 1],
                                                                localLandmarks[2 * 39], localLandmarks[2 * 39 + 1]) / faceWidth;

        if (AvatarScript.avatar5.eye.eyeWidth <= 0.22f)
            AvatarScript.avatar5.eye.width = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar5.eye.width = AvatarScript.Taille.Big;


        // Partie nose
        AvatarScript.avatar5.nose.noseHeight = DistanceEuclidienne(localLandmarks[2 * 27], localLandmarks[2 * 27 + 1],
                                                               localLandmarks[2 * 30], localLandmarks[2 * 30 + 1]) / faceHeight;
        AvatarScript.avatar5.nose.noseWidth = DistanceEuclidienne(localLandmarks[2 * 31], localLandmarks[2 * 31 + 1],
                                                               localLandmarks[2 * 35], localLandmarks[2 * 35 + 1]) / faceWidth;

        if (AvatarScript.avatar5.nose.noseHeight <= 0.39)
            AvatarScript.avatar5.nose.height = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar5.nose.height = AvatarScript.Taille.Big;

        if (AvatarScript.avatar5.nose.noseWidth <= 0.215)
            AvatarScript.avatar5.nose.width = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar5.nose.width = AvatarScript.Taille.Big;

        AvatarScript.avatar5.nose.noseTipHeight = DistanceEuclidienne(localLandmarks[2 * 30], localLandmarks[2 * 30 + 1],
                                         localLandmarks[2 * 33], localLandmarks[2 * 33 + 1]) / AvatarScript.avatar1.nose.noseHeight;
        // Debut algo de prise de decision pour les differents nez
        if (AvatarScript.avatar5.nose.noseTipHeight > 0.38)
        {
            AvatarScript.avatar5.nose.noseTipInclinaison = AvatarScript.NoseTipInclinaison.NezRemonte;
        }
        else if (AvatarScript.avatar5.nose.noseTipHeight < 0.23)
        {
            AvatarScript.avatar5.nose.noseTipInclinaison = AvatarScript.NoseTipInclinaison.NezAbaisse;
        }
        else
        {
            AvatarScript.avatar5.nose.noseTipInclinaison = AvatarScript.NoseTipInclinaison.NezNormal;
        }
        if (AvatarScript.avatar5.nose.noseTipHeight <= 0.305)
        {
            AvatarScript.avatar5.nose.noseTipType = AvatarScript.NoseTipType.NezRond;
        }
        else
        {
            AvatarScript.avatar5.nose.noseTipType = AvatarScript.NoseTipType.NezPointue;
        }


        // Partie mouth
        AvatarScript.avatar5.mouth.distanceBetweenChinAndMouth = DistanceEuclidienne(localLandmarks[2 * 8], localLandmarks[2 * 8 + 1],
                                                                                     localLandmarks[2 * 57], localLandmarks[2 * 57 + 1]) / faceHeight;
        AvatarScript.avatar5.mouth.distanceBetweenNoseTipAndMouth = DistanceEuclidienne(localLandmarks[2 * 33], localLandmarks[2 * 33 + 1],
                                                               localLandmarks[2 * 51], localLandmarks[2 * 51 + 1]) / faceHeight;

        AvatarScript.avatar5.mouth.buttomLipHeight = DistanceEuclidienne(localLandmarks[2 * 66], localLandmarks[2 * 66 + 1],
                                                               localLandmarks[2 * 57], localLandmarks[2 * 57 + 1]) / faceHeight;
        AvatarScript.avatar5.mouth.topLipHeight = DistanceEuclidienne(localLandmarks[2 * 52], localLandmarks[2 * 52 + 1],
                                                               localLandmarks[2 * 63], localLandmarks[2 * 63 + 1]) / faceHeight;



        if (AvatarScript.avatar5.mouth.buttomLipHeight <= 0.09)
            AvatarScript.avatar5.mouth.buttomLipHeight_t = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar5.mouth.buttomLipHeight_t = AvatarScript.Taille.Big;

        if (AvatarScript.avatar5.mouth.topLipHeight <= 0.055)
            AvatarScript.avatar5.mouth.topLipHeight_t = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar5.mouth.topLipHeight_t = AvatarScript.Taille.Big;

        AvatarScript.avatar5.mouth.mouthWidth = DistanceEuclidienne(localLandmarks[2 * 48], localLandmarks[2 * 48 + 1],
                                                               localLandmarks[2 * 54], localLandmarks[2 * 54 + 1]) / faceWidth;

        if (AvatarScript.avatar5.mouth.mouthWidth <= 0.40)
            AvatarScript.avatar5.mouth.width = AvatarScript.Taille.Little;
        else
            AvatarScript.avatar5.mouth.width = AvatarScript.Taille.Big;

        // Partie visage curve
        AvatarScript.avatar5.visage.cornerChinWidth = DistanceEuclidienne(localLandmarks[2 * 4], localLandmarks[2 * 4 + 1],
                                                               localLandmarks[2 * 12], localLandmarks[2 * 12 + 1]) / faceWidth;
        AvatarScript.avatar5.visage.distanceButtomCurve = DistanceEuclidienne(localLandmarks[2 * 5], localLandmarks[2 * 5 + 1],
                                                               localLandmarks[2 * 11], localLandmarks[2 * 11 + 1]) / faceWidth;
    }

    // close the opencv window
    public void OnDestroy()
    {
        Cv2.DestroyAllWindows();

    }


}
