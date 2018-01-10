using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using OpenCvSharp;
using System;
using System.Linq;

public class LocalLandmarks : MonoBehaviour
{
    //mouth landmarks
    public Vec2i mouth_bottom;
    public Vec2i mouth_top;
    public Vec2i mouth_middle;
    public Vec2i mouth_left;
    public Vec2i mouth_right;

    //Eyes landmarks
    public Vec2i eye_bottom;
    public Vec2i eye_top;
    public Vec2i eye_left;
    public Vec2i eye_right;

    public struct Vec3f
    {
        float x;
        float y;
        float z;
    }

    public float[] landmarks;

    [SerializeField]
    private string imagePath;


    private FaceDetectionImage faceDetectionImage;


    // The imported function
    [DllImport("face_landmark_detection_ex", EntryPoint = "FaceLandmarkDetection")] public static extern int Test(String datPath, String filePath, float[] landmarks);

    



    public void Init()
    {
       

    }

    void Start()
    {
        Debug.Log(Test("Assets/Plugins/Dlib/shape_predictor_68_face_landmarks.dat", "Assets/photo.png", landmarks));
        //Test();
    }

    public void mouth_landmarks()
    {
        OpenCvSharp.Rect rectMouth = faceDetectionImage.RectMouth;

        //Calcul d'un intervalle (Cb,Cr) pour le haut de rectMouth

        //Calcul d'un intervalle (Cb,Cr) pour le bas de rectMouth

        //Calcul d'un intervalle (Cb,Cr) pour la gauche de rectMouth

        //Calcul d'un intervalle (Cb,Cr) pour la droite de rectMouth




        //Calcul de mouth_bottom, mouth_middle et mouth_top en parcourant
        //rectMouth de haut en bas

    }

    public void eye_landmarks()
    {

    }

}





//var win = new ImageWindow();
//var winFaces = new ImageWindow();
//var detector = FrontalFaceDetector.GetFrontalFaceDetector();
//var sp = new ShapePredictor(Application.dataPath + "/Plugins/Dlib/shape_predictor_68_face_landmarks.dat");
//var file = "image.png";

////Console.WriteLine($"processing image {file}");

//using (var img = Dlib.LoadImage<RgbPixel>(file))
//{
//    Dlib.PyramidUp(img);

//    var dets = detector.Detect(img);
//    //Console.WriteLine($"Number of faces detected: {dets.Length}");

//    var shapes = new List<FullObjectDetection>();
//    foreach (var rect in dets)
//    {
//        var shape = sp.Detect(img, rect);
//        //Console.WriteLine($"number of parts: {shape.Parts}");
//        if (shape.Parts > 2)
//        {
//            //Console.WriteLine($"pixel position of first part:  {shape.GetPart(0)}");
//            //Console.WriteLine($"pixel position of second part: {shape.GetPart(1)}");
//            shapes.Add(shape);
//        }
//    }

//    win.ClearOverlay();
//    win.SetImage(img);

//    if (shapes.Any())
//    {
//        var lines = Dlib.RenderFaceDetections(shapes);
//        win.AddOverlay(lines);

//        foreach (var l in lines)
//            l.Dispose();

//        var chipLocations = Dlib.GetFaceChipDetails(shapes);
//        var faceChips = Dlib.ExtractImageChips<RgbPixel>(img, chipLocations);
//        var tileImage = Dlib.TileImages(faceChips);
//        winFaces.SetImage(tileImage);

//        foreach (var c in chipLocations)
//            c.Dispose();
//    }

//    //Console.WriteLine("hit enter to process next frame");
//    //Console.ReadKey();

//    foreach (var s in shapes)
//        s.Dispose();
//}