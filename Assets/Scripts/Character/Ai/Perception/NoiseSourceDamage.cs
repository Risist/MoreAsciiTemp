using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseSourceDamage : MonoBehaviour
{
    [Range(0, 1)] public float noiseLevelDamping = 0.9f;
    [Range(0, 1)] public float noiseRandomness = 0.5f;
    public float noiseEventTreshold = 5.0f;

    [Space]
    [Range(0, 1)] public float noiseIndicatorChance = 0.1f;
    public float velocityIndicatorOffsetRatio = 0.0f;
    public GameObject noiseIndicatorPrefab;

    float noiseLevel;

    private void Start()
    {
        var healthController = GetComponentInParent<HealthController>();
        healthController.onDamageCallback += (DamageData data) =>
        {
            noiseLevel += data.damage * Mathf.Lerp(1, Random.value, noiseRandomness);
            if (noiseLevel > noiseEventTreshold)
            {
                noiseLevel = 0;

                AiSenseNoise.NoiseData dataN = new AiSenseNoise.NoiseData();
                dataN.position = data.position;
                dataN.velocity = data.direction;
                AiSenseNoise.SpreadNoise(dataN);

                if (noiseIndicatorPrefab && Random.value < noiseIndicatorChance)
                    Instantiate(noiseIndicatorPrefab, data.position, Quaternion.identity);
            }
        };
    }

    private void FixedUpdate()
    {
        noiseLevel *= noiseLevelDamping;
    }
}
