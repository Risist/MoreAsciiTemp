using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Infecting : MonoBehaviour
{
    public GameObject input;
    public AiFraction fraction;
    public bool isPlayer = true;

    void Infect(Infectable obj)
    {
        var unit = obj.GetComponent<AiPerceiveUnit>();
        unit.fraction = fraction;

        var _input = Instantiate(input, obj.transform);
        _input.transform.localPosition = Vector3.zero;

        var cls = obj.GetComponentsInChildren<ApplyFractionColor>();
        foreach (var it in cls)
            it.ApplyColor();

        if (isPlayer)
            obj.tag = "Player";
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var Infectable = collision.gameObject.GetComponent<Infectable>();
        if(Infectable && Infectable.infected == false)
        {
            Infect(Infectable);
            Infectable.infected = true;
            Destroy(gameObject);
        }
    }
}
