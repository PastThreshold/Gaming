using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultRifle : BasicWeapon
{
    [Header("Fire")]
    [SerializeField] GameObject muzzleFlashVFX = null;
    [SerializeField] Transform secondProjectileSpawn = null;
    [SerializeField] Transform autoTargetFirePointRotation = null;
    [SerializeField] public float minForwardBullet = 0.5f;
    [SerializeField] public float maxForwardBullet = 1.75f;
    [SerializeField] float shotgunFireRate = 0.5f;
    [SerializeField] float shotgunDamagePercent = 0.4f;
    int levelForShotgun = 2;
    bool canShotgun = false;
    bool shotgunCanFire = true;
    int wavesOfShotgun = 2;
    int pelletsPerWave = 5;

    public override void CheckWeaponLevel()
    {
        base.CheckWeaponLevel();
        if (currentWeaponLevel >= levelForShotgun)
        {
            canShotgun = true;
        }
    }

    private void Start()
    {
        BaseStart();
        if (projPool == null)
        {
            projPool = GlobalClass.arPool;
        }
        CheckWeaponLevel();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (Input.GetKey(KeyCode.LeftShift) && canShotgun)
            {
                if (shotgunCanFire && active)
                    StartCoroutine("FireShotgun");
            }
            else
            {
                if (canFire && active)
                    StartCoroutine("Fire");
            }
        }
    }

    IEnumerator Fire()
    {
        Fired();
        canFire = false;
        Projectile bullet = CreateBasicProjectile();
        SetProjectileTargetRaycast(bullet);
        bullet.EnableProjectile();
        if (currentWeaponLevel >= 4)
        {
            bullet = CreateBasicProjectile();
            bullet.transform.position = secondProjectileSpawn.position;
            SetProjectileTargetRaycast(bullet);
            bullet.EnableProjectile();
        }
        yield return new WaitForSeconds(fireRate);
        canFire = true;
    }

    IEnumerator FireShotgun()
    {
        shotgunCanFire = false;
        for (int ii = 0; ii < wavesOfShotgun; ii++)
        {
            AltFired();
            int angle = -8;
            for (int i = 0; i < pelletsPerWave; i++)
            {
                Projectile bullet = CreateBasicProjectile();
                SetProjectileTargetRaycast(bullet);
                bullet.transform.Rotate(0, angle, 0);
                bullet.ChangeDamage(shotgunDamagePercent);
                bullet.EnableProjectile();
                angle += 4;
            }
            yield return new WaitForSeconds(0.15f);
        }
        yield return new WaitForSeconds(shotgunFireRate);
        shotgunCanFire = true;
    }
}