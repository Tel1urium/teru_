using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// �ėp�X�e�[�g�}�V���i��ԃN���X�ƈϏ����Ή��j
/// �R���[�`���ɂ��Enter/Exit���o���T�|�[�g
/// </summary>
/// 

public interface IState
{
    void OnEnter();
    void OnExit();
    void OnUpdate(float deltaTime);
}
public class Transition<TState, TTrigger>
{
    public TState To { get; set; }
    public TTrigger Trigger { get; set; }
}


/// <summary>
/// �e�X�e�[�g�ɕR�Â��U�镑���i�N���X/�Ϗ�/�R���[�`���j��ێ�����
/// </summary>
public class StateMapping
{
    public IState stateClass;

    public Action onEnter;
    public Func<IEnumerator> enterRoutine;

    public Action onExit;
    public Func<IEnumerator> exitRoutine;

    public Action<float> onUpdate;

    /// <summary>
    /// �X�e�[�g�J�n�����i�R���[�`�� or �ʏ�֐��j
    /// </summary>
    public void Enter(MonoBehaviour runner)
    {
        if (stateClass != null) stateClass.OnEnter();
        else if (enterRoutine != null) runner.StartCoroutine(enterRoutine());
        else onEnter?.Invoke();
    }

    /// <summary>
    /// �X�e�[�g�I�������i�R���[�`�� or �ʏ�֐��j
    /// </summary>
    public void Exit(MonoBehaviour runner)
    {
        if (stateClass != null) stateClass.OnExit();
        else if (exitRoutine != null) runner.StartCoroutine(exitRoutine());
        else onExit?.Invoke();
    }

    /// <summary>
    /// ���t���[������
    /// </summary>
    public void Update(float deltaTime)
    {
        if (stateClass != null) stateClass.OnUpdate(deltaTime);
        else onUpdate?.Invoke(deltaTime);
    }
}

public class StateMachine<TState, TTrigger>
    where TState : struct, IConvertible, IComparable
    where TTrigger : struct, IConvertible, IComparable
{
    private TState _current;
    private StateMapping _currentMapping;
    private readonly MonoBehaviour _mono;

    private Dictionary<object, StateMapping> _mappings = new();
    private Dictionary<TState, List<Transition<TState, TTrigger>>> _transitions = new();

    /// <summary>
    /// �������iMonoBehaviour�����ŃR���[�`���g�p�j
    /// </summary>
    public StateMachine(MonoBehaviour mono, TState initialState)
    {
        _mono = mono;

        foreach (TState state in Enum.GetValues(typeof(TState)))
        {
            _mappings[state] = new StateMapping();
        }

        ChangeState(initialState);
    }

    /// <summary>
    /// ��ԃN���X��o�^����
    /// </summary>
    public void RegisterState(TState state, IState stateImpl)
    {
        _mappings[state].stateClass = stateImpl;
    }

    /// <summary>
    /// �Ϗ��ŏ�ԏ�����o�^����i�R���[�`���Ή��j
    /// </summary>
    public void SetupState(
        TState state,
        Action onEnter = null,
        Action onExit = null,
        Action<float> onUpdate = null,
        Func<IEnumerator> enterRoutine = null,
        Func<IEnumerator> exitRoutine = null)
    {
        var mapping = _mappings[state];
        mapping.onEnter = onEnter;
        mapping.onExit = onExit;
        mapping.onUpdate = onUpdate;
        mapping.enterRoutine = enterRoutine;
        mapping.exitRoutine = exitRoutine;
    }

    /// <summary>
    /// �g���K�[�����s���ď�ԑJ�ڂ���
    /// </summary>
    public void ExecuteTrigger(TTrigger trigger)
    {
        //Debug.Log($"[FSM] ExecuteTrigger: {trigger}, current state = {_current}");
        if (_transitions.TryGetValue(_current, out var list))
        {
            foreach (var transition in list)
            {
                if (transition.Trigger.Equals(trigger))
                {
                    ChangeState(transition.To);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// ��ԑJ�ڃ��[����o�^����
    /// </summary>
    public void AddTransition(TState from, TState to, TTrigger trigger)
    {
        if (!_transitions.ContainsKey(from))
        {
            _transitions[from] = new List<Transition<TState, TTrigger>>();
        }
        _transitions[from].Add(new Transition<TState, TTrigger> { To = to, Trigger = trigger });
    }

    /// <summary>
    /// ���݂̏�Ԃ��X�V����i���t���[���j
    /// </summary>
    public void Update(float deltaTime)
    {
        _currentMapping?.Update(deltaTime);
    }

    /// <summary>
    /// ��Ԃ𒼐ڐ؂�ւ���iExit �� Enter ���j
    /// </summary>
    private void ChangeState(TState to)
    {
        _currentMapping?.Exit(_mono);
        _current = to;
        _currentMapping = _mappings[to];
        _currentMapping.Enter(_mono);
    }

    /// <summary>
    /// ���݂̏�Ԃ��擾����
    /// </summary>
    public TState CurrentState => _current;
}




/*using System.Collections.Generic;
using System.Linq;
using System;

// �eState����delagate��o�^���Ă����N���X
public interface IState
{
    void OnEnter();
    void OnExit();
    void OnUpdate(float deltaTime);
}

public class StateMapping
{
    public IState stateClass;

    public Action onEnter;
    public Action onExit;
    public Action<float> onUpdate;

    public void Enter()
    {
        if (stateClass != null) stateClass.OnEnter();
        else onEnter?.Invoke();
    }

    public void Exit()
    {
        if (stateClass != null) stateClass.OnExit();
        else onExit?.Invoke();
    }

    public void Update(float deltaTime)
    {
        if (stateClass != null) stateClass.OnUpdate(deltaTime);
        else onUpdate?.Invoke(deltaTime);
    }
}

public class Transition<TState, TTrigger>
{
    public TState To { get; set; }
    public TTrigger Trigger { get; set; }
}

public class StateMachine<TState, TTrigger>
    where TState : struct, IConvertible, IComparable
    where TTrigger : struct, IConvertible, IComparable
{
    private TState _current;
    private StateMapping _currentMapping;

    private Dictionary<object, StateMapping> _mappings = new();
    private Dictionary<TState, List<Transition<TState, TTrigger>>> _transitions = new();

    public StateMachine(TState initialState)
    {
        // State����StateMapping���쐬
        foreach (TState state in Enum.GetValues(typeof(TState)))
        {
            _mappings[state] = new StateMapping();
        }
        ChangeState(initialState);
    }
    public void RegisterState(TState state, IState stateImpl)
    {
        _mappings[state].stateClass = stateImpl;
        
    }


    /// <summary>
    /// State������������
    /// </summary>
    public void SetupState(TState state, Action onEnter, Action onExit, Action<float> onUpdate)
    {
        var mapping = _mappings[state];
        mapping.onEnter = onEnter;
        mapping.onExit = onExit;
        mapping.onUpdate = onUpdate;
    }

    /// <summary>
    /// �g���K�[�����s����
    /// </summary>
    public void ExecuteTrigger(TTrigger trigger)
    {
        if (_transitions.TryGetValue(_current, out var list))
        {
            foreach (var transition in list)
            {
                if (transition.Trigger.Equals(trigger))
                {
                    ChangeState(transition.To);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// �J�ڏ���o�^����
    /// </summary>
    public void AddTransition(TState from, TState to, TTrigger trigger)
    {
        if (!_transitions.ContainsKey(from))
        {
            _transitions[from] = new List<Transition<TState, TTrigger>>();
        }
        _transitions[from].Add(new Transition<TState, TTrigger> { To = to, Trigger = trigger });
    }




    /// <summary>
    /// �X�V����
    /// </summary>
    public void Update(float deltaTime)
    {
        _currentMapping?.Update(deltaTime);
    }


    /// <summary>
    /// State�𒼐ڕύX����
    /// </summary>
    private void ChangeState(TState to)
    {
        _currentMapping?.Exit();
        _current = to;
        _currentMapping = _mappings[to];
        _currentMapping.Enter();
    }
}*/