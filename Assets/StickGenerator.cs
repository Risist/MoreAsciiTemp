using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickGenerator : MonoBehaviour
{
    public GameObject[] stickPrefabs;
    public RangedInt nSticks;
    public float radius;


    [ContextMenu("Spawn")]
    public void Spawn()
    {
        int nStick = nSticks.GetRandom();

        for(int i = 0; i < nStick; ++i)
        {
            int stickId = Random.Range(0, stickPrefabs.Length);
            GameObject stickPrefab = stickPrefabs[stickId];

            var stick = Instantiate(stickPrefab, transform);
            stick.transform.localPosition = Random.insideUnitCircle*radius;
            stick.transform.rotation = Quaternion.Euler(0,0, Random.value*360);
        }

    }

    private void Start()
    {
        Spawn();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawSphere(Vector3.zero, radius*2);
        Gizmos.DrawLine(Vector3.zero, Vector3.up);
    }
}
