using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using UnityEngine;

public class LandmarksRetriever : MonoBehaviour {

    [SerializeField]
    private string wsUri;

    public static LandmarksRetriever Instance {
        get;
        private set;
    }

    private static HttpClient client = new HttpClient();

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There is multiple instance of singleton LandmarksRetriever");
            return;
        }
        Instance = this;
    }
    
    public void RetrieveLandmarks(string imagePath) {
        client.BaseAddress = new Uri(wsUri);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        Retrieve();
    }
    
    private void Retrieve() {
        var values = new Dictionary<string, string> {
            { "api_key", "4177793aaba14a666e0b5336f20a669c" },
            { "selector", "SETPOSE" },
            { "url", "https://scontent-cdg2-1.xx.fbcdn.net/v/t1.0-9/22050258_1696964700323401_583767717233164251_n.jpg?oh=cfd9989607e893e31b7ce74746ce6b3a&oe=5A9F9027" }
        };

        FormUrlEncodedContent content = new FormUrlEncodedContent(values);

        HttpResponseMessage response = client.PostAsync("v2/detect", content).Result;

        /*
        HttpResponseMessage response = client.PostAsync("/v2/detect", content).Result;
        response.EnsureSuccessStatusCode();

        string responseBodyAsText = response.Content.ReadAsStringAsync().Result;*/

        //write in file (file, responseBodyAsText)
    }

}