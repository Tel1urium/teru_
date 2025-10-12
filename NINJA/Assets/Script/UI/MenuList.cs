using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuList : MonoBehaviour
{
    public GameObject menuList;
    [SerializeField] private bool menuKeys = true;
    [SerializeField] private AudioClip selectSound;
    private AudioSource audioSource;
   //[SerializeField] private AudioSource bgmSound;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (menuKeys)
        {
            if(Input.GetKeyDown(KeyCode.G))
            {
                menuList.SetActive(true);
                menuKeys = false;
                Time.timeScale = (0);
                //bgmSound.Pause();
            }
        }
        else if(Input.GetKeyDown(KeyCode.G)) 
        {
            menuList.SetActive(false);
            menuKeys = true;
            Time.timeScale = (1);
            //bgmSound.Play();
        }
    }
    public void Return()
    {
        menuList.SetActive(false);
        menuKeys = true;
        Time.timeScale = (1);
        //bgmSound.Play();
    }
    public void Title()
    {
        audioSource.PlayOneShot(selectSound);
        FadeManager.Instance.LoadScene("Title",2f);
        Time.timeScale = (1);
    }
    public void OniRestartStage()
    {
        audioSource.PlayOneShot(selectSound);
        FadeManager.Instance.LoadScene("Stage_1",2f);
        Time.timeScale = (1);
    }
    public void YukiRestartStage()
    {
        audioSource.PlayOneShot(selectSound);
        FadeManager.Instance.LoadScene("Stage_2", 2f);
        Time.timeScale = (1);
    }
    public void Next()
    {
        audioSource.PlayOneShot(selectSound);
        FadeManager.Instance.LoadScene("Stage_2", 2f);
        Time.timeScale = (1);
    }

}
