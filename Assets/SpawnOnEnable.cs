using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnEnable : MonoBehaviour
{
    public GameObject prefab;
    public bool onEnable = true;
    public bool onDisable = false;

    private void OnEnable()
    {
        if(onEnable)
            Instantiate(prefab, transform.position, transform.rotation);
    }
    private void OnDisable()
    {
        if (onDisable)
            Instantiate(prefab, transform.position, transform.rotation);
    }
}
