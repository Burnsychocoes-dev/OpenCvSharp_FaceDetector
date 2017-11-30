using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avatar : MonoBehaviour {

    enum Taille
    {
        Big,
        Middle,
        little
    }

    struct Eye
    {
        Taille width;
    }

    struct Nose
    {
        Taille width;
        Taille height;
    }

    struct Mouth
    {
        Taille height;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
