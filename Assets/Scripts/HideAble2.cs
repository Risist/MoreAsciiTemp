using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideAble2 : MonoBehaviour
{
    VisibilityManager visibilityManager;
    CharacterUiIndicator indicator;
    HealthDisplayer healthDisplayer;

    int hidespotCount = 0;
    public void Show() 
    { 
        --hidespotCount;
        if (hidespotCount <= 0)
        {
            SetVisibility(1.0f);
            SetVisibilityUi(1.0f);
        }
    }
    public void Hide() 
    { 
        ++hidespotCount;
    }

    public void SetVisibility(float s)
    {
        visibilityManager.visibilityLevel = s;
    }
    public void SetVisibilityUi(float s)
    {
        indicator.externalVisibility = s;
        healthDisplayer.externalVisibility = s;
    }

    private void Start()
    {
        visibilityManager = GetComponentInChildren<VisibilityManager>();
        indicator = GetComponentInChildren<CharacterUiIndicator>();
        healthDisplayer = GetComponentInChildren<HealthDisplayer>();
    }
}
