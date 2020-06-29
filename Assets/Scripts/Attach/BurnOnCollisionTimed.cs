using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnOnCollisionTimed : BurnOnCollision
{
    public Timer tActive;

    new private void FixedUpdate()
    {
        base.FixedUpdate();
        if (tActive.IsReady())
            enabled = false;
    }

    private void OnEnable()
    {
        tActive.Restart();
    }

}
