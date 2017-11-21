using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdatePhoto : MonoBehaviour {

    Image image;

	// Use this for initialization
	void Start () {
        image = GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
        image.GraphicUpdateComplete();
	}
}
