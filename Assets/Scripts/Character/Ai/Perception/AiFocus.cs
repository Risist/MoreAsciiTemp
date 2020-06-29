using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Ai;

/*
 * This module Will describe how attention point will be choosen by ai agent
 */
namespace Ai
{
    public abstract class AiFocusBase : Method.MethodBase
    {
        #region Public Functions
        public abstract MemoryEvent GetTarget(int i = 0);
        public virtual bool HasTarget(int i = 0)
        {
            return GetTarget(i) != null;
        }
        public virtual Vector2 GetTargetPosition(int i = 0)
        {   if (HasTarget(i))
                return GetTarget(i).position;
            else
                return Vector2.zero;
        }
        public Vector2 GetTargetPosition(float[] scales)
        {
            Vector2 sum = Vector2.zero;
            float scalesSum = 0;
            for (int i = 0; i < scales.Length; ++i)
            {
                if (!HasTarget(i))
                    break;

                sum += GetTargetPosition(i) * scales[i];
                scalesSum += scales[i];
            }

            return scalesSum != 0 ? sum / scalesSum : Vector2.zero;
        }

        public bool IsInRange(float min, float max, int i = 0)
        {
            float distanceSq = ToTarget(i).sqrMagnitude;
            return distanceSq > min * min && distanceSq < max * max;
        }
        public bool IsBehindTarget(float tolerance, int i = 0)
        {
            Vector2 toDestination = BehindTargetPosition(i) - (Vector2)transform.position;
            float distanceSq = toDestination.sqrMagnitude;
            return distanceSq < tolerance * tolerance;
        }
        public bool IsCloser(float distance, int i = 0)
        {
            float distanceSq = ToTarget(i).sqrMagnitude;
            return distanceSq < distance * distance;
        }
        public bool IsFuther(float distance, int i = 0)
        {
            float distanceSq = ToTarget(i).sqrMagnitude;
            return distanceSq > distance * distance;
        }

        public Vector2 ToTargetSide(float preference = 0.5f, int i = 0)
        {
            var toTarget = ToTarget(i);
            toTarget = new Vector2(-toTarget.y, toTarget.x);
            return UnityEngine.Random.value > preference ? toTarget : -toTarget;
        }
        public Vector2 ToTarget(int i = 0)
        {
            return GetTargetPosition(i) - (Vector2)transform.position;
        }

        public Vector2 ToTarget(float[] scales)
        {
            Vector2 targetPosition = GetTargetPosition(scales);

            return targetPosition - (Vector2)transform.position;
        }

        public Vector2 StayInRange(float min, float max, int i = 0)
        {
            Vector2 toTarget = ToTarget(i);
            float distanceSq = toTarget.sqrMagnitude;

            if (distanceSq < min * min)
            {
                return -toTarget;
            }
            else if (distanceSq > max * max)
            {
                return toTarget;
            }
            return Vector2.zero;
        }
        public Vector2 StayInRange(RangedFloat range, int i = 0)
        {
            return StayInRange(range.min, range.max, i);
        }
        public Vector2 StayBehindTargetAdaptive(float closeDist = 2.0f, float avoidance = 600, float distanceToTargetScale = 1.0f, int i = 0)
        {
            Vector2 toTarget = ToTarget(i);
            float distanceFromtarget = toTarget.magnitude;
            
            Vector2 destination = GetTargetPosition(i) - GetTarget(i).forward * distanceFromtarget * distanceToTargetScale;
            Vector2 toDestination = destination - (Vector2)transform.position;

            toDestination = toDestination - toTarget.normalized / toTarget.sqrMagnitude * avoidance;
            toDestination = toDestination.sqrMagnitude > closeDist * closeDist ? toDestination : Vector2.zero;

            return toDestination;
        }
        public Vector2 StayBehindTarget(float closeDist = 2.0f, float avoidance = 600, float distanceToTarget = 10.0f, int i = 0)
        {
            Vector2 toTarget = ToTarget(i);
            Vector2 destination = GetTargetPosition(i) - GetTarget(i).forward * distanceToTarget;
            Vector2 toDestination = destination - (Vector2)transform.position;

            toDestination = toDestination - toTarget.normalized / toTarget.sqrMagnitude * avoidance;
            toDestination = toDestination.sqrMagnitude > closeDist * closeDist ? toDestination : Vector2.zero;

            return toDestination;
        }

        public Vector2 BehindTargetPosition(int i = 0)
        {
            Vector2 toTarget = ToTarget(i);
            float distanceFromtarget = toTarget.magnitude;

            return GetTargetPosition(i) - GetTarget(i).forward * distanceFromtarget;
        }
        
        #endregion Public Functions
    }

    public class AiFocusClosest : AiFocusBase
    {
        public AiFocusClosest(EMemoryEvent eventType = EMemoryEvent.EEnemy, EMemoryState memoryState = EMemoryState.EKnowledge)
        {
            this.eventType = eventType;
            this.memoryState = memoryState;
        }
        public EMemoryEvent eventType;
        public EMemoryState memoryState;

        Vector2 lastPosition;

        public override MemoryEvent GetTarget(int i = 0)
        {
            var ev = memory.SearchInMemory(eventType, i);
            return ev != null /*&& memory.GetEventState(ev) == memoryState*/ ? ev : null;
        }
        public override bool HasTarget(int i = 0)
        {
            return memory.SearchInMemory(eventType, i) != null;
        }
        public override Vector2 GetTargetPosition(int i = 0)
        {
            var target = GetTarget(i);
            if(target != null)
            {
                lastPosition = target.position;
            }
            return lastPosition;
        }
    }

    public enum EAttention
    {
        EIdle,
        EPainShade,
        EPain,
        ENoiseShade,
        ENoise,
        EEnemyShade,
        EEnemy,
    }
    public class AiFocusAttention : AiFocusBase
    {
        public MemoryEvent lastEvent { get; private set; }
        public EAttention lastState { get; private set; }
        public override void Update()
        {
            var enemy = memory.SearchInMemory(EMemoryEvent.EEnemy);
            var pain = memory.SearchInMemory(EMemoryEvent.EPain);
            var noise = memory.SearchInMemory(EMemoryEvent.ENoise);


            if (enemy != null && (!enemy.hadUnit || enemy.unit != null))
            {
                if (!enemy.remainedTime.IsReady(memory.enemyKnowledgeTime))
                {
                    SetLast(EAttention.EEnemy, enemy);
                    return;
                }
                else if(pain != null && (!pain.hadUnit || pain.unit != null) && pain.remainedTime.IsReady(memory.painKnowledgeTime) )
                {
                    SetLast(EAttention.EPainShade, pain);
                    return;
                }
                else if (!enemy.remainedTime.IsReady(memory.enemyShadeTime))
                {
                    SetLast(EAttention.EEnemyShade, enemy);
                    return;
                }
            }

            
            if (pain != null && (!pain.hadUnit || pain.unit != null))
            {
                if (!pain.remainedTime.IsReady(memory.painKnowledgeTime))
                {
                    SetLast(EAttention.EPain, pain);
                    return;
                }
            }

            if (noise != null && (!noise.hadUnit || noise.unit != null))
            {
                if (!noise.remainedTime.IsReady(memory.noiseKnowledgeTime))
                {
                    SetLast(EAttention.ENoise, noise);
                    return;
                }
                else if (!noise.remainedTime.IsReady(memory.noiseShadeTime))
                {
                    SetLast(EAttention.ENoiseShade, noise);
                    return;
                }
            }

            if (pain != null && (!pain.hadUnit || pain.unit != null))
            {
                if (!pain.remainedTime.IsReady(memory.painShadeTime))
                {
                    SetLast(EAttention.EPainShade, pain);
                    return;
                }
            }

            SetLast(EAttention.EIdle, null);
        }

        void SetLast(EAttention state, MemoryEvent ev)
        {
            lastEvent = ev;
            lastState = state;

            // danger
            indicator.animationIndicators[1].use =
                state == EAttention.EEnemy ||
                state == EAttention.EPain
                ;

            // unknown
            indicator.animationIndicators[2].use = 
                state == EAttention.EEnemyShade ||
                state == EAttention.ENoise ||
                state == EAttention.ENoiseShade ||
                state == EAttention.EPainShade
                ;
        }

        public override MemoryEvent GetTarget(int i = 0)
        {
            return lastEvent;
        }
    }
}
