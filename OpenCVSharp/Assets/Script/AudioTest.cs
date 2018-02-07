using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


public class AudioTest : MonoBehaviour {

    AudioClip myAudioClip;
    float timeCount = 0f;
    [SerializeField]
    private float recordTime = 1f;
    bool recordDone = false;
    private MORPH3D.M3DCharacterManager avatarManager;
    private bool mouthUp = true;
    private float timeSpeakCount = 0f;
    [SerializeField]
    private float speechSpeed = 1f;
    private bool isSpeaking;
    private int state = 0;


    [SerializeField]
    private double[] emotions;
    public double[] Emotions
    {
        get { return emotions; }
    }

    // The imported function
    [DllImport("VokaturiDataAnalyseV2", EntryPoint = "AnalyzeSamplesWithoutBuffer")] public static extern int AnalyseEmotions(double[] data, int nbrOfSamples, double[] emotions);

    // Use this for initialization
    void Start () {
        myAudioClip = Microphone.Start(null, true, (int)recordTime, 44100);
        avatarManager = GetComponent<MORPH3D.M3DCharacterManager>();
    }
	
	// Update is called once per frame
	void Update () {
        timeCount += Time.deltaTime;
        timeSpeakCount += Time.deltaTime;
        if (timeCount >= recordTime)
        {
            timeCount = 0;
            Debug.Log("start analysing emotions");
            int nbrOfSamples = myAudioClip.samples;
            float[] data = new float[nbrOfSamples];
            Debug.Log(nbrOfSamples);
            myAudioClip.GetData(data, 0);
            double[] double_data = new double[data.Length];
            for(int i=0; i<data.Length; i++)
            {
                double_data[i] = (double)data[i];
            }
            if(AnalyseEmotions( double_data, nbrOfSamples, emotions)==1)
            {
                isSpeaking = true;
            }
            else
            {
                isSpeaking = false;
            }
            UpdateAvatarEmotion();
            Debug.Log("end analysing emotions");
        }
        if(isSpeaking)
        {
            MakeAvatarSpeak();
        }
        else
        {
            string morphName;
            switch (state)
            {
                case 0:
                    morphName = "eCTRLvAA";
                    break;

                case 1:
                    morphName = "eCTRLvEE";
                    break;

                case 2:
                    morphName = "eCTRLvK";
                    break;

                case 3:
                    morphName = "eCTRLvOW";
                    break;

                case 4:
                    morphName = "eCTRLvS";
                    break;

                case 5:
                    morphName = "eCTRLvTH";
                    break;

                case 6:
                    morphName = "eCTRLvUW";
                    break;

                case 7:
                    morphName = "eCTRLvIY";
                    break;

                default:
                    morphName = "eCTRLvAA";
                    break;
            }
            avatarManager.SetBlendshapeValue(morphName, 0);
        }
    }

    private void UpdateAvatarEmotion()
    {
        //double neutrality = emotions[0];
        //double happiness = emotions[1];
        //double sadness = emotions[2];
        //double anger = emotions[3];
        //double fear = emotions[4];
        float neutrality_value = Avatar.PercentageConvertor((float)emotions[0], 0f, 1f, 0, 100);
        float happiness_value = Avatar.PercentageConvertor((float)emotions[1], 0f, 1f, 0, 100);
        float sadness_value = Avatar.PercentageConvertor((float)emotions[2], 0f, 1f, 0, 100);
        float anger_value = Avatar.PercentageConvertor((float)emotions[3], 0f, 1f, 0, 100);
        float fear_value = Avatar.PercentageConvertor((float)emotions[4], 0f, 1f, 0, 100);
        avatarManager.SetBlendshapeValue("eCTRLHappy", happiness_value);
        avatarManager.SetBlendshapeValue("eCTRLSad", sadness_value);
        avatarManager.SetBlendshapeValue("eCTRLAngry", anger_value);
        avatarManager.SetBlendshapeValue("eCTRLFear", fear_value);
    }


    public void MakeAvatarSpeak()
    {
        string morphName;
        switch(state)
        {
            case 0:
                morphName = "eCTRLvAA";
                break;

            case 1:
                morphName = "eCTRLvEE";
                break;

            case 2:
                morphName = "eCTRLvK";
                break;

            case 3:
                morphName = "eCTRLvOW";
                break;

            case 4:
                morphName = "eCTRLvS";
                break;

            case 5:
                morphName = "eCTRLvTH";
                break;

            case 6:
                morphName = "eCTRLvUW";
                break;

            case 7:
                morphName = "eCTRLvIY";
                break;

            default:
                morphName = "eCTRLvAA";
                break;
        }
        if (timeSpeakCount > speechSpeed)
        {
            timeSpeakCount = 0;
            if (mouthUp)
            {
                avatarManager.SetBlendshapeValue(morphName, 100);               
            }
            else
            {
                avatarManager.SetBlendshapeValue(morphName, 0);
                state = Random.Range(0, 7);
            }
            mouthUp = !mouthUp;
        }
        else
        {
            float value = 0;
            foreach (MORPH3D.FOUNDATIONS.Morph m in avatarManager.coreMorphs.morphs)
            {
                if (m.name == morphName)
                {
                    value = m.value;
                }
            }
            if (mouthUp)
            {
                value += Avatar.PercentageConvertor(Time.deltaTime, 0, speechSpeed, 0, 100);
                avatarManager.SetBlendshapeValue(morphName, value);
            }
            else
            {
                value -= Avatar.PercentageConvertor(Time.deltaTime, 0, speechSpeed, 0, 100);
                avatarManager.SetBlendshapeValue(morphName, value);
            }
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
