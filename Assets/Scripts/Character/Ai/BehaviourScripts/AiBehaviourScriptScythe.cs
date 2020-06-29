using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;
using Ai;

[CreateAssetMenu(fileName = "ScytheBehaviour", menuName = "Character/Ai/Scythe")]
public class AiBehaviourScriptScythe : AiBehaviourScript
{
    public override void InitBehaviourController(AiBehaviourController stateController)
    {

        var instance = CreateInstance<AiBehaviourScriptScythe>();
        instance.InitBase(stateController);

        instance.InitIdle(stateController);
        instance.InitLookAround(stateController);
        instance.InitRandomMovement(stateController);

        instance.InitLookAtTarget(stateController);
        instance.InitLookAtPain(stateController);
        instance.InitStayBehindTarget(stateController);
        instance.InitStayInRangeClose(stateController);
        instance.InitStayInRangeFar(stateController);

        instance.InitSlashForward(stateController);
        instance.InitSlashBackward(stateController);
        instance.InitSlashSide(stateController);

        instance.InitPullForward(stateController);
        instance.InitPullBackward(stateController);
        instance.InitPullSide(stateController);
        
        instance.InitPushForward(stateController);
        instance.InitPushBackward(stateController);

        //instance.InitBlockForward(stateController);
        //instance.InitBlockBackward(stateController);
        //instance.InitBlockSide(stateController);

        stateController.SetCurrentBehaviour(instance.idle);


        //Combat(stateController);
    }


    // components
    Timer executionTimer;
    Ai.Method.LookAround lookAroundMethod;
    Ai.Method.Search searchMethod;
    Ai.Method.CharacterStateChangeDetector characterStateChange;

    AiFocusBase focusPain;
    AiFocusBase focusEnemy;

    void InitBase(AiBehaviourController stateController)
    {
        focusPain = stateController.RengisterMethod(new AiFocusClosest(EMemoryEvent.EPain));
        focusEnemy = stateController.RengisterMethod(new AiFocusClosest(EMemoryEvent.EEnemy));

        executionTimer = new Timer();
        lookAroundMethod = stateController.RengisterMethod(new Ai.Method.LookAround());
        searchMethod = stateController.RengisterMethod(new Ai.Method.Search(focusEnemy, 50));
        characterStateChange = stateController.RengisterMethod(new Ai.Method.CharacterStateChangeDetector());

    }

    BehaviourHolder idle;
    BehaviourHolder lookAround;
    BehaviourHolder randomMovement;

    BehaviourHolder lookAtTarget;
    BehaviourHolder lookAtPain;
    BehaviourHolder standBehindTarget;
    BehaviourHolder stayInRangeClose;
    BehaviourHolder stayInRangeFar;

    BehaviourHolder hitSlashForward;
    BehaviourHolder hitSlashBackward;
    BehaviourHolder hitSlashSide;

    BehaviourHolder hitPullForward;
    BehaviourHolder hitPullBackward;
    BehaviourHolder hitPullSide;

    BehaviourHolder hitPushForward;
    BehaviourHolder hitPushBackward;


    BehaviourHolder blockForward;
    BehaviourHolder blockBackward;
    BehaviourHolder blockSide;

    void InitIdle(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        idle = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.15f, 0.3f))

            .SetCanEnter(() => !focusEnemy.HasTarget())

            .SetReturnMethod(() => focusEnemy.HasTarget() || executionTimer.IsReady())
        ;
    }
    void InitLookAround(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;


        lookAround = stateController.AddNewBehaviour()
            .AddBegin(lookAroundMethod)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.25f, 0.4f))

            .SetRotationInput(() => lookAroundMethod.GetRotationInput())
            .SetReturnMethod(() => focusEnemy.HasTarget() || executionTimer.IsReady())
            .SetCanEnter(() => !focusEnemy.HasTarget())
            .SetUtilityMethod(() => 6)
        ;
    }
    void InitRandomMovement(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        randomMovement = stateController.AddNewBehaviour()
            .AddBegin(searchMethod)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.75f, 1.5f))

            .SetPositionInput(() => searchMethod.GetDirectionLength())
            .SetReturnMethod(() => focusPain.HasTarget() || executionTimer.IsReady())
            .SetUtilityMethod(() => focusEnemy.HasTarget() ? 10 : 1f)
            .SetCanEnter(() => true)
            ;
    }

    void InitLookAtTarget(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        lookAtTarget = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.25f, 0.35f))

            .SetRotationInput(() => focusEnemy.ToTarget())
            .SetReturnMethod(() => !focusEnemy.HasTarget() || executionTimer.IsReady())
            .SetCanEnter(() => focusEnemy.HasTarget())
            .SetUtilityMethod(() => focusEnemy.IsInRange(11, 200) ? 30 : 5f)
            ;
    }
    void InitLookAtPain(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        lookAtPain = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.15f, 0.25f))

            .SetRotationInput(() => focusPain.ToTarget())
            .SetReturnMethod(() => !focusPain.HasTarget() || executionTimer.IsReady())
            .SetCanEnter(() => !focusEnemy.HasTarget() && focusPain.HasTarget())
            .SetUtilityMethod(() => 100.0f)
            ;
    }
    void InitStayBehindTarget(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        standBehindTarget = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.75f, 1.0f))

            .SetPositionInput(() => {
                return focusEnemy.StayBehindTargetAdaptive(2.0f, 300.0f);
            })
            .SetReturnMethod(() => !focusEnemy.HasTarget() || executionTimer.IsReady())
            .SetCanEnter(() => focusEnemy.HasTarget())
            .SetUtilityMethod(() => focusEnemy.IsInRange(9, 15) ? 100 : 40)
        ;
    }
    void InitStayInRangeClose(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        stayInRangeClose = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.5f, 0.75f))

            .SetPositionInput(() => focusEnemy.StayInRange(5.0f, 8.0f))
            .SetReturnMethod(() => !focusEnemy.HasTarget() || executionTimer.IsReady())
            .SetCanEnter(() => focusEnemy.HasTarget())
            .SetUtilityMethod(() => !focusEnemy.IsInRange(4.5f, 8.5f) ? 40 : 10 )
        ;
    }
    void InitStayInRangeFar(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        stayInRangeFar = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.65f, 0.9f))

            .SetPositionInput(() => focusEnemy.StayInRange(12.0f, 15.0f))
            .SetReturnMethod(() => !focusEnemy.HasTarget() || executionTimer.IsReady())
            .SetCanEnter(() => focusEnemy.HasTarget())
            .SetUtilityMethod(() => !focusEnemy.IsInRange(7.5f, 20.0f) ? 50 : 20)
        ;
    }

    void InitSlashForward(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        hitSlashForward = stateController.AddNewBehaviour()
            .SetBehaviourType((ulong)EBehaviourType.EAtack | (ulong)EBehaviourType.EDash | (ulong)EBehaviourType.EForward)
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => focusEnemy.ToTarget())
            .SetDirectionInput(() => focusEnemy.ToTarget())
            .SetKey(0, () => true)
            .SetCanEnter(() => focusEnemy.HasTarget() && character.CanEnterTo(1) && focusEnemy.IsInRange(6.5f, 10.5f))
            .SetReturnMethod(() => !focusEnemy.HasTarget() || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(2))
            .SetUtilityMethod(() => focusEnemy.IsInRange(8.5f, 10.0f) ? 150 : 30)
        ;
    }
    void InitSlashBackward(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        hitSlashBackward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => -focusEnemy.ToTarget())
            .SetDirectionInput(() => focusEnemy.ToTarget())
            .SetKey(0, () => true)
            .SetCanEnter(() => focusEnemy.HasTarget() && character.CanEnterTo(1) && focusEnemy.IsInRange(1.0f, 7.5f))
            .SetReturnMethod(() => !focusEnemy.HasTarget() || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(2))
            .SetUtilityMethod(() => focusEnemy.IsInRange(4f, 6.25f) ? 150 : 20)
        ;
    }
    void InitSlashSide(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        hitSlashSide = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => focusEnemy.ToTargetSide())
            .SetDirectionInput(() => focusEnemy.ToTarget())
            .SetKey(0, () => true)
            .SetCanEnter(() => focusEnemy.HasTarget() && character.CanEnterTo(1) && focusEnemy.IsInRange(2.0f, 8.5f))
            .SetReturnMethod(() => !focusEnemy.HasTarget() || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(2))
            .SetUtilityMethod(() => focusEnemy.IsInRange(5.0f, 7.5f) ? 150 : 20)
        ;
    }

    void InitPullForward(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        hitPullForward = stateController.AddNewBehaviour()
            .SetBehaviourType((ulong)EBehaviourType.EAtack | (ulong)EBehaviourType.EDash | (ulong)EBehaviourType.EForward)
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => focusEnemy.ToTarget())
            .SetDirectionInput(() => focusEnemy.ToTarget())
            .SetKey(1, () => true)
            .SetCanEnter(() => focusEnemy.HasTarget() && character.CanEnterTo(2) && focusEnemy.IsInRange(4, 11.25f))
            .SetReturnMethod(() => !focusEnemy.HasTarget() || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => focusEnemy.IsInRange(6.0f, 10.5f) ? 135 : 40.0f)
        ;
    }
    void InitPullBackward(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        hitPullBackward = stateController.AddNewBehaviour()
            .SetBehaviourType((ulong)EBehaviourType.EAtack | (ulong)EBehaviourType.EDash | (ulong)EBehaviourType.EForward)
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => -focusEnemy.ToTarget())
            .SetDirectionInput(() => focusEnemy.ToTarget())
            .SetKey(1, () => true)
            .SetCanEnter(() => focusEnemy.HasTarget() && character.CanEnterTo(2) && focusEnemy.IsInRange(2, 7.5f))
            .SetReturnMethod(() => !focusEnemy.HasTarget() || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => focusEnemy.IsInRange(4.0f, 6.5f) ? 135 : 30.0f)
        ;
    }
    void InitPullSide(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        hitPullSide = stateController.AddNewBehaviour()
            .SetBehaviourType((ulong)EBehaviourType.EAtack | (ulong)EBehaviourType.EDash | (ulong)EBehaviourType.EForward)
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => focusEnemy.ToTargetSide())
            .SetDirectionInput(() => focusEnemy.ToTarget())
            .SetKey(1, () => true)
            .SetCanEnter(() => focusEnemy.HasTarget() && character.CanEnterTo(2) && focusEnemy.IsInRange(2, 9.0f))
            .SetReturnMethod(() => !focusEnemy.HasTarget() || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => focusEnemy.IsInRange(4.0f, 8.0f) ? 135 : 30.0f)
        ;
    }


    void InitPushForward(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        hitPushForward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => focusEnemy.ToTarget())
            .SetDirectionInput(() => focusEnemy.ToTarget())
            .SetKey(2, () => true)
            .SetCanEnter(() => focusEnemy.HasTarget() && character.CanEnterTo(3) && focusEnemy.IsInRange(0, 20.0f))
            .SetReturnMethod(() => !focusEnemy.HasTarget() || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => focusEnemy.IsInRange(0, 7.5f) ? 135 : 50)
        ;
    }
    void InitPushBackward(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        hitPushBackward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => -focusEnemy.ToTarget())
            .SetDirectionInput(() => focusEnemy.ToTarget())
            .SetKey(2, () => true)
            .SetCanEnter(() => focusEnemy.HasTarget() && character.CanEnterTo(3) && focusEnemy.IsInRange(0, 15.0f))
            .SetReturnMethod(() => !focusEnemy.HasTarget() || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => focusEnemy.IsInRange(0, 7.5f) ? 90 : 40)
        ;
    }
}

