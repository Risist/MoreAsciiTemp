using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;

[CreateAssetMenu(fileName = "DaggerWeapon", menuName = "Character/Weapon/Dagger")]
public class CharacterWeaponDagger : CharacterWeaponBase
{
    public override void InitCharacterStateController(CharacterStateController stateController)
    {
        var idle = stateController.AddNewState("idle");
        var dash = stateController.AddNewState("dash");
        var slash = stateController.AddNewState("slash");
        var slash2 = stateController.AddNewState("slash2");
        //var pull = stateController.AddNewState("scythe_pull");
        //var push = stateController.AddNewState("scythe_push 1");
        //var push = stateController.AddNewState("scythe_push");
        //var block = stateController.AddNewState("scythe_block");
        //var stagger = stateController.AddNewState("scythe_pain1");
        //var stagger2 = stateController.AddNewState("scythe_pain2");

        //stateController.AddTransitionToAllStates(stagger, new AnimationTransitionData(0.1f));
        //stateController.AddTransitionToAllStates(stagger, (CharacterState st) => st == stagger2, new AnimationTransitionData(0.25f));
        //stateController.AddTransitionToAllStates(stagger2, (CharacterState st) => st == stagger, new AnimationTransitionData(0.25f));
        stateController.SetCurrentState(idle);

        idle
            .AddTransition(dash, new AnimationTransitionData(0))
            .AddTransition(slash, new AnimationTransitionData(0))
            .AddTransition(slash2, new AnimationTransitionData(0))
            //.AddTransition(push, new AnimationTransitionData(0))
            //.AddTransition(stagger, new AnimationTransitionData(0.1f) )
            //.AddTransition(slash, new AnimationTransitionData(0))
            //.AddTransition(pull, new AnimationTransitionData(0))
            //.AddTransition(push, new AnimationTransitionData(0))

            ;
        DamagedRecorder damaged = new DamagedRecorder(stateController.health);



        int cdDash = stateController.CreateCd(0.5f);
        int cdSlash = stateController.CreateCd(1.25f);
        int cdSlash2 = stateController.CreateCd(1.25f);
        int cdPull = stateController.CreateCd(0.5f);
        int cdStagger = stateController.CreateCd(0.5f);

        dash
            .AddComponent(new CState_Input(2))
            .AddComponent(new CState_Cd(cdDash))
            .AddComponent(new CState_RotationToDirection(new RangedFloat(0f, 0.6f), 0.15f, 0.975f))
            .AddComponent(new CState_JumpMotor(new float[] { 230, 230, 250, 250 }, new RangedFloat(0.0f, 0.35f))
                .SetDefaultDirection(0))

            //.AddComponent(new CState_JumpMotor(new float[] { 275, -100}, new RangedFloat(0.325f, 0.45f))
            //    .SetDefaultDirection(0))

            //.AddComponent(new CState_Motor(new Vector2(0, 275.0f), new RangedFloat(0.325f, 0.45f)))
            .AddComponent(new CState_AutoTransition(idle, 1f))

            //.AddTransition(pull, new RangedFloat(0.6f), new AnimationTransitionData(0.25f, 0.0f))
            .AddTransition(slash, new RangedFloat(0.5f), new AnimationTransitionData(0.15f, 0.05f))
            .AddTransition(slash2, new RangedFloat(0.5f), new AnimationTransitionData(0.15f, 0.05f))
        ;

        slash
            .AddComponent(new CState_Input(0))
            .AddComponent(new CState_Cd(cdSlash))
            .AddComponent(new CState_RotationToDirection(new RangedFloat(0f, 0.4f), 0.15f, 0.875f))
            .AddComponent(new CState_JumpMotor(new float[] { 300, 300, 350, 350 }, new RangedFloat(0.2f, 0.35f))
                .SetDefaultDirection(0))

            .AddComponent(new CState_AutoTransition(idle, 1f))

            .AddTransition(slash2, new RangedFloat(0.425f), new AnimationTransitionData(0.15f, 0.1f), false)
        ;
        slash2
            .AddComponent(new CState_Input(0))
            .AddComponent(new CState_Cd(cdSlash2))
            .AddComponent(new CState_RotationToDirection(new RangedFloat(0f, 0.4f), 0.15f, 0.875f))
            .AddComponent(new CState_JumpMotor(new float[] { 350, 350, 400, 400 }, new RangedFloat(0.2f, 0.35f))
                .SetDefaultDirection(0))

            .AddComponent(new CState_AutoTransition(idle, 1f))
            .AddTransition(slash, new RangedFloat(0.425f), new AnimationTransitionData(0.15f, 0.1f), false)
        ;

        /*stagger
            .AddComponent(new CState_Cd(cdStagger))
            .AddComponent(new CState_StaggerCondition(damaged))
            .AddComponent(new CState_AutoTransition(idle, 1f))
        ;

        slash
            .AddComponent(new CState_Input(0))
            .AddComponent(new CState_Cd(cdSlash))
            //.AddComponent(new CState_Cd(cdBlock, CState_Cd.EMode.ERestartOnly))
            .AddComponent(new CState_RotationToDirection(new RangedFloat(0, 0.4f), 0.1f, 0.55f))
            //.AddComponent(new CState_RotationToDirection(new RangedFloat(0.3f, 0.7f), 0.1f, 1.0f))
            .AddComponent(new CState_JumpMotor(new float[] { 60, 135, 160, 160 }, new RangedFloat(0.05f, 0.2f), -100.0f).SetDefaultDirection(0))
            .AddComponent(new CState_Motor(new Vector2(0, 325.0f), new RangedFloat(0.325f, 0.4f)))
            .AddComponent(new CState_AutoTransition(idle, 1f))

            .AddTransition(push, new RangedFloat(0.6f), new AnimationTransitionData(0.2f, 0f))
            .AddTransition(pull, new RangedFloat(0.55f, 0.7f), new AnimationTransitionData(0.25f, 0.0f))
        ;
        pull
            .AddComponent(new CState_Input(1))
            .AddComponent(new CState_Cd(cdPull))
            //.AddComponent(new CState_Cd(cdBlock, CState_Cd.EMode.ERestartOnly))
            .AddComponent(new CState_RotationToDirection(new RangedFloat(0, 0.4f), 0.15f, 0.55f))
            .AddComponent(new CState_JumpMotor(new float[] { 230, 110, 180, 180 }, new RangedFloat(0.050f, 0.1250f), -100.0f).SetDefaultDirection(0))
            //.AddComponent(new CState_Motor(new Vector2(0, 250.0f), new RangedFloat(0.050f, 0.1250f)))
            .AddComponent(new CState_Motor(new Vector2(0, -250.0f), new RangedFloat(0.325f, 0.425f)))
            .AddComponent(new CState_AutoTransition(idle, 1f))

            .AddTransition(push, new RangedFloat(0.6f), new AnimationTransitionData(0.2f, 0f))
            .AddTransition(slash, new RangedFloat(0.55f, 0.7f), new AnimationTransitionData(0.2f, 0.0f))
        ;
        push
            .AddComponent(new CState_Input(2))
            .AddComponent(new CState_Cd(cdPush))
            .AddComponent(new CState_RotationToDirection(new RangedFloat(0f, 0.6f), 0.25f, 0.55f))
            .AddComponent(new CState_JumpMotor(new float[] { 150, 170, 150, 150 }, new RangedFloat(0.0f, 0.125f))
                .SetDefaultDirection(0))

            //.AddComponent(new CState_JumpMotor(new float[] { 275, -100}, new RangedFloat(0.325f, 0.45f))
            //    .SetDefaultDirection(0))

            .AddComponent(new CState_Motor(new Vector2(0, 275.0f), new RangedFloat(0.325f, 0.45f)))
            .AddComponent(new CState_AutoTransition(idle, 1f))

            .AddTransition(pull, new RangedFloat(0.6f), new AnimationTransitionData(0.25f, 0.0f))
            .AddTransition(slash, new RangedFloat(0.6f), new AnimationTransitionData(0.2f, 0.0f))
        ;*/
    }
}

