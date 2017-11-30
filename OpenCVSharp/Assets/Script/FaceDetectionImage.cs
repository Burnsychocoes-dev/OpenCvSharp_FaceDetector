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


// class for video display and processed video display
// current process is canny edge detection
public class FaceDetectionImage : MonoBehaviour
{
    SoundEffectsHelper soundEffects;
    private int frameCount = 0;
    [SerializeField]
    private int maxCount = 3;
    private bool waitSoundEffect = false;

    // Video parameters
    private Image image;
    private Texture2D imageTexture;
    public MeshRenderer ProcessedTextureRenderer;

    // Variables des rectangles de decoupe
    float rectShapeHeight = 50;
    OpenCvSharp.Rect rectFront;
    public OpenCvSharp.Rect RectFront
    {
        get { return rectFront; }
    }
    OpenCvSharp.Rect rectEyeLeft;
    public OpenCvSharp.Rect RectEyeLeft
    {
        get { return rectEyeLeft; }
    }
    OpenCvSharp.Rect rectEyeRight;
    public OpenCvSharp.Rect RectEyeRight
    {
        get { return rectEyeRight; }
    }
    OpenCvSharp.Rect face;
    public OpenCvSharp.Rect Face
    {
        get { return face; }
    }
    OpenCvSharp.Rect rectCheveux;
    OpenCvSharp.Rect rectMouth;

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

    void Start()
    {
        image = FindObjectOfType<Image>();
        imageTexture = (Texture2D) image.mainTexture;
        
        // initialize video / image with given size
        videoSourceImage = new Mat(imHeight, imWidth, MatType.CV_8UC3);
        videoSourceImageData = new Vec3b[imHeight * imWidth];
        cannyImage = new Mat(imHeight, imWidth, MatType.CV_8UC1);
        cannyImageData = new byte[imHeight * imWidth];

        // create processed video texture as Texture2D object
        processedTexture = new Texture2D(imWidth, imHeight, TextureFormat.RGBA32, true, true);

        // assign the processedTexture to the meshrenderer for display
        ProcessedTextureRenderer.material.mainTexture = processedTexture;

        updateFrameCount++;

        // convert texture of original video to OpenCVSharp Mat object
        TextureToMat();

        // create the canny edge image out of source image
        ProcessImage(videoSourceImage);

        // update the opencv window of source video
        UpdateWindow(videoSourceImage);

        CalculateSkinColor();

        //DrawTheLineSeparatingHairAndSkin();

        // update the opencv window of source video
        //UpdateWindow(videoSourceImage);

        //CalculateHairColor();

        // convert the OpenCVSharp Mat of canny image to Texture2D
        // the texture will be displayed automatically
        MatToTexture();


        Debug.Log("Couleur de la peau au niveau du front");
        Debug.Log(couleurPeauFront.Item0);
        Debug.Log(couleurPeauFront.Item1);
        Debug.Log(couleurPeauFront.Item2);
        Debug.Log("Couleur de la peau au niveau de l'oeil droit");
        Debug.Log(couleurPeauBasOeilDroit.Item0);
        Debug.Log(couleurPeauBasOeilDroit.Item1);
        Debug.Log(couleurPeauBasOeilDroit.Item2);
        Debug.Log("Couleur de la peau au niveau de l'oeil gauche");
        Debug.Log(couleurPeauBasOeilGauche.Item0);
        Debug.Log(couleurPeauBasOeilGauche.Item1);
        Debug.Log(couleurPeauBasOeilGauche.Item2);
        Debug.Log("Couleur des cheveux");
        Debug.Log(couleurCheveux.Item0);
        Debug.Log(couleurCheveux.Item1);
        Debug.Log(couleurCheveux.Item2);
    }



    void Update()
    {
        //updateFrameCount++;

        //// convert texture of original video to OpenCVSharp Mat object
        //TextureToMat();

        //// create the canny edge image out of source image
        //ProcessImage(videoSourceImage);

        //// update the opencv window of source video
        //UpdateWindow(videoSourceImage);

        //// convert the OpenCVSharp Mat of canny image to Texture2D
        //// the texture will be displayed automatically
        //MatToTexture();


        //DrawTheLineSeparatingHairAndSkin();


        //MatToTexture();
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
    void MatToTexture()
    {
        // cannyImageData is byte array, because canny image is grayscale

        cannyImage.GetArray(0, 0, cannyImageData);

        //cannyImage.GetArray(0, 0, cannyImageData);
        videoSourceImage.GetArray(0, 0, videoSourceImageData);

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

                if (coordX > rectFront.X && coordX < rectFront.X + rectFront.Width &&
                    coordY > rectFront.Y && coordY < rectFront.Y + rectFront.Height)
                {
                    var color32 = new Color32
                    {
                        r = 255,
                        g = 255,
                        b = 255,


                        a = 0
                    };
                    c[j + i * imWidth] = color32;
                }
                else if (coordX > rectEyeLeft.X && coordX < rectEyeLeft.X + rectEyeLeft.Width &&
                    coordY > rectEyeLeft.Y && coordY < rectEyeLeft.Y + rectEyeLeft.Height)
                {
                    var color32 = new Color32
                    {
                        r = 255,
                        g = 255,
                        b = 255,


                        a = 0
                    };
                    c[j + i * imWidth] = color32;
                }
                else if (coordX > rectEyeRight.X && coordX < rectEyeRight.X + rectEyeRight.Width &&
                    coordY > rectEyeRight.Y && coordY < rectEyeRight.Y + rectEyeRight.Height)
                {
                    var color32 = new Color32
                    {
                        r = 255,
                        g = 255,
                        b = 255,


                        a = 0
                    };
                    c[j + i * imWidth] = color32;
                }
                else if (coordX > rectCheveux.X && coordX < rectCheveux.X + rectCheveux.Width &&
                     coordY > rectCheveux.Y && coordY < rectCheveux.Y + rectCheveux.Height)
                {
                    var color32 = new Color32
                    {
                        r = 255,
                        g = 255,
                        b = 255,


                        a = 0
                    };
                    c[j + i * imWidth] = color32;
                }
                else
                {
                    var color32 = new Color32
                    {
                        r = vec.Item2,
                        g = vec.Item1,
                        b = vec.Item0,


                        a = 0
                    };
                    c[j + i * imWidth] = color32;
                }
            }
        });

        processedTexture.SetPixels32(c);
        // to update the texture, OpenGL manner
        processedTexture.Apply();
    }


    void CalculateSkinColor()
    {
        // Variables de calcules de moyennne
        int leftCompteur = 0;
        float leftEyeSideSkinRedColorAverage = 0f;
        float leftEyeSideSkinGreenColorAverage = 0f;
        float leftEyeSideSkinBlueColorAverage = 0f;

        int rightCompteur = 0;
        float rightEyeSideSkinRedColorAverage = 0f;
        float rightEyeSideSkinGreenColorAverage = 0f;
        float rightEyeSideSkinBlueColorAverage = 0f;

        int frontCompteur = 0;
        float frontSideSkinRedColorAverage = 0f;
        float frontSideSkinGreenColorAverage = 0f;
        float frontSideSkinBlueColorAverage = 0f;

        // parallel for loop
        Parallel.For(0, imHeight, i =>
        {
            for (var j = 0; j < imWidth; j++)
            {
                // Ne jamais mettre de debug ici !!!

                float coordX = j;
                float coordY = imHeight - i;

                Vec3b vec = videoSourceImageData[j + i * imWidth];

                if (coordX > rectFront.X && coordX < rectFront.X + rectFront.Width &&
                    coordY > rectFront.Y && coordY < rectFront.Y + rectFront.Height)
                {
                    frontCompteur++;
                    frontSideSkinRedColorAverage += vec.Item2;
                    frontSideSkinGreenColorAverage += vec.Item1;
                    frontSideSkinBlueColorAverage += vec.Item0;
                }
                else if (coordX > rectEyeLeft.X && coordX < rectEyeLeft.X + rectEyeLeft.Width &&
                    coordY > rectEyeLeft.Y && coordY < rectEyeLeft.Y + rectEyeLeft.Height)
                {
                    leftCompteur++;
                    leftEyeSideSkinRedColorAverage += vec.Item2;
                    leftEyeSideSkinGreenColorAverage += vec.Item1;
                    leftEyeSideSkinBlueColorAverage += vec.Item0;
                }
                else if (coordX > rectEyeRight.X && coordX < rectEyeRight.X + rectEyeRight.Width &&
                    coordY > rectEyeRight.Y && coordY < rectEyeRight.Y + rectEyeRight.Height)
                {
                    rightCompteur++;
                    rightEyeSideSkinRedColorAverage += vec.Item2;
                    rightEyeSideSkinGreenColorAverage += vec.Item1;
                    rightEyeSideSkinBlueColorAverage += vec.Item0;
                }
            }
        });

        // Front skin color
        frontSideSkinRedColorAverage = frontSideSkinRedColorAverage / frontCompteur;
        frontSideSkinGreenColorAverage = frontSideSkinGreenColorAverage / frontCompteur;
        frontSideSkinBlueColorAverage = frontSideSkinBlueColorAverage / frontCompteur;
        couleurPeauFront = new Vec3f(frontSideSkinRedColorAverage, frontSideSkinGreenColorAverage, frontSideSkinBlueColorAverage);
        Debug.Log(frontCompteur);

        // Right eye side skin color
        rightEyeSideSkinRedColorAverage = rightEyeSideSkinRedColorAverage / rightCompteur;
        rightEyeSideSkinGreenColorAverage = rightEyeSideSkinGreenColorAverage / rightCompteur;
        rightEyeSideSkinBlueColorAverage = rightEyeSideSkinBlueColorAverage / rightCompteur;
        couleurPeauBasOeilDroit = new Vec3f(rightEyeSideSkinRedColorAverage, rightEyeSideSkinGreenColorAverage, rightEyeSideSkinBlueColorAverage);
        Debug.Log(rightCompteur);

        // Left eye side skin color
        leftEyeSideSkinRedColorAverage = leftEyeSideSkinRedColorAverage / leftCompteur;
        leftEyeSideSkinGreenColorAverage = leftEyeSideSkinGreenColorAverage / leftCompteur;
        leftEyeSideSkinBlueColorAverage = leftEyeSideSkinBlueColorAverage / leftCompteur;
        couleurPeauBasOeilGauche = new Vec3f(leftEyeSideSkinRedColorAverage, leftEyeSideSkinGreenColorAverage, leftEyeSideSkinBlueColorAverage);
        Debug.Log(leftCompteur);
    }


    void CalculateHairColor()
    {
        int hairCompteur = 0;
        float hairRedColorAverage = 0f;
        float hairGreenColorAverage = 0f;
        float hairBlueColorAverage = 0f;

        // parallel for loop
        Parallel.For(0, imHeight, i =>
        {
            for (var j = 0; j < imWidth; j++)
            {
                float coordX = j;
                float coordY = imHeight - i;

                Vec3b vec = videoSourceImageData[j + i * imWidth];

                if (coordX > rectCheveux.X && coordX < rectCheveux.X + rectCheveux.Width &&
                     coordY > rectCheveux.Y && coordY < rectCheveux.Y + rectCheveux.Height)
                {
                    hairCompteur++;
                    hairRedColorAverage += vec.Item2;
                    hairGreenColorAverage += vec.Item1;
                    hairBlueColorAverage += vec.Item0;
                }
            }
        });

        // hair color
        hairRedColorAverage = hairRedColorAverage / hairCompteur;
        hairGreenColorAverage = hairGreenColorAverage / hairCompteur;
        hairBlueColorAverage = hairBlueColorAverage / hairCompteur;
        couleurCheveux = new Vec3f(hairRedColorAverage, hairGreenColorAverage, hairBlueColorAverage);
        Debug.Log(hairCompteur);
    }


    void DrawTheLineSeparatingHairAndSkin()
    {
        Cv2.Flip(videoSourceImage, videoSourceImage, FlipMode.X);

        // parallel for loop
        Parallel.For(0, imHeight, i =>
        {
            for (var j = 0; j < imWidth; j++)
            {
                float coordX = j;
                float coordY = imHeight - i;

                Vec3b vec = videoSourceImageData[j + i * imWidth];

                if(coordX == rectFront.X && coordY < rectFront.Y)
                {
                    if(Mathf.Abs(vec.Item2 - couleurPeauFront.Item0) < margeErreurCouleurPeau &&
                        Mathf.Abs(vec.Item1 - couleurPeauFront.Item1) < margeErreurCouleurPeau &&
                        Mathf.Abs(vec.Item0 - couleurPeauFront.Item2) < margeErreurCouleurPeau)
                    {
                        if(!isFind)
                        {
                            Debug.Log("jai reussi !");
                            if(coordY > maxCoordY)
                            {
                                maxCoordY = coordY;
                            }
                            isFind = true;
                        }
                        else
                        {
                            
                        }
                    }
                }
            }
        });

        Debug.Log("minCoordY");
        Debug.Log(maxCoordY);
        //Debug.Log(rectFront.Y);
        var lineColor = Scalar.FromRgb(0, 0, 255);
        Cv2.Line(videoSourceImage, face.X, (int)maxCoordY, face.X + face.Width, (int)maxCoordY, lineColor);
        rectCheveux = new OpenCvSharp.Rect(rectFront.X, (int)maxCoordY, 25, 25);
        Cv2.Rectangle(videoSourceImage, rectCheveux, lineColor, 3);
    }


    // Simple example of canny edge detect
    void ProcessImage(Mat _image)
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
        var mouth_count = 0;
        var eye_count = 0;
        foreach (var faceRect in faces)
        {
            var detectedFaceImage = new Mat(_image, faceRect);
            //Cv2.ImShow(string.Format("Face {0}", face_count), detectedFaceImage);
            //Cv2.WaitKey(1); // do events

            var facec_rectangle_color = Scalar.FromRgb(255, 0, 0);
            face = faceRect;
            Cv2.Rectangle(_image, faceRect, facec_rectangle_color, 3);


            rectFront = new OpenCvSharp.Rect(faceRect.X + faceRect.Width/2 - 50, faceRect.Y + 50, 100, 50);
            //Cv2.Rectangle(_image, rectFront, global_rectangle_color, 3);



            var detectedFaceGrayImage = new Mat();
            Cv2.CvtColor(detectedFaceImage, detectedFaceGrayImage, ColorConversionCodes.BGRA2GRAY);

            var eyes = eye_cascade.DetectMultiScale(
                image: grayImage,
                scaleFactor: 1.3,
                minNeighbors: 5,
                flags: HaarDetectionType.DoRoughSearch | HaarDetectionType.ScaleImage,
                minSize: new Size(50, 50)
            );

            
            foreach (var eyeRect in eyes)
            {
                var detectedEyeImage = new Mat(_image, eyeRect);
                //Cv2.ImShow(string.Format("Face {0}", eye_count), detectedEyeImage);
                //Cv2.WaitKey(1); // do events

                var eye_rectangle_color = Scalar.FromRgb(0, 255, 0);
                //Cv2.Rectangle(_image, eyeRect, eye_rectangle_color, 3);

                if(eye_count == 1)
                {
                    // Par rapport à la position de l'oeil gauche
                    rectEyeLeft = new OpenCvSharp.Rect(eyeRect.X + 75, eyeRect.Y + 100, 25, 25);
                    //Cv2.Rectangle(_image, rectEyeLeft, global_rectangle_color, 3);
                }
                else
                {
                    // Par rapport à la position de l'oeil droit
                    rectEyeRight = new OpenCvSharp.Rect(eyeRect.X, eyeRect.Y + 100, 25, 25);
                    //Cv2.Rectangle(_image, rectEyeRight, global_rectangle_color, 3);
                }
                


                var detectedEyeGrayImage = new Mat();
                Cv2.CvtColor(detectedEyeImage, detectedEyeGrayImage, ColorConversionCodes.BGRA2GRAY);

                eye_count++;
            }


            var mouth = mouth_cascade.DetectMultiScale(
              image: grayImage,
              scaleFactor: 1.3,
              minNeighbors: 5,
              flags: HaarDetectionType.DoRoughSearch | HaarDetectionType.ScaleImage,
              minSize: new Size(50, 50)
            );
            foreach (var m in mouth)
            {
                var detectedEarImage = new Mat(_image, m);
                //Cv2.ImShow(string.Format("Face {0}", eye_count), detectedEyeImage);
                //Cv2.WaitKey(1); // do events

                if(m.Y > eyes[0].Y && Mathf.Abs(m.Y - eyes[0].Y) > 100)
                {
                    //Debug.Log("mouth height :");
                    //Debug.Log(m.Height);
                    var eye_rectangle_color = Scalar.FromRgb(0, 255, 0);
                    Cv2.Rectangle(_image, m, eye_rectangle_color, 3);
                    lipHeight = (float)m.Height / (float)face.Height;
                }

                var detectedEyeGrayImage = new Mat();
                Cv2.CvtColor(detectedEarImage, detectedEyeGrayImage, ColorConversionCodes.BGRA2GRAY);

                mouth_count++;
            }

            face_count++;
        }
    }



    // Display the original video in a opencv window
    void UpdateWindow(Mat _image)
    {
        Cv2.Flip(_image, _image, FlipMode.X);
        //Cv2.ImShow("Copy video", _image);
        displayCount++;
    }

    // close the opencv window
    public void OnDestroy()
    {
        Cv2.DestroyAllWindows();

    }


}
