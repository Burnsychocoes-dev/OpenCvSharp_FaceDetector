using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdatePhoto : MonoBehaviour {

    private Image image;
    private Texture2D imageTexture;

    // Use this for initialization
    void Start () {
        image = FindObjectOfType<Image>();
        imageTexture = (Texture2D)image.mainTexture;
    }
	
	// Update is called once per frame
	void Update () {
        imageTexture.Apply();
	}
}
