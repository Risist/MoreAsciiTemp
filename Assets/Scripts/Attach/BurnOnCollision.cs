using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnOnCollision : MonoBehaviour
{
    public BurnData burnData;
    Timer tSpawn;

    protected void Awake()
    {
        Debug.Assert(burnData);
        tSpawn = new Timer(burnData.minimalSpawnTime);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!enabled)
            return;

        Debug.Assert(burnData);
        
        var burnable = collision.gameObject.GetComponent<Burnable>();
        if(burnable)
        {
            Debug.Assert(burnable);
            Debug.Assert(burnData);
            Debug.Assert(tSpawn != null);
            if (Random.value > burnData.attachChance * burnable.burnChanceModificator || 
                !tSpawn.IsReady())
                return;

            tSpawn.Restart();
            tSpawn.cd += burnData.spawnTimeIncrease;
            Attach(burnable);
        }
    }

    protected void FixedUpdate()
    {
        tSpawn.cd = Mathf.Lerp(burnData.minimalSpawnTime, tSpawn.cd, burnData.spawnTimeDrag);
    }

    void Attach(Burnable burnable)
    {
        Transform target = burnable.GetRandomBurnTarget();
        GameObject prefab = burnable.fireDealsDamage ? burnData.GetPrefabToSpawnDamage() : burnData.GetPrefabToSpawn();
        burnable.ActivateBurnSpread();

        var obj = Instantiate(prefab, target);

        

        obj.transform.localPosition = burnable.GetPositionOffset();
        obj.transform.localRotation = burnable.GetRotationOffset();


        float lifeTime = burnData.attachLifetime * burnable.fireLifetimeModificator;
        SpritesFader fader = obj.GetComponent<SpritesFader>();
        if (fader)
        {
            float t = fader.timerAppear.cd + fader.timerRevert.cd;
            fader.timerFull.cd = lifeTime - t;
        }

        Destroy(obj, lifeTime);
    }


}
