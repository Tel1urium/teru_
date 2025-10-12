using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    public enum State
    {
        Start,   // �J�E���g�_�E����
        GameStartMessage, // "GAME START!!!" �\����
        Ingame,  // �Q�[���i�s��
        End
    }

    [SerializeField] private TMP_Text CDtext;
    [SerializeField] private float countdownTime = 3f; // �J�E���g�_�E���b��
    [SerializeField] private float gameStartDisplayDuration = 1.5f; // "GAME START!!!" �̕\������

    private float countdown;
    private float gameStartTimer;
    private State state;

    void Start()
    {
        Application.targetFrameRate = 60;

        if (CDtext == null)
        {
            CDtext = GetComponent<TMP_Text>();
        }

        countdown = countdownTime;
        state = State.Start;
    }

    void Update()
    {
        switch (state)
        {
            case State.Start:
                countdown -= Time.deltaTime;

                if (countdown > 0)
                {
                    CDtext.text = Mathf.CeilToInt(countdown).ToString(); // �����_�؂�グ
                }
                else
                {
                    CDtext.text = "GAME START!!!";
                    gameStartTimer = gameStartDisplayDuration;
                    state = State.GameStartMessage;
                }
                break;

            case State.GameStartMessage:
                gameStartTimer -= Time.deltaTime;
                if (gameStartTimer <= 0)
                {
                    state = State.Ingame; 
                    Debug.Log("Game has started.");
                    CDtext.gameObject.SetActive(false); // �e�L�X�g��\��
                   
                    
                }
                break;

            case State.Ingame:
                // ���ۂ̃Q�[�����W�b�N�͂����œ�����
                break;

            case State.End:
                break;
        }
    }

    // ��Ԃ��O������Q�Ƃ������Ƃ��p��Getter
    public State GetGameState() => state;
}
