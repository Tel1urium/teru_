using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.Users;

public class PlayerGenerator : MonoBehaviour
{
    public GameObject[] playerPrefab;
    private int playerIndex = 0;
    [SerializeField] private ChargeUIManager[] chargeUIManagers;
    void Update()
    {
        foreach (var gamepad in Gamepad.all)
        {
            // ���łɂǂ����̃v���C���[�Ɏg���Ă���΃X�L�b�v
            bool alreadyUsed = PlayerInput.all.Any(p => p.user.pairedDevices.Contains(gamepad));
            if (alreadyUsed) continue;

            // A�{�^���������ꂽ��Q��
            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                JoinPlayer(gamepad);
            }
        }
    }

    void JoinPlayer(Gamepad gamepad)
    {
        var playerInput = PlayerInput.Instantiate(
        playerPrefab[playerIndex],
        controlScheme: "Gamepad",  // Control Scheme ����InputActionAsset�ɉ�����
        pairWithDevice: gamepad,
        splitScreenIndex: playerIndex++
    );
        // �������� Player �� UI �ɓn��
        if (playerIndex - 1 < chargeUIManagers.Length)
        {
            chargeUIManagers[playerIndex - 1].AssignPlayer(playerInput.GetComponent<Player>());
            Debug.Log($"Assigned {playerInput.name} to {chargeUIManagers[playerIndex - 1].name}");
        }
        Debug.Log($"Player {playerIndex} joined with {gamepad.displayName}");
        //�p�[�e�B�N���V�X�e���֘A
        var attractor = FindObjectOfType<ParticleGnerater>();
        if (attractor != null)
        {
            attractor.SetTarget(playerInput.transform);
        }
    }
}
