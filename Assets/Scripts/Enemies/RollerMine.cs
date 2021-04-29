using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RollerMine : Enemy
{
    [SerializeField] float damage;
    [SerializeField] float moveSpeed;
    NavMeshPath navPath;
    Vector3 direction;

    [SerializeField] float distanceFactor = 5;
    [Range(1000f, 10000f)] [SerializeField] float velFactorWrongDirection = 1000f;
    [Range(10f, 500f)] [SerializeField] float velFactorRightDirection = 100f;
    [Range(5f, 100f)] [SerializeField] float velFactorReversingDirectionMax = 20f;
    [Range(0f, 5f)] [SerializeField] float velFactorReversingDirectionMin = 5f;
    [Range(1f, 10f)] [SerializeField] float velReverseExponent = 3f;
    [SerializeField] float closeToPlayerDistanceSquared = 2.5f;
    [SerializeField] float testFactor = 10f;
    [SerializeField] float maxSpeed = 100f;

    bool justMissed = false;
    bool checkingDistance = true;


    private void Start()
    {
        BaseStart();
        nma.isStopped = true;
        navPath = new NavMeshPath();
    }

    void Update()
    {
        nma.CalculatePath(player.transform.position, navPath);
        int safetyBreak = 0;
        int i = 1;
        while (i < navPath.corners.Length)
        {
            if (Vector3.Distance(transform.position, navPath.corners[i]) > 0.5f)
            {
                direction = navPath.corners[i] - transform.position;
                break;
            }
            i++;
            if (Extra.CheckSafetyBreak(safetyBreak, 1000))
            {
                Debug.Log("Infinite Loop Safety Break Triggered");
                break;
            }
            safetyBreak++;
        }
    }

    void FixedUpdate()
    {
            
        rb.AddForce(direction.normalized * moveSpeed * Time.deltaTime);
        Vector3 velocityNoY = rb.velocity; velocityNoY.y = 0; velocityNoY = velocityNoY.normalized;
        Vector3 dirToPlayerNoY = (player.transform.position - transform.position).normalized; dirToPlayerNoY.y = 0;

        if (checkingDistance)
        {
            if ((transform.position - player.transform.position).sqrMagnitude < closeToPlayerDistanceSquared)
            {
                justMissed = true;
                checkingDistance = false;
                StartCoroutine("ResetMovement");
            }
        }

        if (!justMissed)
        {
            float alignment = Vector3.Dot(velocityNoY, dirToPlayerNoY);
            float factor = Mathf.Lerp(velFactorWrongDirection, velFactorRightDirection, Mathf.InverseLerp(-1, 1, alignment));
            rb.AddForce(dirToPlayerNoY * factor / 10 * Time.deltaTime);

            Vector3 directionForce = (velocityNoY + dirToPlayerNoY).normalized;
            float angle = Vector3.Angle(directionForce, dirToPlayerNoY);
            if ((Quaternion.Euler(0, -angle, 0) * directionForce).normalized == dirToPlayerNoY.normalized)
                directionForce = Quaternion.Euler(0, -angle * 2, 0) * directionForce;
            else
                directionForce = Quaternion.Euler(0, angle * 2, 0) * directionForce;

            bool moreForce = false;
            float bonusFactor = 20;
            int rand = Random.Range(0, 10);
            if (rand == 1)
            {
                moreForce = true;
            }
            if (moreForce)
                bonusFactor *= 2;
            rb.AddForce(directionForce * factor * bonusFactor * Time.deltaTime);

            float reverseFactor = Mathf.Lerp(velFactorReversingDirectionMax, velFactorReversingDirectionMin, Mathf.InverseLerp(-1, 1, alignment));
            reverseFactor = Mathf.Pow(reverseFactor, velReverseExponent);
            rb.AddForce(-velocityNoY * reverseFactor * Time.deltaTime);
        }

        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    IEnumerator ResetMovement()
    {
        yield return new WaitForSeconds(3f);
        justMissed = false;
        checkingDistance = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>())
        {
            other.GetComponent<Player>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
