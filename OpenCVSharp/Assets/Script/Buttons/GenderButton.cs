using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GenderButton : ChangeSceneOnClickScript {
    protected enum Gender
    {
        MAN,
        WOMAN
    }

    [SerializeField]
    protected Gender gender;

    protected override IEnumerator OnMouseDown()
    {
        switch (gender)
        {
            case Gender.MAN:
                break;
            case Gender.WOMAN:
                break;
        }
        //SoundEffectsHelper.Instance.MakeButtonSelectedSound();
        float fadeTime = fadingScene.BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene(_nextScene);
    }

}
