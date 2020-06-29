using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AiPerceptionHolder))]
public class AiSenseNoise : MonoBehaviour
{
    public class NoiseData
    {
        public Vector2 position;
        public Vector2 velocity;
        public AiFraction fraction;
    }
    static System.Action<NoiseData> onSpreadNoise = (NoiseData data) => { };
    public static Timer tSpreadNoise = new Timer(0.45f);
    public static bool CanSpreadNoise() { return tSpreadNoise.IsReady(); }
    public static void SpreadNoise(NoiseData data)
    {
        if (tSpreadNoise.IsReadyRestart())
        {
            onSpreadNoise(data);
        }
    }

    [Range(0, 1)] public float reactionChance = 1.0f;
    public float hearingDistance = 1.0f;
    AiPerceptionHolder memory;

    private void OnEnable()
    {
        onSpreadNoise += ReactToNoise;
    }
    private void OnDisable()
    {
        onSpreadNoise -= ReactToNoise;
    }

    void ReactToNoise(NoiseData data)
    {
        AiPerceiveUnit unit = GetComponentInParent<AiPerceiveUnit>();
        if (unit && data.fraction && unit.fraction.GetAttitude(data.fraction) != AiFraction.Attitude.enemy)
            return;

        Vector2 toNoise = (Vector2)transform.position - data.position;
        if (toNoise.sqrMagnitude > hearingDistance * hearingDistance || Random.value > reactionChance)
            return;

        memory.InsertToMemory(EMemoryEvent.ENoise, data.position, data.velocity, data.velocity);
    }

    void Start()
    {
        memory = GetComponent<AiPerceptionHolder>();
    }
}
