using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTransition : MonoBehaviour
{
    public bool enabled = true;
    [Space]
    public GameObject currentMap;
    public GameObject nextPrefab;
    public Transform spawnPosition;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(enabled && collision.CompareTag("Player"))
        {
            Instantiate(nextPrefab, spawnPosition.position, Quaternion.identity);
            Destroy(currentMap);
        }
    }
}
