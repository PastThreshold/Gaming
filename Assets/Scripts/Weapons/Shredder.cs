using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shredder : BasicWeapon
{
    private void Awake()
    {
        BaseAwake();
    }

    void Start()
    {
        BaseStart();
        projPool = GlobalClass.shredderPool;
        CheckWeaponLevel();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (canFire && active)
                StartCoroutine("Fire");
        }
    }

    IEnumerator Fire()
    {
        canFire = false;
        Fired();
        Projectile saw = projPool.GetNextProjectile(currentWeaponLevel);
        saw.transform.position = projectileSpawn.position;
        saw.transform.rotation = firePointRotation.transform.rotation;
        SetProjectileTarget(saw);
        saw.EnableProjectile();
        yield return new WaitForSeconds(fireRate);
        canFire = true;
    }
}
