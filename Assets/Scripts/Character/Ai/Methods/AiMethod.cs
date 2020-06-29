using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ai;
using Pathfinding;

namespace Ai.Method
{
    #region Methods
    public delegate Vector2 VectorMethod();
    public delegate Transform TargetMethod();
    public delegate bool BoolMethod();
    public delegate float FloatMethod();

    public class MethodBase
    {
        #region References
        public AiBehaviourController controller;

        public CharacterStateController character { get { return controller.character; } }
        public InputHolder inputHolder { get { return controller.inputHolder; } }
        public AiPerceptionHolder memory { get { return controller.memory; } }
        public Transform transform { get { return controller.transform; } }
        public HealthController health { get { return controller.health; } }
        public Seeker seeker { get { return controller.seeker; } }
        public CharacterUiIndicator indicator { get { return controller.indicator; } }

        #endregion References

        #region Events
        public virtual void Begin() { }
        public virtual void Update() { }
        #endregion Events 
    }
    public class MethodFocus : MethodBase
    {
        public MethodFocus(AiFocusBase focus = null)
        {
            this.focus = focus;
        }
        #region References
        public AiFocusBase focus;
        // Todo change Transform to MemoryEvent
        public MemoryEvent target { get { return focus.GetTarget(); } }
        #endregion References
    }

    #endregion Methods

    public class ToTarget : MethodFocus
    {
        public ToTarget(AiFocusBase focus) : base(focus)
        {
        }
        public Vector2 GetVectorToTarget()
        {
            return (Vector2)target.position - (Vector2)transform.position;
        }
    }
    /*public class InsideRange : ToTarget
    {
        public InsideRange(float min = 4.0f, float max = 7.0f)
        {
            range = new RangedFloat(min, max);
        }
        public InsideRange(RangedFloat range)
        {
            this.range = range;
        }
        public RangedFloat range;

        public bool IsInRange()
        {
            Vector2 toTarget = target.position - (Vector2)transform.position;
            float distanceSq = toTarget.sqrMagnitude;
            return distanceSq > range.min * range.min && distanceSq < range.max * range.max;
        }
        public bool IsInRange(float min, float max)
        {
            Vector2 toTarget = target.position - (Vector2)transform.position;
            float distanceSq = toTarget.sqrMagnitude;
            return distanceSq > min * min && distanceSq < max * max;
        }
        public Vector2 StayInRange()
        {
            Vector2 toTarget = target.position - (Vector2)transform.position;
            float distanceSq = toTarget.sqrMagnitude;

            if (distanceSq < range.min * range.min)
            {
                return -toTarget;
            }
            else if (distanceSq > range.max * range.max)
            {
                return toTarget;
            }
            return Vector2.zero;
        }

    }*/


    public class MoveToDestination : ToTarget
    {
        public MoveToDestination(AiFocusBase focus) : base(focus)
        {
        }

        public Vector2 destination;
        public Vector2 toDestination { get { return destination - (Vector2)transform.position; } }

        #region New Target
        /// moves to destination in area around target (max in search area)
        /// if target is null treat yourself as target
        /// target position is predicted in future, You can adjust the prediction via @maxRespectedTime and @timeScale 
        /// @maxRespectedTime -> clamps how much in future the event will be predicted
        /// @timeScale -> specifies how important prediction is 
        public void SetNewDestination_Search(float searchArea, float maxRespectedTime = float.MaxValue, float timeScale = 1.0f)
        {
            if (target == null)
            {
                destination = (Vector2)transform.position + Random.insideUnitCircle * searchArea;
                return;
            }

            destination = (Vector2)target.GetPosition(maxRespectedTime, timeScale) + Random.insideUnitCircle * searchArea;
        }
        public void SetNewDestination_Flee(RangedFloat desiredDistance, float searchArea = 1.0f)
        {
            if (target == null)
            {
                destination = (Vector2)transform.position + Random.insideUnitCircle.normalized * desiredDistance.GetRandom() + Random.insideUnitCircle * searchArea;
                return;
            }

            Vector2 toTarget = -focus.ToTarget();
            destination = (Vector2)transform.position + toTarget.normalized * desiredDistance.GetRandom() + Random.insideUnitCircle * searchArea;
        }



        #endregion New Target

        #region Utility
        public bool IsCloseToDestination(float closeDistance)
        {
            return toDestination.sqrMagnitude < closeDistance * closeDistance;
        }
        #endregion Utility
    }

    public class MoveToDestinationNavigation : MoveToDestination
    {
        public MoveToDestinationNavigation(AiFocusBase focus) : base(focus)
        {
        }

        Path path;
        int nodeId;

        #region New Target
        /// moves to destination in area around target (max in search area)
        /// if target is null treat yourself as target
        /// target position is predicted in future, You can adjust the prediction via @maxRespectedTime and @timeScale 
        /// @maxRespectedTime -> clamps how much in future the event will be predicted
        /// @timeScale -> specifies how important prediction is 
        new public void SetNewDestination_Search(float searchArea, float maxRespectedTime = float.MaxValue, float timeScale = 1.0f)
        {
            base.SetNewDestination_Search(searchArea, maxRespectedTime, timeScale);
            seeker.StartPath(transform.position, destination, (Path p) => { path = p; });

            nodeId = 0;
        }
        new public void SetNewDestination_Flee(RangedFloat desiredDistance, float searchArea = 1.0f)
        {
            base.SetNewDestination_Flee(desiredDistance, searchArea);
            seeker.StartPath(transform.position, destination, (Path p) => { path = p; });

            nodeId = 0;
        }
        public void SetNewDestination(Vector2 d)
        {
            destination = d;
            seeker.StartPath(transform.position, destination, (Path p) => { path = p; });

            nodeId = 0;
        }




        #endregion New Target

        public Vector2 ToDestination(float closeDist = 1.5f, float nextNodeDist = 0.5f) {
            
            if (IsCloseToDestination(closeDist))
                return Vector2.zero;

            
            if (path != null && path.IsDone())
            {
                if(nodeId < path.vectorPath.Count)
                {
                    Vector2 toDestination = path.vectorPath[nodeId] - transform.position;
                    
                    if (toDestination.sqrMagnitude < nextNodeDist * nextNodeDist)
                        ++nodeId;

                    return toDestination;
                }

                return Vector2.zero;
            }

            return toDestination;
        }
    }

    /*Stright direction towards position around target
     */
    public class Search : ToTarget
    {
        public Search(AiFocusBase focus, float searchArea = 5.0f) : base(focus)
        {
            this.searchArea = searchArea;
        }
        public float searchArea;
        Vector2 destination;

        #region Events
        public override void Begin()
        {
            SetNewTarget();
        }
        #endregion Events

        #region Public Functions
        public void SetNewTarget(float searchArea)
        {
            if (target == null)
            {
                destination = (Vector2)transform.position + Random.insideUnitCircle * searchArea;
                return;
            }

            destination = (Vector2)target.position + Random.insideUnitCircle * searchArea;
        }
        public void SetNewTarget()
        {
            SetNewTarget(searchArea);
        }

        public Vector2 GetDestination()
        {
            return destination;
        }
        public Vector2 GetDirectionLength()
        {
            return destination - (Vector2)transform.position;
        }
        public Vector2 GetDirection()
        {
            return GetDirectionLength().normalized;
        }
        public float GetDistanceToDestination()
        {
            return GetDirectionLength().magnitude;
        }
        public bool IsCloseToDestination(float closeDistance)
        {
            return GetDirectionLength().sqrMagnitude < closeDistance * closeDistance;
        }
        #endregion Public Functions
    }

    public class LookAround : MethodBase
    {
        public LookAround() { }
        public LookAround(RangedFloat tChangeAngle) {
            this.tChangeAngle = tChangeAngle;
        }
        public LookAround(RangedFloat tChangeAngle, RangedFloat desiredAngleDifference)
        {
            this.tChangeAngle = tChangeAngle;
            this.desiredAngleDifference = desiredAngleDifference;
        }

        public float rotationLerp;
        public RangedFloat desiredAngleDifference = new RangedFloat(45.0f, 120.0f);
        public RangedFloat tChangeAngle = new RangedFloat(1.0f, 1.0f);

        Timer tChangeDesiredAngle = new Timer();
        float desiredAngle;
        float angle;

        #region Events
        public override void Begin()
        {
            SetNewDesiredAngle();
            tChangeDesiredAngle.cd = Random.Range(tChangeAngle.min, tChangeAngle.max);
            tChangeDesiredAngle.Restart();

            angle = transform.eulerAngles.z;
        }
        public override void Update()
        {
            
        }
        #endregion Events

        #region Public functions
        public void SetNewDesiredAngle()
        {
            float diff = Random.Range(desiredAngleDifference.min, desiredAngleDifference.max);
            diff = Random.value > 0.5f ? diff : -diff;

            desiredAngle = transform.rotation.eulerAngles.z + diff;
        }
        public Vector2 GetRotationInput(float rotationLerp = 0.1f)
        {
            if (tChangeDesiredAngle.IsReadyRestart())
            {
                SetNewDesiredAngle();
                tChangeDesiredAngle.cd = Random.Range(tChangeAngle.min, tChangeAngle.max);
            }

            angle = Mathf.LerpAngle(angle, desiredAngle, rotationLerp);
            Vector2 desiredRotationInput = Quaternion.Euler(0, 0, angle) * Vector2.up;
            return desiredRotationInput;
        }

        public bool CloseEnoughToDediredAngle(float minAngleDifference = 0.0f)
        {
            return Mathf.Abs(Mathf.DeltaAngle(transform.rotation.eulerAngles.z, desiredAngle)) < minAngleDifference;
        }
        #endregion Public functions
    }

    public class ResetInput : MethodBase
    {
        public ResetInput(bool position = true, bool direction = true, bool rotation = true, bool keys = true)
        {
            this.position = position;
            this.rotation = rotation;
            this.direction = direction;
            this.keys = keys;
        }
        public bool position;
        public bool rotation;
        public bool direction;
        public bool keys;

        public override void Begin()
        {
            if (position)
                inputHolder.positionInput = Vector2.zero;
            if (rotation)
                inputHolder.rotationInput = Vector2.zero;
            if (direction)
                inputHolder.directionInput = Vector2.zero;

            if (keys)
                for (int i = 0; i < inputHolder.keys.Length; ++i)
                    inputHolder.keys[i] = false;
        }
    }

    public class CharacterStateChangeDetector : MethodBase
    {

        CharacterState state;
        int i;
        public override void Begin()
        {
            state = character.currentState;
            i = 5;
        }

        public bool HasChanged()
        {
            return --i <= 0 && state != character.currentState;
        }
    }


    
}