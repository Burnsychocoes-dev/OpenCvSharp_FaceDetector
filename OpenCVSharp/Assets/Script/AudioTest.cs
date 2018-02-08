using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


public class AudioTest : MonoBehaviour {

    AudioClip myAudioClip;
    float timeCount = 0f;
    private int count05 = 0;
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

    private static int sizeBuffer = 12;

    [SerializeField]
    private static int nMA = 5;
    private static int nMAVolatile = 3;

    private int saveCnt = 0;
    private int saveCntVolatile = nMA - nMAVolatile;
    private int cnt = nMA - 1;
    private float[] neutrality = new float[sizeBuffer];
    private float[] happiness = new float[sizeBuffer];
    private float[] sadness = new float[sizeBuffer];
    private float[] anger = new float[sizeBuffer];
    private float[] fear = new float[sizeBuffer];

    private float neutrality_value = 0;
    private float happiness_value = 0;
    private float sadness_value = 0;
    private float anger_value = 0;
    private float fear_value = 0;
    [SerializeField]
    private float updateOnRealTimeSpeed = 0.5f;

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
        if (timeCount >= recordTime/2)
        {
            timeCount = 0;

            Debug.Log("start analysing emotions");
            int nbrOfSamples = myAudioClip.samples / 2;
            float[] data = new float[nbrOfSamples];
            Debug.Log(nbrOfSamples);

            if (count05 == 0)
            {
                myAudioClip.GetData(data, 0);
                count05 = 1;
            }else if (count05 == 1)
            {
                myAudioClip.GetData(data, 44100/2);
                count05 = 0;
            }            
            
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
            //UpdateAvatarEmotion();
            UpdateAvatarEmotionMA(nMA, nMAVolatile);
            //UpdateAvatarEmotionMAA(nMA);
            Debug.Log("happiness : " + happiness_value);
            Debug.Log("sadness : " + sadness_value);
            Debug.Log("anger : " + anger_value);
            Debug.Log("fear : " + fear_value);

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
        UpdateAvatarBlendshapeOnRealTime("eCTRLHappy", happiness_value);
        UpdateAvatarBlendshapeOnRealTime("eCTRLSad", sadness_value);
        UpdateAvatarBlendshapeOnRealTime("eCTRLAngry", anger_value);
        UpdateAvatarBlendshapeOnRealTime("eCTRLFear", fear_value);
    }

    private void UpdateAvatarEmotion()
    {
        //double neutrality = emotions[0];
        //double happiness = emotions[1];
        //double sadness = emotions[2];
        //double anger = emotions[3];
        //double fear = emotions[4];
        neutrality_value = Avatar.PercentageConvertor((float)emotions[0], 0f, 1f, 0, 100);
        happiness_value = Avatar.PercentageConvertor((float)emotions[1], 0f, 1f, 0, 100);
        sadness_value = Avatar.PercentageConvertor((float)emotions[2], 0f, 1f, 0, 100);
        anger_value = Avatar.PercentageConvertor((float)emotions[3], 0f, 1f, 0, 100);
        fear_value = Avatar.PercentageConvertor((float)emotions[4], 0f, 1f, 0, 100);
    }

    private void UpdateAvatarEmotionMA(int n, int nVolatile)
    {
        neutrality[cnt] = (float)emotions[0];
        happiness[cnt] = (float)emotions[1];
        sadness[cnt] = (float)emotions[2];
        anger[cnt] = (float)emotions[3];
        fear[cnt] = (float)emotions[4];

        neutrality_value=0;
        happiness_value=0;
        sadness_value=0;
        anger_value=0;
        fear_value=0;

        for(int i=0; i < nMA; i++)
        {
            neutrality_value += neutrality[(saveCnt + i) % sizeBuffer];
            happiness_value += happiness[(saveCnt + i) % sizeBuffer];
            sadness_value += sadness[(saveCnt + i) % sizeBuffer];
        }
        for(int i=0; i < nMAVolatile; i++)
        {
            anger_value += anger[(saveCntVolatile + i) % sizeBuffer];
            fear_value += fear[(saveCntVolatile + i) % sizeBuffer];
        }

        neutrality_value = neutrality_value / (nMA + 1);
        happiness_value = happiness_value / (nMA + 1);
        sadness_value = sadness_value / (nMA + 1);
        anger_value = anger_value / (nMAVolatile + 1);
        fear_value = fear_value / (nMAVolatile + 1);

        saveCnt = (saveCnt + 1) % sizeBuffer;
        saveCntVolatile = (saveCntVolatile + 1) % sizeBuffer;
        cnt = (cnt + 1) % sizeBuffer;

        neutrality_value = Avatar.PercentageConvertor(neutrality_value, 0f, 1f, 0, 100);
        happiness_value = Avatar.PercentageConvertor(happiness_value, 0f, 1f, 0, 100);
        sadness_value = Avatar.PercentageConvertor(sadness_value, 0f, 1f, 0, 100);
        anger_value = Avatar.PercentageConvertor(anger_value, 0f, 1f, 0, 100);
        fear_value = Avatar.PercentageConvertor(fear_value, 0f, 1f, 0, 100);
    }

    private void UpdateAvatarEmotionMAA(int n)
    {
        neutrality_value = 0;
        happiness_value = 0;
        sadness_value = 0;
        anger_value = 0;
        fear_value = 0;

        for (int i = 0; i < nMA-1; i++)
        {
            neutrality_value += neutrality[(saveCnt + i) % sizeBuffer];
            happiness_value += happiness[(saveCnt + i) % sizeBuffer];
            sadness_value += sadness[(saveCnt + i) % sizeBuffer];
            anger_value += anger[(saveCnt + i) % sizeBuffer];
            fear_value += fear[(saveCnt + i) % sizeBuffer];
        }

        neutrality_value += (float)emotions[0];
        happiness_value += (float)emotions[1];
        sadness_value += (float)emotions[2];
        anger_value += (float)emotions[3];
        fear_value += (float)emotions[4]; 

        neutrality_value = neutrality_value / (nMA + 1);
        happiness_value = happiness_value / (nMA + 1);
        sadness_value = sadness_value / (nMA + 1);
        anger_value = anger_value / (nMA + 1);
        fear_value = fear_value / (nMA + 1);

        neutrality[cnt] = neutrality_value;
        happiness[cnt] = happiness_value;
        sadness[cnt] = sadness_value;
        anger[cnt] = anger_value;
        fear[cnt] = fear_value;

        saveCnt = (saveCnt + 1) % sizeBuffer;
        cnt = (cnt + 1) % sizeBuffer;

        neutrality_value = Avatar.PercentageConvertor(neutrality_value, 0f, 1f, 0, 100);
        happiness_value = Avatar.PercentageConvertor(happiness_value, 0f, 1f, 0, 100);
        sadness_value = Avatar.PercentageConvertor(sadness_value, 0f, 1f, 0, 100);
        anger_value = Avatar.PercentageConvertor(anger_value, 0f, 1f, 0, 100);
        fear_value = Avatar.PercentageConvertor(fear_value, 0f, 1f, 0, 100);
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

    public void UpdateAvatarBlendshapeOnRealTime(string morphName, float newValue)
    {
        float previousValue = 0;
        foreach(MORPH3D.FOUNDATIONS.Morph m in avatarManager.coreMorphs.morphs)
        {
            if(m.name == morphName)
            {
                previousValue = m.value;
            }
        }
        if(newValue > previousValue)
        {
            float updateValue = previousValue + Avatar.PercentageConvertor(Time.deltaTime, 0, updateOnRealTimeSpeed, 0, 100);
            if(updateValue < newValue)
            {
                avatarManager.SetBlendshapeValue(morphName, updateValue);
            }
            else
            {
                avatarManager.SetBlendshapeValue(morphName, newValue);
            }
        }
        if(newValue < previousValue)
        {
            float updateValue = previousValue - Avatar.PercentageConvertor(Time.deltaTime, 0, updateOnRealTimeSpeed, 0, 100);
            if (updateValue > newValue)
            {
                avatarManager.SetBlendshapeValue(morphName, updateValue);
            }
            else
            {
                avatarManager.SetBlendshapeValue(morphName, newValue);
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
