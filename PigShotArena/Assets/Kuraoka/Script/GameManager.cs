using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    public enum State
    {
        Start,   // カウントダウン中
        GameStartMessage, // "GAME START!!!" 表示中
        Ingame,  // ゲーム進行中
        End
    }

    [SerializeField] private TMP_Text CDtext;
    [SerializeField] private float countdownTime = 3f; // カウントダウン秒数
    [SerializeField] private float gameStartDisplayDuration = 1.5f; // "GAME START!!!" の表示時間

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
                    CDtext.text = Mathf.CeilToInt(countdown).ToString(); // 小数点切り上げ
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
                    CDtext.gameObject.SetActive(false); // テキスト非表示
                   
                    
                }
                break;

            case State.Ingame:
                // 実際のゲームロジックはここで動かす
                break;

            case State.End:
                break;
        }
    }

    // 状態を外部から参照したいとき用のGetter
    public State GetGameState() => state;
}
