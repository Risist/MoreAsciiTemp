using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;
using Ai;

[CreateAssetMenu(fileName = "ScytheBehaviourAttentionTest", menuName = "Character/Ai/AttentionTest")]
public class AiBehaviourScriptScytheAttentionTest : AiBehaviourScript
{
    public float agressiveness;

    public override void InitBehaviourController(AiBehaviourController stateController)
    {
        var instance = CreateInstance<AiBehaviourScriptScytheAttentionTest>();
        instance.agressiveness = agressiveness;
        instance.InitBase(stateController);
        instance.InitIdle(stateController);
        instance.InitEnemyShade(stateController);

        instance.InitCombat2(stateController);
        //instance.InitCombatAgressive(stateController);
        //instance.InitFollowTest(stateController);

        instance.InitPain(stateController);
        instance.InitPainShade(stateController);

        instance.InitNoise(stateController);
        instance.InitNoiseShade(stateController);
    }

    // components
    Timer executionTimer;
    AiFocusAttention attention;
    AiFocusClosest ally;
    AiFocusClosest noise;
    AiFocusClosest hidespot;

    void InitBase(AiBehaviourController stateController)
    {
        attention = stateController.RengisterMethod(new AiFocusAttention() );
        ally = stateController.RengisterMethod(new AiFocusClosest(EMemoryEvent.EAlly));
        noise = stateController.RengisterMethod(new AiFocusClosest(EMemoryEvent.ENoise));
        hidespot = stateController.RengisterMethod(new AiFocusClosest(EMemoryEvent.EHideSpot));

        executionTimer = new Timer();
        
    }

    /*void InitCombat(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;
        var memory = stateController.memory;

        //var moveToDestination = stateController.RengisterMethod(new Ai.Method.MoveToDestinationNavigation(attention));

        var lookAtTarget = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.25f, 0.35f))

            .SetRotationInput(() => attention.ToTarget())
            
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy)
            .SetReturnMethod(() => memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null || attention.lastState != EAttention.EEnemy|| executionTimer.IsReady())
            .SetUtilityMethod(() => 30f * attention.lastEvent.remainedTime.ElapsedTime() - 15)
            ;


        RangedFloat closeRange = new RangedFloat(5.0f, 8.0f);
        var stayInRangeClose = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.75f, 1.5f))

            .SetPositionInput(() => attention.StayInRange(closeRange))
            .SetReturnMethod(() => memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null || attention.lastState != EAttention.EEnemy || executionTimer.IsReady())
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy)
            .SetUtilityMethod(() =>
            {
                return !attention.IsInRange(closeRange.min - 0.5f, closeRange.max + 0.5f) ? 
                    Mathf.Lerp(5, 6, prefferedDistanceScale) : 1f;
            })
        ;

        RangedFloat farRange = new RangedFloat(13.0f, 17.0f);
        var stayInRangeFar = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.75f, 1.5f))

            .SetPositionInput(() => attention.StayInRange(farRange))
            .SetReturnMethod(() => memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null || attention.lastState != EAttention.EEnemy || executionTimer.IsReady())
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy)
            .SetUtilityMethod(() =>
            {
                return !attention.IsInRange(farRange.min - 3.5f, farRange.max + 3.5f) ?
                    Mathf.Lerp(6, 1, prefferedDistanceScale) : Mathf.Lerp(3, 1, prefferedDistanceScale);
            })
        ;

        var standBehindTarget = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.75f, 1.0f))

            .SetPositionInput(() => {
                return attention.StayBehindTargetAdaptive(2.0f, 300.0f);
            })
            .SetReturnMethod(() => memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null || attention.lastState != EAttention.EEnemy || executionTimer.IsReady())
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy)
            .SetUtilityMethod(() => attention.IsInRange(9, 20) ? 12 : 6)
        ;

        ////////////////////
        ///
        var characterStateChange = stateController.RengisterMethod(new Ai.Method.CharacterStateChangeDetector());


        var hitSlashForward = stateController.AddNewBehaviour()
            .SetBehaviourType((ulong)EBehaviourType.EAtack | (ulong)EBehaviourType.EDash | (ulong)EBehaviourType.EForward)
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(0, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(1) && attention.IsInRange(6.5f, 10.5f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(2))
            .SetUtilityMethod(() => attention.IsInRange(8.5f, 10.0f) ? 18 : 3)
        ;

        var hitSlashBackward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => -attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(0, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(1) && attention.IsInRange(1.0f, 7.5f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(2))
            .SetUtilityMethod(() => attention.IsInRange(4f, 6.25f) ? 18 : 2)
        ;

        var hitSlashSide = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => attention.ToTargetSide())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(0, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(1) && attention.IsInRange(2.0f, 8.5f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(2))
            .SetUtilityMethod(() => attention.IsInRange(5.0f, 7.5f) ? 18 : 2)
        ;

        var hitPullForward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(1, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(2) && attention.IsInRange(4, 11.25f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => attention.IsInRange(6.0f, 10.5f) ? 16 : 4.0f)
        ;

        var hitPullBackward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => -attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(1, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(2) && attention.IsInRange(2, 7.5f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => attention.IsInRange(4.0f, 6.5f) ? 16 : 3.0f)
        ;

        var hitPullSide = stateController.AddNewBehaviour()
            .SetBehaviourType((ulong)EBehaviourType.EAtack | (ulong)EBehaviourType.EDash | (ulong)EBehaviourType.EForward)
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => attention.ToTargetSide())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(1, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(2) && attention.IsInRange(2, 9.0f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => attention.IsInRange(4.0f, 8.0f) ? 16 : 3.0f)
        ;

        var hitPushForward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(2, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(3) && attention.IsInRange(0, 20.0f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => attention.IsInRange(0, 7.5f) ? 16 : 5)
        ;

        var hitPushBackward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => -attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(2, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(3) && attention.IsInRange(0, 15.0f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => attention.IsInRange(0, 7.5f) ? 12 : 4)
        ;
    }*/


    /// init behaviours of character when not allerted
    void InitIdle(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;
        var indicator = stateController.indicator;

        var idle = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.75f, 1.35f))

            .SetCanEnter(() => attention.lastState == EAttention.EIdle)
            .SetReturnMethod(() => attention.lastState != EAttention.EIdle || executionTimer.IsReady())
            .SetUtilityMethod(() => 1.25f)
        ;
        stateController.SetCurrentBehaviour(idle);

        var lookAroundMethod = stateController.RengisterMethod(new Ai.Method.LookAround(new RangedFloat(1.25f, 2.25f), new RangedFloat(45.0f, 90.0f)));
        var lookAround = stateController.AddNewBehaviour()
            .AddBegin(lookAroundMethod)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 3.0f))

            .SetRotationInput(() => lookAroundMethod.GetRotationInput(0.03f))

            .SetCanEnter(() => attention.lastState == EAttention.EIdle)
            .SetReturnMethod(() => attention.lastState != EAttention.EIdle || executionTimer.IsReady())
            .SetUtilityMethod(() => 1)
        ;


        Ai.Method.BoolMethod canJumpOverHurdle = () =>
                indicator.environmentIndicators[0].use &&
                Random.value < 0.1f;
        var moveToDestination = stateController.RengisterMethod(new Ai.Method.MoveToDestination(attention));
        var randomMovement = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => moveToDestination.SetNewDestination_Search(50.0f, 7.0f, 1.0f))
            .AddBegin(() => executionTimer.RestartRandom(1.25f, 2.75f))

            .SetPositionInput(() => moveToDestination.toDestination)
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)


            .SetCanEnter(() => attention.lastState == EAttention.EIdle)
            .SetReturnMethod(() => attention.lastState != EAttention.EIdle || executionTimer.IsReady())
            .SetUtilityMethod(() => 0.75f)
            ;

        var moveToAlly = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            //.AddBegin(() => moveToDestination.SetNewDestination_Search(5.0f, 7.0f, 1.0f))
            .AddBegin(() => executionTimer.RestartRandom(0.25f, 0.5f))

            .SetPositionInput(() => ally.ToTarget(new float[] { 1.0f, 0.75f, 0.5f, 0.25f }))
            //.SetPositionInput(() => moveToDestination.toDestination)
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)


            .SetCanEnter(() => attention.lastState == EAttention.EIdle && ally.HasTarget() && ally.IsFuther(10.5f))
            .SetReturnMethod(() => attention.lastState != EAttention.EIdle || executionTimer.IsReady() || ally.IsCloser(6.0f))
            .SetUtilityMethod(() => 2.5f)
            ;

        var moveAwayFromAlly = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            //.AddBegin(() => moveToDestination.SetNewDestination_Search(5.0f, 7.0f, 1.0f))
            .AddBegin(() => executionTimer.RestartRandom(0.25f, 0.5f))

            .SetPositionInput(() => -ally.ToTarget(new float[] { 1.0f, 0.75f, 0.5f, 0.25f }))
            //.SetPositionInput(() => moveToDestination.toDestination)
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)


            .SetCanEnter(() => attention.lastState == EAttention.EIdle && ally.HasTarget() && ally.IsCloser(5.5f))
            .SetReturnMethod(() => attention.lastState != EAttention.EIdle || executionTimer.IsReady() || ally.IsFuther(9.0f))
            .SetUtilityMethod(() => 2.5f)
            ;

        /*var pathFollower = stateController.GetComponentInChildren<AiPathFollower>();
        var followPath = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 3.5f))
            //.AddBegin(pathFollower.ResetPath)
            //.AddBegin(() => Debug.Log("follow path"))

            .SetPositionInput(() => pathFollower.FollowPath(2.5f))
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)


            .SetCanEnter(() => pathFollower.path && attention.lastState == EAttention.EIdle)
            .SetReturnMethod(() => attention.lastState != EAttention.EIdle || executionTimer.IsReady())
            .SetUtilityMethod(() => 0.75f)
            ;*/
    }

    void InitCombat2(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;
        var memory = stateController.memory;
        var indicator = stateController.indicator;
        var health = stateController.health;

        RangedFloat closeRange = new RangedFloat(8.75f, 10.25f);
        RangedFloat middleRange = new RangedFloat(11f, 14f);
        float margin = 0.5f;

        Ai.Method.BoolMethod canJumpOverHurdle = () =>
                indicator.environmentIndicators[0].use &&
                Random.value < 0.1f;


        var lookAtTargetCheck = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.3f, 0.5f))

            .SetRotationInput(() => attention.ToTarget())

            .SetCanEnter(() => attention.lastState == EAttention.EEnemy)
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EEnemy ||
                executionTimer.IsReady())
            .SetUtilityMethod(() => 25f * attention.lastEvent.remainedTime.ElapsedTime() - 15)
            ;

        var lookAtTarget = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.75f, 1.25f))

            .SetRotationInput(() => attention.ToTarget())

            .SetCanEnter(() =>
                attention.lastState == EAttention.EEnemy
                )
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EEnemy ||
                executionTimer.IsReady()
                )
            .SetUtilityMethod(() => 0.2f * attention.ToTarget().magnitude - 1.5f)
            ;


        var stayInRangeClose_into = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.75f, 1.5f))

            .SetPositionInput(() => attention.ToTarget())
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)

            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EEnemy ||
                executionTimer.IsReady() ||
                attention.IsCloser(closeRange.min)
                )
            .SetCanEnter(() =>
                attention.lastState == EAttention.EEnemy &&
                attention.IsFuther(closeRange.max + margin)
            )
            .SetUtilityMethod(() => 4.5f)
        ;
        var stayInRangeClose_away = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.75f, 1.5f))

            .SetPositionInput(() => -attention.ToTarget())
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)

            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EEnemy ||
                executionTimer.IsReady() ||
                attention.IsFuther(closeRange.max)
                )
            .SetCanEnter(() =>
                attention.lastState == EAttention.EEnemy &&
                attention.IsCloser(closeRange.min - margin)
            )
            .SetUtilityMethod(() => 6f)
        ;

        var stayInRangeMiddle_into = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.75f, 1.5f))

            .SetPositionInput(() => attention.ToTarget())
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)

            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EEnemy ||
                executionTimer.IsReady() ||
                attention.IsCloser(middleRange.min)
                )
            .SetCanEnter(() =>
                attention.lastState == EAttention.EEnemy &&
                attention.IsFuther(middleRange.max + margin)
            )
            .SetUtilityMethod(() => 3f)
        ;
        var stayInRangeMiddle_away = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.75f, 1.5f))

            .SetPositionInput(() => -attention.ToTarget())
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)

            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EEnemy ||
                executionTimer.IsReady() ||
                attention.IsFuther(middleRange.max)
                )
            .SetCanEnter(() =>
                attention.lastState == EAttention.EEnemy &&
                attention.IsCloser(middleRange.min - margin)
            )
            .SetUtilityMethod(() => 3f)
        ;

        var standBehindTarget = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.75f, 1.25f))

            .SetPositionInput(() => attention.StayBehindTargetAdaptive(1.0f, 700.0f, 0.8f))
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)

            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EEnemy ||
                executionTimer.IsReady() ||
                attention.IsBehindTarget(1)
                )
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy)
            .SetUtilityMethod(() => 11)
        ;

        var moveToAlly = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            //.AddBegin(() => moveToDestination.SetNewDestination_Search(5.0f, 7.0f, 1.0f))
            .AddBegin(() => executionTimer.RestartRandom(0.25f, 0.5f))

            .SetPositionInput(() => ally.ToTarget(new float[] { 1.0f, 0.75f, 0.5f, 0.25f }))
            //.SetPositionInput(() => moveToDestination.toDestination)
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)


            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && ally.HasTarget() && ally.IsFuther(12.5f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || executionTimer.IsReady() || ally.IsCloser(8.0f))
            .SetUtilityMethod(() => 7.5f)
            ;

        var moveAwayFromAlly = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            //.AddBegin(() => moveToDestination.SetNewDestination_Search(5.0f, 7.0f, 1.0f))
            .AddBegin(() => executionTimer.RestartRandom(0.25f, 0.5f))

            .SetPositionInput(() => -ally.ToTarget(new float[] { 1.0f, 0.75f, 0.5f, 0.25f }))
            //.SetPositionInput(() => moveToDestination.toDestination)
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)


            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && ally.HasTarget() && ally.IsCloser(7.5f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || executionTimer.IsReady() || ally.IsFuther(11.0f))
            .SetUtilityMethod(() => 9f)
            ;

        var moveToDestination = stateController.RengisterMethod(new Ai.Method.MoveToDestination(attention));
        var flee = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => moveToDestination.SetNewDestination_Flee(new RangedFloat(15, 35), 8.0f))
            .AddBegin(() => executionTimer.RestartRandom(2.25f, 4.75f))

            .SetPositionInput(() => moveToDestination.toDestination)
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, () => canJumpOverHurdle() || Random.value < 0.05f)


            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && attention.IsFuther(8))
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EEnemy ||
                executionTimer.IsReady()
                )
            .SetUtilityMethod(() => 2 - 2 * health.currentHealth / health.maxHealth)
            ;


        /////////////////////////////////////////
        ///
        var characterStateChange = stateController.RengisterMethod(new Ai.Method.CharacterStateChangeDetector());

        var hitPullBackward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => -attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(1, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(2) && attention.IsInRange(2, 11.5f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => attention.IsInRange(4.0f, 6.5f) ? 14f : 0.35f)
        ;

        var hitPullSide = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => attention.ToTargetSide())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(1, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(2) && attention.IsInRange(2, 10.5f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => attention.IsInRange(4.0f, 8.0f) ? 15f : 0.35f)
        ;

        var hitPullForward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(1, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(2) && attention.IsInRange(4, 11.25f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => attention.IsInRange(6.5f, 10.5f) ? 14f : 0.25f)
        ;

        var hitSlashSide = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => attention.ToTargetSide())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(0, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(1) && attention.IsInRange(2.0f, 9.5f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(2))
            .SetUtilityMethod(() => attention.IsInRange(6.5f, 8f) ? 14f : 0.25f)
        ;

        var hitSlashBackward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => -attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(0, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(1) && attention.IsInRange(1.0f, 8.25f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(2))
            .SetUtilityMethod(() => attention.IsInRange(4f, 6.25f) ? 11.5f : 0.25F)
        ;

        var hitSlashForward = stateController.AddNewBehaviour()
            .SetBehaviourType((ulong)EBehaviourType.EAtack | (ulong)EBehaviourType.EDash | (ulong)EBehaviourType.EForward)
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(0, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(1) && attention.IsInRange(7.0f, 11.5f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(2))
            .SetUtilityMethod(() => attention.IsInRange(8.0f, 10.5f) ? 13f : 0.25f)
        ;

        var hitPushForward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(2, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(3) && attention.IsInRange(0, 20.0f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => {
                if (attention.IsCloser(6.5f))
                    return 30f;
                else if (attention.IsCloser(8.25f))
                    return 13.5f;
                else
                    return 0.75f;
                })
        ;
    }
    void InitCombat(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;
        var memory = stateController.memory;
        var indicator = stateController.indicator;


        Ai.Method.BoolMethod canJumpOverHurdle = () =>
                indicator.environmentIndicators[0].use &&
                Random.value < 0.1f;

        RangedFloat closeRange = new RangedFloat(7.75f, 9.25f);
        RangedFloat middleRange = new RangedFloat(9f, 12.0f);
        RangedFloat farRange = new RangedFloat(13.0f, 15.0f);
        float margin = 0.4f;

        //var moveToDestination = stateController.RengisterMethod(new Ai.Method.MoveToDestinationNavigation(attention));


        var lookAtTargetCheck = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.30f, 0.4f))

            .SetRotationInput(() => attention.ToTarget())

            .SetCanEnter(() => attention.lastState == EAttention.EEnemy)
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EEnemy ||
                executionTimer.IsReady())
            .SetUtilityMethod(() => 95f * attention.lastEvent.remainedTime.ElapsedTime() - 30)
            ;
        var lookAtTarget = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.25f, 2.5f))

            .SetRotationInput(() => attention.ToTarget())

            .SetCanEnter(() =>
                attention.lastState == EAttention.EEnemy &&
                attention.IsInRange(12, 18)
                )
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EEnemy ||
                executionTimer.IsReady() ||
                attention.IsInRange(0, 10)
                )
            .SetUtilityMethod(() => 1.25f * attention.ToTarget().magnitude - 6.0f )
            ;

        var stayInRangeClose_into = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.75f, 1.5f))

            .SetPositionInput(() => attention.ToTarget() )
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EEnemy ||
                executionTimer.IsReady() ||
                attention.IsCloser(closeRange.min)
                )
            .SetCanEnter(() => 
                attention.lastState == EAttention.EEnemy && 
                attention.IsFuther(closeRange.max + margin)
            )
            .SetUtilityMethod(() =>
            {
                return 35f;
            })
        ;
        var stayInRangeClose_away = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.75f, 1.5f))

            .SetPositionInput(() => -attention.ToTarget())
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EEnemy ||
                executionTimer.IsReady() ||
                attention.IsFuther(closeRange.max)
                )
            .SetCanEnter(() =>
                attention.lastState == EAttention.EEnemy &&
                attention.IsCloser(closeRange.min - margin)
            )
            .SetUtilityMethod(() =>
            {
                return 50.0f;
            })
        ;
        var stayInRangeFar_away = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.25f, 1.75f))

            .SetPositionInput(() => -attention.ToTarget())
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EEnemy ||
                executionTimer.IsReady() ||
                attention.IsFuther(farRange.max)
                )
            .SetCanEnter(() =>
                attention.lastState == EAttention.EEnemy &&
                attention.IsCloser(farRange.min - 0.5f)
            )
            .SetUtilityMethod(() =>
            {
                return 30;
            })
        ;
        
        var standBehindTarget = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.75f, 1.25f))

            .SetPositionInput(() => attention.StayBehindTargetAdaptive(1.0f, 300.0f))
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)


            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                (attention.lastState != EAttention.EEnemy && executionTimer.IsReady(0.75f)) ||
                executionTimer.IsReady() ||
                attention.IsBehindTarget(1)
                )
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy)
            .SetUtilityMethod(() => 60)
        ;

        ////////////////////
        ///
        var characterStateChange = stateController.RengisterMethod(new Ai.Method.CharacterStateChangeDetector());

        var hitSlashForward = stateController.AddNewBehaviour()
            .SetBehaviourType((ulong)EBehaviourType.EAtack | (ulong)EBehaviourType.EDash | (ulong)EBehaviourType.EForward)
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(0, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(1) && attention.IsInRange(7.0f, 11.5f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(2))
            .SetUtilityMethod(() => attention.IsInRange(7.5f, 10.5f) ? 110 : 10)
        ;

        var hitSlashBackward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => -attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(0, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(1) && attention.IsInRange(1.0f, 8.25f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(2))
            .SetUtilityMethod(() => attention.IsInRange(4f, 6.25f) ? 90 : 10)
        ;

        var hitSlashSide = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => attention.ToTargetSide())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(0, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(1) && attention.IsInRange(2.0f, 8.5f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(2))
            .SetUtilityMethod(() => attention.IsInRange(5.0f, 7.5f) ? 100 : 10)
        ;

        var hitPullForward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(1, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(2) && attention.IsInRange(4, 11.25f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => attention.IsInRange(6.0f, 10.5f) ? 75 : 10f)
        ;

        var hitPullBackward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => -attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(1, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(2) && attention.IsInRange(2, 7.5f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => attention.IsInRange(4.0f, 6.5f) ? 85 : 30f)
        ;

        var hitPullSide = stateController.AddNewBehaviour()
            .SetBehaviourType((ulong)EBehaviourType.EAtack | (ulong)EBehaviourType.EDash | (ulong)EBehaviourType.EForward)
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => attention.ToTargetSide())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(1, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(2) && attention.IsInRange(2, 9.0f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => attention.IsInRange(4.0f, 8.0f) ? 80 : 10f)
        ;

        var hitPushForward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(2, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(3) && attention.IsInRange(0, 20.0f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => attention.IsInRange(0, 7.5f) ? 80 : 20)
        ;

        var hitPushBackward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => -attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(2, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(3) && attention.IsInRange(0, 15.0f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => attention.IsInRange(0, 7.5f) ? 70 : 10)
        ;

    }
    void InitEnemyShade(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;
        var memory = stateController.memory;
        var indicator = stateController.indicator;

        Ai.Method.BoolMethod canJumpOverHurdle = () =>
                indicator.environmentIndicators[0].use &&
                Random.value < 0.1f;

        var lookAroundMethod = stateController.RengisterMethod(new Ai.Method.LookAround(new RangedFloat(0.6f, 1.0f), new RangedFloat(55.0f, 125.0f)));
        var lookAround = stateController.AddNewBehaviour()
            .AddBegin(lookAroundMethod)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.9f, 1.75f))

            .SetRotationInput(() => lookAroundMethod.GetRotationInput(0.1f))

            .SetCanEnter(() => attention.lastState == EAttention.EEnemyShade)
            .SetReturnMethod(() => memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null || attention.lastState != EAttention.EEnemyShade || executionTimer.IsReady())
            .SetUtilityMethod(() => 3)
        ;

        var moveToDestination = stateController.RengisterMethod(new Ai.Method.MoveToDestinationNavigation(attention));
        var randomMovement = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => moveToDestination.SetNewDestination_Search(0.0f + 0.5f * attention.lastEvent.remainedTime.ElapsedTime(), 15.0f, 1.35f))
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 3.25f))

            .SetPositionInput(() => moveToDestination.ToDestination(0.75f, 1.0f), 0.25f)
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)

            .SetCanEnter(() => attention.lastState == EAttention.EEnemyShade)
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                moveToDestination.IsCloseToDestination(0.75f) ||
                attention.lastState != EAttention.EEnemyShade ||
                executionTimer.IsReady())
            .SetUtilityMethod(() => 8.5f + 1.5f * attention.ToTarget().magnitude)
            ;

        

        var lookAtTarget = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.15f, 0.25f))

            .SetRotationInput(() => attention.ToTarget())

            .SetCanEnter(() => attention.lastState == EAttention.EEnemyShade)
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EEnemyShade ||
                executionTimer.IsReady())
            .SetUtilityMethod(() => 8.0f + 0.5f * attention.ToTarget().magnitude + -2.0f * (attention.lastEvent.remainedTime.ElapsedTime() - memory.enemyKnowledgeTime))
            ;

        var moveToNoise = stateController.RengisterMethod(new Ai.Method.MoveToDestinationNavigation(noise));
        var noiseMove = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => moveToNoise.SetNewDestination_Search(0.0f + 0.75f * attention.lastEvent.remainedTime.ElapsedTime(), 7.0f, 1.0f))
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 3.25f))

            .SetPositionInput(() => moveToNoise.ToDestination(0.75f, 1.0f), 0.25f)
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)

            .SetCanEnter(() => memory.SearchInMemoryFresh(EMemoryEvent.ENoise, 0.75f) != null && attention.lastState == EAttention.EEnemyShade)
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                moveToNoise.IsCloseToDestination(0.75f) ||
                attention.lastState != EAttention.EEnemyShade ||
                executionTimer.IsReady())
            .SetUtilityMethod(() => 1.5f + 1.5f * attention.ToTarget().magnitude)
            ;
        var lookAtNoise = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.25f, 0.45f))

            .SetRotationInput(() => noise.ToTarget())

            .SetCanEnter(() => memory.SearchInMemoryFresh(EMemoryEvent.ENoise, 0.25f) != null && attention.lastState == EAttention.EEnemyShade)
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EEnemyShade ||
                executionTimer.IsReady())
            .SetUtilityMethod(() => 8.0f + 0.5f * attention.ToTarget().magnitude + -2.0f * (attention.lastEvent.remainedTime.ElapsedTime() - memory.enemyKnowledgeTime))
            ;
    }

    void InitPain(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;

        var lookAtTarget = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.25f, 0.35f))

            .SetRotationInput(() => attention.ToTarget())
            .SetPositionInput(() => -attention.ToTarget())

            .SetCanEnter(() => attention.lastState == EAttention.EPain)
            .SetReturnMethod(() => attention.lastState != EAttention.EPain || executionTimer.IsReady())
            .SetUtilityMethod(() => 1f)
            ;
    }
    void InitPainShade(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;
        var indicator = stateController.indicator;
        var memory = stateController.memory;

        var lookAroundMethod = stateController.RengisterMethod(new Ai.Method.LookAround(new RangedFloat(0.6f, 1.0f), new RangedFloat(45.0f, 125.0f)));
        var lookAround = stateController.AddNewBehaviour()
            .AddBegin(lookAroundMethod)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.9f, 1.75f))

            .SetRotationInput(() => lookAroundMethod.GetRotationInput(0.1f))

            .SetCanEnter(() => attention.lastState == EAttention.EPainShade)
            .SetReturnMethod(() => attention.lastState != EAttention.EPainShade || executionTimer.IsReady())
            .SetUtilityMethod(() => 1)
        ;

        Ai.Method.BoolMethod canJumpOverHurdle = () =>
                indicator.environmentIndicators[0].use &&
                Random.value < 0.1f;
        var moveToDestination = stateController.RengisterMethod(new Ai.Method.MoveToDestinationNavigation(attention));
        var randomMovement = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => moveToDestination.SetNewDestination_Search(5.0f + 0.75f * attention.lastEvent.remainedTime.ElapsedTime(), 7.0f, 1.0f))
            .AddBegin(() => executionTimer.RestartRandom(1.25f, 2.75f))

            .SetPositionInput(() => moveToDestination.ToDestination(0.75f, 1.0f), 0.25f)
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)

            .SetCanEnter(() => attention.lastState == EAttention.EPainShade)
            .SetReturnMethod(() => 
                moveToDestination.IsCloseToDestination(0.75f) ||
                attention.lastState != EAttention.EPainShade ||
                executionTimer.IsReady())
            .SetUtilityMethod(() => 1.0f)
            ;

        var moveToNoise = stateController.RengisterMethod(new Ai.Method.MoveToDestinationNavigation(noise));
        var noiseMove = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => moveToNoise.SetNewDestination_Search(0.0f + 0.75f * attention.lastEvent.remainedTime.ElapsedTime(), 7.0f, 1.0f))
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 3.25f))

            .SetPositionInput(() => moveToNoise.ToDestination(0.75f, 1.0f), 0.25f)
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)

            .SetCanEnter(() => memory.SearchInMemoryFresh(EMemoryEvent.ENoise, 0.75f) != null && attention.lastState == EAttention.EPainShade)
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                moveToNoise.IsCloseToDestination(0.75f) ||
                attention.lastState != EAttention.EPainShade ||
                executionTimer.IsReady())
            .SetUtilityMethod(() => 1.5f + 1.5f * attention.ToTarget().magnitude)
            ;
        var lookAtNoise = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.25f, 0.45f))

            .SetRotationInput(() => noise.ToTarget())

            .SetCanEnter(() => memory.SearchInMemoryFresh(EMemoryEvent.ENoise, 0.25f) != null && attention.lastState == EAttention.EPainShade)
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EPainShade ||
                executionTimer.IsReady())
            .SetUtilityMethod(() => 8.0f + 0.5f * attention.ToTarget().magnitude + -2.0f * (attention.lastEvent.remainedTime.ElapsedTime() - memory.enemyKnowledgeTime))
            ;
    }

    void InitNoise(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;
        var memory = stateController.memory;
        var indicator = stateController.indicator;

        var lookAtTarget = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1f, 2f))
            .AddBegin(() => Debug.Log("is inside Noise"))

            .SetRotationInput(() => attention.ToTarget())

            .SetCanEnter(() =>
                attention.lastState == EAttention.ENoise
                )
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                (attention.lastState != EAttention.ENoise && executionTimer.IsReady(1f)) ||
                executionTimer.IsReady() ||
                attention.IsInRange(0, 8)
                )
            .SetUtilityMethod(() => 10)
            ;

        Ai.Method.BoolMethod canJumpOverHurdle = () =>
                indicator.environmentIndicators[0].use &&
                Random.value < 0.1f;
        var moveToDestination = stateController.RengisterMethod(new Ai.Method.MoveToDestinationNavigation(attention));
        var randomMovement = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => moveToDestination.SetNewDestination_Search(5.0f + 0.75f * attention.lastEvent.remainedTime.ElapsedTime(), 7.0f, 1.0f))
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 3.25f))

            .SetPositionInput(() => moveToDestination.ToDestination(0.75f, 1.0f), 0.25f)
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)

            .SetCanEnter(() => attention.lastState == EAttention.ENoise)
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                moveToDestination.IsCloseToDestination(0.75f) ||
                attention.lastState != EAttention.ENoise ||
                executionTimer.IsReady())
            .SetUtilityMethod(() => 2 + 3.0f * attention.lastEvent.remainedTime.ElapsedTime())
            ;
    }
    void InitNoiseShade(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;
        var memory = stateController.memory;
        var indicator = stateController.indicator;

        Ai.Method.BoolMethod canJumpOverHurdle = () =>
                indicator.environmentIndicators[0].use &&
                Random.value < 0.1f;
        var moveToDestination = stateController.RengisterMethod(new Ai.Method.MoveToDestinationNavigation(attention));
        var randomMovement = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => moveToDestination.SetNewDestination_Search(5.0f + 1f * attention.lastEvent.remainedTime.ElapsedTime(), 7.0f, 1.0f))
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 3.25f))

            .SetPositionInput(() => moveToDestination.ToDestination(0.75f, 1.0f), 0.25f)
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)

            .SetCanEnter(() => attention.lastState == EAttention.ENoiseShade)
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                moveToDestination.IsCloseToDestination(0.75f) ||
                attention.lastState != EAttention.ENoiseShade ||
                executionTimer.IsReady())
            .SetUtilityMethod(() => 2)
            ;

        var lookAroundMethod = stateController.RengisterMethod(new Ai.Method.LookAround(new RangedFloat(0.6f, 1.0f), new RangedFloat(45.0f, 125.0f)));
        var lookAround = stateController.AddNewBehaviour()
            .AddBegin(lookAroundMethod)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.9f, 1.75f))

            .SetRotationInput(() => lookAroundMethod.GetRotationInput(0.1f))

            .SetCanEnter(() => attention.lastState == EAttention.ENoiseShade)
            .SetReturnMethod(() => attention.lastState != EAttention.ENoiseShade || executionTimer.IsReady())
            .SetUtilityMethod(() => 1)
        ;
    }
}

