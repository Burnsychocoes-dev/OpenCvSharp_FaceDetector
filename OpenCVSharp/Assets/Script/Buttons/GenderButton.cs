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
                AvatarScript.avatar1.gender = AvatarScript.Gender.Male;
                AvatarScript.avatar2.gender = AvatarScript.Gender.Male;
                AvatarScript.avatar3.gender = AvatarScript.Gender.Male;
                AvatarScript.avatar4.gender = AvatarScript.Gender.Male;
                AvatarScript.avatar5.gender = AvatarScript.Gender.Male;
                AvatarScript.avatarDefinitif.gender = AvatarScript.Gender.Male;
                break;
            case Gender.WOMAN:
                AvatarScript.avatar1.gender = AvatarScript.Gender.Femelle;
                AvatarScript.avatar2.gender = AvatarScript.Gender.Femelle;
                AvatarScript.avatar3.gender = AvatarScript.Gender.Femelle;
                AvatarScript.avatar4.gender = AvatarScript.Gender.Femelle;
                AvatarScript.avatar5.gender = AvatarScript.Gender.Femelle;
                AvatarScript.avatarDefinitif.gender = AvatarScript.Gender.Femelle;
                break;
        }
        //SoundEffectsHelper.Instance.MakeButtonSelectedSound();
        float fadeTime = fadingScene.BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene(_nextScene);
    }

}
