using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AvatarValidateButton : ChangeSceneOnClickScript
{
    [SerializeField]
    private bool isSelection;
    
    protected override IEnumerator OnMouseDown()
    {
        if(isSelection)
        {
            switch (AvatarScript.avatarSelectionId)
            {
                case 1:
                    Debug.Log("avatar 1 choisi");
                    AvatarScript.avatarDefinitif = AvatarScript.avatar1;
                    break;

                case 2:
                    Debug.Log("avatar 2 choisi");
                    AvatarScript.avatarDefinitif = AvatarScript.avatar2;
                    break;

                case 3:
                    Debug.Log("avatar 3 choisi");
                    AvatarScript.avatarDefinitif = AvatarScript.avatar3;
                    break;

                case 4:
                    Debug.Log("avatar 4 choisi");
                    AvatarScript.avatarDefinitif = AvatarScript.avatar4;
                    break;

                case 5:
                    Debug.Log("avatar 5 choisi");
                    AvatarScript.avatarDefinitif = AvatarScript.avatar5;
                    break;

                default:
                    Debug.Log("avatar defaut choisi");
                    AvatarScript.avatarDefinitif = AvatarScript.avatar1;
                    break;
            }
        }



        //SoundEffectsHelper.Instance.MakeButtonSelectedSound();
        float fadeTime = fadingScene.BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene(_nextScene);
    }

}