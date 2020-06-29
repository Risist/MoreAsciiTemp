using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGenerator : MonoBehaviour
{
    [Header ("Tree")]
    public GameObject[] treePrefabs;
    public float maxTreeRotation = 30.0f;
    public Vector3 treeRotation = new Vector3(-75, 0, 0);
    
    [Header("Root")]
    public GameObject rootPrefab;
    public float rootHeight;
    public float rootOffset;
    [Range(0, 1)] public float maxRootDifference;
    public RangedInt nRoots;

    [ContextMenu("Spawn")]
    public void Spawn()
    {
        int treeId = Random.Range(0,treePrefabs.Length);
        GameObject treePrefab = treePrefabs[treeId];


        var tree = Instantiate(treePrefab, transform);
        tree.transform.position = transform.position;
        tree.transform.rotation = transform.rotation * Quaternion.Euler(0,0, Random.Range(-maxTreeRotation, maxTreeRotation)) * Quaternion.Euler(treeRotation);


        int nRoots = this.nRoots.GetRandom();
        float initialRootRotation = Random.value * 360;
        float angleRange = 360.0f / nRoots;
        for(int i = 0; i < nRoots; ++i)
        {
            var root = Instantiate(rootPrefab, transform);

            float angle = initialRootRotation + angleRange * i + angleRange * Random.Range(-maxRootDifference, maxRootDifference);
            root.transform.rotation = Quaternion.Euler(0, 0, angle);

            root.transform.position = transform.position + root.transform.rotation*Vector3.up * rootOffset + Vector3.forward*rootHeight;
        }

    }


    // Start is called before the first frame update
    void Start()
    {
        Spawn();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(101, 67, 33);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.DrawLine(Vector3.zero, Vector3.up);
    }
}
