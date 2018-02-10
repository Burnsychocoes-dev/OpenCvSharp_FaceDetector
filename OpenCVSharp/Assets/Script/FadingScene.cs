using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadingScene : MonoBehaviour {

    public Texture2D fadeOutTexture; // the texture that will overlay the screen.
    public float fadeSpeed = 0.8f; // fading speed

    private int drawDepth = -1000; // texture's order in the draw hierarchy : a low number means it render on top
    private float alpha = 1.0f; // texture's alpha value between 0 and 1
    private int fadeDir = -1; // direction to fade : in = -1, out = 1
    //Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnGUI()
    {
        //fade out/in the alpha value using a direction, a speed and Time.deltaTime to convert the operation to seconds.
        alpha += fadeDir * fadeSpeed * Time.deltaTime;
        //force (clamp) the number between 0 and  because GUI.color uses alpha values betzeen 0 and 1
        alpha = Mathf.Clamp01(alpha);

        //set color of our GUI (in this case, our texture) All color values remain the same & the Alpha is set to  the alpha variable
        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha); // set the alpha value
        GUI.depth = drawDepth;  //make the black texture render on top (drawn list)
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeOutTexture); //draw the texture to fit the entire screen area
    }

    public float BeginFade(int direction)
    {
        fadeDir = direction;
        return (fadeSpeed);
    }

    private void OnLevelWasLoaded()
    {
        Debug.Log("salut");
        //Debug.Log(KarmaScript.karma);
        Debug.Log(SceneManager.GetActiveScene().name);
        BeginFade(-1);
        
    }
}
