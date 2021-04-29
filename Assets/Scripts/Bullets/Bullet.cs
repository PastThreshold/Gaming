using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Projectile
{
    [Header("Disable Components")]
    [SerializeField] TrailRenderer trail;
    [SerializeField] ParticleSystem pSystem;
    Vector3[] positions;
    bool trailFrozen = false;

    private void Start()
    {
        BaseStart();
        DisableProjectile();
    }

    void FixedUpdate()
    {
        BaseFixedUpdate();
    }

    private void OnTriggerEnter(Collider other)
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
                if (!isEnemyBullet)
                {
                    other.GetComponentInParent<Enemy>().TakeDamage(currentDamage);
                    DisableProjectile();
                }
                break;
            case GlobalClass.PICKUP_TAG:
                break;
            case GlobalClass.ROOM_TAG:
                DisableProjectile();
                break;
            case GlobalClass.SPECIAL_TAG:
                if (!other.GetComponent<SphereWeapon>() && !other.GetComponent<TimeField>())
                    DisableProjectile();
                break;
            case GlobalClass.SHIELD_TAG:
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
        trail.enabled = false;
        pSystem.Stop();
        moving = false;
        rb.velocity = Vector3.zero;
        base.DisableProjectile();
    }

    public override void EnableProjectile()
    {
        trail.enabled = true;
        trail.Clear();
        pSystem.Play();
        moving = true;
        base.EnableProjectile();
    }
}
