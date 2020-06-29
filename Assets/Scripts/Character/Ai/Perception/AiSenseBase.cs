using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AiSenseBase : MonoBehaviour
{

    [System.NonSerialized]
    public AiPerceptionHolder memory;

    [System.NonSerialized]
    public AiPerceiveUnit myUnit;
    
    protected void Start()
    {
        memory = GetComponent<AiPerceptionHolder>();
        myUnit = GetComponentInParent<AiPerceiveUnit>();
    }
}