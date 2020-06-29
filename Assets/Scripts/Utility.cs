using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RangedFloat
{
    public RangedFloat(float min = float.NegativeInfinity, float max = float.PositiveInfinity)
    {
        this.min = min;
        this.max = max;
    }
    public float min;
    public float max;

    public bool InRange(float value)
    {
        return value >= min && value <= max;
    }
    public float GetRandom()
    {
        return Random.Range(min, max);
    }
}

[System.Serializable]
public class RangedInt
{
    public RangedInt(int min = int.MinValue, int max = int.MaxValue)
    {
        this.min = min;
        this.max = max;
    }
    public int min;
    public int max;

    public bool InRange(int value)
    {
        return value >= min && value <= max;
    }
    public int GetRandom()
    {
        return Random.Range(min, max);
    }
}
