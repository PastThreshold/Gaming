using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallRocket : Projectile
{
    [SerializeField] GameObject explosionVFX;
    [SerializeField] MeshRenderer mesh;
    [SerializeField] float circleSpeed = 5f;
    [SerializeField] float xWidth = 4f;
    [SerializeField] float yWidth = 4f;
    Vector3 localUp;
    Vector3 localRight;
    float pos1;
    float pos2;
    float timeCounter = 0;
    [SerializeField] TrailRenderer trail;
    Enemy target;
    bool hasTarget;

    void Start()
    {
        BaseStart();
        DisableProjectile();
    }

    void Update()
    {
        if (moving)
        {
            CreateCircleMotion();
        }
    }

    private void FixedUpdate()
    {
        BaseFixedUpdate();
    }

    private void CreateCircleMotion()
    {
        timeCounter += Time.deltaTime * circleSpeed;
        localRight = Quaternion.Euler(0, -90f, 0) * transform.forward;
        localUp = Quaternion.Euler(90, 0, 0) * transform.forward;
        pos1 = Mathf.Cos(timeCounter) * xWidth;
        pos2 = Mathf.Sin(timeCounter) * yWidth;
        rb.MovePosition(transform.position + (localRight * pos1 + localUp * pos2) * Time.deltaTime);
    }

    public void TargetDestroyed()
    {
        rb.velocity = transform.forward * speed;
    }

    private void ExplodeRocket()
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
                Debug.Log("Different String Collision by: " + other.name + ", tag: " + other.tag);
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
        timeCounter = 0;
        moving = false;
        mesh.enabled = false;
        trail.enabled = false;
        base.DisableProjectile();
        rb.velocity = Vector3.zero;
        enabled = false;
    }

    public override void EnableProjectile()
    {
        timeCounter += Random.Range(0f, 6f);
        mesh.enabled = true;
        trail.enabled = true;
        base.EnableProjectile();
    }
}