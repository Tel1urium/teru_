using UnityEngine;

/// <summary>
/// ������ԁF���n���ɑJ�ځB���͖͂����B���n��Idle/Move�ցB
/// </summary>
public class PlayerFallingState : IState
{
    private PlayerMovement _player;

    public PlayerFallingState(PlayerMovement player) { _player = player; }

    public void OnEnter()
    {
        // �������C���[�փN���X�t�F�[�h�i�J�ڎ��Ԃ͐ݒ�ŉ����j
        _player.BlendToState(PlayerState.Falling);
    }

    public void OnExit()
    {
        // ���ɂȂ�
    }

    public void OnUpdate(float deltaTime)
    {
        _player.HandleMovement(deltaTime);
        if (_player.isHitboxVisible) { /* �f�o�b�O������ */ }
    }
}