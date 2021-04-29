using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Walker : Enemy
{
    [Header("References")]
    [SerializeField] GameObject stand;
    [SerializeField] GameObject turret;
    [SerializeField] GameObject turretBarrel;
    [SerializeField] Animator anim;
    WalkerLeg[] legs = new WalkerLeg[4];
    WalkerLeg movingLeg = null;
    bool waitingForLegs = false;

    [Header("Data")]
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] float waitBetweenAttacks = 2f;
    bool choosingAttack = true;
    bool firing;
    bool upper = false;
    bool upperDirection = false;
    Vector3 movementDirection;

    [SerializeField] float aimUpAtPlayerValue = 1f;


    Vector3 rot;
    [Range(0f, 100f)] [SerializeField] float turretAngleClamp = 30f;

    [SerializeField] float barrelRotationVal = 1f;
    [SerializeField] float rotationSpeed = 0.5f;
    [Range(-1f, 1f)] [SerializeField] float turretRotationAcceptanceValue = 0.75f;
    bool playerHidingUnderneath = false;
    bool threeSixty = false;
    [SerializeField] float playerHiddenTimer = 1.2f;
    float timer;


    // Start is called before the first frame update
    void Start()
    {
        BaseStart();
        projPool = GlobalClass.basicEnemyPool;
        firing = false;
        playerPos = player.transform.position;
        timer = playerHiddenTimer;
        legs = GetComponentsInChildren<WalkerLeg>();
    }

    void Update()
    {
        playerPosPrev = playerPos;
        playerPos = player.transform.position;

        if (firing)
        {
            // Rotates the barrel
            turretBarrel.transform.Rotate(Vector3.up * barrelRotationVal);
            if (upper && !upperDirection)
            {
                movementDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f)).normalized;
                upperDirection = true;
                StartCoroutine("MoveAndWait");
            }
        }

        if (inFormation)
            return;

        if (upper && !waitingForLegs)
        {
            Quaternion toRotation = Quaternion.LookRotation(player.transform.position - turret.transform.position);
            turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, toRotation, Time.deltaTime * rotationSpeed);
            //.transform.LookAt(new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z));
            turret.transform.rotation = Quaternion.RotateTowards(turret.transform.rotation, toRotation, rotationSpeed);
            stand.transform.LookAt(new Vector3(player.transform.position.x, stand.transform.position.y, player.transform.position.z));
            rb.MovePosition(rb.position + movementDirection * movementSpeed * Time.deltaTime);
        }

        if (choosingAttack)
        {
            ChooseAttack();
            ResetLegPositions();
        }
        else if (waitingForLegs)
        {
            foreach (WalkerLeg leg in legs)
            {
                if (leg.IsMoving())
                {
                    break;
                }
                waitingForLegs = false;
            }
            if (!waitingForLegs)
            {
                if (upper)
                {
                    anim.SetTrigger("SwitchToUpper");
                    WalkerLeg.Disable();
                    StartCoroutine("WaitForAnimationToEnd");
                }
                else
                {
                    anim.SetTrigger("SwitchToLower");
                    WalkerLeg.Disable();
                    StartCoroutine("WaitForAnimationToEnd");
                }
            }
        }

        if (playerHidingUnderneath && upper)
        {
            timer -= 1f * Time.deltaTime;
            if (timer <= 0)
            {
                threeSixty = true;
                playerHidingUnderneath = false;
                StopCoroutine("Fire");
                ResetLegPositions();
                upper = false;
            }
        }
        else if (upper && !playerHidingUnderneath)
        {
            timer = Mathf.Clamp(timer + 1f * Time.deltaTime, 0f, playerHiddenTimer);
        }

        rot = turret.transform.eulerAngles;
        rot.x = Mathf.Clamp(rot.x, Mathf.NegativeInfinity, turretAngleClamp);
        turret.transform.eulerAngles = rot;
    }

    private void ChooseAttack()
    {
        playerHidingUnderneath = false;
        timer = playerHiddenTimer;
        choosingAttack = false;
        movementDirection = Vector3.zero;
        upper = false;
        if (Extra.RollChance(50f))
            upper = false;
        else
            upper = true;
    }

    IEnumerator Fire()
    {
        // Upper fires while the walker is standing up, bullets are fired down at the player making them easier
        // to dodge but fire is focused entirly on player. If player stays under or behind the walker for too long it will
        // reset and spew bullets in a 360
        if (upper) 
        {
            WalkerLeg.Enable();
            firing = true;
            for (int i = 0; i < 100; i++)
            {
                if (Vector3.Dot(Extra.SetYToZero((player.transform.position - turret.transform.position).normalized), Extra.SetYToZero(turret.transform.forward)) < turretRotationAcceptanceValue)
                {
                    firePointRotation.transform.rotation = turret.transform.rotation;
                    playerHidingUnderneath = true;
                }
                else
                {
                    playerHidingUnderneath = false;
                    // If player is not standing still predict their movement
                    // else fire straight on with some randomness
                    if (playerPos != playerPosPrev)
                    {
                        Vector3 pVelocity = playerPos - playerPosPrev;
                        Vector3 predictedPosition = playerPos + pVelocity * (predictionValue + 
                            Random.Range(-5f, 10f) + Vector3.Distance(transform.position, playerPos));

                        firePointRotation.transform.LookAt(new Vector3(predictedPosition.x, 
                            playerPos.y + aimUpAtPlayerValue, predictedPosition.z));
                    }
                    else
                    {
                        Vector3 directionToLookWithRandomness = Extra.SetYToZero(playerPos - transform.position);
                        directionToLookWithRandomness = Quaternion.Euler(0, Random.Range(-4f, 4f), 0) 
                            * directionToLookWithRandomness;
                        directionToLookWithRandomness += transform.position;

                        firePointRotation.transform.LookAt(new Vector3(directionToLookWithRandomness.x, 
                            playerPos.y + aimUpAtPlayerValue, directionToLookWithRandomness.z));
                    }
                }
                Projectile bullet = CreateBasicProjectile();
                bullet.EnableProjectile();
                yield return new WaitForSeconds(fireRate);
            }
        }

        // Shoots while lowered in full 360
        else if (threeSixty)
        {
            for (int ii = 0; ii < 4; ii++)
            {
                firing = true;
                Vector3 playerDir = player.transform.position - transform.position;
                Vector3 startAngle = Quaternion.Euler(0, 30f, 0) * playerDir;
                Vector3 positionToLook = startAngle + transform.position;
                float angleEachShot = 6f;
                for (int i = 0; i < 60; i++)
                {
                    turret.transform.LookAt(positionToLook);
                    stand.transform.LookAt(positionToLook);
                    firePointRotation.rotation = turret.transform.rotation;
                    Projectile bullet = CreateBasicProjectile();
                    bullet.EnableProjectile();
                    positionToLook = Quaternion.Euler(0, -angleEachShot * i, 0) * startAngle + transform.position;
                    positionToLook.y = turret.transform.position.y;
                    yield return new WaitForSeconds(fireRate / 2);
                }
                //yield return new WaitForSeconds(0.5f);
            }
            threeSixty = false;
            timer = playerHiddenTimer;
        }
        else  // Shoots while lowered in a wave patter near the player
        {
            for (int ii = 0; ii < 4; ii++)
            {
                firing = true;
                Vector3 playerDir = player.transform.position - transform.position;
                Vector3 startAngle = Quaternion.Euler(0, 30f, 0) * playerDir;
                Vector3 positionToLook = startAngle + transform.position;
                float angleEachShot = 2.5f;
                for (int i = 0; i < 35; i++)
                {
                    turret.transform.LookAt(positionToLook);
                    stand.transform.LookAt(positionToLook);
                    firePointRotation.rotation = turret.transform.rotation;
                    Projectile bullet = CreateBasicProjectile();
                    bullet.EnableProjectile();
                    positionToLook = Quaternion.Euler(0, -angleEachShot * i, 0) * startAngle + transform.position;
                    positionToLook.y = turret.transform.position.y;
                    yield return new WaitForSeconds(fireRate);
                }
                startAngle = Quaternion.Euler(0, -30f, 0) * (player.transform.position - transform.position);
                for (int i = 0; i < 35; i++)
                {
                    turret.transform.LookAt(positionToLook);
                    stand.transform.LookAt(positionToLook);
                    firePointRotation.rotation = turret.transform.rotation;
                    Projectile bullet = CreateBasicProjectile();
                    bullet.EnableProjectile();
                    positionToLook = Quaternion.Euler(0, angleEachShot * i, 0) * startAngle + transform.position;
                    positionToLook.y = turret.transform.position.y;
                    yield return new WaitForSeconds(fireRate);
                }
                //yield return new WaitForSeconds(0.5f);
            }
        }
        firing = false;
        yield return new WaitForSeconds(waitBetweenAttacks);
        choosingAttack = true;
        anim.ResetTrigger("SwitchToUpper");
        anim.ResetTrigger("SwitchToLower");
    }

    IEnumerator WaitForAnimationToEnd()
    {
        yield return new WaitForSeconds(0.95f);
        StartCoroutine("Fire");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Room")
        {
            upperDirection = false;
            movementDirection = Vector3.zero;
            StopCoroutine("MoveAndWait");
        }
    }

    // Before the animator takes over the legs need to be reset, this queues all legs then Update() checks if array is empty
    private void ResetLegPositions()
    {
        foreach(WalkerLeg leg in legs)
        {
            if (leg.IsMoving())
                leg.StopMovement();
            leg.CalculateMovement(true);
        }
        waitingForLegs = true;
    }

    IEnumerator MoveAndWait()
    {
        yield return new WaitForSeconds(Random.Range(2f, 6f));
        upperDirection = false;
        movementDirection = Vector3.zero;
    }

    public void StartMountedWalker(Vector3 position)
    {
        inFormation = true;
        StopAllCoroutines();
    }
}
