using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Ai;
using Ai.Method;
using Pathfinding;

public class AiBehaviourController : InputRecorder
{
    #region References
    [System.NonSerialized] public HealthController health;
    [System.NonSerialized] public CharacterStateController character;
    [System.NonSerialized] public AiPerceptionHolder memory;
    [System.NonSerialized] public Seeker seeker;
    [System.NonSerialized] public CharacterUiIndicator indicator;

    public AiBehaviourScript behaviourScript;
    #endregion References

    #region Behaviours
    public List<BehaviourHolder> behaviours = new List<BehaviourHolder>();
    BehaviourHolder current;
    Action onUpdate = () => { };


    public BehaviourHolder AddBehaviour()
    {
        var bhv = new BehaviourHolder();
        bhv.controller = this;

        return bhv;
    }
    public BehaviourHolder AddNewBehaviour()
    {
        var newBehaviour = new BehaviourHolder();
        newBehaviour.controller = this;
        
        behaviours.Add(newBehaviour);
        newBehaviour.Init();

        return newBehaviour;
    }
    public void SetCurrentBehaviour(BehaviourHolder behaviour)
    {
        if (current != null)
            current.End();
        current = behaviour;
        current.Begin();
    }
    public void SetCurrentBehaviour(int id)
    {
        if (current != null)
            current.End();
        current = behaviours[id];
        current.Begin();
    }
    public BehaviourHolder GetCurrentBehaviour()
    {
        return current;
    }

    #endregion Behaviours

    #region Events
    public void Update()
    {
        Debug.Assert(current != null);
        
        onUpdate();
        var next = current.Update();

        if (next != null)
        {
            SetCurrentBehaviour(next);
        }
    }

    new private void Start()
    {
        base.Start();
        health = GetComponentInParent<HealthController>();
        character = GetComponentInParent<CharacterStateController>();
        memory = GetComponent<AiPerceptionHolder>();
        seeker = GetComponent<Seeker>();
        indicator = transform.root.GetComponentInChildren<CharacterUiIndicator>();

        if (behaviourScript)
            behaviourScript.InitBehaviourController(this);
    }
    #endregion Events

    public T RengisterMethod<T>(T method, bool addToUpdate = true) where T : MethodBase
    {
        method.controller = this;
        if(addToUpdate)
            onUpdate += method.Update;
        return method;
    }

}

namespace Ai
{
    public class BehaviourHolder
    {
        #region References
        public AiBehaviourController controller;

        public InputHolder inputHolder { get { return controller.inputHolder; } }
        public AiPerceptionHolder memory { get { return controller.memory; } }
        public Transform transform { get { return controller.transform; } }
        public HealthController health { get { return controller.health; } }
        public Seeker seeker { get { return controller.seeker; } }
        public CharacterUiIndicator indicator { get { return controller.indicator; } }
        #endregion References

        #region Input
        public VectorMethod positionInput;
        public VectorMethod directionInput;
        public VectorMethod rotationInput;

        public BoolMethod[] keyMethods;


        public BoolMethod canEnterMethod;
        public BoolMethod returnMethod;

        public FloatMethod utilityMethod;

        public Action onBegin = () => { };
        public Action onEnd = () => { };


        public BehaviourHolder SetPositionInput(VectorMethod v)
        {
            positionInput = v;
            return this;
        }
        public BehaviourHolder SetPositionInput(VectorMethod v, float lerp)
        {
            positionInput = () => Vector2.Lerp(inputHolder.positionInput, v(), lerp);
            return this;
        }
        public BehaviourHolder SetDirectionInput(VectorMethod v)
        {
            directionInput = v;
            return this;
        }
        public BehaviourHolder SetDirectionInput(VectorMethod v, float lerp)
        {
            directionInput = () => Vector2.Lerp(inputHolder.directionInput, v(), lerp);
            return this;
        }
        public BehaviourHolder SetRotationInput(VectorMethod v)
        {
            rotationInput = v;
            return this;
        }
        public BehaviourHolder SetRotationInput(VectorMethod v, float lerp)
        {
            rotationInput = () => Vector2.Lerp(inputHolder.rotationInput, v(), lerp);
            return this;
        }

        public BehaviourHolder SetKeys(BoolMethod[] vs)
        {
            keyMethods = vs;
            return this;
        }
        public BehaviourHolder SetKey(int id, BoolMethod v)
        {
            keyMethods[id] = v;
            return this;
        }
        public BehaviourHolder SetReturnMethod(BoolMethod v)
        {
            returnMethod = v;
            return this;
        }
        public BehaviourHolder SetCanEnter(BoolMethod v)
        {
            canEnterMethod = v;
            return this;
        }
        public BehaviourHolder SetUtilityMethod(FloatMethod v)
        {
            utilityMethod = v;
            return this;
        }

        public BehaviourHolder AddBegin(Action bhv)
        {
            onBegin += bhv;
            return this;
        }
        public BehaviourHolder AddBegin(MethodBase method)
        {
            method.controller = controller;
            onBegin += method.Begin;   
            return this;
        }
        public BehaviourHolder AddEnd(Action bhv)
        {
            onEnd += bhv;
            return this;
        }
        #endregion Input

        #region Events
        public void Init()
        {
            if (positionInput == null)
                positionInput = () => Vector2.zero;

            if (directionInput == null)
                directionInput = () => Vector2.zero;

            if (rotationInput == null)
                rotationInput = () => Vector2.zero;

            if (keyMethods == null)
            {
                keyMethods = new BoolMethod[inputHolder.keys.Length];
                for (int i = 0; i < keyMethods.Length; ++i)
                    keyMethods[i] = () => false;
            }

            if (canEnterMethod == null)
                canEnterMethod = () => true;

            if (returnMethod == null)
                returnMethod = () => true;

            if (utilityMethod == null)
                utilityMethod = () => 1.0f;
        }
        public void Begin()
        {
            onBegin();
        }
        public void End()
        {
            onEnd();
        }

        public BehaviourHolder Update()
        {
            bool r = returnMethod();
            if (r)
                return GetNextBehaviour();

            inputHolder.positionInput = positionInput();
            inputHolder.directionInput = directionInput();
            inputHolder.rotationInput = rotationInput();

            for (int i = 0; i < keyMethods.Length; ++i)
            {
                inputHolder.keys[i] = keyMethods[i]();
            }

            return null;
        }

        public bool CanEnter()
        {
            return canEnterMethod();
        }
        // utility method must be persistent, several calculations will be required
        public float GetUtility()
        {
            return utilityMethod();
        }

        #endregion Events

        #region Transition
        public WorldState behaviourType;
        public BehaviourHolder SetBehaviourType(UInt64 turnOn = (UInt64)EBehaviourType.ENoFlag, UInt64 turnOff = (UInt64)EBehaviourType.ENoFlag)
        {
            behaviourType = new WorldState(turnOn, turnOff);
            return this;
        }

        public List<BehaviourHolder> transitions = new List<BehaviourHolder>();
        public List<float> transitionChances = new List<float>();
        public BehaviourHolder AddTransition(BehaviourHolder b, float chance = 1f)
        {
            transitions.Add(b);
            transitionChances.Add(chance);

            return this;
        }

        protected BehaviourHolder GetNextBehaviour()
        {
            float sum = 0;
            /*for (int i = 0; i < transitionChances.Count; ++i)
                if (transitions[i].CanEnter())
                    sum += transitionChances[i]* transitions[i].GetUtility();*/
            var behaviours = controller.behaviours;
            for (int i = 0; i < behaviours.Count; ++i)
                if (behaviours[i].CanEnter())
                    sum += Mathf.Clamp(behaviours[i].GetUtility(),0, float.MaxValue);

            if (sum == 0)
                return this;

            float randed = UnityEngine.Random.Range(0, sum);

            float lastSum = 0;
            for (int i = 0; i < behaviours.Count; ++i)
            {
                float utility = Mathf.Clamp(behaviours[i].GetUtility(), 0, float.MaxValue);
                if (randed >= lastSum && randed <= lastSum + utility)
                {
                    if (behaviours[i].CanEnter())
                        return behaviours[i];
                }
                else
                {
                    if (behaviours[i].CanEnter())
                        lastSum += utility;
                }
            }
            /*for (int i = 0; i < transitionChances.Count; ++i)
            {
                float utility = transitionChances[i] * transitions[i].GetUtility();
                if (randed >= lastSum && randed <= lastSum + utility)
                {
                    if (transitions[i].CanEnter())
                        return transitions[i];
                }
                else
                {
                    if (transitions[i].CanEnter())
                        lastSum += utility;
                }
            }*/

            return this;
        }
        #endregion Transition
    }
}
