using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereWeapon : MonoBehaviour
{
    [SerializeField] ParticleSystem emissionsVariable;
    [SerializeField] ParticleSystem tesla;
    [SerializeField] Light aura;
    [SerializeField] GameObject bulletL1;
    [SerializeField] GameObject bulletL2;
    [SerializeField] GameObject bulletL3;

    [SerializeField] ParticleSystem hitVFX;
    float distanceBetween;
    Enemy temp;

    [SerializeField] float increaseRayBulletDamage = 1.2f;
    public bool waitingForDeaglePair = false;
    public HighCaliber[,] bulletsAndPair;
    public int bulletSize = 0;
    const int MAX_BULLETS = 10;

    private void Start()
    {
        bulletsAndPair = new HighCaliber[MAX_BULLETS, 2];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<RaycastBullet>())
        {
            if (!other.GetComponent<RaycastBullet>().isEnemyBullet)
            {
                var vfx = Instantiate(hitVFX, other.transform.position, transform.rotation);
                Enemy[] enemies = FindObjectsOfType<Enemy>();
                if (enemies.Length > 0)
                {
                    distanceBetween = Mathf.Infinity;
                    foreach (Enemy enemy in enemies)
                    {
                        float distance = Vector3.Distance(transform.position, enemy.transform.position);
                        if (distanceBetween > distance)
                        {
                            temp = enemy;
                            distanceBetween = distance;
                        }
                    }
                    transform.LookAt(temp.transform.position);
                    transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

                    switch (other.GetComponent<RaycastBullet>().level)
                    {
                        case 1:
                            Instantiate(bulletL1, transform.position, transform.rotation);
                            break;
                        case 2:
                            Instantiate(bulletL2, transform.position, transform.rotation);
                            break;
                        case 3:
                            Instantiate(bulletL3, transform.position, transform.rotation);
                            break;
                        default:
                            Debug.Log("Sphere Bullet Level Error");
                            break;
                    }
                    Destroy(other.gameObject);
                }
            }
        }
    }

    /// <summary> Increases the damage of the projectile </summary>
    public void RaycastBulletHit(RaycastBullet projectile)
    {
        projectile.ChangeDamage(increaseRayBulletDamage);
        var vfx = Instantiate(hitVFX, projectile.transform.position, transform.rotation);
    }

    /// <summary> Splits the bullet into three, the other two fly perpendicular to original </summary> 
    public void SniperBulletHit(SBullet projectile)
    {
        projectile.SplitBySphere();
        int rotation = 90;
        for (int i = 0; i < 2; i++)
        {
            Projectile newProjectile = projectile.belongsTo.GetNextProjectile();
            newProjectile.transform.position = projectile.transform.position;
            newProjectile.transform.rotation = projectile.transform.rotation;
            newProjectile.transform.Rotate(0, rotation, 0);
            newProjectile.enabled = true;
            newProjectile.EnableProjectile();
            newProjectile.GetComponent<SBullet>().SplitBySphere();
            rotation = -rotation;
        }
    }

    public void DeagleBulletHit(HighCaliber projectile)
    {
        if (waitingForDeaglePair && projectile.GetBulletPair() != null)
        {
            print("There is one waiting");
            HighCaliber other = projectile.GetBulletPair();
            for (int i = 0; i < MAX_BULLETS; i++)
            {
                print("Testing: " + other.name + " with " + bulletsAndPair[i, 0].name);
                if (other == bulletsAndPair[i, 0])
                {
                    print("Pair found: " + other.name + " with " + bulletsAndPair[i, 0].name);
                    other.transform.position = bulletsAndPair[i, 0].transform.position;
                    bulletsAndPair[i, 0] = null;
                    bulletsAndPair[i, 1] = null;
                    return;
                }
            }
        }
        if (projectile.GetBulletPair() != null)
        {
            print("Adding: " + projectile.name + " and its pair " + projectile.GetBulletPair().name);
            bulletsAndPair[bulletSize, 0] = projectile;
            bulletsAndPair[bulletSize, 1] = projectile.GetBulletPair();
            StartCoroutine("WaitForDeaglePair", bulletSize);
            bulletSize++;
        }
    }

    IEnumerator WaitForDeaglePair(int index)
    {
        waitingForDeaglePair = true;
        yield return new WaitForSeconds(0.1f);
        if (bulletsAndPair[index, 0] != null)
        {
            bulletsAndPair[index, 0] = null;
            bulletsAndPair[index, 1] = null;
        }
        bulletSize--;
        if (bulletSize == 0)
            waitingForDeaglePair = false;
    }

    public void ShredderHit()
    {
        // Handled in shred
    }

    public void StickyBombHit()
    {
        // Handled in StickyBomb
    }

    public void LaserHit()
    {

    } 

    public void ChargeShotHit()
    {

    }

    public void RocketHit()
    {

    }

    public void SmallRocketHit()
    {
        // Handled in SmallRocket
    }

    public void LaserCharge()
    {
        emissionsVariable.emissionRate += 0.01f;
        tesla.startColor = new Color(tesla.startColor.r + .01f, tesla.startColor.b - .01f, tesla.startColor.b - .01f, 255f);
        aura.color = new Color(aura.color.r + .01f, aura.color.b - .01f, aura.color.b - .01f, 255f);
        aura.range += .7f;
        aura.intensity += .03f;

        if (emissionsVariable.emissionRate >= 3.2f)
        {
            Destroy(gameObject);
        }
        
    }
}
