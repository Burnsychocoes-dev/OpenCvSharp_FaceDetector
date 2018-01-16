using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTest : MonoBehaviour {

    AudioClip myAudioClip;
    float timeCount = 0f;
    [SerializeField]
    private float recordTime = 5f;
    bool recordDone = false;

    // Use this for initialization
    void Start () {
        myAudioClip = Microphone.Start(null, false, 10, 44100);
    }
	
	// Update is called once per frame
	void Update () {
        timeCount += Time.deltaTime;
        if (timeCount > recordTime && !recordDone)
        {
            Debug.Log("start saving");
            SavWav.Save("audioTest.wav", myAudioClip);
            Debug.Log("end saving");
            recordDone = true;
        }
    }


    //void OnGUI()
    //{
    //    if (GUI.Button(new Rect(10, 10, 60, 50), "Record"))
    //    {
    //        myAudioClip = Microphone.Start(null, false, 10, 44100);
    //    }
    //    if (GUI.Button(new Rect(10, 70, 60, 50), "Save"))
    //    {
    //        SavWav.Save("myfile", myAudioClip);
    //    }
    //}
}
