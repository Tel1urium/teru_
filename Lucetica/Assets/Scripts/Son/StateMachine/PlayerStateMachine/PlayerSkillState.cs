using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Collections.Generic;
using static EventBus;
using NUnit.Framework;

public class PlayerSkillState : IState
{
    private PlayerMovement _player;

    // �q�~�L�T�[�iPlayerMovement ���̏풓�C���X�^���X���u���̃X�e�[�g����L�v����j
    private AnimationMixerPlayable actionMixer;

    // Skill �͒P���̂��� 1 ������
    private AnimationClipPlayable skillPlayable;
    // ��������̃X���b�g�� 0 �d�݂̃_�~�[�Ő�ʁi���ڑ��ł��ǂ������ʂ��̂��ߕێ��j
    private AnimationClipPlayable dummyPlayable;

    private ComboAction skillAction;
    private double actionDuration;
    private double elapsedTime;

    private bool hasCheckedHit;
    private bool hasSpawnedAttackVFX;
    private bool hasSpawnedAttackPrefab;
    private bool weaponHiddenForThisAction;
    private bool lungeInvoked;

    // �����̐�s�t�F�[�h�J�n�ς݂��i�󓴉��p�FMain �w�̃A�E�g�����𑁂߂ɊJ�n�j
    private bool mainExitStarted;

    private WeaponInstance weapon;

    private const float HIT_VFX_TOWARD_PLAYER = 1f;
    private const float HIT_VFX_SEQUENCE_DELAY = 0.1f;
    private DamageData damageData = new DamageData(1);

    private static readonly Collider[] hitBuffer = new Collider[32];
    private static readonly System.Collections.Generic.HashSet<Enemy> uniqueEnemyHits
        = new System.Collections.Generic.HashSet<Enemy>();

    private readonly AnimationCurve defaultLungeCurve = null;
    private const float LUNGE_INPUT_MIN = 0.2f;

    private const float AUTO_LUNGE_FIND_RADIUS = 10f; // ���G���a�i�K�v�Ȃ� Inspector ���j
    private static readonly Collider[] sphereBuffer = new Collider[64];

    private List<bool> attackListBook = new List<bool>();

    public PlayerSkillState(PlayerMovement player) { _player = player; }

    public void OnEnter()
    {
        // === �@ �O��`�F�b�N�F�����ł܂����C���w�u�����h�͐G��Ȃ� ===
        weapon = _player.GetMainWeapon();
        if (weapon == null || weapon.template == null ||
            weapon.template.finisherAttack == null || weapon.template.finisherAttack.Count == 0 ||
            weapon.template.finisherAttack[0] == null || weapon.template.finisherAttack[0].animation == null)
        {
            // �t�B�j�b�V���[������ �� �󓴂���炸���ޔ�
            _player.ToIdle();
            return;
        }

        skillAction = weapon.template.finisherAttack[0];

        // �ϋv�`�F�b�N�i�����Ȃ����Ȃ��j
        if (weapon.currentDurability < skillAction.durabilityCost)
        {
            _player.ToIdle();
            return;
        }

        // === �A �q�~�L�T�[���u�P���X�L���p���C�A�E�g�v�ɏ��������Đ�L ===
        actionMixer = _player.GetActionSubMixer();
        TakeOverSubMixerForSingleShot(skillAction.animation, Mathf.Max(0.0001f, weapon.template.attackSpeed));

        if(skillAction.hitTimeList != null && skillAction.hitTimeList.Count > 0)
        {
            attackListBook.Clear();
            for (int i = 0; i < skillAction.hitTimeList.Count; i++)
            {
                attackListBook.Add(false);
            }
        }

        // �����p�����m��i0�d�݂ł̎p���W�����v�h�~�j
        skillPlayable.SetTime(0);
        _player.EvaluateGraphOnce();

        // �����őϋv������
        if (!_player.weaponInventory.ConsumeDurability(HandType.Main, skillAction.durabilityCost))
        {
            _player.ToIdle();
            return;
        }

        // === �B �����ŏ��߂ă��C���w�� Action �� ===
        float enterDur = _player.ResolveBlendDuration(_player.lastBlendState, PlayerState.Skill);
        _player.BlendToMainSlot(PlayerMovement.MainLayerSlot.Action, enterDur);

        // �t���O������
        actionDuration = skillAction.animation.length;
        elapsedTime = 0.0;
        hasCheckedHit = false;
        hasSpawnedAttackVFX = false;
        hasSpawnedAttackPrefab = false;
        weaponHiddenForThisAction = false;
        lungeInvoked = false;
        mainExitStarted = false;

        damageData = new DamageData(weapon.template.attackPower + skillAction.actPowerModifier);

        if (skillAction.swingSFXInfo.clip)
            PlayerEvents.PlayClipByPart(PlayerAudioPart.RHand, skillAction.swingSFXInfo.clip, skillAction.swingSFXInfo.volume, skillAction.swingSFXInfo.pitch, skillAction.swingSFXInfo.delay);
        if (skillAction.voiceSFXInfo.clip)
            PlayerEvents.PlayClipByPart(PlayerAudioPart.Mouth, skillAction.voiceSFXInfo.clip, skillAction.voiceSFXInfo.volume, skillAction.voiceSFXInfo.pitch, skillAction.voiceSFXInfo.delay);
    
        PlayerEvents.OnWeaponSkillUsed?.Invoke(weapon.template.weaponType);
    }

    public void OnExit()
    {
        // �� �q�~�L�T�[��K�� 0 �d�݉��i���X�e�[�g�ڍs�ł̋󓴖h�~�j
        if (actionMixer.IsValid())
        {
            actionMixer.SetInputWeight(0, 0f);
            actionMixer.SetInputWeight(1, 0f);
        }

        // �E�蕐��̕\����߂�
        if (weaponHiddenForThisAction)
        {
            _player.ShowMainHandModel();
            weaponHiddenForThisAction = false;
        }

        // Playable �̔j���i���X�e�[�g���č\������O��j
        if (skillPlayable.IsValid()) { actionMixer.DisconnectInput(0); skillPlayable.Destroy(); }
        if (dummyPlayable.IsValid()) { actionMixer.DisconnectInput(1); dummyPlayable.Destroy(); }
    }

    public void OnUpdate(float deltaTime)
    {
        if (skillAction == null) return;

        elapsedTime += deltaTime;

        // --- �������O�u�����h�i�󓴑΍�j�FclipEnd - blendToNext ���烁�C���w�𔲂��n�߂� ---
        double clipEndTime = actionDuration;
        float remToClipEnd = Mathf.Max(0f, (float)(clipEndTime - elapsedTime));
        float gate = Mathf.Max(0f, skillAction.blendToNext);
        if (!mainExitStarted && remToClipEnd <= gate + 1e-4f)
        {
            var nextSlot = _player.HasMoveInput() ? PlayerMovement.MainLayerSlot.Move
                                                  : PlayerMovement.MainLayerSlot.Idle;
            float desired = _player.ResolveBlendDuration(PlayerState.Skill,
                          _player.HasMoveInput() ? PlayerState.Move : PlayerState.Idle);
            float exitDur = Mathf.Min(desired, remToClipEnd);
            _player.BlendToMainSlot(nextSlot, exitDur);
            mainExitStarted = true;
        }

        // --- �ːi ---
        if (!lungeInvoked && elapsedTime >= skillAction.lungeTime)
        {
            DoLungeForAction();
            lungeInvoked = true;
        }

        // --- VFX ---
        if (!hasSpawnedAttackVFX && skillAction.attackVFXPrefab != null && elapsedTime >= skillAction.attackVFXTime)
        {
            SpawnAttackVFX();
            hasSpawnedAttackVFX = true;
        }

        // --- �q�b�g����/���� ---
        if (!hasCheckedHit && skillAction.hitCheckTime >= 0f && elapsedTime >= skillAction.hitCheckTime)
        {
            TrySpawnAttackPrefabNow();
            DoAttackHitCheck();
            hasCheckedHit = true;
        }

        // --- �q�b�g����/����(List) ---
        if (skillAction.hitTimeList != null && skillAction.hitTimeList.Count > 0)
        {
            for (int i = 0; i < skillAction.hitTimeList.Count; i++)
            {
                if (!attackListBook[i] && elapsedTime >= skillAction.hitTimeList[i])
                {
                    TrySpawnAttackPrefabNow();
                    DoAttackHitCheck();
                    attackListBook[i] = true;
                }
            }
        }

        // --- �I�� ---
        if (elapsedTime >= actionDuration)
        {
            _player.ToIdle();
        }
    }

    // ===== �q�~�L�T�[�ڑ��i�P���X�L���p�j =====
    /// <summary>
    /// �q�~�L�T�[���u�P���p�v�ɍ\��:
    /// ����0=�X�L��, ����1=�_�~�[(0�d��)�B�u�����h�͕s�v�A���0:1=1:0�B
    /// </summary>
    private void TakeOverSubMixerForSingleShot(AnimationClip clip, float speed)
    {
        // �������͂��N���[���A�b�v
        for (int i = 0; i < actionMixer.GetInputCount(); ++i)
        {
            var p = actionMixer.GetInput(i);
            if (p.IsValid()) { actionMixer.DisconnectInput(i); p.Destroy(); }
            actionMixer.SetInputWeight(i, 0f);
        }

        // �{��
        skillPlayable = AnimationClipPlayable.Create(_player.playableGraph, clip);
        skillPlayable.SetApplyFootIK(false);
        skillPlayable.SetApplyPlayableIK(false);
        skillPlayable.SetSpeed(speed);
        actionMixer.ConnectInput(0, skillPlayable, 0, 1f);

        // �_�~�[�i�ێ�ړI�B�����Ă��������A�Ӑ}�����m�ɂȂ�j
        dummyPlayable = AnimationClipPlayable.Create(_player.playableGraph, new AnimationClip());
        dummyPlayable.SetSpeed(0.0001f);
        actionMixer.ConnectInput(1, dummyPlayable, 0, 0f);
    }

    // ===== ����E���o =====
    private void DoAttackHitCheck()
    {
        Vector3 worldCenter = _player.transform.TransformPoint(skillAction.hitBoxCenter);
        Vector3 halfExtents = skillAction.hitBoxSize * 0.5f;
        Quaternion rot = _player.transform.rotation;

        int count = Physics.OverlapBoxNonAlloc(worldCenter, halfExtents, hitBuffer, rot, ~0, QueryTriggerInteraction.Ignore);

        if (_player.isHitboxVisible) ShowHitBox(worldCenter, skillAction.hitBoxSize, rot, 0.1f, Color.cyan);

        uniqueEnemyHits.Clear();
        var vfxPositions = new System.Collections.Generic.List<Vector3>(8);

        for (int i = 0; i < count; ++i)
        {
            var col = hitBuffer[i];
            if (col == null) continue;
            var enemy = col.GetComponentInParent<Enemy>();
            if (enemy == null) continue;
            if (!uniqueEnemyHits.Add(enemy)) continue;

            enemy.TakeDamage(damageData);

            Vector3 enemyCenter = col.bounds.center;
            Vector3 toPlayer = _player.transform.position - enemyCenter;
            Vector3 fxPos = (toPlayer.sqrMagnitude > 1e-6f)
                ? enemyCenter + toPlayer.normalized * HIT_VFX_TOWARD_PLAYER
                : enemyCenter + Vector3.up * 0.1f;
            vfxPositions.Add(fxPos);
        }

        if (vfxPositions.Count > 0)
            _player.StartCoroutine(SpawnHitVFXSequence(vfxPositions, HIT_VFX_SEQUENCE_DELAY));

        for (int i = 0; i < count; ++i) hitBuffer[i] = null;
    }

    // �U��Prefab�����i�q�b�g�{�b�N�X���S�j
    private void TrySpawnAttackPrefabNow()
    {
        if (hasSpawnedAttackPrefab) return;
        if (skillAction.attackPrefab == null) return;

        Vector3 spawnPos = _player.transform.TransformPoint(skillAction.hitBoxCenter);
        Quaternion spawnRot = _player.transform.rotation;

        Object.Instantiate(skillAction.attackPrefab, spawnPos, spawnRot);
        hasSpawnedAttackPrefab = true;

        if (!weaponHiddenForThisAction)
        {
            _player.HideMainHandModel();
            weaponHiddenForThisAction = true;
        }
    }

    private void SpawnAttackVFX()
    {
        if (skillAction.attackVFXPrefab == null) return;
        var go = Object.Instantiate(skillAction.attackVFXPrefab, _player.transform.position, _player.transform.rotation);
        Object.Destroy(go, 3f);
    }

    private System.Collections.IEnumerator SpawnHitVFXSequence(System.Collections.Generic.List<Vector3> positions, float interval)
    {
        for (int i = 0; i < positions.Count; ++i)
        {
            SpawnHitVFXAt(positions[i]);
            PlayerEvents.OnGamepadShakeCurve?.Invoke(0.6f, 1f, 0.1f);
            if (i < positions.Count - 1 && interval > 0f)
                yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnHitVFXAt(Vector3 pos)
    {
        var prefab = weapon?.template?.hitVFXPrefab;
        if (prefab == null) return;
        Object.Instantiate(prefab, pos, Quaternion.identity);
    }

    private void ShowHitBox(Vector3 center, Vector3 size, Quaternion rot, float time, Color color)
    {
        var obj = new GameObject("SkillHitBoxVisualizer");
        var vis = obj.AddComponent<AttackHitBoxVisualizer>();
        vis.Init(center, size, rot, time, color);
    }

    // ==== �ːi ====
    private void DoLungeForAction()
    {
        float distance = Mathf.Max(0f, skillAction.lungeDistance);
        float speed = Mathf.Max(0.01f, skillAction.lungeSpeed);

        Vector3 dir;
        bool hasDirection = false;

        // 1) �����ړ����́i�ŗD��j
        if (_player.TryGetMoveDirectionWorld(LUNGE_INPUT_MIN * LUNGE_INPUT_MIN, out dir))
        {
            hasDirection = true;
        }
        // 2) ���b�N�I���Ώہi���_�j
        else if (_player.TryGetLockOnHorizontalDirection(out dir))
        {
            hasDirection = true;
        }
        // 3) �������G�F���a���̍ŋߓG�i�Ō�̃t�H�[���o�b�N�j
        else if (TryFindNearestEnemyDirXZ(AUTO_LUNGE_FIND_RADIUS, out dir))
        {
            hasDirection = true;
        }

        // --- �񓪁i�L���ȕ���������ꂽ�ꍇ�̂݁j ---
        if (hasDirection)
        {
            // �����X�i�b�v�񓪁F�X�L�����o�J�n�t���[���Ō��������킹��
            _player.RotateYawOverTime(dir, 0f);
        }

        // --- �ːi�̎��s�i���� > 0 �̂Ƃ������j ---
        if (distance > 0f)
        {
            EventBus.PlayerEvents.LungeByDistance?.Invoke(
                hasDirection ? LungeManager.LungeAim.CustomDir : LungeManager.LungeAim.Forward,
                Vector3.zero,
                hasDirection ? dir : Vector3.zero,
                speed,
                distance,
                defaultLungeCurve
            );
        }
    }
    private bool TryFindNearestEnemyDirXZ(float radius, out Vector3 dirXZ)
    {
        dirXZ = Vector3.zero;

        // --- ���a���̌����擾 ---
        int count = Physics.OverlapSphereNonAlloc(
            _player.transform.position,
            radius,
            sphereBuffer,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        float bestSqr = float.PositiveInfinity;
        Enemy bestEnemy = null;

        for (int i = 0; i < count; ++i)
        {
            var col = sphereBuffer[i];
            if (!col) continue;

            // �e�K�w���܂߂� Enemy �������iCollider ���q�ɂ���\����z��j
            var enemy = col.GetComponentInParent<Enemy>();
            if (!enemy) continue;

            // ���������������iXZ�j�̓����̗p
            Vector3 v = enemy.transform.position - _player.transform.position;
            v.y = 0f;
            float d2 = v.sqrMagnitude;
            if (d2 < 1e-6f) continue;

            if (d2 < bestSqr)
            {
                bestSqr = d2;
                bestEnemy = enemy;
            }
        }

        // �o�b�t�@�̃N���[���A�b�v
        for (int i = 0; i < count; ++i) sphereBuffer[i] = null;

        // �Ŋ��̓G�����������琳�K������ XZ �x�N�g����Ԃ�
        if (bestEnemy != null)
        {
            dirXZ = bestEnemy.transform.position - _player.transform.position;
            dirXZ.y = 0f;
            if (dirXZ.sqrMagnitude < 1e-6f) return false;
            dirXZ.Normalize();
            return true;
        }
        return false;
    }
}
