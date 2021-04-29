using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltProjectile : MonoBehaviour
{
    public enum ProjectileType
    {
        deflected,
        hookshot
    }

    [SerializeField] public ProjectileType projectileType;
    [SerializeField] public bool isEnemyBullet = false;
    [SerializeField] public float speed = 1f;
    [SerializeField] public float startDamage = 1f; protected float currentDamage = 1f;
    [SerializeField] public float force = 1f;
    public bool moving = false;
    public bool beingUsed = false;
    public int spotInArray = 0;
    public ProjectilePool belongsTo;
    Rigidbody rb;

    protected void BaseStart()
    {
        currentDamage = startDamage;
        rb = GetComponent<Rigidbody>();
        if (rb)
        {
            GravityWell.AddProjectile(rb);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<GravityWell>())
            Destroy(gameObject);
    }

    public virtual void StopMoving()
    {
        rb.velocity = Vector3.zero;
        moving = false;
    }

    public virtual void ContinueMoving()
    {
        rb.velocity = transform.forward * speed;
        moving = true;
    }

    public virtual void Freeze()
    {
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        moving = false;
        GetComponent<Collider>().enabled = false;
    }

    public virtual void Unfreeze()
    {
        rb.velocity = transform.forward * speed;
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
        currentDamage = startDamage;
        belongsTo.ReturnBullet(spotInArray);
    }

    /// <summary>
    /// Sets velocity forward * speed if projectiles are frozen, and sets moving true
    /// </summary>
    public virtual void EnableProjectile()
    {
        if (!BulletTime.isFrozen)
        {
            rb.velocity = transform.forward * speed;
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
}
