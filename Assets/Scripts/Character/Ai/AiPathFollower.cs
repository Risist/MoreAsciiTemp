using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPathFollower : MonoBehaviour
{
    public AiPatrolPath path;
    private void Start()
    {
        //ResetPath();
    }

    int current;
    int direction;
    public void ResetPath()
    {
        current = path.GetClosest(transform.position);

        if (current == 0)
            direction = 1;
        else if (current == path.transform.childCount - 1)
            direction = -1;
        else
            direction = Random.value > 0.5f ? 1 : -1;
    }
    public Vector2 GetCurrent()
    {
        return path.transform.GetChild(current).position;
    }
    
    public Vector2 GetAroundCurrent(float dist)
    {
        return GetCurrent() + Random.insideUnitCircle * dist;
    }
    
    public Vector2 ToCurrent()
    {
        return GetCurrent() - (Vector2)transform.position;
    }
    public Vector2 FollowPath(float closeDirection)
    {
        Vector2 toCurrent = ToCurrent();
        if(toCurrent.sqrMagnitude < closeDirection * closeDirection)
        {
            Debug.Log("dd: " + current);
            current += direction;
            if(direction == 1 && current == path.transform.childCount)
            {
                direction = -1;
                current += direction;
            }
            else if (direction == -1 && current == -1)
            {
                direction = 1;
                current += direction;
            }
        }
        return ToCurrent();
    }
}