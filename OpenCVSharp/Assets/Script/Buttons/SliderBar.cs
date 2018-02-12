using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.UI;

public class SliderBar : MonoBehaviour {

    AvatarScript.Personnage previousAvatar;

    private void Start()
    {
        previousAvatar = AvatarScript.avatarDefinitif;
    }

    public void NoseInclinaison(float newValue)
    {
        AvatarScript.avatarDefinitif.nose.noseTipHeightBlendShapeValue = newValue;
    }
    public void NoseRondeur(float newValue)
    {
        AvatarScript.avatarDefinitif.nose.noseTipRoundBlendShapeValue = newValue;
    }
    public void NoseLargeur(float newValue)
    {
        if (newValue < 0)
        {
            float valeur_little = AvatarMaker.PercentageConvertorNeg(newValue, 0, 100, 0.18f, 0.215f);
            AvatarScript.avatarDefinitif.nose.noseWidth = valeur_little;
        }
        else
        {
            float valeur_big = AvatarMaker.PercentageConvertor(newValue, 0, 100, 0.215f, 0.25f);
            AvatarScript.avatarDefinitif.nose.noseWidth = valeur_big;
        }
    }
    public void NoseEcartement(float newValue)
    {
        AvatarScript.avatarDefinitif.nose.NosePinchBlendShapeValue = newValue;
    }
    public void EyesAbaissement(float newValue)
    {
        AvatarScript.avatarDefinitif.eye.EyesAbaissementBlendShapeValue = newValue;
    }


}
