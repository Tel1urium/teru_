using System;
using System.Collections.Generic;
using UnityEngine;
using static EventBus;

[DisallowMultipleComponent]
public class PlayerPersistence : MonoBehaviour
{
    public static PlayerPersistence Instance { get; private set; }

    [Header("�V�K�Q�[�����̏����l")]
    public int defaultMaxHp = 50;
    public int defaultCurrentHp = 50;

    [Tooltip("������������iInspector �ɒ��ڃh���b�O�j")]
    public List<WeaponItem> defaultWeaponItems = new List<WeaponItem>() { };

    [Tooltip("�e����̏����ϋv�idefaultWeaponItems �Ɠ����B����Ȃ���� prefab ����=5�j")]
    public List<int> defaultDurabilities = new List<int>() { };

    [Tooltip("�E��̏��������C���f�b�N�X�i-1 �Ŗ������j")]
    public int defaultMainIndex = 0;

    [Header("�ۑ��Ώۂ̏�ԁiGameState�j")]
    [Tooltip("���̏�ԂɑJ�ڂ���ƃX�i�b�v�V���b�g�ۑ����s��")]
    public List<GameState> saveOnStates = new List<GameState>();

    // === �����^�C�� ===
    private PlayerSaveData _current;
    public PlayerSaveData Current => _current;
    private double _runStartTimeSec = 0.0;
    private HashSet<GameState> _saveStateSet;
    private bool _isCounting = false;
    private double _sessionResumeAt = 0.0;
    private double _currentSessionTime= 0.0;

    private int currentKillCount = 0;
    private int currentSkillUseCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        _saveStateSet = new HashSet<GameState>(saveOnStates);

        SystemEvents.OnGameStateChange += HandleGameStateChange;
        PlayerEvents.OnPlayerSpawned += HandlePlayerSpawned;
        PlayerEvents.OnWeaponSkillUsed += HandleWeaponSkillUse;
        EnemyEvents.OnEnemyDeath += HandleEnemyDefeated;
        PlayerEvents.OnPlayerDead += HandlePlayerDied;

    }

    private void OnDestroy()
    {
        SystemEvents.OnGameStateChange -= HandleGameStateChange;
        PlayerEvents.OnPlayerSpawned -= HandlePlayerSpawned;
        PlayerEvents.OnWeaponSkillUsed -= HandleWeaponSkillUse;
        EnemyEvents.OnEnemyDeath -= HandleEnemyDefeated;
        PlayerEvents.OnPlayerDead -= HandlePlayerDied;
    }

    private void HandleGameStateChange(GameState next)
    {
        if (next == GameState.Title)
        {
            CreateNewGameData();
            return;
        }

        if (_saveStateSet.Contains(next))
        {
            SnapshotSaveFromCurrentPlayer();
        }

        if (next != GameState.Playing)
        {
            StopSessionCounterAndAccumulate(); 
        }
        else
        {
            StartSessionCounter();
        }
    }

    private void StartSessionCounter()
    {
        // ���łɃJ�E���g���Ȃ牽�����Ȃ�
        if (_isCounting) return;

        // ���݂̃��A���^�C���b
        _sessionResumeAt = Time.realtimeSinceStartupAsDouble;
        _isCounting = true;
    }

    /// <summary>
    /// �v���I�����č����� current �ɉ��Z
    /// </summary>
    private void StopSessionCounterAndAccumulate()
    {
        if (!_isCounting) return;

        double now = Time.realtimeSinceStartupAsDouble;
        double delta = Math.Max(0.0, now - _sessionResumeAt);

        _currentSessionTime += delta;

        _isCounting = false;
    }

    private void HandlePlayerSpawned(GameObject playerObj)
    {
        if (_current == null) CreateNewGameData();

        // HP ��z�z
        PlayerEvents.ApplyHP?.Invoke(
            Mathf.Clamp(_current.currentHp, 0, _current.maxHp),
            Mathf.Max(1, _current.maxHp)
        );

        // ����C���X�^���X���ۂ��Ɣz�z�iClone �ŕی�j
        var payload = new List<WeaponInstance>(_current.inventory.Count);
        for (int i = 0; i < _current.inventory.Count; ++i)
        {
            var src = _current.inventory[i];
            payload.Add(src != null ? src.Clone() : null);
        }
        PlayerEvents.ApplyLoadoutInstances?.Invoke(payload, _current.mainIndex);

        _currentSessionTime = 0.0;
        currentKillCount = 0;
        currentSkillUseCount = 0;
    }

    // === Title �ŏ����f�[�^���� ===
    private void CreateNewGameData()
    {
        _runStartTimeSec = Time.realtimeSinceStartupAsDouble;

        var data = new PlayerSaveData();
        data.dataVersion = 1;

        data.maxHp = Mathf.Max(1, defaultMaxHp);
        data.currentHp = Mathf.Clamp(defaultCurrentHp, 1, data.maxHp);

        data.inventory.Clear();
        int n = defaultWeaponItems.Count;
        for (int i = 0; i < n; ++i)
        {
            var item = defaultWeaponItems[i];
            if (item == null) continue;

            int dur = (i < defaultDurabilities.Count) ? defaultDurabilities[i] : 5;
            dur = Mathf.Clamp(dur, 0, item.maxDurability);

            data.inventory.Add(new WeaponInstance(item, dur));
        }
        data.mainIndex = Mathf.Clamp(defaultMainIndex, -1, Mathf.Max(-1, data.inventory.Count - 1));
        data.elapsedGameTimeSec = 0.0;

        _current = data;
    }

    // === �X�i�b�v�V���b�g�ۑ� ===
    private void SnapshotSaveFromCurrentPlayer()
    {
        var playerObj = PlayerEvents.GetPlayerObject?.Invoke();
        if (playerObj == null) { Debug.LogWarning("[PlayerPersistence] �v���C���[���擾�̂��ߕۑ��X�L�b�v"); return; }

        var pm = playerObj.GetComponent<PlayerMovement>();
        if (pm == null) { Debug.LogWarning("[PlayerPersistence] PlayerMovement �����o"); return; }

        _current ??= new PlayerSaveData();
        _current.dataVersion = 1;

        _current.maxHp = Mathf.RoundToInt(pm.maxHealth);
        _current.currentHp = _current.maxHp;// Mathf.Clamp(Mathf.RoundToInt(pm.CurrentHealth), 0, _current.maxHp);

        _current.inventory.Clear();
        var inv = pm.weaponInventory;
        for (int i = 0; i < inv.weapons.Count; ++i)
        {
            var inst = inv.weapons[i];
            if (inst == null || inst.template == null) continue;
            _current.inventory.Add(inst.Clone()); // ��Clone
        }

        _current.mainIndex = inv.mainIndex;

        StopSessionCounterAndAccumulate();
        _current.elapsedGameTimeSec += _currentSessionTime;
        _currentSessionTime = 0.0;
        _current.skillUseCount += currentSkillUseCount;
        _current.enemyDefeatCount += currentKillCount;
        currentSkillUseCount = 0;
        currentKillCount = 0;
    }

    public void ReapplyNow()
    {
        if (_current == null) CreateNewGameData();

        PlayerEvents.ApplyHP?.Invoke(
            Mathf.Clamp(_current.currentHp, 0, _current.maxHp),
            Mathf.Max(1, _current.maxHp)
        );

        var payload = new List<WeaponInstance>(_current.inventory.Count);
        for (int i = 0; i < _current.inventory.Count; ++i)
        {
            var src = _current.inventory[i];
            payload.Add(src != null ? src.Clone() : null);
        }
        PlayerEvents.ApplyLoadoutInstances?.Invoke(payload, _current.mainIndex);
    }

    private void HandleWeaponSkillUse(WeaponType type)
    {
        if (_current == null) return;
        currentSkillUseCount++;
    }
    private void HandleEnemyDefeated(GameObject enemy)
    {
        if (_current == null) return;
        currentKillCount++;
    }
    private void HandlePlayerDied()
    {
        StopSessionCounterAndAccumulate();

        _currentSessionTime = 0.0;
        currentKillCount = 0;
        currentSkillUseCount = 0;

        _isCounting = false;
        _sessionResumeAt = Time.realtimeSinceStartupAsDouble;
    }
}
