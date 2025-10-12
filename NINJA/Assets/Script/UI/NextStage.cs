using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextStage : MonoBehaviour
{
    public GameObject character;
    public GameObject enemy;

    public Image gameOverImage;
    public Button restartButton;
    public Button returnButton;
    // Start is called before the first frame update
    void Start()
    {
        character = GameObject.Find("Character");
        enemy = GameObject.Find("Enemy");

        gameOverImage.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(character == null)
        {
            StartCoroutine(GameOver());
        }

    }
    private IEnumerator GameOver()
    {

        yield return new WaitForSeconds(2f);
        gameOverImage.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
        returnButton.gameObject.SetActive(true);

    }
    private IEnumerator StageClear()
    {

        yield return new WaitForSeconds(2f);

    }
}
