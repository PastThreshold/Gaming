using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Commander : Enemy
{
    public List<Enemy> activeBuffedEnemies;
    int enemyCountForFirstBuff = 2;
    float firstBuffFactor = 1.8f;
    int enemyCountForSecondBuff = 5;
    float secondBuffFactor = 1.4f;
    float thirdBuffFactor = 1.2f;
    float buffCooldown = 15f;
    float buffDuration = 10f;
    bool buffing = false;
    bool buffOnCooldown = false;

    bool randomMovementCooldown = false;
    bool movingRandomly = false;
    [SerializeField] LineRenderer tetherPrefab;
    public List<LineRenderer> tethers;

    void Start()
    {
        projPool = GlobalClass.timedBombPool;
        BaseStart();
        activeBuffedEnemies = new List<Enemy>();
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
                        StartCoroutine("Shoot");
                    break;
                default:
                    break;
            }
        }
        else
        {
            if (!randomMovementCooldown)
                StartCoroutine("MoveRandomly");
            if (canShoot)
                StartCoroutine("Shoot");
            if (CheckIfAtEndOfPath())
                StopMovement();
            if (!buffOnCooldown)
                StartCoroutine("BuffCooldown");
        }

        if (buffing)
        {
            for (int i = 0; i < activeBuffedEnemies.Count; i++)
            {
                tethers[i].transform.position = transform.position;
                tethers[i].transform.LookAt(activeBuffedEnemies[i].transform.position);
                tethers[i].SetPosition(1, 
                    new Vector3(0, 0, (transform.position - activeBuffedEnemies[i].transform.position).magnitude));
            }
        }
        transform.LookAt(Extra.SetYToTransform(player.transform.position, transform));
    }

    IEnumerator BuffCooldown()
    {
        buffOnCooldown = true;
        buffing = true;
        activeBuffedEnemies = Extra.GetEnemiesFromPhysicsOverLapSphere(transform.position, 8f, GlobalClass.exD.enemiesOnlyLM);
        activeBuffedEnemies.Remove(this);
        UpdateBuff(true);
        yield return new WaitForSeconds(buffDuration);
        buffing = false;
        UpdateBuff(false);
        ClearBuffedEnemies();
        yield return new WaitForSeconds(buffCooldown);
        buffOnCooldown = false;
    }

    /// <summary>
    /// Will decide a random endpoint and midpoint, then move along a bezier curve to randomize its movement
    /// </summary>
    IEnumerator MoveRandomly()
    {
        movingRandomly = true;
        randomMovementCooldown = true;
        Vector3 start = transform.position;
        Vector3 endPos = start + Extra.CreateRandomVectorWithMagnitude(15f, 15f);
        Vector3 middlePoint = start + Quaternion.Euler(0, 90f, 0) * (endPos - start).normalized * UnityEngine.Random.Range(7f, 12f);
        middlePoint += (endPos - start).normalized * UnityEngine.Random.Range(7f, 12f);
        if (NavMesh.SamplePosition(endPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            endPos = hit.position;
        }
        else
        {
            NavMesh.FindClosestEdge(endPos, out NavMeshHit edge, NavMesh.AllAreas);
            endPos = edge.position;
        }

        Vector3 posToMoveTo;
        for (float i = 0f; i < 1f; i += 0.01f)
        {
            posToMoveTo = Extra.CalculateQuadraticBezierCurve(i, start, middlePoint, endPos);
            Move(posToMoveTo);
            yield return new WaitForSeconds(0.01f);
        }


        movingRandomly = false;
        StartCoroutine("RandomMovementCooldown");
    }

    IEnumerator RandomMovementCooldown()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 2f));
        randomMovementCooldown = false;
    }

    IEnumerator Shoot()
    {
        canShoot = false;
        yield return new WaitForSeconds(fireRate);
        for (int i = 0; i < 4; i++)
        {
            firePointRotation.transform.LookAt(Extra.SetYToTransform(player.transform.position, firePointRotation.transform));
            firePointRotation.transform.Rotate(0, UnityEngine.Random.Range(-1f, 1f), 0);
            Projectile bullet = CreateBasicProjectile();
            bullet.EnableProjectile();
            yield return new WaitForSeconds(.1f);
        }
        canShoot = true;
    }

    private void ClearBuffedEnemies()
    {
        RevertBoostsToEnemies();
        activeBuffedEnemies.Clear();
        for (int i = 0; i < tethers.Count; i++)
        {
            Destroy(tethers[i].gameObject);
        }
        tethers.Clear();
    }

    public void BuffSpecificEnemies(List<Enemy> enemiesToBuff)
    {
        ClearBuffedEnemies();
        for (int i = 0; i < enemiesToBuff.Count; i++)
        {
            activeBuffedEnemies.Add(enemiesToBuff[i]);
        }
        UpdateBuff(true); 
    }

    private void UpdateBuff(bool boosting)
    {
        if (boosting)
        {
            if (activeBuffedEnemies.Count <= enemyCountForFirstBuff)
                GiveBoostsToBuffedEnemies(firstBuffFactor);
            else if (activeBuffedEnemies.Count <= enemyCountForSecondBuff)
                GiveBoostsToBuffedEnemies(secondBuffFactor);
            else
                GiveBoostsToBuffedEnemies(thirdBuffFactor);
        }
        else
        {
            RevertBoostsToEnemies();
        }

        for (int i = 0; i < activeBuffedEnemies.Count; i++)
        {
            tethers.Add(Instantiate(tetherPrefab, transform.position, Quaternion.identity));
        }
    }

    private void GiveBoostsToBuffedEnemies(float damageIncreaseFactor)
    {
        foreach (Enemy enemy in activeBuffedEnemies)
        {
            print("Buffing: " + enemy.name);
            enemy.AddCommander(gameObject, this, damageIncreaseFactor);
            //enemy.ChangeDamageChangeFactor(gameObject, damageIncreaseFactor);
        }
    }

    private void RevertBoostsToEnemies()
    {
        foreach (Enemy enemy in activeBuffedEnemies)
        {
            enemy.RemoveCommander(gameObject, this);
        }
    }

    public void TetheredEnemyDied(Enemy enemy)
    {
        int index = -1;
        for (int i = 0; i < activeBuffedEnemies.Count; i++)
        {
            if (activeBuffedEnemies[i] == enemy)
            {
                index = i;
            }
        }
        if (index == -1)
            return;
        activeBuffedEnemies.RemoveAt(index);
        Destroy(tethers[index]);
        tethers.RemoveAt(index);
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        ClearBuffedEnemies();
    }

    public void StartFSquad(Vector3 position, List<Enemy> enemiesToBuff)
    {
        inFormation = true;
        Move(position);
        BuffSpecificEnemies(enemiesToBuff);
        StopAllCoroutines();
    }

    public void StartMountedWalker(Vector3 position)
    {
        inFormation = true;
        StopCoroutine("MoveRandomly");
        Move(position);
    }

    public void SecondStartMountedWalker(Walker walker)
    {
        nma.enabled = false;
        transform.parent = walker.transform;
        transform.position = walker.transform.position + new Vector3(1f, 0.5f, 0.5f);
    }

    public override void EndFomation()
    {
        base.EndFomation();
    }
}
