using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using LitJson;

public class LandmarksRetriever : MonoBehaviour {

    [SerializeField]
    private string apiEndpoint;

    [SerializeField]
    private string imagePath;

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
                { "api_key", "4177793aaba14a666e0b5336f20a669c" },
                { "selector", "SETPOSE" }
            });

        JsonData data = JsonMapper.ToObject(jsonResponse)["images"][0]["faces"][0];
    }

}