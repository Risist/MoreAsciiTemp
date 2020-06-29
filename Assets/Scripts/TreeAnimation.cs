using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeAnimation : MonoBehaviour
{
    [Header("Animation")]
    [Range(0,1)] public float rotationLerp = 0.05f;
    public Vector3 rotationStrength;
    public RangedFloat animationPlaybackSpeedRange = new RangedFloat(1.0f,1.0f);

    [Header("DamageReceiving")]
    [Range(0,1)] public float rotationDamping = 0.1f;
    Vector3 damageOffset;



    Quaternion initialRotation;
    float animationPlaybackSpeed;

    private void Start()
    {
        initialRotation = transform.rotation;
        animationPlaybackSpeed = animationPlaybackSpeedRange.GetRandom();

        var health = GetComponent<HealthController>();
        if (health)
            health.onDamageCallback += (DamageData data) => 
            {
                damageOffset += Random.insideUnitSphere * 1f * data.damage;
            };
    }
    private void LateUpdate()
    {
        Vector3 desiredRot = initialRotation.eulerAngles + rotationStrength * Mathf.Sin(Time.time * animationPlaybackSpeed) + damageOffset;

        Vector3 v = Vector3.Lerp(transform.eulerAngles, desiredRot, rotationLerp);
        transform.rotation = Quaternion.Euler(v);
    }
    private void FixedUpdate()
    {
        damageOffset *= rotationDamping;
    }
}
