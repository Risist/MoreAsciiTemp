using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RandomAcumulatedForce : MonoBehaviour
{
    [Range(0, 1)] public float newForceLerp = 1f;
    public float force = 1.0f;
    Vector2 direction;
    Vector2 direction2;

    private void FixedUpdate()
    {
        Vector2 randomForce = Random.insideUnitCircle;
        direction = Vector2.Lerp(direction, randomForce, newForceLerp);
        direction2 = Vector2.Lerp(direction2, direction, newForceLerp);

        transform.Translate(direction2.normalized * force);
    }
}
