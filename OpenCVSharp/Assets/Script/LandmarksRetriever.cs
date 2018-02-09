using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using LitJson;
using OpenCvSharp;

public class LandmarksRetriever : MonoBehaviour {
    private HairDetection hairDetection;

    [SerializeField]
    private string apiEndpoint;

    [SerializeField]
    private string imagePath;
	
	public JsonData landmarks;

    // Information sur le visage
    public string gender;
    public double faceHeight;
    public double faceWidth;
    public double distanceBetweenLipAndChin;

    // Information sur les yeux
    public Vec2d leftEyeCenter;
    public Vec2d rightEyeCenter;
    public double leftEyeWidth;
    public double rightEyeWidth;
    private Vec2d leftEyeBrowLeft;
    public Vec2d LeftEyeBrowLeft
    {
        get { return leftEyeBrowLeft; }
    }
    private Vec2d leftEyeBrowMiddle;
    public Vec2d LeftEyeBrowMiddle
    {
        get { return leftEyeBrowMiddle; }
    }
    private Vec2d leftEyeBrowRight;
    public Vec2d LeftEyeBrowRight
    {
        get { return leftEyeBrowRight; }
    }
    private Vec2d rightEyeBrowLeft;
    public Vec2d RightEyeBrowLeft
    {
        get { return rightEyeBrowLeft; }
    }
    private Vec2d rightEyeBrowMiddle;
    public Vec2d RightEyeBrowMiddle
    {
        get { return rightEyeBrowMiddle; }
    }
    private Vec2d rightEyeBrowRight;
    public Vec2d RightEyeBrowRight
    {
        get { return rightEyeBrowRight; }
    }
    public double distanceBetweenNoseTopAndEyes;
    public double distanceBetweenEyeAndMiddleBrow;
    public double distanceBetweenLeftEyeCornerAndLeftBrowCorner;
    public double distanceBetweenRightEyeCornerAndRightBrowCorner;

    // Information sur le nez
    public double distanceBetweenNoseTipAndLip;
    public double noseHeight;
    public double noseWidth;
    public double nostrilThickness;
    
    // Information sur les oreilles
    public double distanceBetweenRightEarTragusAndNoseTip;
    public double distanceBetweenLeftEarTragusAndNoseTip;

    // Information sur la lèvre
    public double lipWidth;
    public double topLipHeight;
    public double buttomLipHeight;

    // Information sur la courbure du visage7
    public double chinWidth;
    public double cornerChinWidth;
    public double jawCurveAngle;
    public double distanceButtomCurve;

    // Information utile pour hair detection
    private Vec2d leftEyeCorner;
    private Vec2d rightEyeCorner;
    private Vec2d chin;
    public Vec2d Chin
    {
        get { return chin; }
    }

    private Vec2d nose;
    public Vec2d Nose
    {
        get { return nose; }
    }

    private FaceDetectionImage faceAnalyse;

    private bool updateLip = false;


    public static LandmarksRetriever Instance {
        get;
        private set;
    }

    private static HttpWebRequest client;

    private bool isCallDone = false;


    private void Awake() {
        //if (Instance != null) {
        //    Debug.LogError("There is multiple instance of singleton LandmarksRetriever");
        //    return;
        //}
        //client = (HttpWebRequest)WebRequest.Create(apiEndpoint);
        //client.Method = "POST";
        //client.KeepAlive = true;
        //client.Credentials = CredentialCache.DefaultCredentials;

        //Instance = this;

    }

    public void Init() {
        if(!isCallDone)
        {
            isCallDone = true;
            faceAnalyse = GetComponent<FaceDetectionImage>();
            hairDetection = GetComponent<HairDetection>();

            //RetrieveLandmarks();

            // Récupération des infos sur le visages
            //gender = (string)landmarks["attributes"]["gender"]["type"];
            //Debug.Log(gender);
            //if (gender == "M")
            //{
            //    Debug.Log("c'est un homme");
            //}
            //else
            //{
            //    Debug.Log("C'est une femme");
            //}

            faceHeight = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 8], faceAnalyse.localLandmarks[2 * 8 + 1],
                                                               faceAnalyse.localLandmarks[2 * 27], faceAnalyse.localLandmarks[2 * 27 + 1]);
            Debug.Log("face height :");
            Debug.Log(faceHeight);


            faceWidth = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 1], faceAnalyse.localLandmarks[2 * 1 + 1], 
                                                               faceAnalyse.localLandmarks[2 * 15], faceAnalyse.localLandmarks[2 * 15 + 1]);
            Debug.Log("face width :");
            Debug.Log(faceWidth);


            distanceBetweenLipAndChin = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 8], faceAnalyse.localLandmarks[2 * 8 + 1],
                                                               faceAnalyse.localLandmarks[2 * 57], faceAnalyse.localLandmarks[2 * 57 + 1]) / faceHeight;
            Debug.Log("distance between lip and chin :");
            Debug.Log(distanceBetweenLipAndChin);


            // Récuperation des infos sur les yeux
            
            //leftEyeCenter.Item0 = faceAnalyse.localLandmarks[2 * 66];
            //leftEyeCenter.Item1 = faceAnalyse.localLandmarks[2 * 66];
            //Debug.Log("left eye center position :");
            //Debug.Log(leftEyeCenter.Item0);
            //Debug.Log(leftEyeCenter.Item1);

            //rightEyeCenter.Item0 = (double)landmarks["rightEyeCenterX"];
            //rightEyeCenter.Item1 = (double)landmarks["rightEyeCenterY"];
            //Debug.Log("right eye center position :");
            //Debug.Log(rightEyeCenter.Item0);
            //Debug.Log(rightEyeCenter.Item1);

            leftEyeWidth = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 36], faceAnalyse.localLandmarks[2 * 36 + 1],
                                                               faceAnalyse.localLandmarks[2 * 39], faceAnalyse.localLandmarks[2 * 39 + 1]) / faceWidth;
            Debug.Log("left eye width :");
            Debug.Log(leftEyeWidth);

            rightEyeWidth = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 42], faceAnalyse.localLandmarks[2 * 42 + 1],
                                                               faceAnalyse.localLandmarks[2 * 45], faceAnalyse.localLandmarks[2 * 45 + 1]) / faceWidth;
            Debug.Log("right eye width :");
            Debug.Log(rightEyeWidth);

            distanceBetweenEyeAndMiddleBrow = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 37], faceAnalyse.localLandmarks[2 * 37 + 1],
                                                               faceAnalyse.localLandmarks[2 * 19], faceAnalyse.localLandmarks[2 * 19 + 1]) / faceHeight;
            Debug.Log("distance between eye And middle brow :");
            Debug.Log(distanceBetweenEyeAndMiddleBrow);

            distanceBetweenLeftEyeCornerAndLeftBrowCorner = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 36], faceAnalyse.localLandmarks[2 * 36 + 1],
                                                       faceAnalyse.localLandmarks[2 * 17], faceAnalyse.localLandmarks[2 * 17 + 1]) / faceHeight;
            Debug.Log("distance between eye corner And brow corner :");
            Debug.Log(distanceBetweenLeftEyeCornerAndLeftBrowCorner);

            distanceBetweenRightEyeCornerAndRightBrowCorner = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 39], faceAnalyse.localLandmarks[2 * 39 + 1],
                                           faceAnalyse.localLandmarks[2 * 21], faceAnalyse.localLandmarks[2 * 21 + 1]) / faceHeight;
            Debug.Log("distance between eye corner And brow corner :");
            Debug.Log(distanceBetweenRightEyeCornerAndRightBrowCorner);

            //leftEyeBrowLeft.Item0 = (double)landmarks["leftEyeBrowLeftX"];
            //leftEyeBrowLeft.Item1 = (double)landmarks["leftEyeBrowLeftY"];
            //Debug.Log("left eye brow left position :");
            //Debug.Log(leftEyeBrowLeft.Item0);
            //Debug.Log(leftEyeBrowLeft.Item1);

            //leftEyeBrowMiddle.Item0 = (double)landmarks["leftEyeBrowMiddleX"];
            //leftEyeBrowMiddle.Item1 = (double)landmarks["leftEyeBrowMiddleY"];
            //Debug.Log("left eye brow middle position :");
            //Debug.Log(leftEyeBrowMiddle.Item0);
            //Debug.Log(leftEyeBrowMiddle.Item1);

            //leftEyeBrowRight.Item0 = (double)landmarks["leftEyeBrowRightX"];
            //leftEyeBrowRight.Item1 = (double)landmarks["leftEyeBrowRightY"];
            //Debug.Log("left eye brow right position :");
            //Debug.Log(leftEyeBrowRight.Item0);
            //Debug.Log(leftEyeBrowRight.Item1);

            //rightEyeBrowLeft.Item0 = (double)landmarks["rightEyeBrowLeftX"];
            //rightEyeBrowLeft.Item1 = (double)landmarks["rightEyeBrowLeftY"];
            //Debug.Log("right eye brow left position :");
            //Debug.Log(rightEyeBrowLeft.Item0);
            //Debug.Log(rightEyeBrowLeft.Item1);

            //rightEyeBrowMiddle.Item0 = (double)landmarks["rightEyeBrowMiddleX"];
            //rightEyeBrowMiddle.Item1 = (double)landmarks["rightEyeBrowMiddleY"];
            //Debug.Log("right eye brow middle position :");
            //Debug.Log(rightEyeBrowMiddle.Item0);
            //Debug.Log(rightEyeBrowMiddle.Item1);

            //rightEyeBrowRight.Item0 = (double)landmarks["rightEyeBrowRightX"];
            //rightEyeBrowRight.Item1 = (double)landmarks["rightEyeBrowRightY"];
            //Debug.Log("right eye brow right position :");
            //Debug.Log(rightEyeBrowRight.Item0);
            //Debug.Log(rightEyeBrowRight.Item1);

            distanceBetweenNoseTopAndEyes = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 27], faceAnalyse.localLandmarks[2 * 27 + 1],
                                                               faceAnalyse.localLandmarks[2 * 42], faceAnalyse.localLandmarks[2 * 42 + 1]) / faceWidth;
            Debug.Log("distance between nose and eyes :");
            Debug.Log(distanceBetweenNoseTopAndEyes);


            // Récuperation des infos sur le nez

            distanceBetweenNoseTipAndLip = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 33], faceAnalyse.localLandmarks[2 * 33 + 1],
                                                               faceAnalyse.localLandmarks[2 * 51], faceAnalyse.localLandmarks[2 * 51 + 1]) / faceHeight;
            Debug.Log("distance between nose tip and lip :");
            Debug.Log(distanceBetweenNoseTipAndLip);

            noseHeight = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 27], faceAnalyse.localLandmarks[2 * 27 + 1],
                                                               faceAnalyse.localLandmarks[2 * 30], faceAnalyse.localLandmarks[2 * 30 + 1]) / faceHeight;
            Debug.Log("nose height :");
            Debug.Log(noseHeight);

            noseWidth = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 31], faceAnalyse.localLandmarks[2 * 31 + 1],
                                                               faceAnalyse.localLandmarks[2 * 35], faceAnalyse.localLandmarks[2 * 35 + 1]) / faceWidth;
            Debug.Log("nose width :");
            Debug.Log(noseWidth);

            //nostrilThickness = Math.Abs((double)landmarks["nostrilRightHoleBottomX"] - (double)landmarks["nostrilRightSideX"]) / faceWidth;
            //Debug.Log("nostril thickness :");
            //Debug.Log(nostrilThickness);



            // Récuperation des infos utiles pour la hair detection
            leftEyeCorner.Item0 = faceAnalyse.localLandmarks[2 * 36];
            leftEyeCorner.Item1 = faceAnalyse.localLandmarks[2 * 36 + 1];
            Debug.Log("left eye corner position :");
            Debug.Log(leftEyeCorner.Item0);
            Debug.Log(leftEyeCorner.Item1);

            rightEyeCorner.Item0 = faceAnalyse.localLandmarks[2 * 45];
            rightEyeCorner.Item1 = faceAnalyse.localLandmarks[2 * 45 + 1];
            Debug.Log("right eye corner position :");
            Debug.Log(rightEyeCorner.Item0);
            Debug.Log(rightEyeCorner.Item1);

            chin.Item0 = faceAnalyse.localLandmarks[2 * 8];
            chin.Item1 = faceAnalyse.localLandmarks[2 * 8 + 1];
            Debug.Log("chin tip position :");
            Debug.Log(chin.Item0);
            Debug.Log(chin.Item1);

            nose.Item0 = faceAnalyse.localLandmarks[2 * 30];
            nose.Item1 = faceAnalyse.localLandmarks[2 * 30 + 1];
            Debug.Log("nose tip position :");
            Debug.Log(nose.Item0);
            Debug.Log(nose.Item1);

            // Récuperation des infos sur la bouche
            lipWidth = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 48], faceAnalyse.localLandmarks[2 * 48 + 1],
                                                               faceAnalyse.localLandmarks[2 * 54], faceAnalyse.localLandmarks[2 * 54 + 1]) / faceWidth;
            Debug.Log("lip width :");
            Debug.Log(lipWidth);

            topLipHeight = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 52], faceAnalyse.localLandmarks[2 * 52 + 1],
                                                               faceAnalyse.localLandmarks[2 * 63], faceAnalyse.localLandmarks[2 * 63 + 1]) / faceHeight;
            Debug.Log("topLipHeight :");
            Debug.Log(topLipHeight);

            buttomLipHeight = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 66], faceAnalyse.localLandmarks[2 * 66 + 1],
                                                               faceAnalyse.localLandmarks[2 * 57], faceAnalyse.localLandmarks[2 * 57 + 1]) / faceHeight;
            Debug.Log("buttomLipHeight :");
            Debug.Log(buttomLipHeight);

            // Récuperation des infos sur la forme du visage
            chinWidth = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 7], faceAnalyse.localLandmarks[2 * 7 + 1],
                                                               faceAnalyse.localLandmarks[2 * 9], faceAnalyse.localLandmarks[2 * 9 + 1]) / faceWidth;
            Debug.Log("chinWidth : " + chinWidth);

            cornerChinWidth = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 4], faceAnalyse.localLandmarks[2 * 4 + 1],
                                                               faceAnalyse.localLandmarks[2 * 12], faceAnalyse.localLandmarks[2 * 12 + 1]) / faceWidth;
            Debug.Log("cornerChinWidth : " + cornerChinWidth);

            distanceButtomCurve = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 5], faceAnalyse.localLandmarks[2 * 5 + 1],
                                                               faceAnalyse.localLandmarks[2 * 11], faceAnalyse.localLandmarks[2 * 11 + 1]) / faceWidth;
            Debug.Log("distanceButtomCurve : " + distanceButtomCurve);

            //jawCornerWidth2 = FaceDetectionImage.DistanceEuclidienne(faceAnalyse.localLandmarks[2 * 3], faceAnalyse.localLandmarks[2 * 3 + 1],
            //                                                   faceAnalyse.localLandmarks[2 * 13], faceAnalyse.localLandmarks[2 * 13 + 1]) / faceWidth;
            //Debug.Log("jawCornerWidth2 : " + jawCornerWidth2);
        }
        
    }

    void Update()
    {
      //if(faceAnalyse.LipHeight != 0 && !updateLip)
      //  {
      //      updateLip = true;
      //      Debug.Log("lip height :");
      //      Debug.Log(faceAnalyse.LipHeight);
      //      try
      //      {

      //          //Pass the filepath and filename to the StreamWriter Constructor
      //          StreamWriter sw = new StreamWriter("Assets/Test.txt");

      //          sw.WriteLine("Start of analyse");

      //          sw.WriteLine(gender);

      //          sw.WriteLine(leftEyeWidth);

      //          sw.WriteLine(rightEyeWidth);

      //          sw.WriteLine(distanceBetweenNoseTopAndEyes);

      //          sw.WriteLine(distanceBetweenNoseTipAndLip);

      //          sw.WriteLine(noseHeight);

      //          sw.WriteLine(noseWidth);

      //          sw.WriteLine(nostrilThickness);

      //          sw.WriteLine(lipWidth);

      //          sw.WriteLine(faceAnalyse.LipHeight);

      //          sw.WriteLine("End of analyse");

      //          //Close the file
      //          sw.Close();
      //      }
      //      catch (Exception e)
      //      {
      //          Console.WriteLine("Exception: " + e.Message);
      //      }
      //      finally
      //      {
      //          Console.WriteLine("Executing finally block.");
      //      }
      //  }
            
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
                { "api_key", "87a845cef7df66481a72f0606528a518" },
                { "selector", "SETPOSE" }
            });

        Debug.Log(jsonResponse);
        
        landmarks = JsonMapper.ToObject(jsonResponse)["images"][0]["faces"][0];

        
    }

}