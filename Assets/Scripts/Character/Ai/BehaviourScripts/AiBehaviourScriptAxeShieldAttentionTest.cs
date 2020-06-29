using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;
using Ai;

[CreateAssetMenu(fileName = "AxeShieldBehaviourAttentionTest", menuName = "Character/Ai/AxeShieldAttentionTest")]
public class AiBehaviourScriptAxeShieldAttentionTest : AiBehaviourScript
{
    public float agressiveness;

    public override void InitBehaviourController(AiBehaviourController stateController)
    {

        var instance = CreateInstance<AiBehaviourScriptAxeShieldAttentionTest>();
        instance.agressiveness = agressiveness;
        instance.InitBase(stateController);
        instance.InitIdle(stateController);
        instance.InitEnemyShade(stateController);

        instance.InitCombat(stateController);
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
        attention = stateController.RengisterMethod(new AiFocusAttention());
        ally = stateController.RengisterMethod(new AiFocusClosest(EMemoryEvent.EAlly));
        noise = stateController.RengisterMethod(new AiFocusClosest(EMemoryEvent.ENoise));
        hidespot = stateController.RengisterMethod(new AiFocusClosest(EMemoryEvent.EHideSpot));
        executionTimer = new Timer();

    }

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
    void InitCombat(AiBehaviourController stateController)
    {
        // Aliases
        var inputHolder = stateController.inputHolder;
        var character = stateController.character;
        var transform = stateController.transform;
        var memory = stateController.memory;
        var indicator = stateController.indicator;
        var health = stateController.health;

        //var moveToDestination = stateController.RengisterMethod(new Ai.Method.MoveToDestinationNavigation(attention));
        
        
        var lookAtTargetCheck = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.2f, 0.3f))

            .SetRotationInput(() => attention.ToTarget())

            .SetCanEnter(() => attention.lastState == EAttention.EEnemy)
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EEnemy ||
                executionTimer.IsReady())
            .SetUtilityMethod(() => 80f * attention.lastEvent.remainedTime.ElapsedTime() - 30)
            ;
        var lookAtTarget = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.25f, 2.5f))

            .SetRotationInput(() => attention.ToTarget())

            .SetCanEnter(() => 
                attention.lastState == EAttention.EEnemy &&
                attention.IsInRange(10,18)
                )
            .SetReturnMethod(() => 
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                (attention.lastState != EAttention.EEnemy && executionTimer.IsReady(1.25f) )||
                executionTimer.IsReady() ||
                attention.IsInRange(0,8)
                )
            .SetUtilityMethod(() => Mathf.Lerp(
                0.5f * attention.ToTarget().magnitude - 5.0f,
                0.5f * attention.ToTarget().magnitude - 5.0f, agressiveness)  )
            ;

        Ai.Method.BoolMethod canJumpOverHurdle = () =>
                indicator.environmentIndicators[0].use &&
                Random.value < 0.1f;

        var stayInRangeClose = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.75f, 1.5f))

            .SetPositionInput(() => attention.ToTarget() )
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, () => canJumpOverHurdle() || Random.value < 0.05f)


            .SetReturnMethod(() => 
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                (attention.lastState != EAttention.EEnemy && executionTimer.IsReady(0.4f)) ||
                executionTimer.IsReady() ||
                attention.IsInRange(0,4f)
                )
            .SetCanEnter(() => 
                attention.lastState == EAttention.EEnemy &&
                !attention.IsInRange(0.0f, 4.5f) )
            .SetUtilityMethod(() => Mathf.Lerp(11.0f, 12.0f, agressiveness) )
        ;

        var stayInRangeMiddle = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.25f, 2.25f))

            .SetPositionInput(() => attention.StayInRange(7f, 8.0f))
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)

            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                (attention.lastState != EAttention.EEnemy && executionTimer.IsReady(0.15f)) ||
                executionTimer.IsReady() ||
                (attention.IsInRange(6.5f, 8.5f) && executionTimer.IsReady(1.25f)))
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && !attention.IsInRange(5.0f, 10.5f))
            .SetUtilityMethod(() => Mathf.Lerp(7.0f, 6f, agressiveness))
        ;

        var stayInRangeFar = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.25f, 2.25f))

            .SetPositionInput(() => attention.StayInRange(12f, 13.0f))
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)

            .SetReturnMethod(() => 
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null || 
                (attention.lastState != EAttention.EEnemy && executionTimer.IsReady(0.15f) ) ||
                executionTimer.IsReady() ||
                (attention.IsInRange(11.5f, 13.5f) &&  executionTimer.IsReady(1.25f) ))
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && !attention.IsInRange(10.0f, 15.5f) )
            .SetUtilityMethod(() => Mathf.Lerp(8.0f, 4f, agressiveness) )
        ;

        var standBehindTarget = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(0.75f, 1.5f))

            .SetPositionInput(() => attention.StayBehindTargetAdaptive(1.0f, 300.0f))
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)


            .SetReturnMethod(() => 
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                attention.lastState != EAttention.EEnemy ||
                executionTimer.IsReady() ||
                attention.IsBehindTarget(1)
                )
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy)
            .SetUtilityMethod(() => Mathf.Lerp(14, 16f, agressiveness))
        ;

        var moveToAlly = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            //.AddBegin(() => moveToDestination.SetNewDestination_Search(5.0f, 7.0f, 1.0f))
            .AddBegin(() => executionTimer.RestartRandom(0.25f, 0.5f))

            .SetPositionInput(() => ally.ToTarget(new float[] { 1.0f, 0.75f, 0.5f, 0.25f }))
            //.SetPositionInput(() => moveToDestination.toDestination)
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)


            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && ally.HasTarget() && ally.IsFuther(11.5f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || executionTimer.IsReady() || ally.IsCloser(7.0f))
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


            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && ally.HasTarget() && ally.IsCloser(6.5f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || executionTimer.IsReady() || ally.IsFuther(10.0f))
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
        ////////////////////
        ///
        var characterStateChange = stateController.RengisterMethod(new Ai.Method.CharacterStateChangeDetector());

        var hitSlashForward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(0, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(1) && attention.IsInRange(2, 8.0f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(2))
            .SetUtilityMethod(() => attention.IsInRange(4, 7.0f) ? 100f : 40.0f)
        ;

        var hitPushForward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(1, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(2) && attention.IsInRange(0, 7.0f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy|| characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => 90f)
        ;
        
        var hitPushBackward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 2.5f))

            .SetPositionInput(() => -attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(1, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(2) && attention.IsInRange(0, 5.0f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady())// || character.CanEnterTo(1) )
            .SetUtilityMethod(() => 75f)
        ;
        
        var blockForward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.5f, 1.5f))

            .SetPositionInput(() => attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(2, () => true)
            .SetKey(0, () => executionTimer.ElapsedTime() > 0.2f && Random.value < 0.75f)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(3))
            .SetReturnMethod(() => 
                attention.lastState != EAttention.EEnemy ||
                characterStateChange.HasChanged() ||
                executionTimer.IsReady() //||
                //( Random.value < 0.35f && character.CanEnterTo(1) )
                )
            .SetUtilityMethod(() => attention.IsInRange(0, 8) ? 40 : 15 )
        ;
        var blockBackward = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.5f, 1.5f))

            .SetPositionInput(() => -attention.ToTarget())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(2, () => true)
            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(3) && attention.IsInRange(0.0f, 11.0f))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady()  )// || character.CanEnterTo(2))
            .SetUtilityMethod(() => attention.IsInRange(0.0f, 6.5f) ?  30f : 12.5f)
        ;
        var blockSide = stateController.AddNewBehaviour()
            .AddBegin(characterStateChange)
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => executionTimer.RestartRandom(1.5f, 1.5f))

            .SetPositionInput(() => attention.ToTargetSide())
            .SetDirectionInput(() => attention.ToTarget())
            .SetKey(2, () => true)

            .SetCanEnter(() => attention.lastState == EAttention.EEnemy && character.CanEnterTo(3) && attention.IsInRange(4.5f, 12))
            .SetReturnMethod(() => attention.lastState != EAttention.EEnemy || characterStateChange.HasChanged() || executionTimer.IsReady() )// || character.CanEnterTo(2))
            .SetUtilityMethod(() => 27.5f )
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

            .SetPositionInput(() => moveToDestination.ToDestination(3.5f, 1.0f), 0.25f)
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)

            .SetCanEnter(() => attention.lastState == EAttention.EEnemyShade)
            .SetReturnMethod(() => 
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                moveToDestination.IsCloseToDestination(0.75f) ||
                attention.lastState != EAttention.EEnemyShade ||
                executionTimer.IsReady())
            .SetUtilityMethod(() => 1.5f + 1.5f * attention.ToTarget().magnitude)
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
            .AddBegin(() => moveToDestination.SetNewDestination_Search(5.0f, 7.0f, 1.0f))
            .AddBegin(() => executionTimer.RestartRandom(1.25f, 2.75f))

            .SetPositionInput(() => moveToDestination.ToDestination(3.5f, 1.0f), 0.25f)
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)

            .SetCanEnter(() => attention.lastState == EAttention.EPainShade)
            .SetReturnMethod(() => moveToDestination.IsCloseToDestination(3.5f) || attention.lastState != EAttention.EPainShade || executionTimer.IsReady())
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
            .SetUtilityMethod(() => 10 )
            ;

        Ai.Method.BoolMethod canJumpOverHurdle = () =>
                indicator.environmentIndicators[0].use &&
                Random.value < 0.1f;
        var moveToDestination = stateController.RengisterMethod(new Ai.Method.MoveToDestinationNavigation(attention));
        var randomMovement = stateController.AddNewBehaviour()
            .AddBegin(inputHolder.ResetInput)
            .AddBegin(() => moveToDestination.SetNewDestination_Search(5.0f, 7.0f, 1.0f))
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 3.25f))

            .SetPositionInput(() => moveToDestination.ToDestination(3.5f, 1.0f), 0.25f)
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)

            .SetCanEnter(() => attention.lastState == EAttention.ENoise)
            .SetReturnMethod(() => 
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                moveToDestination.IsCloseToDestination(3.5f) ||
                attention.lastState != EAttention.ENoise ||
                executionTimer.IsReady())
            .SetUtilityMethod(() => 2 + 3.0f * attention.lastEvent.remainedTime.ElapsedTime() )
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
            .AddBegin(() => moveToDestination.SetNewDestination_Search(25.0f, 7.0f, 1.0f))
            .AddBegin(() => executionTimer.RestartRandom(1.75f, 3.25f))

            .SetPositionInput(() => moveToDestination.ToDestination(3.5f, 1.0f), 0.25f)
            .SetDirectionInput(() => inputHolder.positionInput, 0.25f)
            .SetKey(2, canJumpOverHurdle)

            .SetCanEnter(() => attention.lastState == EAttention.ENoiseShade)
            .SetReturnMethod(() =>
                memory.SearchInMemoryFresh(EMemoryEvent.EPain, 0.25f) != null ||
                moveToDestination.IsCloseToDestination(3.5f) ||
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

