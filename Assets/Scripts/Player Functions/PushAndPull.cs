using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushAndPull : BasicAbility, AbilityADT
{
    
    float radius = 4f;
    [SerializeField] float radiusL1 = 4f;
    [SerializeField] float radiusL2 = 4f;
    [SerializeField] float radiusL3 = 4f;
    [SerializeField] float radiusL4 = 4f;
    [SerializeField] float timeToMove = 1f;
    [SerializeField] float pushDistance = 5f;
    [SerializeField] GameObject explosionObject;
    [SerializeField] GameObject explosionVfx;
    [SerializeField] float explosionTime;
    [SerializeField] float scaleAdd;
    [SerializeField] LayerMask enemyLayerMask;

    bool inUse = false;
    bool pushingOrPulling = false;
    bool push = false;
    bool ExplosionExpanding = false;
    GameObject expObj;

    List<Enemy> enemiesHit;
    Vector3[] enemiesDirectionsToMove;
    float[] enemiesSpeeds;
    bool scriptRunning = false;

    void Update()
    {
        if (scriptRunning)
        {
            if (pushingOrPulling)
            {
                for (int i = 0; i < enemiesHit.Count; i++)
                {
                    enemiesHit[i].transform.position += enemiesDirectionsToMove[i] * enemiesSpeeds[i] * Time.deltaTime;
                }
            }

            if (ExplosionExpanding)
            {
                float adjustedScaleAdd = scaleAdd;
                adjustedScaleAdd *= Time.deltaTime;
                expObj.transform.localScale += new Vector3(adjustedScaleAdd, adjustedScaleAdd, adjustedScaleAdd);
            }
        }
    }

    private void PushOrPullNearbyEnemies(Vector3 centerOfAbility, bool pushing)
    {
        Collider[] collidersHit = Physics.OverlapSphere(centerOfAbility, radius, enemyLayerMask);
        enemiesHit = new List<Enemy>();

        for (int i = 0; i < collidersHit.Length; i++)
        {
            Enemy enemyHit = collidersHit[i].GetComponentInParent<Enemy>();
            if (enemyHit)
            {
                if (!enemiesHit.Contains(enemyHit))
                {
                    enemiesHit.Add(enemyHit);
                }
            }
        }

        enemiesDirectionsToMove = new Vector3[enemiesHit.Count];
        enemiesSpeeds = new float[enemiesHit.Count];

        if (pushing)
        {
            for (int i = 0; i < enemiesHit.Count; i++)
            {
                Vector3 direction = (enemiesHit[i].transform.position - centerOfAbility).normalized;
                direction.y = 0;
                enemiesDirectionsToMove[i] = direction;

                float distance = pushDistance;
                float speed = distance / timeToMove;
                enemiesSpeeds[i] = speed;
            }
        }
        else
        {
            for (int i = 0; i < enemiesHit.Count; i++)
            {

                Vector3 direction = (centerOfAbility - enemiesHit[i].transform.position).normalized;
                direction.y = 0;
                enemiesDirectionsToMove[i] = direction;

                float distance = (centerOfAbility - enemiesHit[i].transform.position).magnitude;
                float speed = distance / timeToMove;
                enemiesSpeeds[i] = speed;
            }
        }

        pushingOrPulling = true;
        StartCoroutine("WaitAndEndMovement");
    }

    IEnumerator WaitAndEndMovement()
    {
        yield return new WaitForSeconds(timeToMove);
        pushingOrPulling = false;
        inUse = false;
    }

    IEnumerator WaitAndEndExplosion(bool willPush)
    {
        yield return new WaitForSeconds(explosionTime);
        ExplosionExpanding = false;
        Instantiate(explosionVfx, expObj.transform.position, Quaternion.identity);
        PushOrPullNearbyEnemies(expObj.transform.position, willPush);
        Destroy(expObj);
    }

    public void InputGetDown()
    {
        if (!inUse)
        {
            Vector3 centerOfAbility = GlobalClass.player.mouseLocationConverted;
            inUse = true;

            push = false;
            if (Input.GetKey(KeyCode.LeftShift))
                push = true;

            ExplosionExpanding = true;
            AbilityUsed();
            StartCoroutine("WaitAndEndExplosion", push);
            expObj = Instantiate(explosionObject, centerOfAbility, Quaternion.identity);
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
