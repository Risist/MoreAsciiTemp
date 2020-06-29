using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HealthController))]
public class DeathEventTreeDestruction : MonoBehaviour
{
    [Range(0,1)]public float animationSpeed = 0.05f;
    public float tolerance = 0.25f;
    Quaternion desiredRotation;

    void Start()
    {
        var healthController = GetComponent<HealthController>();
        healthController.onDeathCallback += DeathEvent;

        enabled = false;
    }



    void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, animationSpeed);
        
        if(Quaternion.Angle(transform.rotation, desiredRotation) < tolerance)
        {
            enabled = false;
            var rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;

            var deathEventPhysicsDestruction = GetComponent<DeathEventPhysicsDestruction>();
            if (deathEventPhysicsDestruction) deathEventPhysicsDestruction.enabled = true;

            var healthController = GetComponent<HealthController>();
            healthController.onDeathCallback -= DeathEvent;
            healthController.Ressurect();

            Destroy(this);
        }
    }


    void DeathEvent(DamageData data)
    {
        enabled = true;

        Vector3 forward = Quaternion.Euler(0, 0, Random.value * 360) * Vector3.up;
        desiredRotation = Quaternion.Euler(0, 0, Random.value * 360);

        var circleColl = GetComponent<CircleCollider2D>();
        if (circleColl)
            Destroy(circleColl);

        var boxColl = GetComponent<BoxCollider2D>();
        if (boxColl)
            boxColl.enabled = true;
    }
}
