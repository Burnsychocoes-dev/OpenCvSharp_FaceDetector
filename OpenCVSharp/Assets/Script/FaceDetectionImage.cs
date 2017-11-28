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

    // Variables des rectangles de decoupe pour la peau
    float rectShapeHeight = 50;
    OpenCvSharp.Rect rectFront;
    OpenCvSharp.Rect rectEyeLeft;
    OpenCvSharp.Rect rectEyeRight;

    // Video size
    private const int imWidth = 1280;
    private const int imHeight = 720;
    private int imFrameRate;

    // OpenCVSharp parameters
    private Mat videoSourceImage;
    private Mat cannyImage;
    private Texture2D processedTexture;
    private Vec3b[] videoSourceImageData;
    private byte[] cannyImageData;

    // Frame rate parameter
    private int updateFrameCount = 0;
    private int textureCount = 0;
    private int displayCount = 0;
    
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
    }



    void Update()
    {

        updateFrameCount++;

        // convert texture of original video to OpenCVSharp Mat object
        TextureToMat();

        // create the canny edge image out of source image
        ProcessImage(videoSourceImage);

        // update the opencv window of source video
        UpdateWindow(videoSourceImage);

        // convert the OpenCVSharp Mat of canny image to Texture2D
        // the texture will be displayed automatically
        MatToTexture();


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
                //byte vec = cannyImageData[j + i * imWidth];
                //var color32 = new Color32
                //{
                //    r = vec,
                //    g = vec,
                //    b = vec,
                //};
                //byte vec = cannyImageData[j + i * imWidth];
                if (coordX > rectFront.X && coordX < rectFront.X + rectFront.Width &&
                    coordY > rectFront.Y && coordY < rectFront.Y + rectFront.Height)
                {                  
                    var color32 = new Color32
                    {
                        r = 0,
                        g = 0,
                        b = 0,


                        a = 0
                    };
                    c[j + i * imWidth] = color32;
                }
                else if (coordX > rectEyeLeft.X && coordX < rectEyeLeft.X + rectEyeLeft.Width &&
                    coordY > rectEyeLeft.Y && coordY < rectEyeLeft.Y + rectEyeLeft.Height)
                {                 
                    var color32 = new Color32
                    {
                        r = 0,
                        g = 0,
                        b = 0,


                        a = 0
                    };
                    c[j + i * imWidth] = color32;
                }
                else if (coordX > rectEyeRight.X && coordX < rectEyeRight.X + rectEyeRight.Width &&
                    coordY > rectEyeRight.Y && coordY < rectEyeRight.Y + rectEyeRight.Height)
                {
                    var color32 = new Color32
                    {
                        r = 0,
                        g = 0,
                        b = 0,


                        a = 0
                    };
                    c[j + i * imWidth] = color32;
                }
                else
                {
                    Vec3b vec = videoSourceImageData[j + i * imWidth];
                    var color32 = new Color32
                    {
                        r = vec.Item0,
                        g = vec.Item1,
                        b = vec.Item2,


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
            Cv2.Rectangle(_image, faceRect, facec_rectangle_color, 3);

            rectFront = new OpenCvSharp.Rect(faceRect.X + faceRect.Width/2 - 25, faceRect.Y + 50, 50, 50);
            Cv2.Rectangle(_image, rectFront, global_rectangle_color, 3);



            var detectedFaceGrayImage = new Mat();
            Cv2.CvtColor(detectedFaceImage, detectedFaceGrayImage, ColorConversionCodes.BGRA2GRAY);

            var eyes = eye_cascade.DetectMultiScale(
                image: grayImage,
                scaleFactor: 1.3,
                minNeighbors: 5,
                flags: HaarDetectionType.DoRoughSearch | HaarDetectionType.ScaleImage,
                minSize: new Size(50, 50)
            );

            var eye_count = 0;
            foreach (var eyeRect in eyes)
            {
                var detectedEyeImage = new Mat(_image, eyeRect);
                //Cv2.ImShow(string.Format("Face {0}", eye_count), detectedEyeImage);
                //Cv2.WaitKey(1); // do events

                var eye_rectangle_color = Scalar.FromRgb(0, 255, 0);
                Cv2.Rectangle(_image, eyeRect, eye_rectangle_color, 3);

                if(eye_count == 1)
                {
                    // Par rapport à la position de l'oeil gauche
                    rectEyeLeft = new OpenCvSharp.Rect(eyeRect.X + 10, eyeRect.Y + 100, 50, 50);
                    Cv2.Rectangle(_image, rectEyeLeft, global_rectangle_color, 3);
                }
                else
                {
                    // Par rapport à la position de l'oeil droit
                    rectEyeRight = new OpenCvSharp.Rect(eyeRect.X + 35, eyeRect.Y + 100, 50, 50);
                    Cv2.Rectangle(_image, rectEyeRight, global_rectangle_color, 3);
                }
                


                var detectedEyeGrayImage = new Mat();
                Cv2.CvtColor(detectedEyeImage, detectedEyeGrayImage, ColorConversionCodes.BGRA2GRAY);

                eye_count++;
            }
            face_count++;
        }
        //Debug.Log(face_count);
        //if (face_count == 1 && !waitSoundEffect)
        //{
        //    //Debug.Log(faces[0]);
        //    //Debug.Log(meshRendererCenter.x);
        //    //Debug.Log((int)meshRendererCenter.y + 50);
        //    Point origin = faces[0].Location;
        //    float width = faces[0].Width;
        //    float height = faces[0].Height;
        //    // Verification si le rect de la face est bien dans la zone de photo
        //    if(origin.X > (int)meshRendererCenter.x + 350 && 
        //        origin.X + width < (int)meshRendererCenter.x + 350 + 600 &&
        //        origin.Y > (int)meshRendererCenter.y + 50 &&
        //        origin.Y + height < (int)meshRendererCenter.y + 5 + 600 &&
        //        width > 300 &&
        //        height > 300)
        //    {
        //        Debug.Log("Take photo !");
        //        //TakePhoto();
        //        soundEffects.MakePhotoSound();
        //        waitSoundEffect = true;
        //    }
        //}



        //Cv2.ImShow("Haar Detection", _image);
        //Cv2.WaitKey(1); // do events
    }


    //void TakePhoto()
    //{
    //    Texture2D photo = new Texture2D(_webcamTexture.width, _webcamTexture.height);
    //    photo.SetPixels(_webcamTexture.GetPixels());
    //    photo.Apply();

    //    // Encoding the photo to a PNG
    //    byte[] bytes = photo.EncodeToPNG();

    //    // Write out the PNG in the mention path
    //    File.WriteAllBytes("C:/Users/steph/OneDrive/Documents/GitHub/OpenCvSharp_FaceDetector/OpenCVSharp/Assets/" + "photo.png", bytes);
    //}


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
