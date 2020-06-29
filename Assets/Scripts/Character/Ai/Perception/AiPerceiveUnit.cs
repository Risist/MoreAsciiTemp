using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Marker for objects captured by perception system
 */
public class AiPerceiveUnit : MonoBehaviour
{
    public static List<AiPerceiveUnit> memoriableUnits = new List<AiPerceiveUnit>();

    /// modifies how far the agents will perceive this unit
	public float distanceModificator = 1.0f;
    public float transparencyLevel = 1.0f;
    public bool blocksVision = true;
    public bool memoriable = true;
    public bool hidespot = false;

    public AiFraction fraction;
    

    private void OnEnable()
    {
        if(memoriable)
            memoriableUnits.Add(this);
    }
    private void OnDisable()
    {
        if(memoriable)
            memoriableUnits.Remove(this);
    }
}
