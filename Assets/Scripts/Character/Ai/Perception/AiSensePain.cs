using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class AiSensePain : AiSenseBase
{

    new private void Start()
    {
        base.Start();
        var healthController = GetComponentInParent<HealthController>();
        healthController.onDamageCallback += (DamageData data) =>
        {
            memory.InsertToMemory(EMemoryEvent.EPain, data.position, Vector2.zero, Vector2.zero);
        };
    }
}
