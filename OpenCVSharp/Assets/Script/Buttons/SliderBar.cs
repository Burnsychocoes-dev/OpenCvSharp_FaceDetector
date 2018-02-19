using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderBar : MonoBehaviour {
    protected enum Slidebar
    {
        NONE,
        NOSEINCLINAISON,
        NOSERONDEUR,
        NOSELARGEUR,
        NOSEECARTEMENT,
        EYESABAISSEMENT
    }

    AvatarScript.Personnage previousAvatar;
    private int previousHaircutId;

    [SerializeField]
    protected Slidebar type = Slidebar.NONE;

    Slider slider;

    private void Start()
    {
        previousAvatar = AvatarScript.avatarDefinitif;
        slider = GetComponent<Slider>();
        switch (type)
        {
            case Slidebar.NOSEINCLINAISON:
                slider.value = AvatarScript.avatarDefinitif.nose.noseTipHeightBlendShapeValue;
                break;

            case Slidebar.NOSERONDEUR:
                slider.value = AvatarScript.avatarDefinitif.nose.noseTipRoundBlendShapeValue;
                break;

            case Slidebar.NOSELARGEUR:
                switch (AvatarScript.avatarDefinitif.nose.width)
                {
                    case AvatarScript.Taille.Little:
                        Debug.Log(AvatarScript.avatarDefinitif.nose.noseWidth);
                        float valeur_little = AvatarMaker.PercentageConvertorNeg(AvatarScript.avatarDefinitif.nose.noseWidth, 0.18f, 0.215f, 0, 100);
                        slider.value = -valeur_little;
                        break;

                    case AvatarScript.Taille.Big:
                        Debug.Log(AvatarScript.avatarDefinitif.nose.noseWidth);
                        float valeur_big = AvatarMaker.PercentageConvertor(AvatarScript.avatarDefinitif.nose.noseWidth, 0.215f, 0.25f, 0, 100);
                        slider.value = valeur_big;
                        break;
                }               
                break;

            case Slidebar.NOSEECARTEMENT:
                slider.value = AvatarScript.avatarDefinitif.nose.NosePinchBlendShapeValue;
                break;

            case Slidebar.EYESABAISSEMENT:
                slider.value = AvatarScript.avatarDefinitif.eye.EyesAbaissementBlendShapeValue;
                break;
        }
    }



    public void InitSlideBar()
    {
        AvatarScript.avatarDefinitif = previousAvatar;
        switch (type)
        {
            case Slidebar.NOSEINCLINAISON:
                slider.value = AvatarScript.avatarDefinitif.nose.noseTipHeightBlendShapeValue;
                break;

            case Slidebar.NOSERONDEUR:
                slider.value = AvatarScript.avatarDefinitif.nose.noseTipRoundBlendShapeValue;
                break;

            case Slidebar.NOSELARGEUR:
                switch (AvatarScript.avatarDefinitif.nose.width)
                {
                    case AvatarScript.Taille.Little:
                        float valeur_little = AvatarMaker.PercentageConvertorNeg(AvatarScript.avatarDefinitif.nose.noseWidth, 0.18f, 0.215f, 0, 100);
                        slider.value = valeur_little;
                        break;

                    case AvatarScript.Taille.Big:
                        float valeur_big = AvatarMaker.PercentageConvertor(AvatarScript.avatarDefinitif.nose.noseWidth, 0.215f, 0.25f, 0, 100);
                        slider.value = valeur_big;
                        break;
                }
                break;

            case Slidebar.NOSEECARTEMENT:
                slider.value = AvatarScript.avatarDefinitif.nose.NosePinchBlendShapeValue;
                break;

            case Slidebar.EYESABAISSEMENT:
                slider.value = AvatarScript.avatarDefinitif.eye.EyesAbaissementBlendShapeValue;
                break;
        }
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
