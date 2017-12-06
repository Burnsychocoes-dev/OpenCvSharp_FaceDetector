using UnityEngine;
using System.Collections;

public class UserAuthorization : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return Application.RequestUserAuthorization(UnityEngine.UserAuthorization.WebCam | UnityEngine.UserAuthorization.Microphone);
        if (Application.HasUserAuthorization(UnityEngine.UserAuthorization.WebCam | UnityEngine.UserAuthorization.Microphone))
        {
        }
        else
        {
        }
    }
}