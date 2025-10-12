using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class FadeIn : MonoBehaviour
{
    public float fadeDuration = 1f;
    public Image fadeImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(OnFadeIn());
    }
    private IEnumerator OnFadeIn()
    {
        float t = fadeDuration;
        Color color = fadeImage.color;

        while (t > 0f)
        {
            t -= Time.deltaTime;
            color.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = color;

            yield return null;
        }
        Debug.Log("FadeIn end");
    }
}
