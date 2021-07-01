using System.Collections;
using UnityEngine;

public class Turret : MonoBehaviour
{
    ProjectilePoolHandler projPool;
    Player player;
    int originalLevel;
    GameObject bullet;
    [SerializeField] GameObject barrel;
    [SerializeField] GameObject barrelBase;
    bool canFire;
    float fireRate;
    float timeUntilDeath;
    bool shotty = false;
    bool laser = false;
    GameObject spawnedLaser;
    float laserDamage;
    Enemy target = null;
    bool hasTarget = false;

    void Start()
    {
        // Find player, get the active weapon, use that weapons projectile pool to get projectiles
        player = GlobalClass.player;
        BasicWeapon activeWeapon = GlobalClass.weaponSwitcher.GetActiveWeapon();
        BasicWeapon.WeaponType activeWeaponType = activeWeapon.GetWeaponType();
        projPool = activeWeapon.projPool;
        originalLevel = activeWeapon.GetWeaponLevel();

        // Special cases
        switch (activeWeaponType)
        {
             case BasicWeapon.WeaponType.assaultRifle:
                 if (Input.GetKey(KeyCode.LeftShift))
                    shotty = true;
                 break;
             case BasicWeapon.WeaponType.laser:
                 laser = true;
                 laserDamage = activeWeapon.GetComponent<LaserBeam>().GetDamage();
                 break;
             default:
                 break;
        }
        bullet = activeWeapon.GetProjectile();

        canFire = true;
        fireRate = activeWeapon.GetBaseFireRate() * 1.5f;
        print(fireRate);
        if (laser)
        {
            spawnedLaser = Instantiate(bullet, transform);
            spawnedLaser.transform.parent = barrel.transform;
        }
        FindTargetCloseToPlayer();
    }

    void Update()
    {
        if (hasTarget)
        {
            if (target == null)
            {
                hasTarget = false;
                FindTargetCloseToPlayer();
                return;
            }
            transform.LookAt(Extra.SetYToTransform(target.transform.position, transform));
            barrelBase.transform.LookAt(target.transform.position);
            if (canFire)
            {
                StartCoroutine("Fire");
            }
        }
        else if (LevelController.allEnemiesInScene.Count > 0)
            FindTargetCloseToPlayer();
    }

    public void TargetDestroyed()
    {
        FindTargetCloseToPlayer();
    }

    private void FindTargetCloseToPlayer()
    {
        float distance = Mathf.Infinity;
        float distanceBetween;
        target = null;
        foreach(Enemy enemy in LevelController.allEnemiesInScene)
        {
            distanceBetween = (player.transform.position - enemy.transform.position).sqrMagnitude;
            if (distanceBetween < distance)
            {
                distance = distanceBetween;
                target = enemy;
            }
        }
        if (target != null)
            hasTarget = true;
    }

    IEnumerator Fire()
    {
        canFire = false;

        if (shotty)
        {
            int angle = -8;
            for (int i = 0; i < 5; i++)
            {
                Projectile bullet = CreateBasicProjectile();
                bullet.transform.Rotate(0, angle, 0);
                bullet.EnableProjectile();
                angle += 4;
            }
        }
        else
        {
            Projectile bullet = CreateBasicProjectile();
            bullet.transform.position += bullet.transform.forward * Random.Range(0.5f, 1.5f);
            bullet.EnableProjectile();
        }

        yield return new WaitForSeconds(fireRate);
        canFire = true;
    }

    public void SetTimeUntilDeath(float time)
    {
        timeUntilDeath = time;
        StartCoroutine("Destroy");
    }

    IEnumerator Destroy()
    {
        yield return new WaitForSeconds(timeUntilDeath);
        Destroy(gameObject);
    }

    private Projectile CreateBasicProjectile()
    {
        Projectile bullet = projPool.GetNextProjectile(originalLevel);
        bullet.transform.position = barrel.transform.position;
        bullet.transform.rotation = barrelBase.transform.rotation;
        bullet.enabled = true;
        return bullet;
    }
}
