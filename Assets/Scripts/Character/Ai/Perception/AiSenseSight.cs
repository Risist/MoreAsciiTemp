using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AiSenseSight : AiSenseBase
{
    public float predictionScale = 1f;
    [Space]
    public int nSearch = 1;
    [Space]
    public float coneRadius = 170.0f;
    public float searchDistance = 5.0f;
    public float addictionalRotation = 0.0f;


    void Update()
    {
        for (int i = 0; i < nSearch; ++i)
            PerformSearch();
    }
    public class CompareRays : Comparer<RaycastHit2D>
    {
        public override int Compare(RaycastHit2D item1, RaycastHit2D item2)
        {
            return item1.distance.CompareTo(item2.distance);
        }
    }
    static CompareRays compareRays = new CompareRays();
    static RaycastHit2D[] rays = new RaycastHit2D[50];
    public void PerformSearch()
    {
        AiFraction myFraction = myUnit.fraction;
        if (!myFraction)
            return; /// no point in recording enemy && ally



        float angleOffset = coneRadius * Random.value;

        int nRays = Physics2D.RaycastNonAlloc(transform.position, Quaternion.Euler(0, 0, -coneRadius * 0.5f + angleOffset + addictionalRotation) * transform.up, rays, searchDistance);
        //Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, -coneRadius * 0.5f + angleOffset + addictionalRotation) * transform.up * searchDistance, Color.green, 0.25f);

        var rayList = new List<RaycastHit2D>(rays);
        rayList.Sort(0, nRays, compareRays);

        float maxDist = searchDistance;
        float minTransparency = 1.0f;

        for (int i = 0; i < nRays; ++i)
        {
            var it = rayList[i];
            if (it.collider)
            {
                var unit = it.collider.GetComponent<AiPerceiveUnit>();
                if (unit && unit != myUnit)
                {

                    if (it.distance > searchDistance * Mathf.Min(unit.distanceModificator, minTransparency) )
                    {
                        maxDist = it.distance;
                        break;
                    }
                    minTransparency = Mathf.Min( minTransparency, unit.transparencyLevel);
                    //minTransparency *= unit.transparencyLevel;

                    var fraction = unit.fraction;
                    if (!fraction)
                    {
                        if (unit.blocksVision)
                        {
                            maxDist = it.distance;
                            break;
                        }

                        continue;
                    }

                    var eventType = unit.hidespot ? EMemoryEvent.EHideSpot : ConvertAttitudeToEventType(myFraction.GetAttitude(fraction));
                    if (eventType == EMemoryEvent.ECount)
                    { /// cant convert
                        if (unit.blocksVision)
                        {
                            maxDist = it.distance;
                            break;
                        }

                        continue;
                    }

                    //Debug.Log("insert: " + unit + "; distMod = " + unit.distanceModificator + "dist = " + it.distance);
                    memory.InsertToMemory(unit, eventType,
                        unit.transform.position, predictionScale);
                        
                    if (unit.blocksVision)
                    {
                        maxDist = it.distance;
                        break;
                    }
                }
            }
        }
        Debug.DrawRay(transform.position,
            Quaternion.Euler(0, 0, -coneRadius * 0.5f + angleOffset + addictionalRotation) * transform.up * maxDist,
            Color.green, 0.25f);

    }


    EMemoryEvent ConvertAttitudeToEventType(AiFraction.Attitude attitude)
    {
        switch (attitude)
        {
            case AiFraction.Attitude.enemy: return EMemoryEvent.EEnemy;
            case AiFraction.Attitude.friendly: return EMemoryEvent.EAlly;
            /// no way to convert
            default: return EMemoryEvent.ECount;
        }
    }

}
