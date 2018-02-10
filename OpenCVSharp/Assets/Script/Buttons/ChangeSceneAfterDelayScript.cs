using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ChangeSceneAfterDelayScript : MonoBehaviour {

	public string _nextScene = "";

	public float _delay = 5f;

	public IEnumerator Start()
	{
        yield return new WaitForSeconds(_delay);
        float fadeTime = /*GameObject.Find("FadeScript").*/GetComponent<FadingScene>().BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
        
        SceneManager.LoadScene(_nextScene);
	}
}
