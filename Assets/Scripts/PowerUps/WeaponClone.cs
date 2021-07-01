using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponClone : MonoBehaviour
{
    [SerializeField] BasicWeapon.WeaponType weaponCloneType = BasicWeapon.WeaponType.assaultRifle;
    [SerializeField] Transform firePointRotation = null;
    [SerializeField] Transform firePoint = null;
    [SerializeField] Transform autoTargetFirePointRotation = null;

    int originalWeaponLevel = 1;
    ProjectilePoolHandler projPool = null;

    [Header("DEPENDS ON TYPE")]
    public GameObject projectile = null;
    [SerializeField] Transform secondFirePoint;

    public GameObject firingProjectile = null; // Intended for the laser and charge rifle because they dont just instantiate bullets
    float scaleAddPerSecond = 0;
    public ChargeShot chargeRifleShot = null;

    bool waitingToTick = false; // Laser, it waits between damage ticks
    float damage = 0f;

    ChargeRifle chargeRifleScript = null; 
    LaserBeam laserScript = null; // These need to be assigned because they have special cases
    Deagles deagleScript = null;
    StickyBombLauncher stickyBLScript = null;


    private void Start()
    {
        switch (weaponCloneType)
        {
            case BasicWeapon.WeaponType.assaultRifle:
                projPool = GlobalClass.arPool;
                break;
            case BasicWeapon.WeaponType.sniperRifle:
                projPool = GlobalClass.sniperPool;
                break;
            case BasicWeapon.WeaponType.deagle:
                deagleScript = FindObjectOfType<Deagles>();
                projPool = GlobalClass.deaglePool;
                break;
            case BasicWeapon.WeaponType.stickyBombLauncher:
                stickyBLScript = FindObjectOfType<StickyBombLauncher>();
                projPool = GlobalClass.stickyblPool;
                break;
            case BasicWeapon.WeaponType.shredder:
                projPool = GlobalClass.shredderPool;
                break;
            case BasicWeapon.WeaponType.laser:
                laserScript = FindObjectOfType<LaserBeam>();
                break;
            case BasicWeapon.WeaponType.chargeRifle:
                chargeRifleScript = FindObjectOfType<ChargeRifle>();
                projPool = GlobalClass.crPool;
                break;
            case BasicWeapon.WeaponType.rpg:
                projPool = GlobalClass.rpgPool;
                break;

        }
    }

    // Weapons first must call their own Fire() which calls all clones which calls their active weapons fire()
    public void Fire()
    {
        switch(weaponCloneType)
        {
            case BasicWeapon.WeaponType.assaultRifle:
                var bullet = CreateBasicProjectile();
                if (GlobalClass.player.HasTarget())
                {
                    autoTargetFirePointRotation.transform.LookAt(Extra.SetYToTransform(GlobalClass.player.autoTargetLocation, firePoint));
                    bullet.transform.rotation = autoTargetFirePointRotation.rotation;
                }
                bullet.EnableProjectile();
                if (originalWeaponLevel >= 4)
                {
                    bullet = CreateBasicProjectile();
                    bullet.transform.position = secondFirePoint.transform.position;
                    bullet.transform.position += firePointRotation.forward * Random.Range(0.5f, 1.75f);
                    bullet.EnableProjectile();
                }
                break;
            case BasicWeapon.WeaponType.shotgun:
                int angle = -6;
                for (int i = 0; i < 5; i++)
                {
                    var newBullet = CreateBasicProjectile();
                    newBullet.transform.Rotate(0, angle, 0);
                    newBullet.EnableProjectile();
                    angle += 3;
                }
                break;
            case BasicWeapon.WeaponType.deagle:
                firePoint.LookAt(Extra.SetYToTransform(GlobalClass.player.mouseLocationConverted, firePoint));
                bullet = CreateBasicProjectile();
                bullet.transform.rotation = firePoint.rotation;
                bullet.EnableProjectile();
                if (deagleScript.bothGunsActive)
                {
                    bullet = CreateBasicProjectile();
                    secondFirePoint.LookAt(Extra.SetYToTransform(GlobalClass.player.mouseLocationConverted, secondFirePoint));
                    bullet.transform.position = secondFirePoint.position;
                    bullet.transform.rotation = secondFirePoint.rotation;
                    bullet.EnableProjectile();
                }
                break;
            case BasicWeapon.WeaponType.stickyBombLauncher:
                var sticky = CreateBasicProjectile();
                sticky.EnableProjectile();
                stickyBLScript.allStickyBombsActive.Add(sticky.GetComponent<StickyBomb>());
                break;
            case BasicWeapon.WeaponType.laser:
                if (firingProjectile == null)
                    firingProjectile = Instantiate(projectile, firePoint.position, firePointRotation.rotation);
                firingProjectile.transform.parent = firePoint.transform;
                break;
            case BasicWeapon.WeaponType.chargeRifle:
                if (chargeRifleShot == null)
                    chargeRifleShot = CreateBasicProjectile().GetComponent<ChargeShot>();
                chargeRifleShot.EnableProjectile();
                break;
            default:
                bullet = CreateBasicProjectile();
                bullet.EnableProjectile();
                break;
        }
    }

    // Called by clone to set the current projectile and level, if this weapon is the laser and is currently firing
    // it will destroy the current laser and instantiate the new one
    public void UpdateValues(GameObject projectile, int level)
    {
        this.projectile = projectile;
        originalWeaponLevel = level;
        if (weaponCloneType == BasicWeapon.WeaponType.laser)
        {
            if (firingProjectile != null)
            {
                Destroy(firingProjectile);
                firingProjectile = Instantiate(this.projectile, firePoint.position, firePointRotation.rotation); ;
                firingProjectile.transform.parent = firePoint.parent;
            }
        }
    }

    public void AltFire()
    {
        switch (weaponCloneType)
        {
            case BasicWeapon.WeaponType.assaultRifle:
                Vector3 randomForward = firePointRotation.transform.forward * Random.Range(0.5f, 1.5f);
                int angle = -8;
                for (int i = 0; i < 5; i++)
                {
                    Projectile bullet = CreateBasicProjectile();
                    bullet.transform.position += randomForward;
                    bullet.transform.Rotate(0, angle, 0);
                    bullet.EnableProjectile();
                    angle += 4;
                }
                break;
            case BasicWeapon.WeaponType.sniperRifle:
                //Instantiate(projectile, transform.position, firePointRotation.rotation);
                break;
            case BasicWeapon.WeaponType.deagle:
                //Instantiate(projectile, transform.position, firePointRotation.rotation);
                break;
            case BasicWeapon.WeaponType.stickyBombLauncher:
                //Instantiate(projectile, transform.position, firePointRotation.rotation);
                break;
            case BasicWeapon.WeaponType.shredder:
                //Instantiate(projectile, transform.position, firePointRotation.rotation);
                break;
            case BasicWeapon.WeaponType.laser:
                //Instantiate(projectile, transform.position, firePointRotation.rotation);
                break;
            case BasicWeapon.WeaponType.chargeRifle:
                //Instantiate(projectile, transform.position, firePointRotation.rotation);
                break;
            case BasicWeapon.WeaponType.rpg:
                //Instantiate(projectile, transform.position, firePointRotation.rotation);
                //print("fire");
                break;
            case BasicWeapon.WeaponType.strike:
                //Instantiate(projectile, transform.position, firePointRotation.rotation);
                break;
        }
    }

    // This is a function for the charge rifle and laser as they must update while lmb is held down
    public void HoldFire()
    {
        switch (weaponCloneType)
        {
            case BasicWeapon.WeaponType.laser:
                damage = laserScript.damage;
                if (firingProjectile != null)
                {
                    firingProjectile.transform.position = firePoint.transform.position;
                    if (!waitingToTick)
                    {
                        RaycastHit[] hits;
                        hits = Physics.RaycastAll(firePoint.transform.position, firePointRotation.transform.forward);

                        foreach (RaycastHit hit in hits)
                        {
                            if (hit.collider.GetComponent<SphereWeapon>())
                            {
                                hit.collider.GetComponent<SphereWeapon>().LaserCharge();
                            }

                            if (hit.collider.GetComponentInParent<Enemy>())
                            {
                                hit.collider.GetComponentInParent<Enemy>().TakeDamage(damage);
                            }
                        }
                        StartCoroutine("Wait");
                    }
                }
                else
                {
                    firingProjectile = Instantiate(projectile, firePoint.position, firePointRotation.rotation);
                    firingProjectile.transform.parent = firePoint.parent;
                }
                break;
            case BasicWeapon.WeaponType.chargeRifle:
                print("holding");
                scaleAddPerSecond = chargeRifleScript.scaleAddPerSecond;
                chargeRifleShot.Charge(scaleAddPerSecond);
                chargeRifleShot.transform.position = firePoint.position;
                chargeRifleShot.transform.rotation = firePointRotation.rotation;
                break;
            default:
                break;
        }
    }

    public void StopFire()
    {
        switch (weaponCloneType)
        {
            case BasicWeapon.WeaponType.laser:
                Destroy(firingProjectile);
                firingProjectile = null;
                break;
            case BasicWeapon.WeaponType.chargeRifle:
                chargeRifleShot.transform.parent = null;
                chargeRifleShot.FireShot();
                chargeRifleShot = null;
                break;
            default:
                break;
        }
    }

    private Projectile CreateBasicProjectile()
    {
        Projectile bullet = projPool.GetNextProjectile(originalWeaponLevel);
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePointRotation.transform.rotation;
        bullet.enabled = true;
        return bullet;
    }
}