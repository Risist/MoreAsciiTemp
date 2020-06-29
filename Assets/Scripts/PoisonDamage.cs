using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonDamage : MonoBehaviour
{
    public Timer tDeal;
    public DamageData damage;
    HealthController healthController;

    void Start()
    {
        healthController = GetComponentInParent<HealthController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (tDeal.IsReadyRestart())
        {
            damage.position = transform.position;
            healthController.DealDamage(damage);
        }
    }
}
