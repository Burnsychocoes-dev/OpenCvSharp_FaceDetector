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

    public void Init() {
        if(!isCallDone)
        {
            isCallDone = true;
            faceAnalyse = GetComponent<FaceDetectionImage>();
            hairDetection = GetComponent<HairDetection>();

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
                Debug.Log("C'est une femme");
            }

            faceHeight = faceAnalyse.Face.Height;
            Debug.Log("face height :");
            Debug.Log(faceHeight);

            //if((int)landmarks["rightEarTragusX"] != -1 && (int)landmarks["leftEarTragusX"] != -1)
            //{
            //faceWidth = Math.Abs((double)landmarks["rightEarTragusX"] - (double)landmarks["leftEarTragusX"]);
            //Debug.Log("face width :");
            //Debug.Log(faceWidth);
            //}
            //else
            //{
            faceWidth = hairDetection.J_max - hairDetection.J_min;
            Debug.Log("face width :");
            Debug.Log(faceWidth);
            //}

            //distanceBetweenLipAndChin = Math.Abs((double)landmarks["lipLineMiddleY"] - (double)landmarks["chinTipY"]) / faceHeight;
            //Debug.Log("distance between lip and chin :");
            //Debug.Log(distanceBetweenLipAndChin);


            // Récuperation des infos sur les yeux
            //leftEyeCenter.Item0 = (double)landmarks["leftEyeCenterX"];
            //leftEyeCenter.Item1 = (double)landmarks["leftEyeCenterY"];
            //Debug.Log("left eye center position :");
            //Debug.Log(leftEyeCenter.Item0);
            //Debug.Log(leftEyeCenter.Item1);

            //rightEyeCenter.Item0 = (double)landmarks["rightEyeCenterX"];
            //rightEyeCenter.Item1 = (double)landmarks["rightEyeCenterY"];
            //Debug.Log("right eye center position :");
            //Debug.Log(rightEyeCenter.Item0);
            //Debug.Log(rightEyeCenter.Item1);

            leftEyeWidth = Math.Abs((double)landmarks["leftEyeCornerLeftX"] - (double)landmarks["leftEyeCornerRightX"]) / faceWidth;
            Debug.Log("left eye width :");
            Debug.Log(leftEyeWidth);

            rightEyeWidth = Math.Abs((double)landmarks["rightEyeCornerLeftX"] - (double)landmarks["rightEyeCornerRightX"]) / faceWidth;
            Debug.Log("right eye width :");
            Debug.Log(rightEyeWidth);

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

            distanceBetweenNoseTopAndEyes = Math.Abs((double)landmarks["noseBtwEyesX"] - (double)landmarks["rightEyeCornerLeftX"]) / faceWidth;
            Debug.Log("distance between nose and eyes :");
            Debug.Log(distanceBetweenNoseTopAndEyes);


            // Récuperation des infos sur le nez
            distanceBetweenNoseTipAndLip = Math.Abs((double)landmarks["noseTipY"] - (double)landmarks["lipLineMiddleY"]) / faceHeight;
            Debug.Log("distance between nose tip and lip :");
            Debug.Log(distanceBetweenNoseTipAndLip);

            noseHeight = Math.Abs((double)landmarks["noseTipY"] - (double)landmarks["noseBtwEyesY"]) / faceHeight;
            Debug.Log("nose height :");
            Debug.Log(noseHeight);

            noseWidth = Math.Abs((double)landmarks["nostrilLeftSideX"] - (double)landmarks["nostrilRightSideX"]) / faceWidth;
            Debug.Log("nose width :");
            Debug.Log(noseWidth);

            nostrilThickness = Math.Abs((double)landmarks["nostrilRightHoleBottomX"] - (double)landmarks["nostrilRightSideX"]) / faceWidth;
            Debug.Log("nostril thickness :");
            Debug.Log(nostrilThickness);



            // Récuperation des infos utiles pour la hair detection
            leftEyeCorner.Item0 = (double)landmarks["leftEyeCornerLeftX"];
            leftEyeCorner.Item1 = (double)landmarks["leftEyeCornerLeftY"];
            Debug.Log("left eye corner position :");
            Debug.Log(leftEyeCorner.Item0);
            Debug.Log(leftEyeCorner.Item1);

            rightEyeCorner.Item0 = (double)landmarks["rightEyeCornerRightX"];
            rightEyeCorner.Item1 = (double)landmarks["rightEyeCornerRightY"];
            Debug.Log("right eye corner position :");
            Debug.Log(rightEyeCorner.Item0);
            Debug.Log(rightEyeCorner.Item1);

            chin.Item0 = (double)landmarks["chinTipX"];
            chin.Item1 = (double)landmarks["chinTipY"];
            Debug.Log("chin tip position :");
            Debug.Log(chin.Item0);
            Debug.Log(chin.Item1);

            nose.Item0 = (double)landmarks["noseTipX"];
            nose.Item1 = (double)landmarks["noseTipY"];
            Debug.Log("nose tip position :");
            Debug.Log(nose.Item0);
            Debug.Log(nose.Item1);

            // Récuperation des infos sur la bouche
            lipWidth = Math.Abs((double)landmarks["lipCornerLeftX"] - (double)landmarks["lipCornerRightX"]) / faceWidth;
            Debug.Log("lip width :");
            Debug.Log(lipWidth);

            topLipHeight = Math.Abs((double)landmarks["lipLineMiddleY"] - faceAnalyse.RectMouth.Y) / faceHeight;
            Debug.Log("topLipHeight :");
            Debug.Log(topLipHeight);

            buttomLipHeight = Math.Abs((double)landmarks["lipLineMiddleY"] - faceAnalyse.RectMouth.Y + faceAnalyse.RectMouth.Height) / faceHeight;
            Debug.Log("buttomLipHeight :");
            Debug.Log(buttomLipHeight);
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