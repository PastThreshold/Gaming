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
        BaseUpdate();
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
            {
                if (Extra.RollChance(0f))
                    StartCoroutine(MoveRandomly());
                else
                {
                    Vector3 position = CheckForGroup();
                    if (position != Vector3.zero)
                    {
                        Move(position, MovementReason.commanderIntoGroup);
                        randomMovementCooldown = true;
                    }
                    else
                        StartCoroutine(MoveRandomly());
                }
            }
            if (canShoot)
                StartCoroutine(Shoot());
            if (CheckIfAtEndOfPath())
            {
                HandleMovementReason();
                StopMovement();
            }
            if (!buffOnCooldown)
                StartCoroutine(BuffCooldown());
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


    /// <summary> Will test a random enemies on the scene if there is a group of enemies near them </summary>
    private Vector3 CheckForGroup()
    {
        Vector3 middlePoint = Vector3.zero;
        List<Enemy> enemies = LevelController.allEnemiesInScene;
        int rand = UnityEngine.Random.Range(0, enemies.Count);
        List<Enemy> enemiesTogether = Extra.GetEnemiesFromPhysicsOverLapSphere(
            enemies[rand].transform.position, 6.5f, GlobalClass.exD.enemiesOnlyLM);
        Extra.DrawStrangeOutCenterThing(enemies[rand].transform.position, 13f, Color.blue, 10f);
        if (enemiesTogether.Count >= 3)
        {
            middlePoint = Extra.FindCenterOfListOfEnemies(enemiesTogether);
            middlePoint += (middlePoint - playerPos).normalized;
        }
        return middlePoint;
    }

    IEnumerator Buff()
    {
        buffOnCooldown = true;
        buffing = true;
        activeBuffedEnemies = Extra.GetEnemiesFromPhysicsOverLapSphere(transform.position, 8f, GlobalClass.exD.enemiesOnlyLM);
        activeBuffedEnemies.Remove(this); // Will add itself to list so must be removed first
        UpdateBuff(true);
        yield return new WaitForSeconds(buffDuration);
        StartCoroutine(BuffCooldown());
    }

    IEnumerator BuffCooldown()
    {
        buffing = false;
        UpdateBuff(false);
        ClearBuffedEnemies();
        yield return new WaitForSeconds(buffCooldown);
        buffOnCooldown = false;
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
            enemy.AddDamageChangeFactor(gameObject, damageIncreaseFactor);
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

    private void HandleMovementReason()
    {
        MovementReason reason = GetCurrentMovementReason();
        switch (reason)
        {
            case MovementReason.commanderIntoGroup:
                if (buffing)
                    StopCoroutine(Buff());
                else if (buffOnCooldown)
                    StopCoroutine(BuffCooldown());
                StartCoroutine(Buff());
                break;
        }
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
