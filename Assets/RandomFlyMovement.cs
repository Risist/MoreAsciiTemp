using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomFlyMovement : MonoBehaviour
{
    [Range(0, 1)] public float directionChangeFactor = 0.05f;
    public RangedFloat destinationDistance;
    public Timer tChangeDestination;
    public float movementSpeed = 1.0f;
    Vector2 velocity;
    Vector2 destination;
    Vector2 initialPosition;

    void SetNewDestination()
    {
        destination = initialPosition + Random.insideUnitCircle * destinationDistance.GetRandom();
    }

    private void Start()
    {
        initialPosition = transform.position;
        SetNewDestination();
    }
    private void FixedUpdate()
    {
        if (tChangeDestination.IsReadyRestart())
            SetNewDestination();

        

        Vector2 toDestination = destination - (Vector2)transform.position;
        velocity = Vector2.Lerp(velocity, toDestination*movementSpeed, directionChangeFactor);

        transform.position += (Vector3)velocity;
        transform.rotation = Quaternion.Euler(0,0, -Vector2.SignedAngle(velocity, Vector2.up));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(transform.position, 0.45f);
    }
}
