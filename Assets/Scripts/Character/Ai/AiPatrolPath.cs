using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPatrolPath : MonoBehaviour
{

    public int GetClosest(Vector2 position)
    {
        float closestDistSq = float.MaxValue;
        int n = transform.childCount;
        int bestId = -1;
        for(int i = 0; i < n; ++i)
        {
            float distSq = ((Vector2)transform.position - position).sqrMagnitude;
            if(distSq < closestDistSq)
            {
                closestDistSq = distSq;
                bestId = i;
            }
        }
        return bestId;
    }

    private void OnDrawGizmos()
    {
        if (transform.childCount < 2)
            return;

        int n = transform.childCount;
        for(int i = 1; i < n; ++i)
        {
            Gizmos.DrawLine(transform.GetChild(i - 1).position, transform.GetChild(i).position);
        }
    }
}
