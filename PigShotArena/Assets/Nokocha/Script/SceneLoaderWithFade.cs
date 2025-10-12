using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoaderWithFade : MonoBehaviour
{
    public static SceneLoaderWithFade Instance;

    [Header("Fade Settings")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("SE Settings")]
    public AudioClip fallSE;
    private AudioSource audioSource;

    private void Start()
    {
        Application.targetFrameRate = 60;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); //シーンをまたいで生き残る
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadSceneWithFade(string sceneName)
    {

        if (fallSE != null)
        {
            audioSource.PlayOneShot(fallSE);
        }

        StartCoroutine(FadeAndLoad(sceneName));

        
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
        yield return new WaitForSeconds(0.1f); // 読み込み待機（任意）
        
    }
        
    private IEnumerator FadeOut()
    {
        float t = 0f;
        Color color = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = color;
            Debug.Log("FageOut end");
            yield return null;
        }
    }

    private IEnumerator FadeIn()
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
