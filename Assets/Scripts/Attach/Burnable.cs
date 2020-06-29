using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burnable : MonoBehaviour
{
    public Transform[] burnTargets;
    public BurnOnCollision[] burnSpread;
    [Range(0, 10)] public float burnChanceModificator = 1.0f;
    [Range(0, 10)] public float fireLifetimeModificator = 1.0f;
    public float fireSpawnDistance = 0.5f;
    public bool fireDealsDamage = true;

    public Transform GetRandomBurnTarget()
    {
        return burnTargets[Random.Range(0, burnTargets.Length)];
    }
    public void ActivateBurnSpread()
    {
        foreach(var it in burnSpread)
        {
            it.enabled = false;
            it.enabled = true;
        }
    }

    public Vector2 GetPositionOffset()
    {
        return Quaternion.Euler(0, 0, Random.value * 360) * Vector3.up * fireSpawnDistance * Random.value;
    }
    public Quaternion GetRotationOffset()
    {
        return Quaternion.Euler(0, 0, Random.value * 360);
    }
}
