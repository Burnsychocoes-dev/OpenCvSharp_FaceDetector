using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


public class AudioEmotionAnalyser : MonoBehaviour {

    AudioClip myAudioClip;
    float timeCount = 0f;
    private int count05 = 0;

    [SerializeField]
    private float recordTime = 1f;    
    bool recordDone = false;
    private MORPH3D.M3DCharacterManager AvatarMakerManager;
    private bool mouthUp = true;
    private float timeSpeakCount = 0f;
    [SerializeField]
    private float speechSpeed = 1f;
    private bool isSpeaking;
    private int state = 0;
    private bool audioRecord = false;
    public bool AudioRecord
    {
        get { return audioRecord; }
        set { audioRecord = value; }
    }
    private bool audioRecordButton = false;
    public bool AudioRecordButton
    {
        get { return audioRecordButton; }
        set { audioRecordButton = value; }
    }


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
    //
    private static int secondeDivisor = 2;

    private float anger_offset = 0.2f;
	//précédente valeur : 3
	private int falseSilenceCnt = 2;
    private int countDivisor = 0;

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

    private float correction_value = 1.15f;
    //correction à la hause de 15%
    [SerializeField]
    private float updateOnRealTimeSpeed = 0.5f;

    // The imported function
    [DllImport("VokaturiDataAnalyseV2", EntryPoint = "AnalyzeSamplesWithoutBuffer")] public static extern int AnalyseEmotions(double[] data, int nbrOfSamples, double[] emotions);

    // Use this for initialization
    void Start () {
        AvatarMakerManager = GetComponent<MORPH3D.M3DCharacterManager>();
    }
	
	// Update is called once per frame
	void Update () {
        if (audioRecord)
        {
            if (audioRecordButton)
            {
                myAudioClip = Microphone.Start(null, true, (int)recordTime, 44100);
                audioRecordButton = false;
            }
            timeCount += Time.deltaTime;
            timeSpeakCount += Time.deltaTime;
            //
            if (timeCount >= recordTime / secondeDivisor)
            {
                timeCount = 0;

                Debug.Log("start analysing emotions");
                int nbrOfSamples = myAudioClip.samples / secondeDivisor;
                float[] data = new float[nbrOfSamples];
                Debug.Log(nbrOfSamples);
                myAudioClip.GetData(data, countDivisor*44100/secondeDivisor);
                countDivisor = (countDivisor+1)%secondeDivisor;
                /*if (count05 == 0)
                {
                    myAudioClip.GetData(data, 0);
                    count05 = 1;
                }
                else if (count05 == 1)
                {
                    myAudioClip.GetData(data, 44100 / 2);
                    count05 = 0;
                }*/

                double[] double_data = new double[data.Length];
                for (int i = 0; i < data.Length; i++)
                {
                    double_data[i] = (double)data[i];
                }
                if (AnalyseEmotions(double_data, nbrOfSamples, emotions) == 1)
                {
                    isSpeaking = true;
                }
                else
                {
                    isSpeaking = false;
                }
                //UpdateAvatarEmotionMA();
                UpdateAvatarEmotionMA_correctSilence();
                Debug.Log("Neutrality : " + neutrality_value);
                Debug.Log("happiness : " + happiness_value);
                Debug.Log("sadness : " + sadness_value);
                Debug.Log("anger : " + anger_value);
                Debug.Log("fear : " + fear_value);

                Debug.Log("end analysing emotions");
            }
            if (isSpeaking)
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
                AvatarMakerManager.SetBlendshapeValue(morphName, 0);
            }
            UpdateAvatarBlendshapeOnRealTime("eCTRLHappy", happiness_value);
            UpdateAvatarBlendshapeOnRealTime("eCTRLSad", sadness_value);
            UpdateAvatarBlendshapeOnRealTime("eCTRLAngry", anger_value);
            UpdateAvatarBlendshapeOnRealTime("eCTRLFear", fear_value);
        }
        else
        {
            if (audioRecordButton)
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
                AvatarMakerManager.SetBlendshapeValue(morphName, 0);
                UpdateAvatarBlendshapeOnRealTime("eCTRLHappy", 0);
                UpdateAvatarBlendshapeOnRealTime("eCTRLSad", 0);
                UpdateAvatarBlendshapeOnRealTime("eCTRLAngry", 0);
                UpdateAvatarBlendshapeOnRealTime("eCTRLFear", 0);
            }
        }
    }

    private void UpdateAvatarMakerEmotion()
    {
        //double neutrality = emotions[0];
        //double happiness = emotions[1];
        //double sadness = emotions[2];
        //double anger = emotions[3];
        //double fear = emotions[4];
        neutrality_value = AvatarMaker.PercentageConvertor((float)emotions[0], 0f, 1f, 0, 100);
        happiness_value = AvatarMaker.PercentageConvertor((float)emotions[1], 0f, 1f, 0, 100);
        sadness_value = AvatarMaker.PercentageConvertor((float)emotions[2], 0f, 1f, 0, 100);
        anger_value = AvatarMaker.PercentageConvertor((float)emotions[3], 0f, 1f, 0, 100);
        fear_value = AvatarMaker.PercentageConvertor((float)emotions[4], 0f, 1f, 0, 100);
    }

    private void UpdateAvatarEmotionMA()
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

        neutrality_value = neutrality_value / nMA;
        happiness_value = happiness_value / nMA;
        sadness_value = sadness_value / nMA;
        anger_value = anger_value / nMAVolatile;
        fear_value = fear_value / nMAVolatile;

        saveCnt = (saveCnt + 1) % sizeBuffer;
        saveCntVolatile = (saveCntVolatile + 1) % sizeBuffer;
        cnt = (cnt + 1) % sizeBuffer;

        //// Petit test de prise de decision par max
        //float[] tableau = new float[5];
        //tableau[0] = neutrality_value;
        //tableau[1] = happiness_value;
        //tableau[2] = sadness_value;
        //tableau[3] = anger_value;
        //tableau[4] = fear_value;

        //int maxIndice = IndexOfMaxEmotion(tableau);

        //switch(maxIndice)
        //{
        //    case 1:
        //        happiness_value = 1;
        //        break;

        //    case 2:
        //        sadness_value = 1;
        //        break;

        //    case 3:
        //        anger_value = 1;
        //        break;

        //    case 4:
        //        fear_value = 1;
        //        break;

        //    default:
        //        neutrality_value = 1;
        //        break;
        //}


        neutrality_value = AvatarMaker.PercentageConvertor(neutrality_value, 0f, 1f, 0, 100);
        happiness_value = AvatarMaker.PercentageConvertor(happiness_value, 0f, 1f, 0, 100);
        sadness_value = AvatarMaker.PercentageConvertor(sadness_value, 0f, 1f, 0, 100);
        anger_value = AvatarMaker.PercentageConvertor(anger_value, 0f, 1f, 0, 100);
        fear_value = AvatarMaker.PercentageConvertor(fear_value, 0f, 1f, 0, 100);
    }

    private void UpdateAvatarEmotionMA_correctSilence(){
        int cnt0 = 0;
        int cnt0Volatile = 0;
        int totalCnt0 = 0;
        int totalCnt0Volatile = 0;
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
            //comptage de faux silences
            if (neutrality[(saveCnt + i) % sizeBuffer] == 0 &&
                    happiness[(saveCnt + i) % sizeBuffer] == 0 &&
                    sadness[(saveCnt + i) % sizeBuffer] == 0 &&
                    anger[(saveCnt + i) % sizeBuffer] == 0 &&
                    fear[(saveCnt + i) % sizeBuffer] == 0 ) {
                    cnt0++;
                }
                else {
                    if (cnt0 <= falseSilenceCnt) {
                        totalCnt0 += cnt0;                      
                    }
                    cnt0 = 0;
                }
        }
        for(int i=0; i < nMAVolatile; i++)
        {
            anger_value += anger[(saveCntVolatile + i) % sizeBuffer];
            fear_value += fear[(saveCntVolatile + i) % sizeBuffer];

            //comptage de faux silences
            if (neutrality[(saveCntVolatile + i) % sizeBuffer] == 0 &&
                    happiness[(saveCntVolatile + i) % sizeBuffer] == 0 &&
                    sadness[(saveCntVolatile + i) % sizeBuffer] == 0 &&
                    anger[(saveCntVolatile + i) % sizeBuffer] == 0 &&
                    fear[(saveCntVolatile + i) % sizeBuffer] == 0) {
                    cnt0Volatile++;
                }
                else {
                    if (cnt0Volatile <= falseSilenceCnt) {
                        totalCnt0Volatile += cnt0Volatile;
                    }
                    cnt0Volatile = 0;
                }
        }

        neutrality_value = neutrality_value * correction_value / (nMA-totalCnt0) ;
        happiness_value = happiness_value * correction_value / (nMA-totalCnt0);
        sadness_value = sadness_value * correction_value / (nMA-totalCnt0);
        anger_value = anger_value * correction_value/ (nMAVolatile-totalCnt0Volatile);
        fear_value = fear_value * correction_value/ (nMAVolatile-totalCnt0Volatile);

        saveCnt = (saveCnt + 1) % sizeBuffer;
        saveCntVolatile = (saveCntVolatile + 1) % sizeBuffer;
        cnt = (cnt + 1) % sizeBuffer;
        totalCnt0 =0;
        totalCnt0Volatile = 0;
        
        //correction neutrality/colere
        if(neutrality_value-anger_value>anger_offset){
            anger_value = 0;
        }

        neutrality_value = AvatarMaker.PercentageConvertor(neutrality_value, 0f, 1f, 0, 100);
        happiness_value = AvatarMaker.PercentageConvertor(happiness_value, 0f, 1f, 0, 100);
        sadness_value = AvatarMaker.PercentageConvertor(sadness_value, 0f, 1f, 0, 100);
        anger_value = AvatarMaker.PercentageConvertor(anger_value, 0f, 1f, 0, 100);
        fear_value = AvatarMaker.PercentageConvertor(fear_value, 0f, 1f, 0, 100);
    }

    private void UpdateAvatarEmotionMAA()
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

        neutrality_value = AvatarMaker.PercentageConvertor(neutrality_value, 0f, 1f, 0, 100);
        happiness_value = AvatarMaker.PercentageConvertor(happiness_value, 0f, 1f, 0, 100);
        sadness_value = AvatarMaker.PercentageConvertor(sadness_value, 0f, 1f, 0, 100);
        anger_value = AvatarMaker.PercentageConvertor(anger_value, 0f, 1f, 0, 100);
        fear_value = AvatarMaker.PercentageConvertor(fear_value, 0f, 1f, 0, 100);
    }

    private void ResetTables(){
        for(int i=0; i<sizeBuffer; i++){
            neutrality[i] = 0;
            happiness[i] = 0;
            sadness[i] = 0;
            anger[i] = 0;
            fear[i] = 0;
        }
        for(int i=0; i<5; i++){
            emotions[i] = 0;
        }
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
                AvatarMakerManager.SetBlendshapeValue(morphName, 100);               
            }
            else
            {
                AvatarMakerManager.SetBlendshapeValue(morphName, 0);
                state = Random.Range(0, 7);
            }
            mouthUp = !mouthUp;
        }
        else
        {
            float value = 0;
            foreach (MORPH3D.FOUNDATIONS.Morph m in AvatarMakerManager.coreMorphs.morphs)
            {
                if (m.name == morphName)
                {
                    value = m.value;
                }
            }
            if (mouthUp)
            {
                value += AvatarMaker.PercentageConvertor(Time.deltaTime, 0, speechSpeed, 0, 100);
                AvatarMakerManager.SetBlendshapeValue(morphName, value);
            }
            else
            {
                value -= AvatarMaker.PercentageConvertor(Time.deltaTime, 0, speechSpeed, 0, 100);
                AvatarMakerManager.SetBlendshapeValue(morphName, value);
            }
        }
    }


    public int IndexOfMaxEmotion(float[] emotions)
    {
        int maxIndex = 0;
        float maxValue = emotions[0];
        for(int i=1; i<emotions.Length; i++)
        {
            if (emotions[i]>maxValue)
            {
                maxValue = emotions[i];
                maxIndex = i;
            }
        }
        return maxIndex;
    }

    public void UpdateAvatarBlendshapeOnRealTime(string morphName, float newValue)
    {
        float previousValue = 0;
        foreach(MORPH3D.FOUNDATIONS.Morph m in AvatarMakerManager.coreMorphs.morphs)
        {
            if(m.name == morphName)
            {
                previousValue = m.value;
            }
        }
        if(newValue > previousValue)
        {
            float updateValue = previousValue + AvatarMaker.PercentageConvertor(Time.deltaTime, 0, updateOnRealTimeSpeed, 0, 100);
            if(updateValue < newValue)
            {
                AvatarMakerManager.SetBlendshapeValue(morphName, updateValue);
            }
            else
            {
                AvatarMakerManager.SetBlendshapeValue(morphName, newValue);
            }
        }
        if(newValue < previousValue)
        {
            float updateValue = previousValue - AvatarMaker.PercentageConvertor(Time.deltaTime, 0, updateOnRealTimeSpeed, 0, 100);
            if (updateValue > newValue)
            {
                AvatarMakerManager.SetBlendshapeValue(morphName, updateValue);
            }
            else
            {
                AvatarMakerManager.SetBlendshapeValue(morphName, newValue);
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
