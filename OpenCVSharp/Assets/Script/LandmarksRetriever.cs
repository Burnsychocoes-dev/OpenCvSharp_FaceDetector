﻿using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using LitJson;
using OpenCvSharp;

public class LandmarksRetriever : MonoBehaviour {

    [SerializeField]
    private string apiEndpoint;

    [SerializeField]
    private string imagePath;
	
	public JsonData landmarks;

    // Information sur le visage
    private string gender;
    private double faceHeight;
    private double faceWidth;
    private double distanceBetweenLipAndChin;

    // Information sur les yeux
    private Vec2d leftEyeCenter;
    private Vec2d rightEyeCenter;
    private double leftEyeWidth;
    private double rightEyeWidth;
    private Vec2d leftEyeBrowLeft;
    private Vec2d leftEyeBrowMiddle;
    private Vec2d leftEyeBrowRight;
    private Vec2d rightEyeBrowLeft;
    private Vec2d rightEyeBrowMiddle;
    private Vec2d rightEyeBrowRight;
    private double distanceBetweenNoseTopAndEyes;

    // Information sur le nez
    private double distanceBetweenNoseTipAndLip;
    private double noseHeight;
    private double noseWidth;
    private double nostrilThickness;
    
    // Information sur les oreilles
    private double distanceBetweenRightEarTragusAndNoseTip;
    private double distanceBetweenLeftEarTragusAndNoseTip;

    // Information sur la lèvre
    private double lipWidth;
    private double topLipHeight;
    private double buttomLipHeight;


    public static LandmarksRetriever Instance {
        get;
        private set;
    }

    private static HttpWebRequest client;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There is multiple instance of singleton LandmarksRetriever");
            return;
        }
        client = (HttpWebRequest)WebRequest.Create(apiEndpoint);
        client.Method = "POST";
        client.KeepAlive = true;
        client.Credentials = CredentialCache.DefaultCredentials;

        Instance = this;
    }

    void Start() {
        RetrieveLandmarks();

        // Récupération des infos sur le visages
        gender = (string)landmarks["attributes"]["gender"]["type"];
        Debug.Log(gender);
        if (gender == "M")
        {
            Debug.Log("c'est un homme");
        }
        else
        {
            Debug.Log("C'est une femmme");
        }

        distanceBetweenLipAndChin = Math.Abs((double)landmarks["lipLineMiddleY"] - (double)landmarks["chinTipY"]);
        Debug.Log("distance between lip and chin :");
        Debug.Log(distanceBetweenLipAndChin);


        // Récuperation des infos sur les yeux
        leftEyeCenter.Item0 = (double)landmarks["leftEyeCenterX"];
        leftEyeCenter.Item1 = (double)landmarks["leftEyeCenterY"];
        Debug.Log("left eye center position :");
        Debug.Log(leftEyeCenter.Item0);
        Debug.Log(leftEyeCenter.Item1);

        rightEyeCenter.Item0 = (double)landmarks["rightEyeCenterX"];
        rightEyeCenter.Item1 = (double)landmarks["rightEyeCenterY"];
        Debug.Log("right eye center position :");
        Debug.Log(rightEyeCenter.Item0);
        Debug.Log(rightEyeCenter.Item1);

        leftEyeWidth = Math.Abs((double)landmarks["leftEyeCornerLeftX"] - (double)landmarks["leftEyeCornerRightX"]);
        Debug.Log("left eye width :");
        Debug.Log(leftEyeWidth);

        rightEyeWidth = Math.Abs((double)landmarks["rightEyeCornerLeftX"] - (double)landmarks["rightEyeCornerRightX"]);
        Debug.Log("right eye width :");
        Debug.Log(rightEyeWidth);

        leftEyeBrowLeft.Item0 = (double)landmarks["leftEyeBrowLeftX"];
        leftEyeBrowLeft.Item1 = (double)landmarks["leftEyeBrowLeftY"];
        Debug.Log("left eye brow left position :");
        Debug.Log(leftEyeBrowLeft.Item0);
        Debug.Log(leftEyeBrowLeft.Item1);

        leftEyeBrowMiddle.Item0 = (double)landmarks["leftEyeBrowMiddleX"];
        leftEyeBrowMiddle.Item1 = (double)landmarks["leftEyeBrowMiddleY"];
        Debug.Log("left eye brow middle position :");
        Debug.Log(leftEyeBrowMiddle.Item0);
        Debug.Log(leftEyeBrowMiddle.Item1);

        leftEyeBrowRight.Item0 = (double)landmarks["leftEyeBrowRightX"];
        leftEyeBrowRight.Item1 = (double)landmarks["leftEyeBrowRightY"];
        Debug.Log("left eye brow right position :");
        Debug.Log(leftEyeBrowRight.Item0);
        Debug.Log(leftEyeBrowRight.Item1);

        rightEyeBrowLeft.Item0 = (double)landmarks["rightEyeBrowLeftX"];
        rightEyeBrowLeft.Item1 = (double)landmarks["rightEyeBrowLeftY"];
        Debug.Log("right eye brow left position :");
        Debug.Log(rightEyeBrowLeft.Item0);
        Debug.Log(rightEyeBrowLeft.Item1);

        rightEyeBrowMiddle.Item0 = (double)landmarks["rightEyeBrowMiddleX"];
        rightEyeBrowMiddle.Item1 = (double)landmarks["rightEyeBrowMiddleY"];
        Debug.Log("right eye brow middle position :");
        Debug.Log(rightEyeBrowMiddle.Item0);
        Debug.Log(rightEyeBrowMiddle.Item1);

        rightEyeBrowRight.Item0 = (double)landmarks["rightEyeBrowRightX"];
        rightEyeBrowRight.Item1 = (double)landmarks["rightEyeBrowRightY"];
        Debug.Log("right eye brow right position :");
        Debug.Log(rightEyeBrowRight.Item0);
        Debug.Log(rightEyeBrowRight.Item1);

        distanceBetweenNoseTopAndEyes = Math.Abs((double)landmarks["noseBtwEyesX"] - (double)landmarks["rightEyeCornerLeftX"]);
        Debug.Log("distance between nose and eyes :");
        Debug.Log(distanceBetweenNoseTopAndEyes);


        // Récuperation des infos sur le nez
        distanceBetweenNoseTipAndLip = Math.Abs((double)landmarks["noseTipY"] - (double)landmarks["lipLineMiddleY"]);
        Debug.Log("distance between nose tip and lip :");
        Debug.Log(distanceBetweenNoseTipAndLip);

        noseHeight = Math.Abs((double)landmarks["noseTipY"] - (double)landmarks["noseBtwEyesY"]);
        Debug.Log("nose height :");
        Debug.Log(noseHeight);

        noseWidth = Math.Abs((double)landmarks["nostrilLeftSideX"] - (double)landmarks["nostrilRightSideX"]);
        Debug.Log("nose width :");
        Debug.Log(noseWidth);

        nostrilThickness = Math.Abs((double)landmarks["nostrilRightHoleBottomX"] - (double)landmarks["nostrilRightSideX"]);
        Debug.Log("nostril thickness :");
        Debug.Log(nostrilThickness);


        // Récuperation des infos sur la bouche
        lipWidth = Math.Abs((double)landmarks["lipCornerLeftX"] - (double)landmarks["lipCornerRightX"]);
        Debug.Log("lip width :");
        Debug.Log(lipWidth);
    }

    public string PostRequest(NameValueCollection parameters) {
        string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
        byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
        client.ContentType = "multipart/form-data; boundary=" + boundary;

        Stream rs = client.GetRequestStream();
        string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

        foreach (string paramName in parameters.Keys) {
            rs.Write(boundarybytes, 0, boundarybytes.Length);
            byte[] formitembytes = Encoding.UTF8.GetBytes(string.Format(formdataTemplate, paramName, parameters[paramName]));
            rs.Write(formitembytes, 0, formitembytes.Length);
        }

        rs.Write(boundarybytes, 0, boundarybytes.Length);

        string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
        string header = string.Format(headerTemplate, "image", imagePath, "image/jpeg");
        byte[] headerbytes = Encoding.UTF8.GetBytes(header);
        rs.Write(headerbytes, 0, headerbytes.Length);

        FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
        byte[] buffer = new byte[4096];
        int bytesRead = 0;
        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0) {
            rs.Write(buffer, 0, bytesRead);
        }
        fileStream.Close();

        byte[] trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
        rs.Write(trailer, 0, trailer.Length);
        rs.Close();

        WebResponse wresp = null;
        wresp = client.GetResponse();
        Stream stream2 = wresp.GetResponseStream();
        StreamReader reader2 = new StreamReader(stream2);

        return reader2.ReadToEnd();
    }

    public void RetrieveLandmarks() {
        string jsonResponse = PostRequest(new NameValueCollection() {
                { "api_key", "30e4a976e664b960fe9be4223061168d" },
                { "selector", "SETPOSE" }
            });
        
        landmarks = JsonMapper.ToObject(jsonResponse)["images"][0]["faces"][0];

        
    }

}