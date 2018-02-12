using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RefreshNumber : MonoBehaviour {

    public enum NumberType
    {
        AvatarID,
        HaircutID
    }
    [SerializeField]
    private NumberType numberType;

    Text text;

	// Use this for initialization
	void Start () {
        text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		switch(numberType)
        {
            case NumberType.AvatarID:
                text.text = AvatarScript.avatarSelectionId.ToString();
                break;

            case NumberType.HaircutID:
                text.text = AvatarScript.avatarHaircutSelectionId.ToString();
                break;
        }
	}
}
