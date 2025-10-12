using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayGuidCon : MonoBehaviour
{

    public static PlayGuidCon Instance;

    [Header("背景設定")]
    public Image back;
    public float FadeDuration = 0.5f;

    [Header("イメージ設定")]
    public Image title;
    public Image start;
    public Image close;
    public Image guid;
    public Image Aimage;
    public Image Entryimage;
    public Image LStick;
    public Image Move;
    public Image RT;
    public Image Charge;

    [Header("ボタン設定")]
    public Button stgbtn;
    public Button opnbtn;
    public Button clzbtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        clzbtn.interactable = false;
        clzbtn.gameObject.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OpenOnButtonPressed()
    {
        StartCoroutine(OpenFade());
    }
    public void CloseOnButtonPressed()
    {
        StartCoroutine(CloseFade());
    }

    private IEnumerator OpenFade()
    {
        clzbtn.interactable = true;
        stgbtn.interactable = false;
        opnbtn.interactable = false;
        yield return StartCoroutine(BackFadeIn());
        yield return StartCoroutine(TitleFadeOut());
        yield return StartCoroutine(StartFadeOut());
        yield return StartCoroutine(PlayGuidFadeOut());
        yield return StartCoroutine(CloseFadeIn());
        yield return StartCoroutine(TextImageFadeIn());
    }

    private IEnumerator CloseFade()
    {
        clzbtn.interactable = false;
        opnbtn.interactable = true;
        stgbtn.interactable = true;
        yield return StartCoroutine(CloseFadeOut());
        yield return StartCoroutine(TextImageFadeOut());
        yield return StartCoroutine(BackFadeOut());
        yield return StartCoroutine(TitleFadeIn());
        yield return StartCoroutine(StartFadeIn());
        yield return StartCoroutine(PlayGuidFadeIn());
    }

    private IEnumerator TextImageFadeIn()
    {
        float t = 0f;
        Color color = Aimage.color;

        while (t < FadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Clamp01(t / FadeDuration);
            Aimage.color = color;
            Entryimage.color = color;
            LStick.color = color;
            Move.color = color;
            RT.color = color;
            Charge.color = color;
            yield return null;
        }
    }

    private IEnumerator TextImageFadeOut()
    {
        float t = FadeDuration - 0.2f;
        Color color = Aimage.color;

        while (t > 0f)
        {
            t -= Time.deltaTime;
            color.a = Mathf.Clamp01(t / FadeDuration);
            Aimage.color = color;
            Entryimage.color = color;
            LStick.color = color;
            Move.color = color;
            RT.color = color;
            Charge.color = color;
            yield return null;
        }
    }
    private IEnumerator BackFadeIn()
    {
        float t = 0f;
        Color color = back.color;

        while (t < FadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(0.0f, 0.8f, t / FadeDuration);
            back.color = color;
            yield return null;
        }
    }
    private IEnumerator BackFadeOut()
    {
        float t = FadeDuration - 0.2f;
        Color color = back.color;

        while (t > 0f)
        {
            t -= Time.deltaTime;
            color.a = Mathf.Clamp01(t / FadeDuration);
            back.color = color;
            yield return null;
        }
    }

    private IEnumerator CloseFadeOut()
    {
        float t = FadeDuration;
        Color color = close.color;

        while (t > 0f)
        {
            t -= Time.deltaTime;
            color.a = Mathf.Clamp01(t / FadeDuration);
            close.color = color;
            yield return null;
        }
    }

    private IEnumerator CloseFadeIn()
    {
        clzbtn.gameObject.SetActive(true); //表示
        float t = 0f;
        Color color = close.color;
        color.a = 0f;
        close.color = color;

        while (t < FadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Clamp01(t / FadeDuration);
            close.color = color;
            yield return null;
        }
    }

    private IEnumerator TitleFadeOut()
    {
        float t = FadeDuration;
        Color color = title.color;

        while (t > 0f)
        {
            t -= Time.deltaTime;
            color.a = Mathf.Clamp01(t / FadeDuration);
            title.color = color;
            yield return null;
        }
    }

    private IEnumerator TitleFadeIn()
    {
        float t = 0f;
        Color color = title.color;

        while (t < FadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Clamp01(t / FadeDuration);
            title.color = color;
            yield return null;
        }
    }

    private IEnumerator StartFadeOut()
    {
        float t = FadeDuration;
        Color color = start.color;

        while (t > 0f)
        {
            t -= Time.deltaTime;
            color.a = Mathf.Clamp01(t / FadeDuration);
            start.color = color;
            yield return null;
        }
    }

    private IEnumerator StartFadeIn()
    {
        float t = 0f;
        Color color = start.color;

        while (t < FadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Clamp01(t / FadeDuration);
            start.color = color;
            yield return null;
        }

    }

    private IEnumerator PlayGuidFadeOut()
    {
        float t = FadeDuration;
        Color color = guid.color;

        while (t > 0f)
        {
            t -= Time.deltaTime;
            color.a = Mathf.Clamp01(t / FadeDuration);
            guid.color = color;
            yield return null;
        }
    }

    private IEnumerator PlayGuidFadeIn()
    {
        float t = 0f;
        Color color = guid.color;

        while (t < FadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Clamp01(t / FadeDuration);
            guid.color = color;
            yield return null;
        }

    }
}
