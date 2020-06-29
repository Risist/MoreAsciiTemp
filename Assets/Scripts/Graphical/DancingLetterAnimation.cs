using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DancingLetterAnimation : MonoBehaviour
{
    public float positionLerpFactor = 0.1f;
    public float positionOscilation;

    public float rotationLerpFactor = 0.1f;
    public float rotationOscilation;

    Vector3 initialPosition;
    //Vector3 rotation;
    float initialRotation;



    private void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localEulerAngles.z;
    }

    private void LateUpdate()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition + new Vector3(positionOscilation, positionOscilation) * Random.Range(-1.0f,1.0f), positionLerpFactor);
        float desiredRotation = initialRotation + Random.Range(-1.0f, 1.0f) * rotationOscilation;
        transform.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(transform.localEulerAngles.z, desiredRotation, rotationLerpFactor));
    }
}
