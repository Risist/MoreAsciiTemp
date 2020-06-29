using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Infectable : MonoBehaviour
{
    [System.NonSerialized] public bool infected;
    private void Start()
    {
        infected = GetComponentInChildren<InputRecorder>();
    }
}
