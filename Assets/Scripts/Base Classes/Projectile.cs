using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private class SavedSpeedUp
    {
        public GameObject objCalledThis;
        public float savedSpeed;

        public SavedSpeedUp(GameObject obj, float speed)
        {
            objCalledThis = obj;
            savedSpeed = speed;
        }
    }

    public enum ProjectileType
    {
        arbullet,
        sniperBullet,
        pellet,
        deagleBullet,
        stickyBomb,
        sawblade,
        chargeShot,
        rocket,
    }

    [SerializeField] public ProjectileType projectileType;
    [SerializeField] public bool isEnemyBullet = false;
    [SerializeField] public float speed = 1f; protected float currentSpeed;
    [SerializeField] public float damage = 1f; protected float currentDamage = 1f;
    [SerializeField] public float force = 1f;
    [SerializeField] public int level = 1; // This is for specific cases like the sphere
    public bool moving = false;
    public bool beingUsed = false;
    public int spotInArray = 0;
    public ProjectilePool belongsTo;
    [SerializeField] protected HitEffect hitEffect;
    [SerializeField] protected HitEffect enemyHitEffect;
    Transform autoTargetLockedEnemy;
    bool hasTarget = false;
    float turnSpeed = 1.5f;
    


    Collider objCollider;
    protected Rigidbody rb;

    // TimeField
    List<SavedSpeedUp> addedSpeed = new List<SavedSpeedUp>();

    float totalTimesDeflected = 0;

    void Start()
    {
        BaseStart();
    }

    protected void BaseStart()
    {
        currentDamage = damage;
        currentSpeed = speed;
        rb = GetComponent<Rigidbody>();
        if (GetComponent<Rigidbody>())
        {
            GravityWell.AddProjectile(rb);
        }
    }

    protected void BaseFixedUpdate()
    {
        if (hasTarget)
        {
            if (autoTargetLockedEnemy == null)
            {
                hasTarget = false;
                return;
            }
            rb.velocity = transform.forward * currentSpeed;
            Quaternion rotation = Quaternion.LookRotation(autoTargetLockedEnemy.position - transform.position);
            rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, turnSpeed));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<GravityWell>())
            Destroy(gameObject);
    }

    public void SpeedUpForRestOfLife(float factor)
    {
        currentSpeed *= factor;
        if (rb != null)
            rb.velocity = transform.forward * speed;
    }

    // These Methods are specific to the Time Field / Stim pickup
    public void SpeedUp(GameObject obj, float factor)
    {
        float added = currentSpeed * factor - currentSpeed;
        addedSpeed.Add(new SavedSpeedUp(obj, added));
        currentSpeed += added;
        rb.velocity = transform.forward * currentSpeed;
    }

    public void SpeedDown(GameObject obj)
    {
        foreach(SavedSpeedUp su in addedSpeed)
        {
            if (su.objCalledThis == obj)
            {
                currentSpeed -= su.savedSpeed;
                addedSpeed.Remove(su);
                break;
            }
        }
        rb.velocity = transform.forward * currentSpeed;
    }
 
    public virtual void StopMoving()
    {
        rb.velocity = Vector3.zero;
        moving = false;
    }

    public virtual void ContinueMoving()
    {
        rb.velocity = transform.forward * currentSpeed;
        moving = true;
    }


    /// <summary>
    /// Specific to the shield ability for now, likely to be removed
    /// </summary>
    public virtual void Freeze()
    {
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        moving = false;
        GetComponent<Collider>().enabled = false;
    }

    public virtual void Unfreeze()
    {
        rb.velocity = transform.forward * currentSpeed;
        rb.isKinematic = false;
        moving = true;
        GetComponent<Collider>().enabled = true;
    }

    /// <summary>
    /// Object Pooling, makes sure the projectile will reset damage, speed, and force
    /// Projectiles call disableProjectile themselves while the projPool calls enable
    /// every projectile script should override these methods to account for differences in 
    /// values, variables, and rendering components like particle systems and meshes
    /// 
    /// Returns to projectile pool, resets damage, and sets moving false;
    /// </summary>
    public virtual void DisableProjectile()
    {
        moving = false;
        hasTarget = false;
        autoTargetLockedEnemy = null;
        currentDamage = damage;
        currentSpeed = speed;
        totalTimesDeflected = 0;
        addedSpeed.Clear();
        belongsTo.ReturnBullet(spotInArray);
    }

    /// <summary>
    /// Sets velocity forward * speed if projectiles are frozen, and sets moving true
    /// </summary>
    public virtual void EnableProjectile()
    {
        if (!BulletTime.isFrozen)
        {
            rb.velocity = transform.forward * currentSpeed;
            moving = true;
        }
    }

    /// <summary>
    /// Increases damage by multipling by factor
    /// </summary>
    /// <param name="factor"></param>
    public void ChangeDamage(float factor)
    {
        currentDamage *= factor;
    }


    public void FlipSide()
    {
        isEnemyBullet = !isEnemyBullet;
        if (gameObject.layer == GlobalClass.PROJECTILE_LAYER)
        {
            gameObject.layer = GlobalClass.ENEMY_PROJECTILE_LAYER;
        }
        else
        {
            gameObject.layer = GlobalClass.PROJECTILE_LAYER;
        }
    }

    public void Deflect()
    {
        totalTimesDeflected++;
    }

    public float GetTotalDeflections()
    {
        return totalTimesDeflected;
    }

    public float GetDamage()
    {
        return currentDamage;
    }

    public void SetTarget(Transform enemy)
    {
        autoTargetLockedEnemy = enemy;
        hasTarget = true;
    }
}
