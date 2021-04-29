using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : Projectile
{
    [SerializeField] GameObject explosionVFX;
    [SerializeField] MeshRenderer mesh;

    private void Start()
    {
        BaseStart();
        DisableProjectile();
    }

    void Update()
    {
        //if (moving)
            //transform.rotation = Quaternion.LookRotation(rb.velocity);
    }

    private void FixedUpdate()
    {
        BaseFixedUpdate();
    }

    public void ExplodeRocket()
    {
        Instantiate(explosionVFX, transform.position, transform.rotation);
        DisableProjectile();
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case GlobalClass.DEFAULT_TAG:
                ExplodeRocket();
                break;
            case GlobalClass.PLAYER_TAG:
                other.GetComponent<Player>().TakeDamage(currentDamage);
                break;
            case GlobalClass.PROJECTILE_TAG:
                break;
            case GlobalClass.ENEMY_TAG:
                ExplodeRocket();
                break;
            case GlobalClass.PICKUP_TAG:
                break;
            case GlobalClass.ROOM_TAG:
                ExplodeRocket();
                break;
            case GlobalClass.SPECIAL_TAG:
                if (!other.GetComponent<SphereWeapon>())
                {
                    if (!other.GetComponent<TimeField>())
                        ExplodeRocket();
                }
                else
                    rb.velocity = transform.forward * speed * 2;
                break;
            case GlobalClass.SHIELD_TAG:
                if (other.GetComponent<EnemyShield>())
                    ExplodeRocket();
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
        moving = false;
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