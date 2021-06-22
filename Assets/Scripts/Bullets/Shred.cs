using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shred : Projectile
{
    [Header("Universal Data")]
    [SerializeField] Collider damageCollider;
    [SerializeField] Collider physicsCollider;

    [Header("Normal Fuction Data")]
    [SerializeField] float timeToDestroy = 5f;
    [SerializeField] float damageSubtractValue = 0.5f;
    [SerializeField] float speedSubtractValue = 0.5f;

    [Header("Sphere Function Data")]
    [SerializeField] float sphereSpeed = 10f;
    [SerializeField] float maxFindDistance;

    bool waitingToDamage;
    float damageInterval = 0.4f;
    Enemy stuckTo;
    EnemyShield stuckToShield;
    bool hitBySphere = false;
    Enemy nextTarget;
    Vector3 targetLookAt = Vector3.zero;
    List<Enemy> alreadyHit = new List<Enemy>();
    List<Enemy> allEnemies;

    private void Start()
    {
        BaseStart();
        damageCollider.enabled = true;
        physicsCollider.enabled = false;
        DisableProjectile();
    }

    void FixedUpdate()
    {
        if (nextTarget != null)
            rb.velocity = (nextTarget.transform.position - transform.position).normalized * speed;

        BaseFixedUpdate();
    }

    void Update()
    {
        if (!waitingToDamage)
            StartCoroutine(DamageTarget());
    }

    IEnumerator DamageTarget()
    {
        waitingToDamage = true;
        if (stuckTo != null)
            stuckTo.TakeDamage(currentDamage);
        else if (stuckToShield != null)
            stuckToShield.TakeDamage(currentDamage);
        yield return new WaitForSeconds(damageInterval);
        waitingToDamage = false;
    }

    void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case GlobalClass.DEFAULT_TAG:
                DisableProjectile();
                break;
            case GlobalClass.PLAYER_TAG:
                other.GetComponent<Player>().TakeDamage(currentDamage);
                break;
            case GlobalClass.PROJECTILE_TAG:
                break;
            case GlobalClass.ENEMY_TAG:
                if (hitBySphere)
                {
                    bool hitAlready = false;
                    var thisEnemy = other.GetComponentInParent<Enemy>();
                    foreach (Enemy enemyAlreadyHit in alreadyHit)
                    {
                        if (thisEnemy == enemyAlreadyHit)
                        {
                            hitAlready = true;
                        }
                    }
                    if (!hitAlready)
                    {
                        thisEnemy.TakeDamage(currentDamage);
                        alreadyHit.Add(thisEnemy);
                    }
                    if (alreadyHit.Count == 1)
                        speed = sphereSpeed;
                    CalculateClosestTarget();
                }
                else
                {
                    rb.isKinematic = true;
                    moving = false;
                    stuckTo = other.GetComponentInParent<Enemy>();
                    transform.parent = stuckTo.transform;
                    damageCollider.enabled = false;
                }
                break;
            case GlobalClass.PICKUP_TAG:
                break;
            case GlobalClass.ROOM_TAG:
                moving = false;
                StopMoving();
                StartCoroutine("WaitBeforeDisappear");
                break;
            case GlobalClass.SPECIAL_TAG:
                if (!other.GetComponent<SphereWeapon>())
                {
                    if (!other.GetComponent<TimeField>())
                        DisableProjectile();
                }
                else
                    hitBySphere = true;
                break;
            case GlobalClass.SHIELD_TAG:
                if (other.GetComponent<EnemyShield>())
                {
                    rb.isKinematic = true;
                    moving = false;
                    stuckToShield = other.GetComponentInParent<EnemyShield>();
                    transform.parent = stuckToShield.transform;
                    damageCollider.enabled = false;
                }
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

    /// <summary>
    /// Calculated the closest enemy that is still within the find range of the saw
    /// If none if found, Unparents and enables collision
    /// </summary>
    private void CalculateClosestTarget()
    {
        allEnemies = LevelController.allEnemiesInScene;
        float distanceBetween = Mathf.Infinity;
        float distance;
        bool alreadyHit;
        nextTarget = null;
        print(allEnemies.Count);

        foreach (Enemy enemy in allEnemies)
        {
            alreadyHit = false;
            foreach (Enemy alreadyHitEnemy in this.alreadyHit)
            {
                if (enemy == alreadyHitEnemy)
                {
                    alreadyHit = true;
                }
            }
            if (!alreadyHit)
            {
                distance = Extra.SquaredDistanceWithoutY(transform.position, enemy.transform.position);
                print(distance + " current highest: " + distanceBetween + " and findDistance: " + maxFindDistance);
                if (distanceBetween > distance && distance <= maxFindDistance)
                {
                    distanceBetween = distance;
                    nextTarget = enemy;
                }
            }
        }

        if (nextTarget == null)
        {
            Unparent();
        }
    }

    /// <summary>
    /// Unparents the saw from whatever its attached to, enables collision with the ground and walls,
    /// Subject to change since final version will likely not be a sawblade
    /// Called by enemies before death using broadcast message
    /// </summary>
    public void Unparent()
    {
        print("fuck you");
        stuckTo = null;
        gameObject.layer = 16;
        damageCollider.enabled = false;
        physicsCollider.enabled = true;
        transform.parent = null;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.velocity = transform.forward * 10;
        StartCoroutine("WaitBeforeDisappear");
    }

    IEnumerator WaitBeforeDisappear()
    {
        yield return new WaitForSeconds(2.5f);
        DisableProjectile();
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
        rb.useGravity = false;
        transform.parent = belongsTo.transform;
        stuckTo = null;
        stuckToShield = null;
        nextTarget = null;
        hitBySphere = false;
        gameObject.layer = GlobalClass.PROJECTILE_LAYER;
        rb.velocity = Vector3.zero;
        base.DisableProjectile();
    }

    public override void EnableProjectile()
    {
        rb.isKinematic = false;
        base.EnableProjectile();
    }
}
