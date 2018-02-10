using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotoButton : ChangeSceneOnClickScript {

    FaceDetection photo;
    SoundEffectsHelper soundEffects;

    protected override IEnumerator OnMouseDown()
    {
        //Code pour prendre la photo ici
        photo = GetComponentInChildren<FaceDetection>();
        soundEffects = GetComponent<SoundEffectsHelper>();
        soundEffects.MakePhotoSound();
        photo.TakePhoto();


        //SoundEffectsHelper.Instance.MakeButtonSelectedSound();
        float fadeTime = fadingScene.BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene(_nextScene);
    }

}
