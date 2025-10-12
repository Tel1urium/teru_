using UnityEngine;
using UnityEngine.UI;

public class ChargeUIManager : MonoBehaviour
{
    [SerializeField] private Player player; // このバーが対応するPlayer
    [SerializeField] private Image chargeBarFill; // ImageのfillAmountでバーを伸ばす
    [SerializeField] private float maxCharge = 100f; // Playerで設定した mChragePow に合わせる

    public void AssignPlayer(Player assignedPlayer)
    {
        player = assignedPlayer;
    }

    void Update()
    {
        if (player == null || chargeBarFill == null) return;

        // 現在のチャージ量を取得し、割合を計算
        float ratio = Mathf.Clamp01(player.GetChargePow() / maxCharge);
        chargeBarFill.fillAmount = ratio;
    }
}
