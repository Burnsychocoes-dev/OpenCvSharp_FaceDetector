using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AvatarValidateButton : ChangeSceneOnClickScript
{


    protected override IEnumerator OnMouseDown()
    {
        switch (AvatarScript.avatarSelectionId)
        {
            case 1:
                AvatarScript.avatarDefinitif = AvatarScript.avatar1;
                break;

            case 2:
                AvatarScript.avatarDefinitif = AvatarScript.avatar2;
                break;

            case 3:
                AvatarScript.avatarDefinitif = AvatarScript.avatar3;
                break;

            default:
                AvatarScript.avatarDefinitif = AvatarScript.avatar1;
                break;
        }


        //SoundEffectsHelper.Instance.MakeButtonSelectedSound();
        float fadeTime = fadingScene.BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene(_nextScene);
    }

}