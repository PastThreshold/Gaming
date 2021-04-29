using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SBullet : Projectile
{
    [SerializeField] float damageMultiplyer = 2f;
    [SerializeField] float timeBetweenSphereSplit = 0.05f;
    [SerializeField] ParticleSystem burningHeadTrail;
    [SerializeField] ParticleSystem sparksTrail;


    [SerializeField] TrailRenderer trail;
    float originalTrailTime;
    [SerializeField] ParticleSystem pSystem;
    Vector3[] positions;
    bool trailFrozen;
    public bool canSplitBySphere = true;

    [SerializeField] bool penetrateOnKill = false;
    [SerializeField] bool penetrateAlways = false;

    private void Start()
    {
        BaseStart();
        originalTrailTime = trail.time;
        burningHeadTrail.gameObject.SetActive(false);
        sparksTrail.gameObject.SetActive(false);
        DisableProjectile();
    }

    void Update()
    {
        //transform.rotation = Quaternion.LookRotation(rb.velocity);
    }

    private void FixedUpdate()
    {
        BaseFixedUpdate();
    }

    void OnTriggerEnter(Collider other)
    {
        bool penetrates = penetrateAlways;
        switch (other.tag)
        {
            case GlobalClass.DEFAULT_TAG:
                DisableProjectile();
                break;
            case GlobalClass.PLAYER_TAG:
                other.GetComponentInParent<Player>().TakeDamage(currentDamage);
                DisableProjectile();
                break;
            case GlobalClass.PROJECTILE_TAG:
                break;
            case GlobalClass.ENEMY_TAG:
                Enemy enemy = other.GetComponent<Enemy>();
                if (penetrateOnKill)
                {
                    if (enemy.health - currentDamage < 0)
                    {
                        penetrates = true;
                    }
                }
                enemy.TakeDamage(currentDamage, force, transform);
                if (!penetrates)
                    DisableProjectile();
                break;
            case GlobalClass.PICKUP_TAG:
                break;
            case GlobalClass.ROOM_TAG:
                DisableProjectile();
                break;
            case GlobalClass.SPECIAL_TAG:
                if (!other.GetComponent<SphereWeapon>())
                {
                    if (!other.GetComponent<TimeField>())
                        DisableProjectile();
                }
                else
                {
                    if(canSplitBySphere)
                        other.GetComponent<SphereWeapon>().SniperBulletHit(this);
                }
                break;
            case GlobalClass.SHIELD_TAG:
                if (other.GetComponent<EnemyShield>())
                {
                    EnemyShield shield = other.GetComponent<EnemyShield>();
                    penetrates = penetrateAlways;
                    if (penetrateOnKill)
                    {
                        if (shield.GetHealth() - currentDamage < 0)
                        {
                            penetrates = true;
                        }
                    }
                    shield.TakeDamage(currentDamage);
                    if (!penetrates)
                        DisableProjectile();
                }
                break;
            case GlobalClass.DETECT_BULLETS_TAG:
                break;
            default:
                Debug.Log("Different String Collision");
                DisableProjectile();
                break;
        }
    }

    private void UnParentSparks()
    {
        if (burningHeadTrail.isEmitting)
        {
            sparksTrail.transform.parent = null;
            sparksTrail.Stop();
            sparksTrail.gameObject.AddComponent<Destroy>();
            sparksTrail.GetComponent<Destroy>().ResetTime(4f);
        }
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
            trail.time = originalTrailTime;
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
        burningHeadTrail.Stop();
        sparksTrail.Stop();
        base.DisableProjectile();
        rb.velocity = Vector3.zero;
        enabled = false;
    }

    public override void EnableProjectile()
    {
        trail.enabled = true;
        trail.Clear();
        pSystem.Play();
        base.EnableProjectile();
    }

    public void SplitBySphere()
    {
        canSplitBySphere = false;
        StartCoroutine("WaitBetweenSplits");
    }

    IEnumerator WaitBetweenSplits()
    {
        yield return new WaitForSeconds(timeBetweenSphereSplit);
        canSplitBySphere = true;
    }
}
