using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class StageLoader : MonoBehaviour
{

    public AudioSource audioSource;
    public AudioClip seClip;
    public Image fadeImage;
    public string nextSceneNameN = "Normal";
    public string nextSceneNameI = "Ice";
    public string nextSceneNameT = "trampoline";
    public float fadeDuration = 1f;
    public void LoadStageN()
    {
        StartCoroutine(TransitionRouTimeN());
    }
    public void LoadStageI()
    {
        StartCoroutine(TransitionRouTimeI());
    }
    public void LoadStageT()
    {
        StartCoroutine(TransitionRouTimeT());
    }

    private IEnumerator TransitionRouTimeN()
    {
        if (audioSource && seClip)
        {
            audioSource.PlayOneShot(seClip);
        }

        yield return StartCoroutine(FadeOut());

        SceneManager.LoadScene(nextSceneNameN);
    }
    private IEnumerator TransitionRouTimeI()
    {
        if (audioSource && seClip)
        {
            audioSource.PlayOneShot(seClip);
        }

        yield return StartCoroutine(FadeOut());

        SceneManager.LoadScene(nextSceneNameI);
    }
    private IEnumerator TransitionRouTimeT()
    {
        if (audioSource && seClip)
        {
            audioSource.PlayOneShot(seClip);
        }

        yield return StartCoroutine(FadeOut());

        SceneManager.LoadScene(nextSceneNameT);
    }

    IEnumerator FadeOut()
    {
        float t = 0;
        Color color = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

    }
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
