using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefreshAvatarSetup : MonoBehaviour {

    public Sprite sprite;
    protected Sprite spritesave;
    protected SpriteRenderer spriterender;

    AvatarScript.Personnage previousAvatar;
    private int previousHaircutId;

    void Start()
    {
        spriterender = GetComponent<SpriteRenderer>();
        previousHaircutId = AvatarScript.avatarHaircutSelectionId;
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

    }
    protected void OnMouseExit()
    {
        if (sprite != null)
        {
            spriterender.sprite = spritesave;
        }

    }


    protected void OnMouseDown()
    {
        SliderBar[] sliders = FindObjectsOfType<SliderBar>();
        foreach(var s in sliders)
        {
            s.InitSlideBar();
        }
        AvatarScript.avatarHaircutSelectionId = previousHaircutId;
    }
}
