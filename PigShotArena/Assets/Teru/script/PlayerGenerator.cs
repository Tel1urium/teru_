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
            // すでにどこかのプレイヤーに使われていればスキップ
            bool alreadyUsed = PlayerInput.all.Any(p => p.user.pairedDevices.Contains(gamepad));
            if (alreadyUsed) continue;

            // Aボタンが押されたら参加
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
        controlScheme: "Gamepad",  // Control Scheme 名はInputActionAssetに応じて
        pairWithDevice: gamepad,
        splitScreenIndex: playerIndex++
    );
        // 生成した Player を UI に渡す
        if (playerIndex - 1 < chargeUIManagers.Length)
        {
            chargeUIManagers[playerIndex - 1].AssignPlayer(playerInput.GetComponent<Player>());
            Debug.Log($"Assigned {playerInput.name} to {chargeUIManagers[playerIndex - 1].name}");
        }
        Debug.Log($"Player {playerIndex} joined with {gamepad.displayName}");
        //パーティクルシステム関連
        var attractor = FindObjectOfType<ParticleGnerater>();
        if (attractor != null)
        {
            attractor.SetTarget(playerInput.transform);
        }
    }
}
