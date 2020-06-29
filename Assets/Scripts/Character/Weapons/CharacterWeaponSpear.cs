using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;

[CreateAssetMenu(fileName = "SpearWeapon", menuName = "Character/Weapon/Spear")]
public class CharacterWeaponSpear : CharacterWeaponBase
{
    public override void InitCharacterStateController(CharacterStateController stateController)
    {
        var idle = stateController.AddNewState("idle");          // 0
        var slash = stateController.AddNewState("slash");        // 1
        var push = stateController.AddNewState("push");          // 2
        var dash = stateController.AddNewState("dash");          // 3
        var _throw = stateController.AddNewState("throw");       // 4
        var stagger = stateController.AddNewState("pain");       // 5
        var wallDash = stateController.AddNewState("wall dash"); // 6


        stateController.AddTransitionToAllStates(stagger, new AnimationTransitionData(0.1f));
        stateController.SetCurrentState(idle);

        idle
            .AddTransition(dash, new AnimationTransitionData(0))
            .AddTransition(push, new AnimationTransitionData(0))
            .AddTransition(slash, new AnimationTransitionData(0))
            //.AddTransition(blockWallDash, new AnimationTransitionData(0))
            ;

        

        int cdDash = stateController.CreateCd(0.75f);
        int cdSlash = stateController.CreateCd(0.0f);
        int cdPush = stateController.CreateCd(0.0f);
        int cdStagger = stateController.CreateCd(0.5f);

        DamagedRecorder damaged = new DamagedRecorder(stateController.health);
        stagger
            .AddComponent(new CState_Cd(cdStagger))
            .AddComponent(new CState_StaggerCondition(damaged))
            .AddComponent(new CState_AutoTransition(idle, 1f))
        ;

        slash
            .AddComponent(new CState_Input(0))
            .AddComponent(new CState_Cd(cdSlash))
            .AddComponent(new CState_RotationToDirection(new RangedFloat(0, 0.6f), 0.15f, 0.75f))

            .AddComponent(new CState_Motor(new Vector2(0, 325.0f), new RangedFloat(0.25f, 0.375f)))
            .AddComponent(new CState_AutoTransition(idle, 1f))

            .AddTransition(push, new RangedFloat(0.5f), new AnimationTransitionData(0.2f, -0.15f))
        ;

        push
            .AddComponent(new CState_Input(1))
            .AddComponent(new CState_Cd(cdPush))
            //.AddComponent(new CState_Cd(cdBlock, CState_Cd.EMode.ERestartOnly))
            .AddComponent(new CState_RotationToDirection(new RangedFloat(0, 0.425f), 0.2f, 0.85f))
            //.AddComponent(new CState_JumpMotor(new float[] { 300, -150}, new RangedFloat(0.35f, 0.5f), -100.0f).SetDefaultDirection(0))
            //.AddComponent(new CState_Motor(new Vector2(0, 250.0f), new RangedFloat(0.050f, 0.1250f)))
            .AddComponent(new CState_JumpMotor(new float[] { -150, 250, 300, 300 }, new RangedFloat(0.05f, 0.15f), -100.0f).SetDefaultDirection(0))
            //.AddComponent(new CState_Motor(new Vector2(0, -250.0f), new RangedFloat(0.050f, 0.15f)))
            .AddComponent(new CState_Motor(new Vector2(0, 400.0f), new RangedFloat(0.35f, 0.475f)))
            .AddComponent(new CState_AutoTransition(idle, 1f))

            .AddTransition(slash, new RangedFloat(0.5f), new AnimationTransitionData(0.2f, -0.15f))
            //.AddTransition(push, new RangedFloat(0.9f), new AnimationTransitionData(0.2f, -0.125f), false)
        ;
        _throw
            .AddComponent(new CState_Input(0))
            .AddComponent(new CState_Cd(cdPush))
            //.AddComponent(new CState_Cd(cdBlock, CState_Cd.EMode.ERestartOnly))
            .AddComponent(new CState_RotationToDirection(new RangedFloat(0, 0.475f), 0.1f, 0.85f))
            //.AddComponent(new CState_JumpMotor(new float[] { 300, 125, 200, 200 }, new RangedFloat(0.050f, 0.1750f), -100.0f).SetDefaultDirection(0))
            //.AddComponent(new CState_Motor(new Vector2(0, 250.0f), new RangedFloat(0.050f, 0.1250f)))
            .AddComponent(new CState_Motor(new Vector2(0, 150.0f), new RangedFloat(0.3f, 0.5f)))
            .AddComponent(new CState_AutoTransition(idle, 1f))

            //.AddTransition(slash, new RangedFloat(0.65f), new AnimationTransitionData(0.25f, 0.0f))
        ;

        var jumpBlock = new CState_JumpMotor(new float[] { 200, 250, 300, 300 }, new RangedFloat(0.05f, 0.25f))
                .SetDefaultDirection(0);
        dash
            .AddComponent(new CState_Input(2))
            //.AddComponent(new CState_Cd(cdDash, CState_Cd.EMode.EConditionOnly))
            .AddComponent(new CState_RotationToDirection(new RangedFloat(0f, 0.6f), 0.25f, 1.0f))
            .AddComponent(jumpBlock)
            .AddComponent(new CState_AutoTransition(idle, 1f))

            //.AddTransition(_throw, new RangedFloat(0.3f), new AnimationTransitionData(0.2f, 0.15f))
            .AddTransition(slash, new RangedFloat(0.4f), new AnimationTransitionData(0.2f, 0.0f))
            //.AddTransition(blockWallDash, new RangedFloat(0.3f, 0.4f), new AnimationTransitionData(0f, 0.0f), false, (CharacterState target, CharacterStateController controller) => jumpBlock.currentDirectionId == 0)


            .AddTransition(push, new RangedFloat(0.6f), new AnimationTransitionData(0.15f, -0.1f))
            .AddTransition(dash, new RangedFloat(0.7f), new AnimationTransitionData(0.2f, -0.15f), false)
            .AddTransition(wallDash, new RangedFloat(0.2f, 0.4f), new AnimationTransitionData(0.1f, 0.25f), false, (CharacterState target, CharacterStateController controller) => jumpBlock.currentDirectionId == 0)
        ;


        wallDash
            .AddComponent(new CState_Input(2))
            .AddComponent(new CStateJumpOverWall(0.4F, 0.03f, 0.25f, 0.75f))
            .AddComponent(new CState_AutoTransition(idle, 1f))//,0.2f))
        ;
    }
}