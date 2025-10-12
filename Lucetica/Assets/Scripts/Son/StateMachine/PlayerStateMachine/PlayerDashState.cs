using UnityEngine;
using static EventBus;

/// <summary>
/// �_�b�V����ԁF
/// �E�A�j���� 0 �b����Đ����AdashFreezeAtSeconds ���B�ňꎞ��~�i�|�[�Y�ێ��j
/// �E�ړ��iLungeManager�j����������܂ŏ�Ԃ͈ێ�
/// �E�������ɃA�j���ĊJ���A����Ԃփu�����h���ė��E
/// </summary>
public class PlayerDashState : IState
{
    private PlayerMovement _player;
    private LungeManager _lm;

    private Vector3 _dashDir;
    private bool _poseFrozen;     // ���{��F�|�[�Y�Œ�ς݂�
    private bool _lungeIssued;    // ���{��F���E���W�J�n�ς݂�

    private double _dashLen;      // ���{��F�N���b�v��
    private double _freezeAt;     // ���{��F��~�������Ύ����i�b�j

    public PlayerDashState(PlayerMovement player) { _player = player; }

    public void OnEnter()
    {
        // ���{��F�N�[���^�C���������Ȃ瑦���A�i�ی��j
        if (!_player.IsDashCooldownReady())
        {
            ExitToLocomotion();
            return;
        }

        // ���{��F�A�j���w�փu�����h & 0�b����Đ��i�K���擪����j
        _player.BlendToState(PlayerState.Dash);
        _player.ResetDashClipPlayable();

        // ���{��F��~�����i�b�j���N���b�v���ɃN�����v
        _dashLen = _player.GetDashClipLength();
        _freezeAt = Mathf.Clamp(_player.dashFreezeAtSeconds, 0f, (float)_dashLen);
        _poseFrozen = false;

        // ���{��F��������i�J�������΁^�����͂͐��ʂ��g�p�j
        _dashDir = _player.ResolveDashDirectionWorld(0.01f);
        if (_dashDir.sqrMagnitude > 1e-6f)
            _player.transform.rotation = Quaternion.LookRotation(_dashDir, Vector3.up);

        // ���{��F���G�J�n�i�I�����Ɋm���ɃI�t�j
        _player.StartDashInvincibility();

        // ���{��FLungeManager �̏I���C�x���g���w��
        _lm = _player.GetComponent<LungeManager>();
        if (_lm != null)
        {
            _lm.OnLungeFinish += OnLungeEnd;
            _lm.OnLungeBlocked += OnLungeEnd;
            _lm.OnLungeTooSteep += OnLungeEnd;
        }

        // ���{��F�����x�[�X�̓ːi���C�x���g�ŋN���i�_�b�V���͈ړ������܂Ōp���j
        PlayerEvents.LungeByDistance?.Invoke(
            LungeManager.LungeAim.CustomDir,
            Vector3.zero,
            _dashDir,
            _player.dashSpeed,
            _player.dashDistance,
            null
        );
        _lungeIssued = true;
        UIEvents.OnDashUIChange?.Invoke(false);
        if(_player.dashSound != null)
        {
            PlayerEvents.PlayClipByPart?.Invoke(PlayerAudioPart.feet, _player.dashSound,1f,1f,0f);
        }
    }

    public void OnExit()
    {
        // ���{��F���G�I�t�i�ی��j
        _player.EndDashInvincibility();

        // ���{��F�w�ǉ����^�i�s���Ȃ璆�f
        if (_lm != null)
        {
            _lm.OnLungeFinish -= OnLungeEnd;
            _lm.OnLungeBlocked -= OnLungeEnd;
            _lm.OnLungeTooSteep -= OnLungeEnd;

            if (_lungeIssued) _lm.ForceCancel(false);
        }

        // ���{��F�_�b�V���A�j�����x�𕜋A�i�O���e�����c���Ȃ��j
        _player.SetDashPlayableSpeed(1.0);

        // ���{��F�N�[���^�C������
        _player.MarkDashCooldown();
    }

    public void OnUpdate(float deltaTime)
    {
        // ���{��F��~�����ɓ��B������A���̎����ōĐ����~�i�|�[�Y�ێ��j
        if (!_poseFrozen)
        {
            double t = _player.GetDashPlayableTime();
            if (t >= _freezeAt)
            {
                _player.SetDashPlayableTime(_freezeAt);
                _player.SetDashPlayableSpeed(0.0); // �� �Ȍ�͂��̃|�[�Y�ŐÎ~
                _poseFrozen = true;
            }
        }
        // ���{��F�ړ��� LungeManager ���S���B��Ԃ͈ړ������܂ňێ��B
    }

    // === LungeManager �����u�ړ�����/�j�~/�}�Ζʁv�����ꂩ�ŌĂ� ===
    private void OnLungeEnd()
    {
        // ���{��F�܂����G���I��
        _player.EndDashInvincibility();

        // ���{��F�A�j���ĊJ�i�Œ�|�[�Y����㔼���y���Đ����u�����h�J�n�j
        _player.SetDashPlayableSpeed(1.0);
        // �� �K�v�Ȃ�����I�t�Z�b�g�𑫂��ăT���v�����萫���グ�Ă��ǂ��F
        // _player.SetDashPlayableTime(Mathf.Min((float)(_freezeAt + 1e-3), (float)_dashLen - 1e-4));

        // ���{��F����Ԃւ̗��E�i���̗͂L���� Idle/Move �ցj
        ExitToLocomotion();
    }

    // ���{��F���̗͂L���� Idle/Move �ɕ��A
    private void ExitToLocomotion()
    {
        if (_player.HasMoveInput())
            _player.ExecuteTriggerExternal(PlayerTrigger.MoveStart);
        else
            _player.ExecuteTriggerExternal(PlayerTrigger.MoveStop);
    }
}
