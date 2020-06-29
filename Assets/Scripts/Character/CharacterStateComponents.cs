using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Character
{
    public abstract class StateComponent
    {
        #region References
        public CharacterState parent;
        public CharacterStateController stateController { get { return parent.stateController; } }
        public Rigidbody2D       rigidbody   { get { return parent.rigidbody; } }
        public RigidbodyMovement movement    { get { return parent.movement; } }
        public InputHolder       inputHolder { get { return parent.inputHolder; } }
        public Animator          animator    { get { return parent.animator; } }
        public HealthController  health     { get { return parent.health; } }

        #endregion References
        public virtual void Init() { }

        public virtual void Enter() { }
        public virtual void Update(float animationTime) { }
        public virtual void FixedUpdate(float animationTime) { }
        public virtual void Exit() { }

        /// specifies if character has will to use given transition
        /// at least one component has to agree
        public virtual bool IsPressed() { return true; }
        /// specifies possibility of transition to parent state
        /// every component has to agree
        public virtual bool CanEnter() { return true; }
    }




    public class CState_Input : StateComponent
    {
        public CState_Input(int inputId)
        {
            this.inputId = inputId;
        }
        public int inputId;
        public override bool IsPressed()
        {
            return inputHolder.keys[inputId];
        }
    }
    public class CState_AutoTransition : StateComponent
    {
        public CState_AutoTransition(CharacterState target, float transitionTime = 1f, float transitionDuration = 0f, float transitionOffset = 0f)
        {
            this.target = target;
            transitionData.normalizedOffset = transitionOffset;
            transitionData.transitionDuration = transitionDuration;
            this.transitionTime = transitionTime;
        }
        public CharacterState target;
        public float transitionTime;
        public AnimationTransitionData transitionData;
        public override void Update(float animationTime)
        {
            if (!animator.IsInTransition(0) && animationTime >= transitionTime)
            {
                stateController.SetCurrentState(target, transitionData);
            }
        }
    }

    public class CState_RandomEnter : StateComponent
    {
        public CState_RandomEnter(float chance)
        {
            this.chance = chance;
        }
        public float chance;

        public override bool CanEnter() { return Random.value > chance; }
    }

    public class DamagedRecorder
    {
        public DamagedRecorder(HealthController health)
        {
            health.onStaggerCallback += (DamageData data) =>
            {
                damaged = true;
            };
        }
        public bool damaged = false;
    }
    public class CState_StaggerCondition : StateComponent
    {
        public CState_StaggerCondition(DamagedRecorder damaged = null)
        {
            this.damaged = damaged;
        }
        public DamagedRecorder damaged;

        public override void Init() 
        {
            if (damaged == null)
                damaged = new DamagedRecorder(health);
        }
        public override bool CanEnter() { return damaged.damaged; }
        public override void Enter() { damaged.damaged = false; }
        public override void Exit()
        {
            damaged.damaged = false;
        }
    }

    public class CState_RotationToDirection : StateComponent
    {
        public CState_RotationToDirection(float lerpScale = 0.3f, float trackFactor = 1f)
        {
            this.rotationSpeed = lerpScale;
            this.trackFactor = trackFactor;
            this.applicationPeriod = new RangedFloat();
        }
        public CState_RotationToDirection(RangedFloat applicationPeriod, float lerpScale = 0.3f, float trackFactor = 1f)
        {
            this.rotationSpeed = lerpScale;
            this.trackFactor = trackFactor;
            this.applicationPeriod = applicationPeriod;
        }

        public RangedFloat applicationPeriod;
        public float rotationSpeed;
        public float trackFactor = 0f;
        Vector2 destinationDirection;

        public override void Enter()
        {
            destinationDirection = inputHolder.directionInput;
        }
        public override void FixedUpdate(float animationTime)
        {
            Vector2 destination = Vector2.Lerp(inputHolder.directionInput, destinationDirection, trackFactor);

            movement.atExternalRotation = false;
            if (applicationPeriod.InRange(animationTime))
                movement.ApplyExternalRotation(destination, rotationSpeed);
        }
        public override void Exit()
        {
            movement.atExternalRotation = false;
        }
    }
    /*public class CState_RotationToDirection2 : StateComponent
    {
        public CState_RotationToDirection2(float lerpScale = 0.3f, float trackFactor = 1f)
        {
            this.rotationSpeed = lerpScale;
            this.trackFactor = trackFactor;
            this.applicationPeriod = new RangedFloat();
        }
        public CState_RotationToDirection2(RangedFloat applicationPeriod, float lerpScale = 0.3f, float trackFactor = 1f)
        {
            this.rotationSpeed = lerpScale;
            this.trackFactor = trackFactor;
            this.applicationPeriod = applicationPeriod;
        }

        public RangedFloat applicationPeriod;
        public float rotationSpeed;
        public float trackFactor = 0f;

        Vector2 destinationDirection;

        public override void Enter()
        {
            initialDestinationAngle = Vector2.SignedAngle(Vector3.up, inputHolder.directionInput);
        }
        public override void FixedUpdate(float animationTime)
        {
            float angle = Vector2.SignedAngle(Vector3.up, inputHolder.directionInput);
            float destinationAngle = Mathf.Lerp(angle, initialDestinationAngle, trackFactor);
            Vector2 destination = Quaternion.Euler(0, 0, destinationAngle) * Vector2.up;

            movement.atExternalRotation = false;
            if (applicationPeriod.InRange(animationTime))
                movement.ApplyExternalRotation(destination, rotationSpeed);
        }
        public override void Exit()
        {
            movement.atExternalRotation = false;
        }
    }*/
    public class CState_Cd : StateComponent
    {
        public CState_Cd(int cdId, EMode mode = EMode.EAll)
        {
            this.cdId = cdId;
            this.mode = mode;
        }
        public enum EMode
        {
            EAll,
            ERestartOnly,
            EConditionOnly,
        }

        public int cdId;
        public EMode mode;

        public override bool CanEnter()
        {
            if (mode != EMode.EConditionOnly)
                return stateController.IsCdReady(cdId);
            return true;
        }

        /*public override void Update(float animationTime)
        {
            if (mode != EMode.EConditionOnly)
                stateController.RestartCd(cdId);
        }*/
        public override void Exit()
        {
            if (mode != EMode.EConditionOnly)
                stateController.RestartCd(cdId);
        }
    }

    public class CState_Motor : StateComponent
    {
        public CState_Motor(Vector2 movementSpeed)
        {
            this.movementSpeed = movementSpeed;
            this.applicationPeriod = new RangedFloat();
        }
        public CState_Motor(Vector2 movementSpeed, RangedFloat applicationPeriod)
        {
            this.movementSpeed = movementSpeed;
            this.applicationPeriod = applicationPeriod;
        }
        public Vector2 movementSpeed;
        public RangedFloat applicationPeriod;
        Vector2 initialDir;

        public override void Enter()
        {
            initialDir = new Vector2(inputHolder.directionInput.x, inputHolder.directionInput.y).normalized;
        }
        public override void FixedUpdate(float animationTime)
        {
            if (applicationPeriod.InRange(animationTime))
            {
                rigidbody.AddForce(
                    initialDir * movementSpeed.y +
                    new Vector2(initialDir.y, -initialDir.x) * movementSpeed.x);
            }
        }
    }

    /// like motor applies force at specified part of animation
    /// However this time the direction of movement is choosen by keyboard press to emphasize player direction of move
    public class CState_JumpMotor : StateComponent
    {
        public CState_JumpMotor(float movementSpeed, float minDotValue = -1f)
        {
            this.movementSpeed = new float[] { movementSpeed, movementSpeed, movementSpeed, movementSpeed };
            this.applicationPeriod = new RangedFloat();
            this.minDotValue = minDotValue;
        }
        public CState_JumpMotor(float movementSpeed, RangedFloat applicationPeriod, float minDotValue = -1f)
        {
            this.movementSpeed = new float[] { movementSpeed, movementSpeed, movementSpeed, movementSpeed };
            this.applicationPeriod = applicationPeriod;
            this.minDotValue = minDotValue;
        }
        public CState_JumpMotor(float[] movementSpeed, float minDotValue = -1f)
        {
            Debug.Assert(movementSpeed.Length <= 4);
            this.movementSpeed = movementSpeed;
            this.applicationPeriod = new RangedFloat();
            this.minDotValue = minDotValue;
        }
        public CState_JumpMotor(float[] movementSpeed, RangedFloat applicationPeriod, float minDotValue = -1f)
        {
            Debug.Assert(movementSpeed.Length <= 4);
            this.movementSpeed = movementSpeed;
            this.applicationPeriod = applicationPeriod;
            this.minDotValue = minDotValue;
        }

        public float[] movementSpeed;
        public RangedFloat applicationPeriod;
        public float minDotValue;
        public int defaultDirection = -1;
        Vector2 initialInputDirection;

        public int currentDirectionId;

        public CState_JumpMotor SetDefaultDirection(int d)
        {
            defaultDirection = d;
            return this;
        }

        public override void Enter()
        {


            Vector2 directionInput = inputHolder.directionInput.normalized;
            Vector2 positionInput = inputHolder.positionInput.normalized;

            Vector2[] directions = {
                directionInput,
                -directionInput,
                new Vector2(directionInput.y, -directionInput.x),
                new Vector2(-directionInput.y, directionInput.x)
            };


            float maxDot = float.NegativeInfinity;
            initialInputDirection = defaultDirection == -1 ? Vector2.zero : directions[defaultDirection] * movementSpeed[defaultDirection];
            currentDirectionId = defaultDirection;
            if (!inputHolder.atMove)
                return;

            for (int i = 0; i < movementSpeed.Length; ++i)
            {
                var newDot = Vector2.Dot(positionInput, directions[i]);
                if (newDot > minDotValue && newDot > maxDot)
                {
                    maxDot = newDot;
                    initialInputDirection = directions[i] * movementSpeed[i];
                    currentDirectionId = i;
                }
            }
        }
        public override void FixedUpdate(float animationTime)
        {
            if (applicationPeriod.InRange(animationTime))
            {
                rigidbody.AddForce( initialInputDirection );
            }
        }
    }

}