
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assassin : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] float blinkDamage = 50f;
    [SerializeField] float dashDamage = 100f;

    [Header("Blink Charges")]
    [SerializeField] int teleChargeMax = 3;
    [SerializeField] int teleportCharges = 3;
    bool atMaxCharge;
    bool recharging = false;
    [Tooltip("This is the time it takes for one teleport charge to come back")]
    [SerializeField] float chargeRegenTime = 3f;

    [Header("Lethal Blink Attack")]
    [SerializeField] float closeEnoughForLethal = 20f;
    [SerializeField] float lethalTeleDistance = 7f;
    [SerializeField] float lethBlinkTimeSpeed = 0.35f;
    [SerializeField] float blinkCooldownTime = 0.75f;
    bool waitingBetweenBlink = false;
    bool lethalBlinking= false;
    bool lethalCalculatingNextPosition = false;
    [SerializeField] float attackPlayerRange = 1f;

    [SerializeField] float lethalDashTimeSpeed = 0.25f;
    [SerializeField] float dashWaitTime = 2f;
    bool lethalDash = false;
    bool dashing = false;

    [Header("Other")]
    [Tooltip("This is the max distance the teleport is except for random. Player within this distance will be hit on teleport")]
    [SerializeField] float teleAreaDistance = 5.5f;
    [Tooltip("This is the angle at which random ignores towards Player. It is like a cone shape with both sides extruded and the middle deleted")]
    [SerializeField] float randomTeleportAngleIgnore = 30f;
    [Tooltip("These are the layers that are being ignored by the teleport area checker")]
    [SerializeField] LayerMask ignoreTeleportLayer;



    Player player;
    Rigidbody rb;
    Vector3 differenceVector;
    float distance;
    bool canTeleport;
    bool canTeleportRandomly;
    Vector3 point = Vector3.zero;
    Collider[] bullets;
    Vector3 movement;



    void Start()
    {
        if (teleportCharges == teleChargeMax)
            atMaxCharge = true;

        player = FindObjectOfType<Player>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (atMaxCharge && !lethalDash && !lethalBlinking)
        {
            Vector3 directionToPlayer = player.transform.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;
            if (distanceToPlayer < closeEnoughForLethal)
            {
                lethalBlinking = true;
                lethalCalculatingNextPosition = true;
            }
            else
            {
                lethalDash = true;
                StartCoroutine("ChargeDash");
            }
        }

        if (dashing)
        {
            LethalDash();
        }

        if (lethalBlinking)
        {
            LethalBlink();
        }

        if (!atMaxCharge && !lethalBlinking && !recharging)
            StartCoroutine("RegenCharge");

        transform.LookAt(Extra.SetYToTransform(player.transform.position, transform));
    }

    private void LethalDash()
    {
        transform.position += movement * Time.deltaTime;
    }

    IEnumerator ChargeDash()
    {
        GetComponent<Animation>().Play("ChargeDash");
        yield return new WaitForSeconds(dashWaitTime);
        Vector3 dir = player.transform.position - transform.position;
        movement = dir - dir.normalized * 1.5f;
        float speed = movement.magnitude / lethalDashTimeSpeed;
        movement = movement.normalized * speed;
        movement.y = 0;
        dashing = true;
        rb.isKinematic = true;
        StartCoroutine("WaitForDashToEnd");
    }

    IEnumerator WaitForDashToEnd()
    {
        yield return new WaitForSeconds(lethalDashTimeSpeed);
        teleportCharges = 0;
        atMaxCharge = false;
        rb.isKinematic = false;
        dashing = false;
        lethalDash = false;

        if ((player.transform.position - transform.position).magnitude < closeEnoughForLethal)
        {
            // Check for player deflecting by raycast
            player.TakeDamage(dashDamage);
        }
    }

    private void LethalBlink()
    {
        if (!waitingBetweenBlink)
        {
            if (lethalCalculatingNextPosition)
            {
                if ((player.transform.position - transform.position).magnitude < lethalTeleDistance)
                {
                    Vector3 directionToPlayer = player.transform.position - transform.position;
                    directionToPlayer += Quaternion.Euler(0, 6f, 0) * Vector3.forward;
                    float speed = directionToPlayer.magnitude / lethBlinkTimeSpeed;
                    movement = directionToPlayer.normalized * speed;
                }
                else
                {
                    Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
                    float speed = lethalTeleDistance / lethBlinkTimeSpeed;
                    movement = Quaternion.Euler(0, UnityEngine.Random.Range(-30, 30), 0) * (directionToPlayer * speed);
                }
                StartCoroutine("WaitForAssassinToBlink");
                rb.isKinematic = true;
                lethalCalculatingNextPosition = false;
            }
            else
            {
                transform.position += movement * Time.deltaTime;
            }
        }
    }


    IEnumerator WaitForAssassinToBlink()
    {
        yield return new WaitForSeconds(lethBlinkTimeSpeed);
        teleportCharges--;
        atMaxCharge = false;
        CheckIfCanAttackPlayer();
        rb.isKinematic = false;

        if (teleportCharges > 0)
            StartCoroutine("CooldownForBlink");
        else
            lethalBlinking = false;
    }

    IEnumerator CooldownForBlink()
    {
        waitingBetweenBlink = true;
        lethalCalculatingNextPosition = true;
        yield return new WaitForSeconds(blinkCooldownTime);
        waitingBetweenBlink = false;
    }

    private void CheckIfCanAttackPlayer()
    {
        if ((player.transform.position - transform.position).magnitude < attackPlayerRange)
        {
            player.TakeDamage(blinkDamage);
            GetComponent<Animation>().Play();
        }
    }

    private void Teleport()
    {
        canTeleport = false;
        StartCoroutine("Wait");
        differenceVector = player.transform.position - transform.position;
        distance = Vector3.Distance(transform.position, player.transform.position);

        float slope = differenceVector.z / differenceVector.x;
        float angle = 0;

        if (differenceVector.x > 0 && differenceVector.z > 0)
            angle = Mathf.Atan(slope) * Mathf.Rad2Deg;
        else if (differenceVector.x < 0 && differenceVector.z > 0)
            angle = (Mathf.Atan(slope) * Mathf.Rad2Deg) + 180;
        else if (differenceVector.x < 0 && differenceVector.z < 0)
            angle = Mathf.Atan(slope) * Mathf.Rad2Deg + 180;
        else if (differenceVector.x > 0 && differenceVector.z < 0)
            angle = Mathf.Atan(slope) * Mathf.Rad2Deg + 360;

        float degree = UnityEngine.Random.Range((angle - 30), (angle + 30));

        if (distance > teleAreaDistance)
        {
            degree = degree * Mathf.Deg2Rad;
            float directionX = Mathf.Cos(degree);
            float directionY = Mathf.Sin(degree);

            Vector3 location = new Vector3(transform.position.x + (directionX * teleAreaDistance), 0, transform.position.z + (directionY * teleAreaDistance));

            location = CheckTeleArea(location);
            rb.MovePosition(location);
        }
        else
        {
            Vector3 playerPos = player.transform.position;
            transform.position = (playerPos + -player.transform.forward);
        }
        teleportCharges--;
        if (teleportCharges == 0)
            return;

        
    }

    private Vector3 CheckTeleArea(Vector3 location)
    {
        //This code checks the teleport area for if it will overlap with the collider of the Assassin.
        //If it does it will check in a circle starting with (dist, 0, dist) from its location and then will
        //Check 30 degrees to the right, when it completes a circle and still cannot find a spot, dist will increase so that it goes further.
        Collider[] colliders = Physics.OverlapSphere(location, 0.5f, ignoreTeleportLayer);
        float checkAngle = 0;
        float dist = 0.5f;
        Vector3 newLocation = location;
        Vector3 direction;
        newLocation.x += dist;
        newLocation.z += dist;
        direction = newLocation - location;
        point = location;
        int safetyBreak = 0;
        while (colliders.Length > 0)
        {
            direction = Quaternion.Euler(0, checkAngle, 0) * direction;
            point = direction + location;
            print("checking: " + direction);

            colliders = Physics.OverlapSphere(point, 0.5f, ignoreTeleportLayer);

            checkAngle += 30;
            if (checkAngle >= 360)
            {
                checkAngle = 0;
                newLocation.x -= dist;
                newLocation.z -= dist;
                dist += 0.5f;
                newLocation.x += dist;
                newLocation.z += dist;
                direction = newLocation - location;
            }
            if (Extra.CheckSafetyBreak(safetyBreak, 100))
            {
                Debug.Log("Infinite Loop Safety Break Triggered");
                break;
            }
            safetyBreak++;
        }
        location = point;
        return location;
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(1, 2));
        canTeleport = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        /*
        String itsTag = other.tag;
        if (itsTag == "Bullet" || itsTag == "Shotgun" || itsTag == "Grenade")
        {
            if (teleportCharges > 0 && canTeleportRandomly)
            {
                StopCoroutine("RegenCharge");
                isRegenerating = false;
                canTeleportRandomly = false;
                shot = true;
                TeleportRandom();
            }
        }*/
    }

    private void TeleportRandom()
    {
        /*
        StartCoroutine("WaitRandom");
        differenceVector = player.transform.position - transform.position;
        distance = Vector3.Distance(transform.position, player.transform.position);

        float slope = differenceVector.z / differenceVector.x;
        float angle = 0;

        if (differenceVector.x > 0 && differenceVector.z > 0)
            angle = Mathf.Atan(slope) * Mathf.Rad2Deg;
        else if (differenceVector.x < 0 && differenceVector.z > 0)
            angle = (Mathf.Atan(slope) * Mathf.Rad2Deg) + 180;
        else if (differenceVector.x < 0 && differenceVector.z < 0)
            angle = Mathf.Atan(slope) * Mathf.Rad2Deg + 180;
        else if (differenceVector.x > 0 && differenceVector.z < 0)
            angle = Mathf.Atan(slope) * Mathf.Rad2Deg + 360;

        float degree = 0f;
        int random = UnityEngine.Random.Range(0, 1);

        if (random == 0)
        {
            degree = UnityEngine.Random.Range((angle - 120), (angle - randomTeleportAngleIgnore / 2));
        }
        else
        {
            degree = UnityEngine.Random.Range((angle + 120), (angle + randomTeleportAngleIgnore / 2));
        }

        degree = degree * Mathf.Deg2Rad;
        float directionX = Mathf.Cos(degree);
        float directionY = Mathf.Sin(degree);
        

        Vector3 location = new Vector3(transform.position.x + (directionX * UnityEngine.Random.Range(teleAreaDistance, teleAreaDistance * 2)), 
            0, 
            transform.position.z + (directionY * UnityEngine.Random.Range(teleAreaDistance, teleAreaDistance * 2)));

        location = CheckTeleArea(location);
        transform.position = location;
        teleportCharges--;
        full = false;*/
    }

    // I plan on making an animation for the assassin to play while teleporting so this time may change in the future for now its hard coded.
    IEnumerator WaitRandom()
    {
        yield return new WaitForSeconds(0.1f);
        canTeleportRandomly = true;
    }

    IEnumerator RegenCharge()
    {
        recharging = true;
        yield return new WaitForSeconds(1);
        teleportCharges++;
        if (teleportCharges == teleChargeMax)
            atMaxCharge = true;

        recharging = false;
    }

}
