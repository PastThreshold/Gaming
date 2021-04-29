using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastBullet : Projectile
{
    /// <summary>
    /// Entire purpose is for the coroutines since you cannot pass in more than one parameter
    /// </summary>
    private class HitData
    {
        public RaycastHit hit;
        public float timeUntilDeath;
        public Vector3 lastPosition;

        public HitData(RaycastHit hit, float time, Vector3 pos)
        {
            this.hit = hit;
            timeUntilDeath = time;
            lastPosition = pos;
        }
    }

    [Header("Movement")]
    [SerializeField] float minSpeed = 230f;
    [SerializeField] float maxSpeed = 250f;
    Vector3 lastPos = Vector3.zero;
    float timeUntilDeath = 0;

    const float MAX_LIFE = 3f;
    const float RAY_LENGTH = 60f;
    const float LAST_FRAME_WAIT = 0.001f;

    [Header("Disable Components")]
    [SerializeField] TrailRenderer trail;
    [SerializeField] ParticleSystem pSystem;
    Vector3[] positions;
    bool trailFrozen;

    private void Start()
    {
        speed = Random.Range(minSpeed, maxSpeed);
        DisableProjectile();
    }

    private void Update()
    {
        if(moving)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    /// <summary> Stores all colliders hit in array, sorts by distance, 
    /// then will go through array with IsDestroyingCollider() </summary>
    private void RaycastAllObjects()
    {
        RaycastHit[] hits = Physics.RaycastAll(
            transform.position, transform.forward, RAY_LENGTH, GlobalClass.exD.bulletLayerMask);
        hits = Extra.SortHitsByDistance(hits);

        int index = 0;
        int safetyBreak = 0;
        // This will start coroutines for all objects hit and stop once a collider that will destroy the proj is hit
        while (index < hits.Length && !IsDestroyingCollider(hits[index]))
        {
            index++;
            if (Extra.CheckSafetyBreak(safetyBreak, 50))
            {
                Debug.Log("Infinite Loop Safety Break Triggered");
                break;
            }
            safetyBreak++;
        }
    }

    /// <summary> Checks whether the collider hit is on an object that will destroy the projectile or should be ignored </summary>
    /// <returns>Boolean result of whether it will destroy the projectile</returns>
    private bool IsDestroyingCollider(RaycastHit hit)
    {
        switch (hit.transform.tag)
        {
            case GlobalClass.SPECIAL_TAG:
                StartCoroutine("LastPosition", SetLastPosition(hit));
                return false;
            case GlobalClass.DEFLECT_TAG:
                StartCoroutine("LastPositionAndRaycast", SetLastPosition(hit));
                return true;
            case GlobalClass.PROJECTILE_GATE_TAG:
                StartCoroutine("LastPosition", SetLastPosition(hit));
                return false;
            default:
                StartCoroutine("Disable", SetLastPosition(hit));
                return true;
        }
    }

    /// <summary>
    /// Set the time until death based on the relation of distance, speed, and time. Also sets the 
    /// last position for proper rendering. Stores into data object to be passed into coroutine 
    /// </summary>
    private HitData SetLastPosition(RaycastHit hit)
    {
        float time = hit.distance / speed;
        Vector3 lastPos = hit.point;
        return new HitData(hit, time, lastPos);
    }

    /// <summary>
    /// Things are weird when objects move at such high speeds, slowed down, objects correctly die at
    /// calculated speed / time / distance values, but at high speeds, they render past where they should
    /// To combat this the function sets the transform to the end of the ray and makes the object wait for
    /// 0.001 seconds to render for that last frame
    /// </summary>
    IEnumerator Disable(HitData data)
    {
        yield return new WaitForSeconds(data.timeUntilDeath);
        transform.position = data.lastPosition;
        moving = false;
        yield return new WaitForSeconds(LAST_FRAME_WAIT);
        HandleHit(data.hit);
        DisableProjectile();
    }

    IEnumerator LastPosition(HitData data)
    {
        yield return new WaitForSeconds(data.timeUntilDeath);
        transform.position = data.lastPosition;
        yield return new WaitForSeconds(LAST_FRAME_WAIT);
        HandleHit(data.hit);
    }

    IEnumerator LastPositionAndRaycast(HitData data)
    {
        yield return new WaitForSeconds(data.timeUntilDeath);
        transform.position = data.lastPosition;
        moving = false;
        yield return new WaitForSeconds(LAST_FRAME_WAIT);
        HandleHit(data.hit);
        yield return null;
        RaycastAllObjects();
        moving = true;
    }

    /// <summary> Called as last action when the projectile is "colliding with the object" so OnTriggerEnter essentially </summary>
    private void HandleHit(RaycastHit hit)
    {
        if (hit.transform == null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
            return;
        }
        switch (hit.transform.tag)
        {
            case GlobalClass.PLAYER_TAG:
                if (isEnemyBullet)
                    hit.collider.GetComponentInParent<Player>().TakeDamage(currentDamage);
                Instantiate(hitEffect, transform.position, Quaternion.identity);
                break;
            case GlobalClass.ENEMY_TAG:
                if (!isEnemyBullet)
                    hit.collider.GetComponentInParent<Enemy>().TakeDamage(currentDamage, force, transform);
                Instantiate(hitEffect, transform.position, Quaternion.identity);
                Instantiate(enemyHitEffect, transform.position, transform.rotation);
                break;
            case GlobalClass.SHIELD_TAG:
                if (isEnemyBullet)
                    hit.transform.GetComponentInParent<Shield>().TriggeredByRaycast(this);
                else
                    if (hit.transform.GetComponentInParent<EnemyShield>())
                        hit.transform.GetComponentInParent<EnemyShield>().TriggeredByRaycast(this);
                Instantiate(hitEffect, transform.position, Quaternion.identity);
                break;
            case GlobalClass.SPECIAL_TAG:
                if (hit.transform.GetComponent<SphereWeapon>())
                    hit.transform.GetComponent<SphereWeapon>().RaycastBulletHit(this);
                break;
            case GlobalClass.DEFLECT_TAG:
                if (isEnemyBullet)
                    hit.transform.GetComponentInParent<Deflect>().TriggeredByRaycast(this);
                else
                    hit.transform.GetComponentInParent<EnemyDeflect>().TriggeredByRaycast(this);
                break;
            case GlobalClass.PROJECTILE_GATE_TAG:
                if (isEnemyBullet)
                    hit.transform.GetComponentInParent<EnemyShield>().TriggeredByRaycast(this);
                else if (hit.transform.GetComponent<Shield>())
                    hit.transform.GetComponent<Shield>().TriggeredByRaycast(this);
                break;
            default:
                Instantiate(hitEffect, transform.position, Quaternion.identity);
                break;
        }
    }

    public override void StopMoving()
    {
        moving = false;
        StopCoroutine("Disable");

        trail.time = Mathf.Infinity;
        positions = new Vector3[trail.positionCount];
        trail.GetPositions(positions);
        trailFrozen = true;
    }

    public override void ContinueMoving()
    {
        moving = true;
        if (trailFrozen)
        {
            trail.time = 0.175f;
            for (int i = 0; i < positions.Length; i++)
            {
                trail.AddPosition(positions[i]);
            }
            trailFrozen = false;
        }
        RaycastAllObjects();
    }

    public override void DisableProjectile()
    {
        moving = false;
        trailFrozen = false;
        trail.enabled = false;
        pSystem.Stop();
        base.DisableProjectile();
        timeUntilDeath = 0;
    }

    public override void EnableProjectile()
    {
        trail.enabled = true;
        pSystem.Play();

        if (!BulletTime.isFrozen)
        {
            moving = true;
            RaycastAllObjects();
        }
    }
}
