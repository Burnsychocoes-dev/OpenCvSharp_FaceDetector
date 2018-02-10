using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotoButton : ChangeSceneOnClickScript {

    protected override IEnumerator OnMouseDown()
    {
        //Code pour prendre la photo ici


        //SoundEffectsHelper.Instance.MakeButtonSelectedSound();
        float fadeTime = fadingScene.BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene(_nextScene);
    }

}
