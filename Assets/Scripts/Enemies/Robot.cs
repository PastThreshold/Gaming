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
    [SerializeField] bool bezierMovment = true;

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
        playerPosPrev = player.GetPreviousPos();
        playerPos = player.GetCurrentPos();
        if (inFormation)
        {
            print(nma.hasPath + " Name: " + name + " Path Pending: " + nma.pathPending);

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
            {
                if (IsOutOfBounds())
                {
                    MoveBackInBounds();
                }
                else
                {
                    if (bezierMovment)
                    {
                        StartCoroutine(MoveRandomly());
                    }
                    else
                        MoveRandomlyStraight();
                }
            }
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

    protected void MoveRandomlyStraight()
    {
        movingRandomly = true;
        randomMovementCooldown = true;
        Vector3 start = transform.position;
        Vector3 endPos = start + Extra.CreateRandomVectorWithMagnitude(randMoveDistMin, randMoveDistMax);
        if (NavMesh.SamplePosition(endPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            endPos = hit.position;
        }
        else
        {
            NavMesh.FindClosestEdge(endPos, out NavMeshHit edge, NavMesh.AllAreas);
            endPos = edge.position;
        }

        Move(endPos);
        StartCoroutine(RandomMovementCooldown());
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
        movingRandomly = false;
    }

    public void StartFSquad(Vector3 position)
    {
        print("this is real this is me" + name);
        inFormation = true;
        activeBehavior = BehaviorController.Behavior.robotFSquad;
        StopRandomMovement();
        StopMovement();
        StopAllCoroutines();
        StartCoroutine(WaitForEndFrameAndMove(position));
    }

    /// <summary> For whatever reason trying to use Move() for StartFSquad would result in half the enemies not moving. 
    /// And this somehow fixes that </summary>
    IEnumerator WaitForEndFrameAndMove(Vector3 position)
    {
        yield return new WaitForEndOfFrame();
        Move(position, MovementReason.none);
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
        Move(position, MovementReason.none);
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