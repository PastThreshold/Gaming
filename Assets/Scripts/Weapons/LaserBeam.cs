using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : BasicWeapon
{
    GameObject spawnedLaser;
    Laser spawnedLaserScript;
    [SerializeField] public float damage = 10;
    public bool waitingToTick;

    // Start is called before the first frame update
    void Start()
    {
        CheckWeaponLevel();
        DisableLaser();
    }

    public override void CheckWeaponLevel()
    {
        base.CheckWeaponLevel();
        if (spawnedLaser)
            Destroy(spawnedLaser.gameObject);
        spawnedLaser = Instantiate(projectile, projectileSpawn.transform);
        spawnedLaserScript = spawnedLaser.GetComponent<Laser>();

        if (!active || !Input.GetMouseButton(0)) // If the laser is not equipped or is not being fired disable
            DisableLaser();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && active && canFire)
        {
            EnableLaser();
        }

        if (Input.GetMouseButton(0) && active && canFire)
        {
            UpdateLaser();
        }

        if (Input.GetMouseButtonUp(0) && active)
        {
            DisableLaser();
            StoppedFiring();
        }
    }

    public void EnableLaser()
    {
        spawnedLaser.SetActive(true);
        Fired();
    }

    public void DisableLaser()
    {
        if (spawnedLaser)
            spawnedLaser.SetActive(false);
    }

    public void UpdateLaser()
    {
        if (projectileSpawn != null)
        {
            spawnedLaser.transform.position = projectileSpawn.transform.position;
            Debug.DrawRay(projectileSpawn.transform.position, firePointRotation.transform.forward, Color.black, 5f);
            Physics.Raycast(projectileSpawn.transform.position, firePointRotation.transform.forward, out RaycastHit hit, 100f, GlobalClass.exD.wallsAndEnemyShieldsLayerMask);
            print(hit.distance);
            Debug.DrawRay(projectileSpawn.transform.position, firePointRotation.transform.forward * hit.distance, Color.black, 5f);
            spawnedLaserScript.SetColliderSize(hit.distance);
        }
    }

    public float GetDamage()
    {
        return damage;
    }
}
