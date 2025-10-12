using UnityEngine;
using UnityEngine.InputSystem;

public class MouseManager : MonoBehaviour
{
    [Header("=== �}�E�X�Ǘ� ===")]
    [Tooltip("�}�E�X��\������ړ��ʂ�臒l")]
    public float moveThreshold = 10f;

    [Tooltip("��\���ɂȂ�܂ł̕b��")]
    public float hideDelay = 3f;

    private float idleTimer = 0f;
    private bool isVisible = true;

    private GameState gameState = GameState.Startup;

    public static MouseManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

    private void OnEnable()
    {
        EventBus.SystemEvents.OnGameStateChange += CheckState;
    }
    private void OnDisable()
    {
        EventBus.SystemEvents.OnGameStateChange -= CheckState;
    }

    void Update()
    {
        if (Mouse.current == null || gameState == GameState.Playing) return;

        // �}�E�X�ړ��ʂ��擾
        Vector2 delta = Mouse.current.delta.ReadValue();

        // ���ȏ㓮������\���ɐ؂�ւ�
        if (delta.sqrMagnitude > moveThreshold * moveThreshold)
        {
            ShowMouse();
            idleTimer = 0f;
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= hideDelay)
            {
                HideMouse();
            }
        }
    }

    private void CheckState(GameState state)
    {
        gameState = state;
        if (gameState == GameState.Playing)
        {
            HideMouse();
        }
    }
    private void ShowMouse()
    {
        if (!isVisible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; // ���b�N����
            isVisible = true;
        }
    }

    private void HideMouse()
    {
        if (isVisible)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked; // �����ɌŒ�
            isVisible = false;
        }
    }
}