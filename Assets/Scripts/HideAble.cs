using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideAble : MonoBehaviour
{
    [Range(0, 1)] public float hideSpeed;
    [Range(0, 1)] public float minAlphaScale = 0.35f;
    [Range(0, 1)] public float viewDistanceScale = 0.35f;
    
    SpriteRenderer[] renderers;
    float[] alphas;

    AiPerceiveUnit unit;
    float initialDistanceModificator;

    int hidespotCount = 0;
    public void Show() { --hidespotCount; }
    public void Hide() { ++hidespotCount; }

    void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
        alphas = new float[renderers.Length];
        for (int i = 0; i < renderers.Length; ++i)
            alphas[i] = renderers[i].color.a;

        unit = GetComponentInParent<AiPerceiveUnit>();
        initialDistanceModificator = unit.distanceModificator;
    }
    private void Update()
    {
        if (hidespotCount == 0)
            ShowUpdate();
        else
            HideUpdate();
    }

    void HideUpdate()
    {
        for (int i = 0; i < renderers.Length; ++i)
        {
            Color cl = renderers[i].color;
            cl.a = Mathf.Lerp(cl.a, alphas[i] * minAlphaScale, hideSpeed);
            renderers[i].color = cl;
        }
        unit.distanceModificator = Mathf.Lerp(unit.distanceModificator, initialDistanceModificator * viewDistanceScale, hideSpeed);
    }
    void ShowUpdate()
    {
        for (int i = 0; i < renderers.Length; ++i)
        {
            Color cl = renderers[i].color;
            cl.a = Mathf.Lerp(cl.a, alphas[i], hideSpeed);
            renderers[i].color = cl;
        }
        unit.distanceModificator = Mathf.Lerp(unit.distanceModificator, initialDistanceModificator, hideSpeed);
    }
}
