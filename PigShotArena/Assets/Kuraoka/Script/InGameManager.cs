using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;

public class InGameManager : MonoBehaviour
{
    //ゲーム画面のステート管理
    public enum GameState
    {
       CountDown,//開始時のカウントダウン
       InGame,//試合中
       IsSkill,//スキル発動時
       GameEnd//試合終了時
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
       Text.text = countdowntime.ToString("F0"); // 小数点なし
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
