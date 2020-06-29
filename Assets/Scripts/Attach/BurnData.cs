using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;

[CreateAssetMenu(fileName = "BurnData", menuName = "Ris/BurnData")]
public class BurnData : ScriptableObject
{
    public int type;

    [Header("Attach")]
    public GameObject[] attachPrefabsDamage;
    public GameObject[] attachPrefabs;
    [Range(0.0f, 1.0f)] public float attachChance = 0.035f;
    public float attachLifetime = 5f;

    [Header("SpawnTime")]
    [Range(0.0f, 1.0f)] public float spawnTimeDrag = 0.975f;
    public float minimalSpawnTime = 0.5f;
    public float spawnTimeIncrease = 0.1f;

    public GameObject GetPrefabToSpawn()
    {
        return attachPrefabs[Random.Range(0, attachPrefabs.Length)];
    }
    public GameObject GetPrefabToSpawnDamage()
    {
        return attachPrefabsDamage[Random.Range(0, attachPrefabsDamage.Length)];
    }

}
