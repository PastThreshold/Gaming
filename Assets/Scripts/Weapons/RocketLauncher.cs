using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : BasicWeapon
{
    ProjectilePoolHandler altProjPool;
    [SerializeField] int levelToAltFire = 2;

    private void Start()
    {
        projPool = GlobalClass.rpgPool;
        altProjPool = GlobalClass.rpgAltPool;
        CheckWeaponLevel();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (canFire && active)
            {
                if (Input.GetKey(KeyCode.LeftShift) && currentWeaponLevel >= levelToAltFire)
                    StartCoroutine("FireAltRocket");
                else
                    StartCoroutine("FireRocket");
            }
        }
    }

    IEnumerator FireRocket()
    {
        canFire = false;
        Fired();
        Projectile rocket = CreateBasicProjectile();
        SetProjectileTarget(rocket);
        rocket.EnableProjectile();
        yield return new WaitForSeconds(fireRate);
        canFire = true;
    }

    IEnumerator FireAltRocket()
    {
        canFire = false;
        Fired();
        for (int i = 0; i < 4; i++)
        {
            Projectile rocket = CreateAltProjectile();
            SetProjectileTarget(rocket);
            rocket.EnableProjectile();
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(fireRate);
        canFire = true;
    }

    private Projectile CreateAltProjectile()
    {
        Projectile rocket = altProjPool.GetNextProjectile(currentWeaponLevel);
        rocket.transform.position = projectileSpawn.position;
        rocket.transform.rotation = firePointRotation.rotation;
        rocket.enabled = true;
        return rocket;
    }
}
