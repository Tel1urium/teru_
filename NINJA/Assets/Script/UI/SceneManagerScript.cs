using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    [SerializeField] private string _loadScene;
    [SerializeField] private GameObject _fadePanel;
    private AudioSource _audioSource;
    public int _delay; //遅延させたい秒数
    public float _fadeSpeed = 0.01f;//フェードアウトのスピード
    public AudioClip _selectSound;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if(Input.anyKeyDown)
        {
            if(!Input.GetKey(KeyCode.Escape)&&!Input.GetMouseButtonDown(0)&&!Input.GetKey(KeyCode.G))
            {
                _audioSource.PlayOneShot(_selectSound);
                _fadePanel.GetComponent<FadeInOut>().FadeOutStart(_fadeSpeed);
                Invoke("SceneChange", _delay);
            }
        }
        if(Input.GetButton("Skill_Dash"))
        {
            _audioSource.PlayOneShot(_selectSound);
            Invoke("SceneChange", _delay);
        }
    }

    public void SceneChange()
    {
        SceneManager.LoadScene(_loadScene);
    }
}
