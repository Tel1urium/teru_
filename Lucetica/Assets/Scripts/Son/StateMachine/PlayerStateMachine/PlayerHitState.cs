using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

/// <summary>
/// ��e��ԁF�����ɍĐ����J�n���A�N���b�v�I�Ղ�HP�𔻒肵�� Dead/Idle �֑J�ڂ���B
/// �E�_�b�V�����G�Ȃǂ� TakeDamage ���Œe���݌v
/// �E�n��/�󒆂��킸����������i�����J�ڂ͈ێ����邪���S�͂����ł̂݊m��j
/// </summary>
public class PlayerHitState : IState
{
    private PlayerMovement _player;
    private float timer;
    private float length;

    // ���{��F�I�Ճ`�F�b�N�̗P�\�i�N���b�v�I���̂��̕b����O��HP����j
    private const float ExitCheckWindow = 0.08f;

    public PlayerHitState(PlayerMovement player) { _player = player; }

    public void OnEnter()
    {
        // ���{��F��e���C���[�փN���X�t�F�[�h
        _player.BlendToState(PlayerState.Hit);

        // ���{��F�K�� 0 �b����Đ��iClampForever �΍�j
        _player.ResetHitClipPlayable();

        // ���{��F�N���b�v���i���ݒ莞�̃t�F�C���Z�[�t�j
        length = (_player.hitClip != null) ? _player.hitClip.length : 0.2f;
        timer = 0f;
    }
    public void OnExit()
    {
        // ���ɂȂ�
    }

    public void OnUpdate(float deltaTime)
    {
        // ���{��F�󒆂ɏo���� Falling �ցi���S�m��͂��̃X�e�[�g���݂̂ōs���j
        if (!_player.IsGrounded)
        {
            _player.HandleFalling();
            return;
        }

        timer += deltaTime;

        // ���{��F�I�Ղ� HP ���� �� Dead / Idle or Move
        if (timer >= Mathf.Max(0.01f, length - 0.08f))
        {
            if (_player.CurrentHealth <= 0f)
            {
                // ���S�͔�e����̂�
                EventBus.PlayerEvents.OnPlayerDead?.Invoke();
                _player.ExecuteTriggerExternal(PlayerTrigger.Die);
                return;
            }

            // ���̗͂L���Ŗ߂�������
            if (_player.HasMoveInput())
                _player.ExecuteTriggerExternal(PlayerTrigger.MoveStart); // Move ��
            else
                _player.ExecuteTriggerExternal(PlayerTrigger.MoveStop);  // Idle ��
        }
    }
}