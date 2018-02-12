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

    [SerializeField]
    protected Slidebar type = Slidebar.NONE;

    private void Start()
    {
        Slider slider = GetComponent<Slider>();
        switch (type)
        {
            case Slidebar.NOSEINCLINAISON:
                slider.value = 1;
                break;
            case Slidebar.NOSERONDEUR:
                slider.value = 1;
                break;
            case Slidebar.NOSELARGEUR:
                slider.value = 1;
                break;
            case Slidebar.NOSEECARTEMENT:
                slider.value = 1;
                break;
            case Slidebar.EYESABAISSEMENT:
                slider.value = 1;
                break;

        }
    }


    public void NoseInclinaison(float newValue)
    {
        //newValue = valeur du slider
    }
    public void NoseRondeur(float newValue)
    {

    }
    public void NoseLargeur(float newValue)
    {

    }
    public void NoseEcartement(float newValue)
    {

    }
    public void EyesAbaissement(float newValue)
    {

    }
}
