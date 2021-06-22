using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeRifle : BasicWeapon
{
    [Header("Fire")]
    [SerializeField] public float scaleAddPerSecond = 0.1f;
    
    Projectile shot;
    ChargeShot shotScript;
    public bool shotCharging;

    private void Awake()
    {
        BaseAwake();
    }

    void Start()
    {
        BaseStart();
        CheckWeaponLevel();
        shotCharging = false;
    }

    void Update()
    {
        if (active)
        {
            if (Input.GetMouseButtonDown(0) && !shotCharging && canFire)
            {
                CreateCharge();
            }
            if (Input.GetMouseButton(0) && shotCharging)
            {
                Charge();
            }
            else if (Input.GetMouseButton(0) && !shotCharging && canFire)
            {
                CreateCharge();
            }
            if (Input.GetMouseButtonUp(0) && shotCharging)
            {
                StartCoroutine(FireCharge());
            }
        }
    }

    private void CreateCharge()
    {
        canFire = false;
        Fired();
        shot = CreateBasicProjectile();
        shot.EnableProjectile();
        shotCharging = true;
        shotScript = shot.GetComponent<ChargeShot>();
    }

    private void Charge()
    {
        shotScript.Charge(scaleAddPerSecond);
        shot.transform.position = projectileSpawn.position;
        shot.transform.rotation = firePointRotation.rotation;
        HeldFired();
    }

    IEnumerator FireCharge()
    {
        if (shotCharging)
        {
            shotCharging = false;
            shot.transform.parent = null;
            shot.GetComponent<ChargeShot>().FireShot();
            shot.transform.rotation = firePointRotation.rotation;
            SetProjectileTarget(shot);
            StoppedFiring();
            shot = null;
            yield return new WaitForSeconds(fireRate);
            canFire = true;
        }
    }

    public override void SpeedUpFireRate(float speedUpFactor)
    {
        base.SpeedUpFireRate(speedUpFactor);
        scaleAddPerSecond *= speedUpFactor;
    }

    public override void SpeedDownFireRate(float speedDownFactor)
    {
        base.SpeedDownFireRate(speedDownFactor);
        scaleAddPerSecond /= speedDownFactor;
    }
}
