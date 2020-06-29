using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VisibilityManager))]
public class VisibilityOnPlayerClose : MonoBehaviour
{
    public float desiredDistanceFromPlayer;
    VisibilityManager visibility;

    private void Start()
    {
        visibility = GetComponent<VisibilityManager>();
    }
    private void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player)
        {
            Vector2 toPlayer = transform.position - player.transform.position;
            float ddistSq = desiredDistanceFromPlayer * desiredDistanceFromPlayer;
            float toPlayerDistSq = toPlayer.sqrMagnitude;

            float visibilityScale = toPlayerDistSq / ddistSq;
            visibility.visibilityLevel = toPlayerDistSq < ddistSq ? 1.0f - Mathf.Sqrt(visibilityScale) : 0.0f;
        }
    }
}
