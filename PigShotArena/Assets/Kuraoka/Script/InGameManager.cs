using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;

public class InGameManager : MonoBehaviour
{
    //�Q�[����ʂ̃X�e�[�g�Ǘ�
    public enum GameState
    {
       CountDown,//�J�n���̃J�E���g�_�E��
       InGame,//������
       IsSkill,//�X�L��������
       GameEnd//�����I����
    }
     public GameState State;
    [SerializeField] TMP_Text Text;
    //[SerializeField] Image Image;
    float countdowntime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        countdowntime = 3.0f;
        State = GameState.CountDown;
       Text.gameObject.SetActive(true);
       Text.text = countdowntime.ToString("F0"); // �����_�Ȃ�
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        switch(State)
        {
            case GameState.CountDown:
                CountDown();
                break;
                case GameState.InGame:
                break;
                case GameState.IsSkill:
                break;
                case GameState.GameEnd:
                break;
                default:
                break;
        }
    }
    void CountDown()
    {
        countdowntime -= Time.deltaTime;
        if(countdowntime>0)
        {
            Text.text = Mathf.Ceil(countdowntime).ToString();
        }
        else
        {
            Text.text = "START!";
            State=GameState.CountDown;

            Invoke("HideText", 1f);
        }
    }

    void HideText()
    {
        Text.gameObject.SetActive(false);
    }
}
