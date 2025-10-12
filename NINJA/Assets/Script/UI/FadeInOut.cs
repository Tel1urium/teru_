using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FadeInOut : MonoBehaviour
{
    private float red, green, blue, alfa;
    private bool fadeOutFlag = false;
    private bool fadeInFlag = false;

    private Image fadeImage;//パネル

    private float speed;//フェードするスピード

    void Start()
    {
        fadeImage = GetComponent<Image>();
        red = fadeImage.color.r;
        green = fadeImage.color.g;
        blue = fadeImage.color.b;
        alfa = fadeImage.color.a;
        red = 0;
        green = 0;
        blue = 0;
        Alpha();
    }

    void Update()
    {
        if (fadeInFlag)
        {
            FadeIn();
        }

        if (fadeOutFlag)
        {
            FadeOut();
        }
    }

    private void FadeIn()
    {
        alfa -= speed;
        Alpha();
        if (alfa <= 0)
        {
            fadeInFlag = false;
            fadeImage.enabled = false;
        }
    }

    private void FadeOut()
    {
        fadeImage.enabled = true;
        alfa += speed;
        Alpha();
        if (alfa >= 1)
        {
            fadeOutFlag = false;
        }
    }

    private void Alpha()
    {
        fadeImage.color = new Color(red, green, blue, alfa);
    }

    public void FadeInStart(float fadeSpeed = 0.01f)
    {
        fadeOutFlag = false;
        alfa = 1;
        red = 0;
        green = 0;
        blue = 0;
        Alpha();
        speed = fadeSpeed;
        fadeInFlag = true;
    }

    public void FadeOutStart(float fadeSpeed = 0.01f)
    {
        fadeInFlag = false;
        alfa = 0;
        red = 0;
        green = 0;
        blue = 0;
        Alpha();
        speed = fadeSpeed;
        fadeOutFlag = true;
    }
}
