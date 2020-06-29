using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideForce : MonoBehaviour
{
    public float force;



    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.attachedRigidbody)
            return;

        Vector2 toCenter = transform.position - collision.transform.position;
        toCenter = new Vector2(-toCenter.y, toCenter.x);

        collision.attachedRigidbody.AddForce(toCenter.normalized * force);
    }
}
