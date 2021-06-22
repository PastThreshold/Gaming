using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This script will be used as a base weapon class
 * for use with weapon switcher and the leveling process of weapons
 * It should make it much easier to switch between guns and make the code clearer 
 * Every weapon has a leveling system all working similarly except for the ar class */

public class BasicWeapon : MonoBehaviour
{
    protected static Player player;
    public enum WeaponType
    {
        assaultRifle,
        sniperRifle,
        shotgun,
        deagle,
        stickyBombLauncher,
        shredder,
        laser,
        chargeRifle,
        rpg,
        strike,
        nullVal,
    }
    [SerializeField] protected WeaponType weaponType;

    const int MAX_LEVEL = 4;
    const int PERM_MAX_LEVEL = 3;
    [Range(1, 4)] [SerializeField] protected int permanantWeaponLevel = 1;
    protected int currentWeaponLevel = 1;
    bool temporaryLevelUpgrade = false;
    int weaponTypeInt;
    public ProjectilePoolHandler projPool;

    [Header("Level Data")]
    [SerializeField] protected GameObject projectileL1;
    [SerializeField] protected GameObject projectileL2;
    [SerializeField] protected GameObject projectileL3;
    [SerializeField] protected GameObject projectileL4;
    protected GameObject projectile;

    [SerializeField] protected float fireRateL1;
    [SerializeField] protected float fireRateL2;
    [SerializeField] protected float fireRateL3;
    [SerializeField] protected float fireRateL4;
    protected float fireRate;
    protected float currentBaseFireRate;
    protected int totalSpeedUps;
    private List<float> speedUpFactors = new List<float>();

    [Header("Weapon Fire Data")]
    [SerializeField] protected Transform firePointRotation;
    [SerializeField] protected Transform autoTargetFirePointRotation;
    [SerializeField] protected Transform projectileSpawn;
    [SerializeField] protected bool canFire = true;
    public bool active;

    MeshRenderer[] meshes;

protected AudioSource shotSound;
    [SerializeField] protected AudioClip gunshotSound;
    [SerializeField] protected float pitchRandomization;
    [SerializeField] protected float soundVolume;

    void Awake()
    {
        BaseAwake();
       
    }

    void Start()
    {
        BaseStart();
    }

    protected void BaseAwake()
    {
        meshes = GetComponentsInChildren<MeshRenderer>();
    }

    protected void BaseStart()
    {
        if (player == null)
            player = FindObjectOfType<Player>();
        currentWeaponLevel = permanantWeaponLevel;
    }


    // Every weapon must check their level after a level decrease or increase
    // and change their values of projectile accordingly
    // some will be overidden if they have more conditions like the charge rifle's scaleAddPerSecond
    public virtual void CheckWeaponLevel()
    {
        switch (currentWeaponLevel)
        {
            case 1:
                currentBaseFireRate = fireRateL1;
                projectile = projectileL1;
                break;
            case 2:
                currentBaseFireRate = fireRateL2;
                projectile = projectileL2;
                break;
            case 3:
                currentBaseFireRate = fireRateL3;
                projectile = projectileL3;
                break;
            case 4:
                currentBaseFireRate = fireRateL4;
                projectile = projectileL4;
                break;
            default:
                Debug.Log("Weapon Level Error");
                break;
        }
        fireRate = currentBaseFireRate;
        foreach(float value in speedUpFactors)
        {
            fireRate /= value;
        }
    }

    /// <summary>
    /// These 4 methods are called according to the event in the main weapons scripts,
    /// their purpose is to send messages to all the clones and weapon clones that exist to fire, hold, alt, or stop
    /// each clone has a currently equipped weapon which will then do whatever its respecive fire, hold, alt, and stop functions are
    /// </summary>
    protected void Fired()
    {
        if (shotSound != null)
            shotSound.PlayOneShot(shotSound.clip);

        if (GlobalClass.player.allActiveClones.Count > 0)
        {
            foreach (Clone clone in GlobalClass.player.allActiveClones)
            {
                clone.FireWeapon();
            }
        }
    }

    protected void AltFired()
    {
        if (GlobalClass.player.allActiveClones.Count > 0)
        {
            foreach (Clone clone in GlobalClass.player.allActiveClones)
            {
                clone.AltFireWeapon();
            }
        }
    }

    protected void HeldFired()
    {
        if (GlobalClass.player.allActiveClones.Count > 0)
        {
            foreach (Clone clone in GlobalClass.player.allActiveClones)
            {
                clone.HoldFireWeapon();
            }
        }
    }

    protected void StoppedFiring()
    {
        if (GlobalClass.player.allActiveClones.Count > 0)
        {
            foreach (Clone clone in GlobalClass.player.allActiveClones)
            {
                clone.StopFireWeapon();
            }
        }
    }

    public int GetWeaponLevel() { return currentWeaponLevel; }
    public GameObject GetProjectile() { return projectile; }
    public float GetFireRate() { return fireRate; }
    public int GetCurrentLevel() { return currentWeaponLevel; }
    public float GetBaseFireRate() { return currentBaseFireRate; }
    public WeaponType GetWeaponType() { return weaponType; }

    /// <summary>
    /// Used just before EnableProjectile() to see if auto target is active, and if there is a target, set the projectiles target
    /// so that it becomes homing
    /// </summary>
    protected void SetProjectileTarget(Projectile proj)
    {
        if (player.HasTarget())
        {
            proj.SetTarget(player.GetTargetTransform());
        }
    }

    /// <summary>
    /// Used just before EnableProjectile() to see if auto target is active, and if there is a target, set the projectiles direction
    /// so that is fires at the target (used for raycast projectiles ex. RaycastBullet)
    /// </summary>
    protected void SetProjectileTargetRaycast(Projectile proj)
    {
        if (player.HasTarget())
        {
            autoTargetFirePointRotation.transform.LookAt(Extra.SetYToTransform(player.autoTargetLocation, projectileSpawn));
            proj.transform.rotation = autoTargetFirePointRotation.rotation;
        }
    }

    public void PermanantUpgradeWeaponLevel()
    {
        if (permanantWeaponLevel < PERM_MAX_LEVEL)
        {
            permanantWeaponLevel++;

            if (currentWeaponLevel < MAX_LEVEL)
                currentWeaponLevel++;
        }

        CheckWeaponLevel();
    }

    public void TemporaryUpgradeWeaponLevel(float time)
    {
        if (currentWeaponLevel < MAX_LEVEL)
        {
            if (!temporaryLevelUpgrade)
            {
                currentWeaponLevel = permanantWeaponLevel + 1;
                temporaryLevelUpgrade = true;
            }
            else
            {
                currentWeaponLevel++;
            }

            CheckWeaponLevel();
            StartCoroutine(DowngradeWeaponLevel(time));
        }
    }

    IEnumerator DowngradeWeaponLevel(float time)
    {
        yield return new WaitForSeconds(time);

        if (currentWeaponLevel > permanantWeaponLevel)
        {
            currentWeaponLevel--;

            if (currentWeaponLevel == permanantWeaponLevel)
                temporaryLevelUpgrade = false;
        }
        else if (temporaryLevelUpgrade)
            temporaryLevelUpgrade = false;

        CheckWeaponLevel();

    }

    public virtual void SpeedUpFireRate(float factor) // Virtual means that this method can be overidden by each weapon's script
    {
        fireRate /= factor;
        totalSpeedUps++;
        speedUpFactors.Add(factor);
    }

    public virtual void SpeedDownFireRate(float factor)
    {
        print("Name: " + gameObject.name + ",     CurrentFireRate: " + fireRate + ",   total: " + totalSpeedUps);
        fireRate *= factor;
        totalSpeedUps--;
        speedUpFactors.Remove(factor);
    }
    protected virtual Projectile CreateBasicProjectile()
    {
        Projectile bullet = projPool.GetNextProjectile(currentWeaponLevel);
        bullet.transform.position = projectileSpawn.position;
        bullet.transform.rotation = firePointRotation.transform.rotation;
        bullet.enabled = true;
        GlobalClass.player.WeaponFired(bullet);
        return bullet;
    }
    

    /// <summary>
    /// Currently in weapon switcher to enable and disable the renders of the weapons, 
    /// GetComponentsInChildren is used to get meshes, which is not ideal
    /// TODO - After replace the weapons final model, use those components here and this fuctions in WeaponSwitcher
    /// </summary>
    public virtual void EnableWeapon()
    {
        foreach (MeshRenderer mesh in meshes)
        {
            mesh.enabled = true;
        }
        active = true;
    }

    public virtual void DisableWeapon()
    {
        foreach (MeshRenderer mesh in meshes)
        {
            mesh.enabled = false;
        }
        active = false;
    }
}
