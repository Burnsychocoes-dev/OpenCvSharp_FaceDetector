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

    struct Hair
    {
        Taille height;
        Taille length;
    }

	// Use this for initialization
	public void Create ()
    {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
