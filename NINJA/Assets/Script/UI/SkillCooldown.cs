using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillCooldown : MonoBehaviour
{
    // スキルのクールダウン用画像（ロール、ダッシュ、バック）
    public Image imgCooldownRoll;
    public Image imgCooldownDash;
    public Image imgCooldownBack;
    //----------------------------------------------------------------------

    // 各スキルのクールダウン時間（秒）
    public float cooldownRoll;
    public float cooldownDash;
    public float cooldownBack;
    //----------------------------------------------------------------------

    // 各スキルがクールダウン中かどうかを管理するフラグ
    bool isCooldownRoll;
    bool isCooldownDash;
    bool isCooldownBack;

    // Startはゲーム開始時に一度だけ呼ばれる
    void Start()
    {

    }

    // Updateは毎フレーム呼ばれる
    void Update()
    {
        // ロールスキルが使用された場合
        if (Input.GetButtonUp("Skill_Roll"))
        {
            isCooldownRoll = true; // ロールスキルのクールダウンを開始
        }

        // ロールスキルのクールダウン処理
        if (isCooldownRoll)
        {
            imgCooldownRoll.fillAmount += 1 / cooldownRoll * Time.deltaTime; // 時間に応じて進捗を更新
            if (imgCooldownRoll.fillAmount >= 1)
            {
                imgCooldownRoll.fillAmount = 0; // クールダウン完了後、進捗をリセット
                isCooldownRoll = false; // クールダウン終了
            }
        }

        //----------------------------------------------------------------------

        // バックスキルが使用された場合
        if (Input.GetButtonUp("Skill_Back"))
        {
            isCooldownBack = true; // バックスキルのクールダウンを開始
        }

        // バックスキルのクールダウン処理
        if (isCooldownBack)
        {
            imgCooldownBack.fillAmount += 1 / cooldownBack * Time.deltaTime; // 時間に応じて進捗を更新
            if (imgCooldownBack.fillAmount >= 1)
            {
                imgCooldownBack.fillAmount = 0; // クールダウン完了後、進捗をリセット
                isCooldownBack = false; // クールダウン終了
            }
        }

        //----------------------------------------------------------------------

        // ダッシュスキルが使用された場合
        if (Input.GetButtonUp("Skill_Dash"))
        {
            isCooldownDash = true; // ダッシュスキルのクールダウンを開始
        }

        // ダッシュスキルのクールダウン処理
        if (isCooldownDash)
        {
            imgCooldownDash.fillAmount += 1 / cooldownDash * Time.deltaTime; // 時間に応じて進捗を更新
            if (imgCooldownDash.fillAmount >= 1)
            {
                imgCooldownDash.fillAmount = 0; // クールダウン完了後、進捗をリセット
                isCooldownDash = false; // クールダウン終了
            }
        }
    }
}
