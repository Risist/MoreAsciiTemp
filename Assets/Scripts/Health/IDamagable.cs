using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DamageData
{
    public float damage;
    public float staggerIncrease;
    [System.NonSerialized] public Vector3 position;
    [System.NonSerialized] public Vector3 direction;
}

/// interface for all objects that can be damaged
public interface IDamagable
{
    void DealDamage(DamageData data);
}
