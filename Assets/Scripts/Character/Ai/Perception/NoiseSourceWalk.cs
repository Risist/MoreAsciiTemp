using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseSourceWalk : MonoBehaviour
{
    [Range(0, 1)] public float noiseLevelDamping = 0.9f;
    [Range(0, 1)] public float noiseRandomness = 0.5f;
    public float noiseEventTreshold = 5.0f;
    public float dataVelocityFactor = 1.0f;

    [Space]
    [Range(0, 1)] public float noiseIndicatorChance = 0.1f;
    public float velocityIndicatorOffsetRatio = 0.0f;
    public GameObject noiseIndicatorPrefab;

    float noiseLevel;

    private void OnTriggerStay2D(Collider2D collision)
    {
        var rb = collision.attachedRigidbody;
        if (rb)
        {
            noiseLevel += rb.velocity.magnitude * Mathf.Lerp(1, Random.value, noiseRandomness);
            if (noiseLevel > noiseEventTreshold)
            {
                noiseLevel = 0;

                AiSenseNoise.NoiseData data = new AiSenseNoise.NoiseData();
                data.position = rb.position;
                data.velocity = rb.velocity * dataVelocityFactor;

                AiPerceiveUnit unit = rb.GetComponentInParent<AiPerceiveUnit>();
                Debug.Assert(unit);
                data.fraction = unit ? unit.fraction : null;

                if(AiSenseNoise.CanSpreadNoise() && noiseIndicatorPrefab && Random.value < noiseIndicatorChance)
                {
                    Instantiate(noiseIndicatorPrefab, data.position + rb.velocity*velocityIndicatorOffsetRatio, Quaternion.identity);
                }

                AiSenseNoise.SpreadNoise(data);
            }
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        var rb = collision.collider.attachedRigidbody;
        if (rb)
        {
            noiseLevel += rb.velocity.magnitude * Mathf.Lerp(1, Random.value, noiseRandomness);
            if (noiseLevel > noiseEventTreshold)
            {
                noiseLevel = 0;

                AiSenseNoise.NoiseData data = new AiSenseNoise.NoiseData();
                data.position = rb.position;
                data.velocity = rb.velocity * dataVelocityFactor;

                AiPerceiveUnit unit = rb.GetComponentInParent<AiPerceiveUnit>();
                Debug.Assert(unit);
                data.fraction = unit ? unit.fraction : null;

                if (AiSenseNoise.CanSpreadNoise() && noiseIndicatorPrefab && Random.value < noiseIndicatorChance)
                {
                    Instantiate(noiseIndicatorPrefab, data.position + rb.velocity * velocityIndicatorOffsetRatio, Quaternion.identity);
                }

                AiSenseNoise.SpreadNoise(data);
            }
        }
    }

    private void FixedUpdate()
    {
        noiseLevel *= noiseLevelDamping;
    }
}
