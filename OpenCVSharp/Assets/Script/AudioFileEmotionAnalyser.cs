using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class AudioFileEmotionAnalyser : MonoBehaviour
{
    private StreamReader reader;
    [SerializeField]
    private string path;
    private float cnt = 0;
    private string line;
    private string[] words;
    [SerializeField]
    private double[] emotions = new double[5];
    public double[] Emotions
    {
        get { return emotions; }
    }

    private MORPH3D.M3DCharacterManager avatarManager;

    // Use this for initialization
    void Start()
    {
        avatarManager = GetComponent<MORPH3D.M3DCharacterManager>();
        InitReadString();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("oui");
        if (cnt >= 1)
        {
            cnt = 0;
            if (reader.Peek() >= 0)
            {
                //Debug.Log(reader.ReadLine());
                line = reader.ReadLine();
                words = line.Split(';');
                emotions[0] = Convert.ToDouble(words[1]);
                Debug.Log(emotions[0]);
            }
            else
            {
                Debug.Log("rien");
            }

        }
        cnt += Time.deltaTime;
    }

    [MenuItem("Tools/Read file")]
    private void InitReadString()
    {
        //path = "Assets/Resources/test.txt";

        //Read the text from directly from the test.txt file
        reader = new StreamReader(path);
        //Debug.Log(reader.ReadToEnd());
        //reader.Close();
    }


    private void UpdateAvatarEmotion()
    {
        //double neutrality = emotions[0];
        //double happiness = emotions[1];
        //double sadness = emotions[2];
        //double anger = emotions[3];
        //double fear = emotions[4];
        float neutrality_value = AvatarMaker.PercentageConvertorNeg((float)emotions[0], 0f, 1f, 0, 100);
        float happiness_value = AvatarMaker.PercentageConvertorNeg((float)emotions[1], 0f, 1f, 0, 100);
        float sadness_value = AvatarMaker.PercentageConvertorNeg((float)emotions[2], 0f, 1f, 0, 100);
        float anger_value = AvatarMaker.PercentageConvertorNeg((float)emotions[3], 0f, 1f, 0, 100);
        float fear_value = AvatarMaker.PercentageConvertorNeg((float)emotions[4], 0f, 1f, 0, 100);
        avatarManager.SetBlendshapeValue("eCTRLHappy", happiness_value);
        avatarManager.SetBlendshapeValue("eCTRLSad", sadness_value);
        avatarManager.SetBlendshapeValue("eCTRLAngry", anger_value);
        avatarManager.SetBlendshapeValue("eCTRLFear", fear_value);
    }
}
