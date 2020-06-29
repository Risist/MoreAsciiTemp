using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkParticle : MonoBehaviour
{
    public RangedInt nParticles;
    public Sprite[] sprites;
    public GameObject particlePrefab;
    public RangedFloat distanceRange;
    public RangedFloat rotationSpeedRange;
    public float height;

    struct ParticleData
    {
        public Transform transform;
        public float angle;
        public float rotationSpeed;
        public float distance;
    }
    ParticleData[] particles;
    
    private void Start()
    {
        int n = nParticles.GetRandom();
        particles = new ParticleData[n];
        for(int i = 0; i < n; ++i)
        {
            var obj = Instantiate(particlePrefab, transform);
            //obj.GetComponent<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Length)];

            particles[i].transform = obj.transform;
            particles[i].angle = Random.value * 360;
            particles[i].distance = distanceRange.GetRandom();
            particles[i].rotationSpeed = rotationSpeedRange.GetRandom();
        }
    }

    private void FixedUpdate()
    {
        int n = transform.childCount;
        Vector3 center = transform.position;
        for (int i = 0; i < n; ++i)
        {
            float distance = particles[i].distance;
            float angle = particles[i].angle += particles[i].rotationSpeed;

            Vector3 position = particles[i].transform.position = 
                center + Quaternion.Euler(0,0, angle) * Vector2.up * distance + 
                Vector3.up * height*distance/distanceRange.max;
            
            Vector2 toPosition = center - position;
            particles[i].transform.rotation = Quaternion.LookRotation(toPosition);

        }
    }

}
