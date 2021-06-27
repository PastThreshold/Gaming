using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedBomb : Projectile
{
    [SerializeField] MeshRenderer mesh;
    [SerializeField] ExplosionVFX explosionPrefab;
    bool isStuck = false;
    float timeBeforeExplosion = 2.5f;

    void Start()
    {
        BaseStart();
        DisableProjectile();
    }

    void Update()
    {
        if (moving)
            transform.rotation = Quaternion.LookRotation(rb.velocity);
    }

    public void StickAndExplode()
    {
        isStuck = true;
        moving = false;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        StartCoroutine("ExplosionTimer");
    }

    IEnumerator ExplosionTimer()
    {
        yield return new WaitForSeconds(timeBeforeExplosion);
        Instantiate(explosionPrefab, transform.position, transform.rotation);
        DisableProjectile();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isStuck)
        {
            return;
        }

        switch (other.tag)
        {
            case GlobalClass.DEFAULT_TAG:
                StickAndExplode();
                break;
            case GlobalClass.PLAYER_TAG:
                transform.parent = GlobalClass.player.transform;
                StickAndExplode();
                break;
            case GlobalClass.PROJECTILE_TAG:
                break;
            case GlobalClass.ENEMY_TAG:
                break;
            case GlobalClass.PICKUP_TAG:
                break;
            case GlobalClass.ROOM_TAG:
                StickAndExplode();
                break;
            case GlobalClass.SPECIAL_TAG:
                if (!other.GetComponent<SphereWeapon>())
                {
                    if (!other.GetComponent<TimeField>())
                        StickAndExplode();
                }
                else
                    rb.velocity = transform.forward * speed * 2;
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
    }

    public override void ContinueMoving()
    {
        base.ContinueMoving();
    }

    public override void DisableProjectile()
    {
        transform.parent = belongsTo.transform;
        isStuck = false;
        moving = false;
        rb.isKinematic = false;
        mesh.enabled = false;
        base.DisableProjectile();
        rb.velocity = Vector3.zero;
        enabled = false;
    }

    public override void EnableProjectile()
    {
        mesh.enabled = true;
        base.EnableProjectile();
    }
}
