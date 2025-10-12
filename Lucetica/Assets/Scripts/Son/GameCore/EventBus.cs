using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;
using static LungeManager;

public class EventBus
{
    public static class SystemEvents
    {
        public static Action<GameState> OnGameStateChange;
        public static Action OnGamePause;
        public static Action OnGameResume;
        public static Action OnGameExit;
        public static Action OnSceneLoadComplete;

        public static Action<float, float> OnChangeTimeScaleForSeconds;
    }
    public static class UIEvents
    {
        // ����: (�������탊�X�g, fromIndex, toIndex)
        public static Action<List<WeaponInstance>, int, int> OnRightWeaponSwitch;
        public static Action<List<WeaponInstance>, int, int> OnLeftWeaponSwitch;

        // ����j��i�C���x���g������폜���ꂽ���O�� index �� WeaponItem�j
        public static Action<int, WeaponItem> OnWeaponDestroyed;

        // �ϋv�x�ύX�i�� / ���� index / ���ݑϋv / �ő�ϋv�j
        public static Action<HandType, int, int, int> OnDurabilityChanged;
        public static Action OnWeaponUseFailed;

        // HP�ύX�i����HP / �ő�HP�j
        public static Action<int, int> OnPlayerHpChange;
        public static Action OnShowGameOverUI;
        public static Action OnShowStageClearUI;

        // �_�b�V��UI
        public static Action<bool> OnDashUIChange;

        // �G�C���|�C���g�ύX
        public static Action<Transform> OnAimPointChanged;

        // �U���{�^��������UI
        public static Action<bool> OnAttackHoldUI;         // true=�\�� / false=��\��
        public static Action<float> OnAttackHoldProgress;  // �i���\��
        public static Action OnAttackHoldCommitted;
        public static Action OnAttackHoldDenied;

        //�`���[�g���A��
        public static Action OnShowWeaponSkillTutorial;
        public static Action OnShowDashTutorial;
        public static Action OnShowSwitchWeaponTutorial;
    }
    public static class PlayerEvents
    {
        // �ۑ��f�[�^�K�p
        public static Action<int /*current*/, int /*max*/> ApplyHP;
        public static Action<List<WeaponInstance> /*instances*/, int /*mainIndex*/> ApplyLoadoutInstances;

        // �v���[���[�I�u�W�F�N�g
        public static Action<GameObject> OnPlayerSpawned;
        public static Action OnPlayerDead;
        public static Func<GameObject> GetPlayerObject;

        // ����
        public static System.Func<PlayerAudioPart,AudioClip,float,float,float,bool> PlayClipByPart;

        // �G�C���^�[�Q�b�g�ύX
        public static Action<GameObject> OnAimTargetChanged;
        public static Action<Transform, float, float> ChangeCameraTarget;

        // �ːi�A�_�b�V��
        public static System.Func<LungeAim, Vector3, Vector3, float, float, AnimationCurve,bool> LungeByDistance;
        public static System.Func<LungeAim, Vector3, Vector3, float, float, AnimationCurve,bool> LungeByTime;

        // �Q�[���p�b�h�U��
        public static Action<float, float> OnGamepadShake;
        public static Action<float,float,float> OnGamepadShakeCurve;

        // ����X�L���g�p
        public static Action<WeaponType> OnWeaponSkillUsed;
    }
    public static class EnemyEvents
    {
        public static Action<GameObject> OnEnemyDeath;
    }
}
