using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;
using Ai;

[CreateAssetMenu(fileName = "AxeShieldBehaviour", menuName = "Character/Ai/AxeShield")]
public class AiBehaviourScriptAxeShield : AiBehaviourScript
{
    public override void InitBehaviourController(AiBehaviourController stateController)
    {

        var instance = CreateInstance<AiBehaviourScriptAxeShield>();
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
        instance.InitPushForward(stateController);
        instance.InitPushBackwards(stateController);

        instance.InitBlockForward(stateController);
        instance.InitBlockBackward(stateController);
        instance.InitBlockSide(stateController);

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
            
            .SetCanEnter(() => !focusEnemy.HasTarget() )

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
            .SetUtilityMethod(() => 6 )
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
            .SetReturnMethod(() => focusEnemy.HasTarget() || focusPain.HasTarget() || executionTimer.IsReady())
            .SetUtilityMethod(() => 1f)
            .SetCanEnter(()=> !focusEnemy.HasTarget() )
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
            .SetUtilityMethod(() => 2f)
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
            .SetUtilityMethod(() => focusEnemy.IsInRange(9,15) ? 90 : 40)
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

            .SetPositionInput(() => focusEnemy.StayInRange(3.0f, 6.0f))
            .SetReturnMethod(() => !focusEnemy.HasTarget() || executionTimer.IsReady())
            .SetCanEnter(() => focusEnemy.HasTarget() && !focusEnemy.IsInRange(0.0f, 8.0f))
            .SetUtilityMethod(() => 25)
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

            .SetPositionInput(() => focusEnemy.StayInRange(9.0f, 12.0f))
            .SetReturnMethod(() => !focusEnemy.HasTarget() || executionTimer.IsReady())
            .SetCanEnter(() => focusEnemy.HasTarget() && !focusEnemy.IsInRange(5.5f, 20.0f))
            .SetUtilityMethod(() => 10)
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
            .SetCanEnter(() => focusEnemy.HasTarget() && character.CanEnterTo(1) && focusEnemy.IsInRange(4, 6.25f))
            .SetReturnMethod(() => !focusEnemy.HasTarget() || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(2))
            .SetUtilityMethod(() => 150)
        ;
    }
    void InitPushForward(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        hitPushForward = stateController.AddNewBehaviour()
            .SetBehaviourType((ulong)EBehaviourType.EAtack | (ulong)EBehaviourType.EDash | (ulong)EBehaviourType.EForward)
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => focusEnemy.ToTarget())
            .SetDirectionInput(() => focusEnemy.ToTarget())
            .SetKey(1, () => true)
            .SetCanEnter(() => focusEnemy.HasTarget() && character.CanEnterTo(2) && focusEnemy.IsInRange(0, 8.5f))
            .SetReturnMethod(() => !focusEnemy.HasTarget() || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => 135)
        ;
    }
    void InitPushBackwards(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        hitPushBackward = stateController.AddNewBehaviour()
            .SetBehaviourType((ulong)EBehaviourType.EAtack | (ulong)EBehaviourType.EDash | (ulong)EBehaviourType.EBackwards)
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => -focusEnemy.ToTarget())
            .SetDirectionInput(() => focusEnemy.ToTarget())
            .SetKey(1, () => true)
            .SetCanEnter(() => focusEnemy.HasTarget() && character.CanEnterTo(2) && focusEnemy.IsInRange(0, 4.0f))
            .SetReturnMethod(() => !focusEnemy.HasTarget() || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => 135)
        ;
    }

    void InitBlockForward(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        blockForward = stateController.AddNewBehaviour()
            .SetBehaviourType((ulong)EBehaviourType.EBlock | (ulong)EBehaviourType.EDash | (ulong)EBehaviourType.EForward)
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.5f, 1.5f))

            .SetPositionInput(() => focusEnemy.ToTarget())
            .SetDirectionInput(() => focusEnemy.ToTarget())
            .SetKey(2, () => true)
            .SetCanEnter(() => focusEnemy.HasTarget() && character.CanEnterTo(3) )
            .SetReturnMethod(() => !focusEnemy.HasTarget() || characterStateChange.HasChanged() || executionTimer.IsReady() || character.CanEnterTo(1))
            .SetUtilityMethod(() => focusEnemy.IsInRange(0, 7) ? 50 : 20)
        ;
    }
    void InitBlockBackward(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        blockBackward = stateController.AddNewBehaviour()
            .SetBehaviourType((ulong)EBehaviourType.EBlock | (ulong)EBehaviourType.EDash | (ulong)EBehaviourType.EBackwards)
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.5f, 1.5f))

            .SetPositionInput(() => -focusEnemy.ToTarget())
            .SetDirectionInput(() => focusEnemy.ToTarget())
            .SetKey(2, () => true)
            .SetCanEnter(() => focusEnemy.HasTarget() && character.CanEnterTo(3) )
            .SetReturnMethod(() => !focusEnemy.HasTarget() || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(2))
            .SetUtilityMethod(() => focusEnemy.IsInRange(0.0f, 6.0f) ? 40.0f : 10.0f )
        ;
    }
    void InitBlockSide(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        blockSide = stateController.AddNewBehaviour()
            .SetBehaviourType((ulong)EBehaviourType.EBlock | (ulong)EBehaviourType.EDash | (ulong)EBehaviourType.ESide)
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.5f, 1.5f))

            .SetPositionInput(() => focusEnemy.ToTargetSide())
            .SetDirectionInput(() => focusEnemy.ToTarget())
            .SetKey(2, () => true)
            .SetCanEnter(() => focusEnemy.HasTarget() && character.CanEnterTo(3) && focusEnemy.IsInRange(4.5f, 12))
            .SetReturnMethod(() => !focusEnemy.HasTarget() || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(2))
            .SetUtilityMethod(() => 25)
        ;
    }
}

