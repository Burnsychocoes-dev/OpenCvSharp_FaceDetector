using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputScript : MonoBehaviour {


    private bool avatarZoom = false;

    [SerializeField]
    Camera camera;

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("Jump"))
        {
            if(!avatarZoom)
            {
                camera.transform.Translate(0, 0.09f, 0.29f);
                avatarZoom = true;
            }
            else
            {
                camera.transform.Translate(0, -0.09f, -0.29f);
                avatarZoom = false;
            }
        }
	}
}
