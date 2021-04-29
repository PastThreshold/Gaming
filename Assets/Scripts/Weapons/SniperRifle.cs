using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperRifle : BasicWeapon
{
    [Header("Level and Values")]
    [SerializeField] GameObject laserSight;
    [SerializeField] GameObject laserStartPoint;
    [SerializeField] GameObject laserLight;
    [SerializeField] LayerMask laserLayerMask;
    [SerializeField] float levelThreeDamage = 50f;
    [SerializeField] float damageHitScane = 35f;

    [Header("Fire")]
    [SerializeField] GameObject muzzleFlashVFX;
    RaycastHit hit;
    LineRenderer laserComponent;

    public override void CheckWeaponLevel()
    {
        base.CheckWeaponLevel();
        switch (currentWeaponLevel)
        {
            case 1:
                laserSight.SetActive(false);
                break;
            case 2:
                laserSight.SetActive(true);
                break;
            case 3:
                laserSight.SetActive(true);
                break;
            case 4:
                laserSight.SetActive(true);
                break;
        }
        if (!active)
        {
            laserComponent.enabled = false;
        }
        else
        {
            laserComponent.enabled = true;
        }
    }

    private void Start()
    {
        BaseStart();
        laserComponent = laserSight.GetComponent<LineRenderer>();
        CheckWeaponLevel();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (canFire && active)
            {
                StartCoroutine("Fire");
            }
        }
        UpdateLaser();
    }

    IEnumerator Fire()
    {
        canFire = false;
        Projectile bullet = CreateBasicProjectile();
        SetProjectileTarget(bullet);
        bullet.EnableProjectile();
        Instantiate(muzzleFlashVFX, projectileSpawn.position, firePointRotation.rotation);
        Fired();
            
        yield return new WaitForSeconds(fireRate);
        canFire = true;
    }

    private void UpdateLaser()
    {
        if (laserSight.activeSelf)
        {
            Ray direction = new Ray(laserStartPoint.transform.position, laserStartPoint.transform.forward);
            if (Physics.Raycast(direction, out hit, 1000f, laserLayerMask))
            {
                float dist = Vector3.Distance(laserStartPoint.transform.position, hit.point);
                laserComponent.SetPosition(1, new Vector3(0, 0, dist));
                laserLight.transform.position = hit.point;
                laserLight.transform.position = laserLight.transform.position - (laserLight.transform.forward / 50);
            }
        }
    }

    public override void EnableWeapon()
    {
        laserComponent.enabled = true;
    }

    public override void DisableWeapon()
    {
        laserComponent.enabled = false;
        print("cal");
    }
}