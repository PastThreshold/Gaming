using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Blink : BasicAbility, AbilityADT
{
    [SerializeField] Player player;
    Rigidbody rb;
    bool canBlink = true;
    bool canAltBlink = true;
    [SerializeField] float blinkDistance = 3;
    [SerializeField] float longBlinkDistance = 5;
    [SerializeField] float blinkCooldown = 0.15f;
    [SerializeField] float longBlinkCooldown = 5f;
    [SerializeField] float timeToGetToFinalPosition = 0.2f; // Speed (lower = faster)
    [SerializeField] float longBlinkDamage = 20f;
    //[SerializeField] BoxCollider testBox = null;
    Vector3 startColliderPos = Vector3.zero;
    float currentBlinks = 0;
    [SerializeField] LayerMask enemyLayerMask;

    [Header("Leveling Values")]
    [SerializeField] float maxBlinksL1 = 2;
    [SerializeField] float maxBlinksL2 = 3;
    [SerializeField] float maxBlinksL3 = 4;
    [SerializeField] float maxBlinksL4 = 5;
    float maxBlinks = 0;
    [SerializeField] float blinkRechargeL1 = 0.85f;
    [SerializeField] float blinkRechargeL2 = 0.75f;
    [SerializeField] float blinkRechargeL3 = 0.65f;
    [SerializeField] float blinkRechargeL4 = 0.60f;
    float blinkRecharge = 0;

    bool recharging = false;
    bool isBlinking = false;
    bool isLongBlinking = false;
    Vector3 directionAndMagnitude;
    float speed;
    bool scriptRunning = false;

    public override void CheckAbilityLevel()
    {
        switch(currentAbilityLevel)
        {
            case 1:
                maxBlinks = maxBlinksL1;
                blinkRecharge = blinkRechargeL1;
                break;
            case 2:
                maxBlinks = maxBlinksL2;
                blinkRecharge = blinkRechargeL2;
                break;
            case 3:
                maxBlinks = maxBlinksL3;
                blinkRecharge = blinkRechargeL3;
                break;
            case 4:
                maxBlinks = maxBlinksL4;
                blinkRecharge = blinkRechargeL4;
                break;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        CheckAbilityLevel();
        currentBlinks = maxBlinks;
    }

    // Update is called once per frame
    void Update()
    {
        if (scriptRunning && (isBlinking || isLongBlinking))
        {
            print(directionAndMagnitude * Time.deltaTime);
            transform.position += directionAndMagnitude * Time.deltaTime;
        }
        if (!recharging && currentBlinks < maxBlinks)
        {
            StartCoroutine("ChargeOneBlink");
        }
    }

    private void Teleport()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 positionToMove;
        if (h == 0 && v == 0)
            positionToMove = new Vector3(transform.forward.x, 0, transform.forward.z);
        else
            positionToMove = new Vector3(h, 0, v).normalized * blinkDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(rb.position + positionToMove, out hit, blinkDistance, NavMesh.AllAreas))
        {
            speed = blinkDistance / timeToGetToFinalPosition;
            directionAndMagnitude = (positionToMove).normalized * speed;
            StartCoroutine("Wait");

            if (player.allActiveClones.Count > 0)
            {
                foreach (Clone clone in player.allActiveClones)
                {
                    clone.Blink(directionAndMagnitude, timeToGetToFinalPosition);
                    print(directionAndMagnitude);
                }
            }
        }
        else
            Debug.Log("No position on navmesh found");

        currentBlinks--;
    }

    /* Recieves vector coords from player script, calculates the distance between player and coords
     * calculates how many longBlinkDistances it will take to get there and uses that many
     * All integers so there is loss 
     * If the distance to teleport is too great for how many current blinks there are
     * it will go as far as there are currentBlink * distance for each blink */
    private void AltLongTeleport()
    {
        Vector3 positionToMove = player.mouseLocationConverted - transform.position;
        print(positionToMove + " = " + player.mouseLocationConverted + " - " + transform.position);
        float distanceBetween = positionToMove.magnitude;
        int blinksToUse = (int) (distanceBetween / longBlinkDistance) + 1;
        print(blinksToUse + " = " + distanceBetween + " / " + longBlinkDistance + " + " + 1);
        if (blinksToUse > currentBlinks)
        {
            speed = (currentBlinks * longBlinkDistance) / timeToGetToFinalPosition;
            currentBlinks = 0;
        }
        else
        {
            speed = distanceBetween / timeToGetToFinalPosition;
            print(speed + " = " + distanceBetween + " / " + timeToGetToFinalPosition);
            currentBlinks -= blinksToUse;
        }
        directionAndMagnitude = (positionToMove).normalized * speed;
        startColliderPos = transform.position;
        StartCoroutine("WaitAltLong");

        if (player.allActiveClones.Count > 0)
        {
            foreach (Clone clone in player.allActiveClones)
            {
                clone.Blink(directionAndMagnitude, timeToGetToFinalPosition);
                print(directionAndMagnitude);
            }
        }
    }

    IEnumerator ChargeOneBlink()
    {
        recharging = true;
        yield return new WaitForSeconds(blinkRecharge);
        currentBlinks++;
        recharging = false;
    }

    IEnumerator Wait()
    {
        isBlinking = true;
        player.SetInvulnerable(true);
        yield return new WaitForSeconds(timeToGetToFinalPosition);
        player.SetInvulnerable(false);
        isBlinking = false;
    }

    IEnumerator WaitAltLong()
    {
        isLongBlinking = true;
        player.SetInvulnerable(true);
        yield return new WaitForSeconds(timeToGetToFinalPosition);
        Vector3 halfWay = (transform.position - startColliderPos).normalized * 
            (transform.position - startColliderPos).magnitude / 2 + startColliderPos;
        Vector3 size = new Vector3(0.6f, 0.75f, (transform.position - startColliderPos).magnitude);
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, transform.position - startColliderPos);
        Collider[] colliders = Physics.OverlapBox(halfWay, size, rotation, enemyLayerMask);
        //BoxCollider box = Instantiate(testBox, halfWay, Quaternion.identity);    Testing purposes incase it breaks
        //box.size = new Vector3(0.6f, 0.75f, (transform.position - startColliderPos).magnitude);
        //box.transform.rotation = Quaternion.FromToRotation(Vector3.forward, transform.position - startColliderPos);
        List<GameObject> objsHit = CollisionHandler.TestSingleTriggerArray(colliders);
        foreach(GameObject obj in objsHit)
        {
            obj.GetComponentInParent<Enemy>().TakeDamage(longBlinkDamage);
        }
        player.SetInvulnerable(false);
        isLongBlinking = false;
    }

    IEnumerator WaitToBlinkAgain()
    {
        canBlink = false;
        yield return new WaitForSeconds(blinkCooldown);
        canBlink = true;
    }

    IEnumerator WaitAltLongBlink()
    {
        canAltBlink = false;
        yield return new WaitForSeconds(longBlinkCooldown);
        canAltBlink = true;
    }

    public void InputGetDown()
    {
        if (scriptRunning && currentBlinks > 0)
        {
            if (Input.GetKey(KeyCode.LeftShift) && currentAbilityLevel >= 2 && canAltBlink)
            {
                StartCoroutine("WaitAltLongBlink");
                AltLongTeleport();
            }
            else if (canBlink)
            {
                StartCoroutine("WaitToBlinkAgain");
                Teleport();
            }
        }
    }

    public void InputGet()
    {
        return;
    }

    public void InputGetUp()
    {
        return;
    }

    public void SetScriptStatus(bool status)
    {
        scriptRunning = status;
    }
}
