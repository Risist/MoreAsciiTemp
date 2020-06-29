using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideComplex : MonoBehaviour
{
    [Header("Close")]
    [Range(0, 1)] public float desiredVisibilityClose;
    [Range(0, 1)] public float desiredVisibilityCloseUi;


    [Header("Away")]
    [Range(0, 1)] public float desiredVisibilityAway;
    [Range(0, 1)] public float desiredVisibilityAwayUi;


    int playerCount = 0;
    public void PlayerEnter()
    {
        ++playerCount;
    }
    public void PlayerExit()
    {
        --playerCount;
    }

    public float GetDesiredVisibiity()
    {
        return playerCount > 0 ? desiredVisibilityClose : desiredVisibilityAway;
    }
    public float GetDesiredVisibiityUi()
    {
        return playerCount > 0 ? desiredVisibilityCloseUi : desiredVisibilityAwayUi;
    }

}
