using UnityEngine;
using UnityEngine.UI;

public class ChargeUIManager : MonoBehaviour
{
    [SerializeField] private Player player; // ���̃o�[���Ή�����Player
    [SerializeField] private Image chargeBarFill; // Image��fillAmount�Ńo�[��L�΂�
    [SerializeField] private float maxCharge = 100f; // Player�Őݒ肵�� mChragePow �ɍ��킹��

    public void AssignPlayer(Player assignedPlayer)
    {
        player = assignedPlayer;
    }

    void Update()
    {
        if (player == null || chargeBarFill == null) return;

        // ���݂̃`���[�W�ʂ��擾���A�������v�Z
        float ratio = Mathf.Clamp01(player.GetChargePow() / maxCharge);
        chargeBarFill.fillAmount = ratio;
    }
}
