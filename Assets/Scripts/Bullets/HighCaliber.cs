using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighCaliber : Projectile
{
    [SerializeField] GameObject hitVFX;

    [Header("Bullet Pair Upgrade")]
    [SerializeField] float bulletPairTime = 0.5f;
    [SerializeField] float bulletPairDistance = 1f;
    HighCaliber bulletPair;
    protected Vector3 impactPoint;
    protected GameObject enemyHit;
    bool waitingToHit = false;
    [SerializeField] float bonusDamage = 50f;
    [SerializeField] GameObject bonusHitVFX;

    [SerializeField] TrailRenderer trail;
    Vector3[] positions;
    bool trailFrozen = false;
    [SerializeField] ParticleSystem pSystem;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        moving = false;
        DisableProjectile();
    }

    void Update()
    {

        if (waitingToHit)
        {
            bulletPairTime -= 1f * Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        BaseFixedUpdate();
        //if (moving)
            //transform.rotation = Quaternion.LookRotation(rb.velocity);
    }

    void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case GlobalClass.DEFAULT_TAG:
                DisableProjectile();
                break;
            case GlobalClass.PLAYER_TAG:
                if (isEnemyBullet)
                {
                    other.GetComponentInParent<Player>().TakeDamage(currentDamage);
                    DisableProjectile();
                }
                break;
            case GlobalClass.PROJECTILE_TAG:
                break;
            case GlobalClass.ENEMY_TAG:
                other.GetComponentInParent<Enemy>().TakeDamage(currentDamage);
                if (waitingToHit)
                {
                    if (bulletPairTime >= 0)
                    {
                        enemyHit = other.gameObject;
                        if (bulletPair.enemyHit == enemyHit)
                        {
                            impactPoint = transform.position;
                            float distance = (bulletPair.impactPoint - impactPoint).sqrMagnitude;
                            Vector3 middlePoint = (impactPoint - bulletPair.impactPoint) / 2;
                            if (distance <= bulletPairDistance)
                            {
                                other.GetComponentInParent<Enemy>().TakeDamage(bonusDamage);
                                Instantiate(bonusHitVFX, middlePoint + transform.position, Quaternion.identity);
                            }
                        }
                    }
                }
                else if (bulletPair)
                {
                    enemyHit = other.gameObject;
                    impactPoint = transform.position;
                    bulletPair.SetWaiting();
                }
                DisableProjectile();
                break;
            case GlobalClass.PICKUP_TAG:
                break;
            case GlobalClass.ROOM_TAG:
                DisableProjectile();
                break;
            case GlobalClass.SPECIAL_TAG:
                if (!other.GetComponent<SphereWeapon>())
                    if (!other.GetComponent<TimeField>())
                        DisableProjectile();
                else
                    other.GetComponent<SphereWeapon>().DeagleBulletHit(this);
                break;
            case GlobalClass.SHIELD_TAG:
                if (other.GetComponent<EnemyShield>())
                {
                    other.GetComponent<EnemyShield>().TakeDamage(currentDamage);
                    DisableProjectile();
                }
                break;
            case GlobalClass.DEFLECT_TAG:
                break;
            case GlobalClass.PROJECTILE_GATE_TAG:
                break;
            case GlobalClass.DETECT_BULLETS_TAG:
                break;
            default:
                Debug.Log("Different String Collision");
                DisableProjectile();
                break;
        }
    }
    
    public void SetBulletPair(GameObject otherBullet)
    {
        bulletPair = otherBullet.GetComponent<HighCaliber>();
    }

    public HighCaliber GetBulletPair()
    {
        HighCaliber pair = bulletPair.GetComponent<HighCaliber>();
        if (pair == null)
            return null;
        return bulletPair.GetComponent<HighCaliber>();
    }

    protected void SetWaiting()
    {
        waitingToHit = true;
    }

    public override void StopMoving()
    {
        base.StopMoving();

        trail.time = Mathf.Infinity;
        positions = new Vector3[trail.positionCount];
        trail.GetPositions(positions);
        trailFrozen = true;
    }

    public override void ContinueMoving()
    {
        base.ContinueMoving();
        if (trailFrozen)
        {
            trail.time = 0.175f;
            for (int i = 0; i < positions.Length; i++)
            {
                trail.AddPosition(positions[i]);
            }
            trailFrozen = false;
        }
    }

    public override void DisableProjectile()
    {
        trailFrozen = false;
        trail.enabled = false;
        pSystem.Stop();
        base.DisableProjectile();
        rb.velocity = Vector3.zero;
    }

    public override void EnableProjectile()
    {
        trail.enabled = true;
        trail.Clear();
        pSystem.Play();
        base.EnableProjectile();
    }
}
