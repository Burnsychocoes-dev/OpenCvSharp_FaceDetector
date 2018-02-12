using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyesNoseButton : MonoBehaviour {

    public Sprite sprite;
    public Sprite spriteSelected;
    public Sprite spriteHighlighted;
    protected Sprite spritesave;
    protected SpriteRenderer spriterender;

    protected GameObject hairs;
    protected GameObject nose;
    protected GameObject eyes;
    public enum Choice
    {
        EYES,
        NOSE,
        HAIRS,
        NONE
    }
    [SerializeField]
    protected Choice choice;

    public static Choice currentChoice = Choice.HAIRS;
    protected enum State
    {
        SELECTED,
        NOTSELECTED
    }
    protected State state = State.NOTSELECTED;

    private void Awake()
    {
        hairs = GameObject.Find("HairGestion");
        nose = GameObject.Find("NoseGestion");
        eyes = GameObject.Find("EyesGestion");
        
    }
    //protected FadingScene fadingScene;
    void Start()
    {
        //fadingScene = GameObject.Find("menu").GetComponent<FadingScene>();
        spriterender = GetComponent<SpriteRenderer>();
        if (choice == currentChoice)
        {
            UnactivateOthersAndActivateItself();
            spriterender.sprite = spriteSelected;
            state = State.SELECTED;
        }
        //if (_nextScene.Equals(""))
        //{
        //    _nextScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        //}
        //Debug.Log("yosh");
    }

    private void Update()
    {
        if(currentChoice != choice && state == State.SELECTED)
        {
            state = State.NOTSELECTED;
            spriterender.sprite = sprite;
        }
    }

    protected void OnMouseEnter()
    {

        if (spriteHighlighted != null && state==State.NOTSELECTED)
        {
            spritesave = spriterender.sprite;
            spriterender.sprite = spriteHighlighted;
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
        if (spriteHighlighted != null && state == State.NOTSELECTED)
        {
            spriterender.sprite = sprite;
        }

    }


    protected void OnMouseDown()
    {
        
        switch (state)
            {
                //case State.SELECTED:
                //    spriterender.sprite = sprite;
                //    state = State.NOTSELECTED;                    
                //    break;
                case State.NOTSELECTED:
                        spriterender.sprite = spriteSelected;
                        state = State.SELECTED;
                        currentChoice = choice;
                        UnactivateOthersAndActivateItself();
                        break;
            }
        
    }

    protected void UnactivateOthersAndActivateItself()
    {
        switch (choice)
        {
            case Choice.NOSE:               
                hairs.SetActive(false);
                eyes.SetActive(false);
                nose.SetActive(true);
                break;
            case Choice.HAIRS:
                hairs.SetActive(true);
                eyes.SetActive(false);
                nose.SetActive(false);
                break;
            case Choice.EYES:
                hairs.SetActive(false);
                eyes.SetActive(true);
                nose.SetActive(false);
                break;
        }
    }
}
