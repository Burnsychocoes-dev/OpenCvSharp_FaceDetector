using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charselector : MonoBehaviour {

    //public string _nextScene = "";
    public Sprite sprite;
    protected Sprite spritesave;
    protected SpriteRenderer spriterender;

    protected enum Choice
    {
        CHARACTER,
        HAIRCUT
    }
    [SerializeField]
    protected Choice choice;

    protected enum Type
    {
        PREVIOUS,
        NEXT
    }
    [SerializeField]
    protected Type type;
    //protected FadingScene fadingScene;
    void Start()
    {
        //fadingScene = GameObject.Find("menu").GetComponent<FadingScene>();
        spriterender = GetComponent<SpriteRenderer>();
        //if (_nextScene.Equals(""))
        //{
        //    _nextScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        //}
        //Debug.Log("yosh");
    }
    protected void OnMouseEnter()
    {

        if (sprite != null)
        {
            spritesave = spriterender.sprite;
            spriterender.sprite = sprite;
        }
        else
        {

        }
        //SoundEffectsHelper.Instance.MakeButtonSelectSound();
    }
    protected void OnMouseOver()
    {
        //rend.material.color -= new Color(0.1F, 0, 0) * Time.deltaTime;
    }
    protected void OnMouseExit()
    {
        //rend.material.color = Color.white;
        if (sprite != null)
        {
            spriterender.sprite = spritesave;
        }

    }


    protected void OnMouseDown()
    {
        switch (choice)
        {
            case Choice.CHARACTER:
                switch (type)
                {
                    case Type.NEXT:
                        break;
                    case Type.PREVIOUS:
                        break;
                }
                break;
            case Choice.HAIRCUT:
                switch (type)
                {
                    case Type.NEXT:
                        break;
                    case Type.PREVIOUS:
                        break;
                }
                break;
        }
    }
}
