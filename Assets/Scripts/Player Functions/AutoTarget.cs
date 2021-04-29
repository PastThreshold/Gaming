using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTarget : BasicAbility, AbilityADT
{
    [SerializeField] float maxTime = 15f;
    [SerializeField] float timeLeft = 15;
    [SerializeField] float minTimeToActivate = 3f;
    [SerializeField] float timeBetweenActivations = 1f;
    [SerializeField] float enemyFindRadius = 3f;
    bool waitingBetweenActivations = false;
    bool canActivate = true;
    Enemy target;

    bool inUse = false;
    bool scriptRunning = false;

    private void Update()
    {
        if (!scriptRunning)
            return;

        if (inUse)
        {
            List<Enemy> nearEnemies = Extra.GetEnemiesFromPhysicsOverLapSphere
                (player.mouseLocationConverted, enemyFindRadius, GlobalClass.exD.enemiesOnlyLM);

            if (nearEnemies.Count == 0)
                target = null;
            else if (nearEnemies.Count == 1)
                target = nearEnemies[0];
            else
                target = Extra.ReturnClosestEnemyFromList(nearEnemies);

            if (HasTarget())
                hud.UpdateAutoTargetCrosshair(GetTargetPosition());
            else
                hud.UpdateAutoTargetCrosshair();

            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                inUse = false;
                player.DisableAutoTarget();
                StartCoroutine(TimeBetweenActivate());
            }
        }
        else
        {
            timeLeft += Time.deltaTime;
            if (timeLeft >= maxTime)
                timeLeft = maxTime;
        }
    }

    public void InputGet()
    {

    }

    public void InputGetDown()
    {
        if (inUse && !waitingBetweenActivations)
        {
            inUse = false;
            player.DisableAutoTarget();
            StartCoroutine(TimeBetweenActivate());
        }
        if (canActivate && !waitingBetweenActivations && timeLeft > minTimeToActivate)
        {
            inUse = true;
            player.EnableAutoTarget();
            StartCoroutine(TimeBetweenActivate());
        }
    }

    public void InputGetUp()
    {

    }

    IEnumerator TimeBetweenActivate()
    {
        waitingBetweenActivations = true;
        yield return new WaitForSeconds(timeBetweenActivations);
        waitingBetweenActivations = false;
    }

    public bool HasTarget()
    {
        return target != null;
    }

    public Vector3 GetTargetPosition()
    {
        Extra.DrawBox(target.transform.position, 0.5f, Color.red, 2f);
        return target.transform.position;
    }

    public Transform GetTargetTransform() { return target.transform; }

    public void SetScriptStatus(bool status)
    {
        scriptRunning = status;
    }
}
