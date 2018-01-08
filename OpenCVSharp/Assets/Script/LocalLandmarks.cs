using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;

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



    private FaceDetectionImage faceDetectionImage;

    public void Init()
    {
        faceDetectionImage = GetComponent<FaceDetectionImage>();
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
