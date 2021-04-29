using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[SelectionBase]
public class Robot : Enemy
{
    float distToGround;

    [Header("Extra Firing")]
    [SerializeField] float minTimeBetweenShots = 2f;
    [SerializeField] float maxTimeBetweenShots = 4f;
    [SerializeField] int minAmountOfShots = 3;
    [SerializeField] int maxAmountOfShots = 8;
    [SerializeField] GameObject muzzleFlashVFX;

    float angle;
    float rotationSpeed = 50f;
    [SerializeField] LayerMask ignoreTeleportLayer;

    private void Start()
    {
        BaseStart();
        projPool = GlobalClass.basicEnemyPool;
        canShoot = true;
        distToGround = GetComponent<Collider>().bounds.extents.y;
        movingRandomly = false;
        playerPos = player.transform.position;
    }

    private void Update()
    {
        playerPosPrev = playerPos;
        playerPos = player.transform.position;
        if (inFormation)
        {
            switch (activeBehavior)
            {
                case BehaviorController.Behavior.groupProt:
                    if (canShoot)
                        StartCoroutine(Shoot());
                    break;
                default:
                    break;
            }
        }
        else
        {
            if (!randomMovementCooldown)
                StartCoroutine(MoveRandomly());
            if (canShoot)
                StartCoroutine(Shoot());
            if (CheckIfAtEndOfPath())
                StopMovement();
        }
    }

    private void FixedUpdate()
    {
        if (player)
            transform.LookAt(player.transform.position);
    }

    private void LookAtPlayer()
    {
        var localTarget = transform.InverseTransformPoint(player.transform.position);

        angle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        var eulerAngleVelocity = new Vector3(0, angle, 0);
        var deltaRotation = Quaternion.Euler(eulerAngleVelocity * Time.deltaTime * rotationSpeed);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }

    IEnumerator Shoot()
    {
        canShoot = false;
        yield return new WaitForSeconds(Random.Range(minTimeBetweenShots, maxTimeBetweenShots));
        for (int i = 0; i < Random.Range(minAmountOfShots, maxAmountOfShots); i++)
        {
            // If the player is moving and the prediction is right, predict their movement
            // else shoot at them with randomness
            if (playerPosPrev != playerPos)
            {
                if (Extra.RollChance(predictionChance))
                {
                    Vector3 pVelocity = playerPos - playerPosPrev;
                    Vector3 predictedPosition = playerPos + pVelocity * (predictionValue + Vector3.Distance(transform.position, playerPos));
                    firePointRotation.transform.LookAt(Extra.SetYToTransform(predictedPosition, firePointRotation.transform));
                    Projectile bullet = CreateBasicProjectile();
                    bullet.EnableProjectile();
                    yield return new WaitForSeconds(.1f);
                }
                else
                {
                    firePointRotation.transform.LookAt(Extra.SetYToTransform(player.transform.position, firePointRotation.transform));
                    Projectile bullet = CreateBasicProjectile();
                    bullet.EnableProjectile();
                    yield return new WaitForSeconds(.1f);
                }
            }
            else
            {
                firePointRotation.transform.LookAt(Extra.SetYToTransform(player.transform.position, firePointRotation.transform));
                firePointRotation.transform.Rotate(0, Random.Range(-2f, 2f), 0);
                Projectile bullet = CreateBasicProjectile();
                bullet.EnableProjectile();
                yield return new WaitForSeconds(.1f);
            }
        }
        canShoot = true;
    }

    public override void StopMovement()
    {
        base.StopMovement();
    }

    public void StartFSquad(Vector3 postion)
    {
        inFormation = true;
        activeBehavior = BehaviorController.Behavior.robotFSquad;
        StopMovement();
        StopAllCoroutines();
        Move(postion);
        StopCoroutine(MoveRandomly());
    }

    public void FireInSquad(Vector3 postion)
    {
        firePointRotation.LookAt(Extra.SetYToTransform(postion, firePointRotation.transform));
        Projectile bullet = CreateBasicProjectile();
        bullet.EnableProjectile();
    }

    public override void StartGroupProtection(Vector3 position)
    {
        base.StartGroupProtection(position);
        StopRandomMovement();
        StopMovement();
        StopAllCoroutines();
        Move(position);
    }

    public override void SecondStartGroupProtection()
    {
        print("haha yes ok gamers");
        canShoot = true;
    }

    public override void EndFomation()
    {
        base.EndFomation();
        randomMovementCooldown = false;
        canShoot = true;
    }
}