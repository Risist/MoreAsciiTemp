using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EMemoryEvent
{
    EEnemy,
    EAlly,
    ENoise,

    EPain,

    EHideSpot,
    
    ECount
}
public enum EMemoryState
{
    EKnowledge,
    EShade,
    EToRemove,
}

public class MemoryEvent
{
    public MinimalTimer remainedTime;

    /// position of the event source at record time
    public Vector2 exactPosition;
    /// facing direction of the event; Vector.zero if not applicable
    public Vector2 forward;
    /// direction the movement proceeds when perceived
    public Vector2 movementDirection;

    /// unit responsible for this event
    /// if not null remainedTime is refreshed if trying to add new event with the same unit
    /// otherwise always adds the event
    public AiPerceiveUnit unit;
    public bool hadUnit;

    /// distance which target could possibly travel from the start of the event assuming same speed
    public float elapsedDistance
    {
        get
        {
            return movementDirection.magnitude * remainedTime.ElapsedTime();
        }
    }

    /// fully computed position predicted by this event
    /// comes up with uncertainity area, and linear interpolation of current position by direction and speed of the event
    public Vector2 position
    {
        get
        {
            return exactPosition + movementDirection * remainedTime.ElapsedTime();
        }
    }

    public Vector2 GetPosition(float maxRespectedTime = float.MaxValue, float timeScale = 1.0f)
    {
        float time = remainedTime.ElapsedTime();
        time = Mathf.Clamp(time, 0, maxRespectedTime);

        return exactPosition + timeScale * movementDirection * time;
    }
}

public class AiPerceptionHolder : MonoBehaviour
{
    #region Data
    [Range(0, 1)] public float sortDistanceTimeScale = 0.5f;
    [Space]

    /// how long the data will be taken as valid
    public float enemyKnowledgeTime;
    /// how long the data will be kept in memory
    public float enemyShadeTime;


    /// how long the data will be taken as valid
    public float noiseKnowledgeTime;
    /// how long the data will be kept in memory
    public float noiseShadeTime;

    /// how long the data will be taken as valid
    public float painKnowledgeTime;
    /// how long the data will be kept in memory
    public float painShadeTime;




    public Timer tPerformClear;
    #endregion Data

    #region EventMemory
    [System.NonSerialized] protected List<MemoryEvent>[] eventMemory = new List<MemoryEvent>[(int)EMemoryEvent.ECount];
    /// dirty bit kind of optimalisation
    /// sorts memory only when needed
    bool[] anyEventAdded = new bool[(int)EMemoryEvent.ECount];


    public bool InsertToMemory(AiPerceiveUnit unit, EMemoryEvent eventType, Vector2 position, Vector2 direction )
    {
        if (!unit.memoriable)
            return false;

        int id = (int)eventType;
        var mem = eventMemory[id];

        /// search if the unit is recorded in our memory
        /// if so then update it
        if (unit)
            foreach (var itMemory in mem)
                if (itMemory.unit == unit)
                {
                    /// time data
                    itMemory.remainedTime.Restart();

                    /// spatial data
                    itMemory.exactPosition = position;
                    itMemory.movementDirection = direction;
                    itMemory.forward = unit.transform.up;

                    /// list is not sorted
                    anyEventAdded[id] = true;
                    return false;
                }

        /// otherwise insert new item
        MemoryEvent item = new MemoryEvent();
        item.unit = unit;
        item.hadUnit = unit != null;

        /// spatial data
        item.exactPosition = position;
        item.movementDirection = direction;
        item.forward = unit.transform.up;

        /// time data
        item.remainedTime = new Timer();
        item.remainedTime.Restart();
        
        mem.Add(item);
        /// list is not sorted
        anyEventAdded[id] = true;
        return true;
    }
    /// auto compute direction of unit
    public bool InsertToMemory(AiPerceiveUnit unit, EMemoryEvent eventType, Vector2 position, float predictionScale)
    {
        if (!unit.memoriable)
            return false;

        int id = (int)eventType;
        var mem = eventMemory[id];

        /// search if the unit is recorded in our memory
        /// if so then update it
        if (unit)
            foreach (var itMemory in mem)
                if (itMemory.unit == unit)
                {
                    /// spatial data
                    if (itMemory.remainedTime.ElapsedTime() > 3 * float.Epsilon)
                        itMemory.movementDirection = (position - itMemory.exactPosition) * (predictionScale / itMemory.remainedTime.ElapsedTime()); /// auto compute direction
                                                                                                                                            /// else keep last value... dunno what to do in case of such a small time step

                    itMemory.exactPosition = position;
                    itMemory.forward = unit.transform.up;

                    /// time data
                    itMemory.remainedTime.Restart();

                    /// list is not sorted
                    anyEventAdded[id] = true;
                    return false;
                }

        /// otherwise insert new item
        MemoryEvent item = new MemoryEvent();
        item.unit = unit;
        item.hadUnit = unit != null;

        /// spatial data
        item.exactPosition = position;
        item.movementDirection = Vector2.zero;
        item.forward = unit.transform.up;

        /// time data
        item.remainedTime = new Timer();
        item.remainedTime.Restart();
        
        mem.Add(item);
        /// list is not sorted
        anyEventAdded[id] = true;
        return true;
    }
    public void InsertToMemory(EMemoryEvent eventType, Vector2 position, Vector2 direction, Vector2 forward)
    {
        int id = (int)eventType;
        var mem = eventMemory[id];

        MemoryEvent item = new MemoryEvent();
        /// no unit/ unknown unit responsible for the event
        item.unit = null;/// otherwise insert new item
        item.hadUnit = false;

        /// spatial data
        item.exactPosition = position;
        item.movementDirection = direction;
        item.forward = forward;

        /// time data
        item.remainedTime = new Timer();
        item.remainedTime.Restart();

        mem.Add(item);
        /// list is not sorted
        anyEventAdded[id] = true;
    }


    public List<MemoryEvent> GetMemoryEventList(EMemoryEvent eventType)
    {
        return eventMemory[(int)eventType];
    }
    public MemoryEvent SearchInMemory(EMemoryEvent eventType, int objectId = 0)
    {
        int id = (int)eventType;
        if (eventMemory[id].Count < objectId + 1)
            return null;

        if (anyEventAdded[id])
        {
            SortMemory(eventType);
            anyEventAdded[id] = false;
        }

        return eventMemory[id][objectId];
    }
    public MemoryEvent SearchInMemoryFresh(EMemoryEvent eventType, float maxPerceivedTime, int objectId = 0)
    {
        int id = (int)eventType;
        if (eventMemory[id].Count < objectId + 1)
            return null;

        if (anyEventAdded[id])
        {
            SortMemory(eventType);
            anyEventAdded[id] = false;
        }

        var ev = eventMemory[id][objectId];
        return ev.remainedTime.IsReady(maxPerceivedTime) ? null : ev;
    }
    // TODO fix enemy knowledge time to something more general
    public EMemoryState GetEventState(MemoryEvent ev)
    {
        var timer = ev.remainedTime;
        if (!timer.IsReady(enemyKnowledgeTime))
            return EMemoryState.EKnowledge;
        if (!timer.IsReady(enemyShadeTime))
            return EMemoryState.EShade;

        return EMemoryState.EToRemove;
    }

    void SortMemory(EMemoryEvent eventType)
    {
        eventMemory[(int)eventType].Sort(
            delegate (MemoryEvent item1, MemoryEvent item2)
            {
                

                if (item1.remainedTime.IsReady(noiseKnowledgeTime))
                {
                    if (!item2.remainedTime.IsReady(noiseKnowledgeTime))
                        return 1;
                    else
                        return item1.remainedTime.ElapsedTime().CompareTo(item2.remainedTime.ElapsedTime());
                }
                else if (item2.remainedTime.IsReady(noiseKnowledgeTime))
                    return -1;
                


                if (item1.unit != null)
                {
                    if (item2.unit == null)
                        return 1;
                }
                else if (item2.unit != null)
                    return -1;

                float timeUtility = item1.remainedTime.ElapsedTime() - item2.remainedTime.ElapsedTime();

                float item1Distance = ((Vector2)transform.position - item1.exactPosition).sqrMagnitude;
                float item2Distance = ((Vector2)transform.position - item2.exactPosition).sqrMagnitude;
                float distanceUtility = item1Distance - item2Distance;

                float utility = -Mathf.Lerp(distanceUtility, timeUtility, sortDistanceTimeScale);
                return utility.CompareTo(0);/**/
            });
    }
    #endregion EventMemory

    #region Events
    void PerformClear()
    {
        for (int j = 0; j < (int)EMemoryEvent.ECount; ++j)
        {
            var evMem = eventMemory[j];
            for (int i = 0; i < evMem.Count; ++i)
                if (evMem[i].remainedTime.IsReady(enemyShadeTime) ||
                    (evMem[i].hadUnit && evMem[i].unit == null)
                    )
                {
                    evMem.RemoveAt(i);
                    --i;
                }
        }
    }

    private void Awake()
    {
        for (int i = 0; i < (int)EMemoryEvent.ECount; ++i)
            eventMemory[i] = new List<MemoryEvent>();
    }
    private void Update()
    {
        if (tPerformClear.IsReadyRestart())
            PerformClear();
    }
    #endregion Events

}
