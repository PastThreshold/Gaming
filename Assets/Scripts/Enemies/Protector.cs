using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Protector : Enemy
{
    [Header("Protection")]
    [SerializeField] EnemyShield normalShield;
    [Range(0f, 100f)] [SerializeField] float chanceToChangeTarget = 10f;
    [Range(0f, 100f)] [SerializeField] float chanceToProtect = 10f;
    [SerializeField] float timeBetweenProtectRolls = 0.5f;
    float distanceToStayWithinTarget = 90f;
    public GameObject targetToProtect;
    Enemy targetEnemyScript;
    Protector targetProtector;
    bool protecting;
    bool waitingToProtect;

    [SerializeField] float withinDistanceLargeShield = 150f;
    protected bool withinDistanceLS = false;
    [SerializeField] EnemyShield largeShield;
    [SerializeField] LayerMask enemiesLayerMask;
    public bool groupProtecting = false;
    public bool isMainComputer; // Is this protector the one computing the positons or not?


    protected bool fightPitFormation = false;
    protected List<Protector> connectedProtectors;

    public static List<Protector> allProtectorsOpen;

    bool shieldHealing = false;
    bool largeShieldHealing = false;
    bool shieldDestroyed = false;
    bool largeShieldDestroyed = false;

    [Header("Extra Movement")]
    bool protectionMovement = false;
    float protectionMovementCooldown = 2.5f;
    [SerializeField] Transform protectingArm;
    [SerializeField] Transform protectingHand;
    [SerializeField] Transform protectingHandPoint;




    // Start is called before the first frame update
    void Start()
    {
        BaseStart();
        projPool = GlobalClass.basicEnemyPool;
        playerPos = player.transform.position;
        canShoot = true;
        waitingToProtect = false;
        normalShield.SetCaller(this);
        normalShield.gameObject.SetActive(false);
        largeShield.SetCaller(this);
        largeShield.gameObject.SetActive(false);
        if (allProtectorsOpen == null)
            allProtectorsOpen = new List<Protector>();

        allProtectorsOpen.Add(this);
    }

    /// <summary>
    /// If this script is protecting an enemy check if they have no target to protect
    /// yes - look at their target, update the shield pos, and move to behind
    /// no - set protecting to false, and add to list of scripts with no targets
    /// 
    /// If group protecting and they are the main computer, then update shield pos
    /// the obj pos, and send the data to the other protector
    /// 
    /// If not protecting either way, roll chances to protect if havent already and shoot at player
    /// </summary>
    void Update()
    {

        if (protecting)
        {
            if (normalShield.gameObject.activeSelf)
                CalculateNormalShield();
            if (groupProtecting && isMainComputer) // If the protectors are shielding eachother and it is the main calculator
                CalculateLargeShield();
            protectingArm.LookAt(targetToProtect.transform.position);
        }
        else
        {
            if (!waitingToProtect && LevelController.allEnemiesInScene.Count >= 1 && !shieldDestroyed)
                StartCoroutine(RollChanceToProtect());
        }

        playerPosPrev = playerPos;
        playerPos = player.transform.position;
        transform.LookAt(playerPos);
        if (canShoot)
            StartCoroutine(Shoot());
        if (shieldHealing)
            normalShield.Heal(55f * Time.deltaTime);
        if (largeShieldHealing)
            largeShield.Heal(55f * Time.deltaTime);
        protectingHand.transform.position = protectingHandPoint.transform.position;


        if (inFormation)
            return;

        if (!randomMovementCooldown)
            StartCoroutine(MoveRandomly());
    }


    // While Protector has no target and is shooting at the player, it will roll chanceToProtect to find a new target
    // If it fails nothing happens and waits timeBetweenProtectRolls to roll the chance again.
    IEnumerator RollChanceToProtect()
    {
        waitingToProtect = true;
        yield return new WaitForSeconds(timeBetweenProtectRolls);
        if (Extra.RollChance(chanceToProtect))
        {
            Enemy chosenEnemy;
            chosenEnemy = LevelController.allEnemiesInScene[Random.Range(0, LevelController.allEnemiesInScene.Count)];
            int safetyBreak = 0;
            while (chosenEnemy.gameObject == gameObject)
            {
                chosenEnemy = LevelController.allEnemiesInScene[Random.Range(0, LevelController.allEnemiesInScene.Count)];
                if (Extra.CheckSafetyBreak(safetyBreak, 20))
                {
                    Debug.Log("Infinite Loop Safety Break Triggered");
                    yield break;
                }
                safetyBreak++;
            }
            EnableNormalShield(chosenEnemy);
            normalShield.ChangeEntireSize(chosenEnemy.GetActiveProtectorCount());
            chosenEnemy.AddProtector(this);
            // normalShield.ChangeEntireSize(chosenEnemy.enemyType);

            if (chosenEnemy.enemyType == EnemyType.protector) // If the chosen enemy is also a protector
            {
                targetProtector = chosenEnemy.GetComponent<Protector>();
                EstablishProtectorConnections(chosenEnemy);
            }
        }
        waitingToProtect = false;
    }

    /// <summary>
    /// Checks if protectors are protecting eachother to cast a large shield
    /// </summary>
    private void EstablishProtectorConnections(Enemy chosenEnemy)
    {
        Protector otherProt = chosenEnemy.GetComponent<Protector>();
        if (otherProt == null || otherProt.GetProtectedEnemy() == null)
            return;
        if (otherProt.GetProtectedEnemy() == this)
        {
            isMainComputer = true;
            groupProtecting = true; otherProt.groupProtecting = true;
            DisableShield(normalShield); otherProt.DisableShield(otherProt.normalShield);
        }
    }

    IEnumerator Shoot()
    {
        canShoot = false;
        if (playerPosPrev != playerPos)
        {
            if (Extra.RollChance(predictionChance))
            {
                Vector3 pVelocity = playerPos - playerPosPrev;
                Vector3 predictedPosition = playerPos + pVelocity * (predictionValue + Vector3.Distance(transform.position, playerPos));
                firePointRotation.transform.LookAt(Extra.SetYToTransform(predictedPosition, this.transform));
            }
            else
            {
                firePointRotation.transform.LookAt(Extra.SetYToTransform(player.transform.position, this.transform));
            }
        }
        else
        {
            firePointRotation.transform.LookAt(Extra.SetYToTransform(player.transform.position, this.transform));
            firePointRotation.transform.Rotate(0, UnityEngine.Random.Range(-2f, 2f), 0);
        }
        Projectile bullet = CreateBasicProjectile();
        bullet.EnableProjectile();
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    public void EnemyBeingAttacked(Enemy theEnemy)
    {
        if (protecting && theEnemy.inDanger)
        {
            float chanceRoll = UnityEngine.Random.Range(0f, 100f);
            if (chanceRoll < chanceToChangeTarget)
            {
                targetToProtect = theEnemy.gameObject;
            }
        }
    }

    IEnumerator ProtectionMovementTimer() 
    {
        protectionMovement = true;
        yield return new WaitForSeconds(protectionMovementCooldown); 
        protectionMovement = false; 
    }

    private void CalculateNormalShield()
    {
        normalShield.transform.position = targetToProtect.transform.position;
        if (inFormation)
            return;
        if (!protectionMovement)
        {
            if ((targetToProtect.transform.position - transform.position).sqrMagnitude > distanceToStayWithinTarget)
            {
                StopRandomMovement();
                Vector3 playerToTarget = targetToProtect.transform.position - playerPos;
                Vector3 positionBehindTarget = Quaternion.Euler(0, UnityEngine.Random.Range(-30f, 30f), 0) * playerToTarget.normalized;
                positionBehindTarget = positionBehindTarget * UnityEngine.Random.Range(2f, 5f) + playerToTarget + playerPos;
                Move(positionBehindTarget);
                StartCoroutine(ProtectionMovementTimer());
            }
        }
    }

    /// <summary>
    /// If the protectors are within range of eachother, enable the shield and update its position and rotation between them
    /// else deactivate the sheild or keep it deactivated
    /// </summary>
    private void CalculateLargeShield()
    {
        float distance = (targetToProtect.transform.position - transform.position).sqrMagnitude;
        if (distance < withinDistanceLargeShield)
        {
            if (!largeShield.gameObject.activeSelf)
            {
                if (!largeShieldDestroyed)
                {
                    EnableLargeShield();
                    DisableShield(normalShield); 
                    targetProtector.DisableShield(targetProtector.normalShield);
                }
            }
            Vector3 thisProt = transform.position;
            Vector3 otherProt = targetToProtect.transform.position;
            Vector3 middle = thisProt + (otherProt - thisProt) / 2;
            Vector3 pointToLook = middle + Quaternion.Euler(0, 90f, 0) * (otherProt - thisProt).normalized;
            largeShield.transform.position = middle;
            largeShield.transform.LookAt(pointToLook);
            largeShield.ChangeSize((thisProt - otherProt).magnitude);
        }
        else
        {
            if (largeShield.gameObject.activeSelf)
            {
                DisableShield(largeShield);
                EnableNormalShield();
                targetProtector.EnableNormalShield();
            }
        }

        if (inFormation)
            return;
        if (!protectionMovement)
        {
            StopRandomMovement();
            Vector3 positionToMove = Quaternion.Euler(0, 90f, 0) * (playerPos - transform.position).normalized 
                * Random.Range(2f, 5f);
            Move(positionToMove + transform.position);
            targetProtector.Move(positionToMove + targetProtector.transform.position);
            StartCoroutine(ProtectionMovementTimer());
        }
    }

    protected Enemy GetProtectedEnemy() { return targetEnemyScript; }

    /// <summary> Sets protecting variables in addition to enabling shield </summary>
    public void EnableNormalShield(Enemy targetEnemyScript)
    {
        protecting = true;
        this.targetEnemyScript = targetEnemyScript;
        targetToProtect = targetEnemyScript.gameObject;
        normalShield.gameObject.SetActive(true);
        normalShield.transform.parent = null;
        normalShield.transform.position = targetEnemyScript.transform.position;
        normalShield.PhaseIn();
        allProtectorsOpen.Remove(this);
    }
    /// <summary> Just enables the normal shield </summary>
    public void EnableNormalShield()
    {
        normalShield.gameObject.SetActive(true);
        normalShield.transform.parent = null;
    }

    public void EnableLargeShield()
    {
        largeShield.gameObject.SetActive(true);
        largeShield.transform.parent = null;
        largeShield.PhaseIn();
    }

    public void DisableShield(EnemyShield shield)
    {
        if (shield == null) return;
        shield.gameObject.SetActive(false);
        shield.transform.parent = transform;
    }

    public void ShieldDamaged(EnemyShield shield, float damage)
    {
        /*
        if (fightPitFormation)
        {
            behaviorHandler.PitShieldDamaged(damage);
        }*/
        if (shield == normalShield)
        {
            StopCoroutine(ShieldHealCooldown());
            StartCoroutine(ShieldHealCooldown());
        }
        else
        {
            StopCoroutine(LargeShieldHealCooldown());
            StartCoroutine(LargeShieldHealCooldown());
        }
    }

    public void ShieldDestroyed(EnemyShield shield)
    {
        if (shield == normalShield)
        {
            DisableShield(normalShield);
            StartCoroutine("RehealShield");
        }
        else
        {
            DisableShield(largeShield);
            StartCoroutine("RehealLargeShield");
        }
    }

    public void ShieldFull(EnemyShield shield)
    {
        if (shield == normalShield)
            shieldHealing = false;
        else
            largeShieldHealing = false;
    }

    protected IEnumerator ShieldHealCooldown()
    {
        shieldHealing = false;
        yield return new WaitForSeconds(3f);
        shieldHealing = true;
    }

    protected IEnumerator LargeShieldHealCooldown()
    {
        largeShieldHealing = false;
        yield return new WaitForSeconds(3f);
        largeShieldHealing = true;
    }

    protected IEnumerator RehealShield()
    {
        shieldDestroyed = true;
        yield return new WaitForSeconds(5f);
        normalShield.Heal(50f);
        shieldDestroyed = false;
    }

    protected IEnumerator RehealLargeShield()
    {
        largeShieldDestroyed = true;
        yield return new WaitForSeconds(5f);
        largeShield.Heal(50f);
        largeShieldDestroyed = false;
    }

    public void TargetDied()
    {
        StopProtecting();
    }

    /// <summary> Reset the target, disables the shield, and breaks group connection </summary>
    private void StopProtecting()
    {
        DisableShield(normalShield);
        protecting = false;
        if (groupProtecting)
        {
            StopGroupProtection();
            targetEnemyScript.GetComponent<Protector>().StopGroupProtection();
        }
        targetEnemyScript = null; targetToProtect = null; targetProtector = null;
    }

    /// <summary> Breaks the large shield connection between two protectors, is called by StopProtecting() </summary>
    protected void StopGroupProtection()
    {
        groupProtecting = false;
        if (isMainComputer)
        {
            isMainComputer = false;
            DisableShield(largeShield);
        }
    }


    protected override void OnDeath()
    {
        base.OnDeath();
        TargetDied();
    }

    // TODO public void StartFightPit

    /// <summary>
    /// Move to the specified position and also protect the other protector in the formation
    /// </summary>
    public void StartFSquad(Vector3 postion, Protector target, bool mainComputer)
    {
        inFormation = true;
        StopMovement();
        StopAllCoroutines();
        if (mainComputer)
        {
            largeShield.gameObject.SetActive(true);
            largeShield.transform.parent = null;
            isMainComputer = true;
        }
        EnableNormalShield(target);
        DisableShield(normalShield);
        groupProtecting = true;
        Move(postion);
    }

    public void UpdateGroupProtection(Vector3 position)
    {
        Move(position);
    }

    public override void EndFomation()
    {
        base.EndFomation();
        randomMovementCooldown = false;
        canShoot = true;
    }
}