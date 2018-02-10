using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RecordButton : MonoBehaviour {
    //public string _nextScene = "";
    public Sprite sprite;
    protected Sprite spritesave;
    protected SpriteRenderer spriterender;
    protected enum State
    {
        RECORDING,
        PAUSE
    }

    protected State state = State.PAUSE;
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

        
        //SoundEffectsHelper.Instance.MakeButtonSelectSound();
    }
    protected void OnMouseOver()
    {
        //rend.material.color -= new Color(0.1F, 0, 0) * Time.deltaTime;
    }
    protected void OnMouseExit()
    {
        //rend.material.color = Color.white;
        

    }


    protected void OnMouseDown()
    {
        switch (state)
        {
            case State.PAUSE:
                if (sprite != null)
                {
                    spritesave = spriterender.sprite;
                    spriterender.sprite = sprite;
                }
                else
                {

                }
                state = State.RECORDING;
                break;
            case State.RECORDING:
                if (sprite != null)
                {
                    spriterender.sprite = spritesave;
                }
                state = State.PAUSE;
                break;
        }

        var avatars = GetComponentsInChildren<AvatarMaker>();
        foreach(var a in avatars)
        {
            if(a.PrefabGender == a.Perso.gender)
            {
                var emotionAnalyser = a.GetComponent<AudioEmotionAnalyser>();
                emotionAnalyser.AudioRecord = !emotionAnalyser.AudioRecord;
            }
        }
    }
}
