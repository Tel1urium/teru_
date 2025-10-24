using UnityEngine;
using UnityEngine.UI;

public class ChargeUIManager : MonoBehaviour
{

    [SerializeField] private Player player; // �����̓V�[����UI�ɃA�^�b�`
    [SerializeField] private UnityEngine.UI.Image chargeBarFill;
    [SerializeField] private float maxCharge = 100f;

    public void AssignPlayer(Player p)
    {
        player = p;
    }

    void Update()
    {
        if (player == null || chargeBarFill == null) return;
        float ratio = Mathf.Clamp01(player.GetChargePow() / maxCharge);
        chargeBarFill.fillAmount = ratio;
    }
}
