using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityManager : MonoBehaviour
{
    [Range(0, 1)] public float hideSpeed;
    [Range(0, 1)] public float visibilityLevel = 1.0f;

    SpriteRenderer[] renderers;
    float[] defaultAlphas;

    void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }
    private void Start()
    {
        defaultAlphas = new float[renderers.Length];
        for (int i = 0; i < renderers.Length; ++i)
            defaultAlphas[i] = renderers[i].color.a;
    }

    private void Update()
    {
        UpdateVisibility();
    }

    void UpdateVisibility()
    {
        for (int i = 0; i < renderers.Length; ++i)
        {
            Color cl = renderers[i].color;
            cl.a = Mathf.Lerp(cl.a, defaultAlphas[i]*visibilityLevel, hideSpeed);
            renderers[i].color = cl;
        }
    }

}
