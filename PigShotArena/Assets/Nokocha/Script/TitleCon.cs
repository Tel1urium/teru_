using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class TitleCon : MonoBehaviour
{
    
    public AudioSource audioSource;
    public AudioClip seClip;
    public Image fadeImage;
    public string nextSceneName = "StageSelect";
    public float fadeDuration = 1f;


    void Start()
    {
        Application.targetFrameRate = 60;
    }
    public void OnButtonPressed()
    {
        StartCoroutine(TransitionRouTime());
    }

    private IEnumerator TransitionRouTime()
    {
        if(audioSource && seClip)
        {
            audioSource.PlayOneShot(seClip);
        }

        yield return StartCoroutine(FadeOut());

        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator FadeOut()
    {
        float t = 0;
        Color color = fadeImage.color;

        while(t <  fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

    }

    

    private void EndGame()
    {
        //Escが押された時
        if (Input.GetKey(KeyCode.Escape))
        {

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
    Application.Quit();//ゲームプレイ終了
#endif
        }

    }

    // Update is called once per frame
    void Update()
    {
        EndGame();
    }
   
}
