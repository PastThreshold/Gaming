using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeflectedProjectile : AltProjectile
{

    [SerializeField] Projectile.ProjectileType deflectedType;
    const float MAX_LIFE = 3f;
    const float RAY_LENGTH = 60f;
    const float LAST_FRAME_WAIT = 0.001f;

    Vector3 lastPos = Vector3.zero;
    float timeUntilDeath = 0;

    [SerializeField] ParticleSystem pSystem = null;
    [SerializeField] TrailRenderer trail = null;
    bool trailFrozen = false;
    Vector3[] positions = new Vector3[0];

    private void Start()
    {
        DisableProjectile();
    }

    private void Update()
    {
        if (moving)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    /// <summary>
    /// Raycasts forward for enemies, calclates the amount of moves until reaching the point
    /// of contact, the check for this happen in TimeBetweenMoves()
    /// 
    /// For the special tag which is only the sphere, a second raycast it created with no sphere layer
    /// Another switch for tag is done with near identical execution
    /// </summary>
    private void RaycastForEnemy(bool sphereIncluded)
    {
        LayerMask layer;
        if (sphereIncluded)
            layer = GlobalClass.exD.bulletLayerMask;
        else
            layer = GlobalClass.exD.bulletsNoDefault;

        if (Physics.Raycast(transform.position, transform.forward,
            out RaycastHit hit, RAY_LENGTH, layer))
        {
            switch (hit.transform.tag)
            {
                case GlobalClass.ENEMY_TAG:
                    hit.collider.GetComponentInParent<Enemy>().TakeDamage(currentDamage);
                    lastPos = hit.point;
                    timeUntilDeath = hit.distance / speed;
                    break;
                case GlobalClass.ROOM_TAG:
                    lastPos = hit.point;
                    timeUntilDeath = hit.distance / speed;
                    break;
                case GlobalClass.SPECIAL_TAG:
                    hit.collider.GetComponent<SphereWeapon>(); //SOMETHING
                    RaycastForEnemy(false);
                    break;
                default:
                    Debug.Log("Different String Collision by: " + hit.transform.name + ", tag: " + hit.transform.tag);
                    DisableProjectile();
                    break;
            }
        }
        else
            timeUntilDeath = MAX_LIFE;

        StartCoroutine("Disable");
    }

    /// <summary>
    /// Things are weird when objects move at such high speeds, slowed down, objects correctly die at
    /// calculated speed / time / distance values, but at high speeds, they render past where they should
    /// To combat this the function sets the transform to the end of the ray and makes the object wait for
    /// 0.001 seconds to render for that last frame
    /// </summary>
    IEnumerator Disable()
    {
        yield return new WaitForSeconds(timeUntilDeath);
        moving = false;
        transform.position = lastPos;
        yield return new WaitForSeconds(LAST_FRAME_WAIT);
        DisableProjectile();
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
        RaycastForEnemy(true);
    }

    public override void DisableProjectile()
    {
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
            RaycastForEnemy(true);
        }
    }
}
