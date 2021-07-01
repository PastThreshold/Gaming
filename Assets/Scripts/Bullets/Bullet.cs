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
    Vector3 lastPos;
    Vector3 nowPos;

    private void Start()
    {
        BaseStart();
        DisableProjectile();
        nowPos = transform.position;
    }

    void FixedUpdate()
    {
        lastPos = nowPos;
        nowPos = transform.position;
        //Extra.DrawBox(lastPos, 0.25f, Color.white, 0.25f);
        BaseFixedUpdate();
        print(rb.velocity.sqrMagnitude);
        if (rb.velocity.sqrMagnitude >= 6200)
            RaycastForCollision();
    }

    private void OnTriggerEnter(Collider collider)
    {
        HandleCollision(collider);
    }

    private void HandleCollision(Collider collider)
    {
        switch (collider.tag)
        {
            case GlobalClass.DEFAULT_TAG:
                DisableProjectile();
                break;
            case GlobalClass.PLAYER_TAG:
                if (isEnemyBullet)
                {
                    collider.GetComponentInParent<Player>().TakeDamage(currentDamage);
                    DisableProjectile();
                }
                break;
            case GlobalClass.PROJECTILE_TAG:
                break;
            case GlobalClass.ENEMY_TAG:
                if (!isEnemyBullet)
                {
                    collider.GetComponentInParent<Enemy>().TakeDamage(currentDamage);
                    DisableProjectile();
                }
                break;
            case GlobalClass.PICKUP_TAG:
                break;
            case GlobalClass.ROOM_TAG:
                DisableProjectile();
                break;
            case GlobalClass.SPECIAL_TAG:
                if (!collider.GetComponent<SphereWeapon>() && !collider.GetComponent<TimeField>())
                    DisableProjectile();
                break;
            case GlobalClass.SHIELD_TAG:
                DisableProjectile();
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

    protected void RaycastForCollision()
    {
        RaycastHit hit;
        //Debug.DrawRay(lastPos, (lastPos - nowPos).normalized * (lastPos - nowPos).magnitude, Color.blue, 0.25f);
        if (Physics.Raycast(lastPos, (lastPos - nowPos).normalized, out hit, (lastPos - nowPos).magnitude, GlobalClass.exD.bulletLayerMask))
        {
            HandleCollision(hit.collider);
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
