using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;

public class CharacterStateController : MonoBehaviour
{
    #region References
    [System.NonSerialized] new public Rigidbody2D rigidbody;
    [System.NonSerialized] public RigidbodyMovement movement;
    [System.NonSerialized] public HealthController health;
    [System.NonSerialized] public InputHolder inputHolder;
    [System.NonSerialized] public Animator animator;

    public CharacterWeaponBase weapon;
    #endregion References

    #region States
    List<CharacterState> states = new List<CharacterState>();
    public CharacterState AddNewState(string animationName)
    {
        CharacterState state = new CharacterState();
        states.Add(state);
        state.stateController = this;
        state.animationName = animationName;
        return state;
    }
    public void ClearStates()
    {
        SetCurrentState(null);
        states.Clear();
    }
    public CharacterState currentState { get; internal set; }

    public void SetCurrentState(CharacterState state, AnimationTransitionData transition)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = state;
        currentState.bufferedInput = false;

        if (currentState != null)
            currentState.Enter(transition);

    }
    public void SetCurrentState(CharacterState state)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = state;
        currentState.bufferedInput = false;

        if (currentState != null)
            currentState.Enter();
    }

    public void ResetInputBuffer()
    {
        foreach (var it in states)
            it.bufferedInput = false;
    }

    public void AddTransitionToAllStates(CharacterState target, bool bufferInput = true )
    {
        foreach (var it in states)
            if (it != target)
                it.AddTransition(target, bufferInput);
    }
    public void AddTransitionToAllStates(CharacterState target, RangedFloat transitionRange, bool bufferInput = true)
    {
        foreach (var it in states)
            if (it != target)
                it.AddTransition(target, transitionRange, bufferInput);
    }
    public void AddTransitionToAllStates(CharacterState target, AnimationTransitionData transitionData, bool bufferInput = true)
    {
        foreach (var it in states)
            if (it != target)
                it.AddTransition(target, transitionData, bufferInput);
    }
    public void AddTransitionToAllStates(CharacterState target, RangedFloat transitionRange, AnimationTransitionData transitionData, bool bufferInput = true)
    {
        foreach (var it in states)
            if (it != target)
                it.AddTransition(target, transitionRange, transitionData, bufferInput);
    }

    public delegate bool ExceptionCondition(CharacterState state);
    public void AddTransitionToAllStates(CharacterState target, ExceptionCondition exceptionCondition, bool bufferInput = true)
    {
        foreach (var it in states)
            if (it != target && !exceptionCondition(it))
                it.AddTransition(target, bufferInput);
    }
    public void AddTransitionToAllStates(CharacterState target, ExceptionCondition exceptionCondition, RangedFloat transitionRange, bool bufferInput = true)
    {
        foreach (var it in states)
            if (it != target && !exceptionCondition(it))
                it.AddTransition(target, transitionRange, bufferInput);
    }
    public void AddTransitionToAllStates(CharacterState target, ExceptionCondition exceptionCondition, AnimationTransitionData transitionData, bool bufferInput = true)
    {
        foreach (var it in states)
            if (it != target && !exceptionCondition(it))
                it.AddTransition(target, transitionData, bufferInput);
    }
    public void AddTransitionToAllStates(CharacterState target, ExceptionCondition exceptionCondition, RangedFloat transitionRange, AnimationTransitionData transitionData, bool bufferInput = true)
    {
        foreach (var it in states)
            if (it != target && !exceptionCondition(it))
                it.AddTransition(target, transitionRange, transitionData, bufferInput);
    }

    public bool CanEnterTo(int i)
    {
        return currentState.CanEnterTo(states[i]);
    }
    public bool CanEnterTo(CharacterState state)
    {
        return currentState.CanEnterTo(state);
    }
    #endregion States

    #region Cd
    List<Timer> skillCds = new List<Timer>();
    public int CreateCd(float cd, bool restart = true)
    {
        var timer = new Timer(cd);
        if (restart)
            timer.Restart();
        skillCds.Add(timer);
        return skillCds.Count - 1;
    }

    public bool IsCdReady(int id) { return skillCds[id].IsReady(); }
    public void RestartCd(int id, float state = 0f) { skillCds[id].Restart(state); }
    #endregion Cd

    private void Start()
    {
        rigidbody   = GetComponent<Rigidbody2D>();
        movement    = GetComponent<RigidbodyMovement>();
        inputHolder = GetComponent<InputHolder>();
        animator    = GetComponent<Animator>();
        health    = GetComponent<HealthController>();

        if (weapon)
            weapon.InitCharacterStateController(this);
    }
    void Update()
    {
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float time = stateInfo.normalizedTime;
        currentState.Update(time);
    }

    private void FixedUpdate()
    {
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float time = stateInfo.normalizedTime;
        currentState.FixedUpdate(time);
    }
}


public struct StateTransition
{
    public CharacterState target;
    /// transition can only occur in specified part of animation
    public RangedFloat transitionRange;
    public bool bufferInput;

    public delegate bool CanEnter(CharacterState target, CharacterStateController controller);
    public CanEnter canEnter;

    public AnimationTransitionData animationTransitionData;
}
public struct AnimationTransitionData
{
    public AnimationTransitionData(float transitionDuration, float normalizedOffset = 0f)
    {
        this.transitionDuration = transitionDuration;
        this.normalizedOffset = normalizedOffset;
    }
    public float transitionDuration;
    public float normalizedOffset;
}

public class CharacterState
{
    public override string ToString()
    {
        return "" + animationHash;
    }

    #region Animation
    public int animationHash;
    public string animationName { set { animationHash = Animator.StringToHash(value); } }
    #endregion Animation

    #region References
    public CharacterStateController stateController;
    public Rigidbody2D       rigidbody   { get { return stateController.rigidbody; } }
    public RigidbodyMovement movement    { get { return stateController.movement; } }
    public InputHolder       inputHolder { get { return stateController.inputHolder; } }
    public Animator          animator    { get { return stateController.animator; } }
    public HealthController  health     { get { return stateController.health; } }
    #endregion References

    #region Transitions
    /// for more intuitive usage transitions will not require holding still action key
    /// if requirements are met for transition but it's too early for animation to transition
    ///     will for transition will be saved
    public bool bufferedInput;

    List<StateTransition> transitions = new List<StateTransition>();
    public CharacterState AddTransition(CharacterState target, bool bufferInput = true, StateTransition.CanEnter canEnter = null)
    {
        StateTransition transition = new StateTransition();
        transition.target = target;
        transition.transitionRange = new RangedFloat();
        transition.bufferInput = bufferInput;
        transition.canEnter = canEnter;

        transitions.Add(transition);
        return this;
    }
    public CharacterState AddTransition(CharacterState target, RangedFloat transitionRange, bool bufferInput = true, StateTransition.CanEnter canEnter = null)
    {
        StateTransition transition = new StateTransition();
        transition.target = target;
        transition.transitionRange = transitionRange;
        transition.bufferInput = bufferInput;
        transition.canEnter = canEnter;

        transitions.Add(transition);
        return this;
    }
    public CharacterState AddTransition(CharacterState target, AnimationTransitionData transitionData, bool bufferInput = true, StateTransition.CanEnter canEnter = null)
    {
        StateTransition transition = new StateTransition();
        transition.target = target;
        transition.transitionRange = new RangedFloat();
        transition.animationTransitionData = transitionData;
        transition.bufferInput = bufferInput;
        transition.canEnter = canEnter;

        transitions.Add(transition);
        return this;
    }
    public CharacterState AddTransition(CharacterState target, RangedFloat transitionRange, AnimationTransitionData transitionData, bool bufferInput = true, StateTransition.CanEnter canEnter = null)
    {
        StateTransition transition = new StateTransition();
        transition.target = target;
        transition.transitionRange = transitionRange;
        transition.animationTransitionData = transitionData;
        transition.bufferInput = bufferInput;
        transition.canEnter = canEnter;

        transitions.Add(transition);
        return this;
    }

    public bool CanEnterTo(CharacterState state)
    {
        foreach (var it in transitions)
            if (it.target == state)
                return state.CanEnter();
        return false;
    }
    #endregion Transitions

    #region Components
    List<StateComponent> components = new List<StateComponent>();
    public CharacterState AddComponent(StateComponent newComponent)
    {
        newComponent.parent = this;
        components.Add(newComponent);
        newComponent.Init();

        return this;
    }
    #endregion Components

    #region Events
    public void Enter(AnimationTransitionData transition)
    {
        animator.CrossFade(animationHash, transition.transitionDuration, 0, transition.normalizedOffset);

        foreach (var it in components)
            it.Enter();
    }
    public void Enter()
    {
        animator.Play(animationHash);

        foreach (var it in components)
            it.Enter();
    }
    public void Update(float animationTime)
    {
        foreach (var it in components)
            it.Update(animationTime);

        if (!animator.IsInTransition(0))
        {
            foreach (var it in transitions)
                if (it.canEnter == null || it.canEnter(it.target, stateController))
                {
                    var target = it.target;
                    if (target.CanEnter() && (target.bufferedInput || target.IsPressed()))
                    {
                        var p = it.transitionRange;
                        if (p.InRange(animationTime))
                        {
                            stateController.SetCurrentState(target, it.animationTransitionData);
                            break;
                        }
                        else
                        {
                            it.target.bufferedInput = it.bufferInput;
                            break;
                        }
                    }
                }
        }
    }
    public void FixedUpdate(float animationTime)
    {
        foreach (var it in components)
            it.FixedUpdate(animationTime);
    }
    public void Exit()
    {
        foreach (var it in components)
            it.Exit();
    }

    public bool IsPressed()
    {
        foreach (var it in components)
            if (!it.IsPressed())
                return false;
        return true;
    }
    public bool CanEnter()
    {
        foreach (var it in components)
            if (!it.CanEnter())
                return false;
        return true;
    }
    #endregion Events
}
