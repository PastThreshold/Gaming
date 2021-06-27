using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    protected class DamageBoost
    {
        public GameObject objectCalled;
        public float damageFactor;

        public DamageBoost(GameObject originalObject, float damageFactor)
        {
            objectCalled = originalObject;
            this.damageFactor = damageFactor;
        }
    }

    public enum EnemyType
    {
        robot,
        assassin,
        walker,
        protector,
        roller,
        commander,
    }

    [Header("EnemyData")]
    public EnemyType enemyType;

    [Header("Health")]
    public float health;
    [SerializeField] protected float dangerousHealth;
    public bool inDanger = false;

    [Header("Other")]
    [SerializeField] protected GameObject ragdoll;
    [ColorUsage(true, true)] [SerializeField] protected Color color;
    public List<Commander> activeCommanders = new List<Commander>();
    public List<Protector> activeProtectors = new List<Protector>();
    protected BehaviorController.Behavior activeBehavior = BehaviorController.Behavior.none;
    protected BehaviorController.BehaviorHandler behaviorHandler = null;
    protected bool inFormation = false;

    [Header("Movement")]
    protected MovementReason currentMovementReason = MovementReason.none;
    [SerializeField] protected float randMoveTimerMax = 5f; // Cooldown
    [SerializeField] protected float randMoveTimerMin = 1.5f;
    [SerializeField] protected float randMoveDistMax = 15f;
    [SerializeField] protected float randMoveDistMin = 7.5f;
    [SerializeField] protected float randMoveArcMax = 12f;
    [SerializeField] protected float randMoveArcMin = 7f;
    protected bool randomMovementCooldown;
    protected bool movingRandomly;

    [Header("Firing")]
    [SerializeField] protected Transform firePointRotation;
    [SerializeField] protected Transform projectileSpawn;
    [SerializeField] protected float fireRate = 1.2f;
    [SerializeField] protected float predictionValue = 10f;
    [Range(0f, 100f)] [SerializeField] protected float predictionChance = 75f;
    protected bool canShoot = true;
    protected List<DamageBoost> activeDamageChanges;
    protected ProjectilePoolHandler projPool;

    protected Vector3 playerPosPrev;
    protected Vector3 playerPos;
    protected Player player;
    protected NavMeshAgent nma;
    protected Rigidbody rb;
    protected Material[] activeMaterials;

    protected void BaseStart()
    {
        if (GetComponent<NavMeshAgent>())
            nma = GetComponent<NavMeshAgent>();

        if (GetComponent<Rigidbody>())
            rb = GetComponent<Rigidbody>();

        player = GlobalClass.player;
        LevelController.AddEnemy(this);
        activeDamageChanges = new List<DamageBoost>();

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        activeMaterials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null)
            {
                activeMaterials[i] = renderers[i].material;
            }
        }
    }

    protected void BaseUpdate()
    {
        playerPosPrev = player.GetPreviousPos();
        playerPos = player.GetCurrentPos();
    }

    protected void Move(Vector3 positionToMoveTo)
    {
        nma.SetDestination(positionToMoveTo);
        nma.isStopped = false;
        currentMovementReason =  MovementReason.none;
    }

    protected void Move(Vector3 positionToMoveTo, MovementReason reason)
    {
        nma.SetDestination(positionToMoveTo);
        nma.isStopped = false;
        currentMovementReason = reason;
    }

    protected virtual void MoveBackInBounds()
    {
        Extra.Box2D[] bounds = LevelController.GetAllInBounds();
        Vector3 closest = bounds[0].ClosestPointInBounds(transform.position);
        Vector3 toPos = closest - transform.position;
        Vector3 newPos = Extra.RandomVectorYRotation(-30f, 30f) * toPos.normalized * Random.Range(2.5f, 5f) + closest;
        Move(newPos);
        randomMovementCooldown = true;
        StartCoroutine(RandomMovementCooldown());
    }

    protected IEnumerator MoveRandomly()
    {
        movingRandomly = true;
        randomMovementCooldown = true;
        Vector3 start = transform.position;
        Vector3 endPos = start + Extra.CreateRandomVectorWithMagnitude(randMoveDistMin, randMoveDistMax);
        Vector3 middlePoint = start + Quaternion.Euler(0, 90f, 0) * (endPos - start).normalized;
        middlePoint *= Random.Range(randMoveArcMin, randMoveArcMax);
        //middlePoint += (endPos - start).normalized * Random.Range(randMoveArcMin, randMoveArcMax);
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
        StartCoroutine(RandomMovementCooldown());
    }


    protected IEnumerator RandomMovementCooldown()
    {
        yield return new WaitForSeconds(Random.Range(randMoveTimerMin, randMoveTimerMax));
        randomMovementCooldown = false;
    }

    protected IEnumerator RandomMovementCooldown(float min, float max)
    {
        yield return new WaitForSeconds(Random.Range(min, max));
        randomMovementCooldown = false;
    }

    protected void StopRandomMovement()
    {
        movingRandomly = false;
        randomMovementCooldown = true;
        StopCoroutine(MoveRandomly());
        StopCoroutine(RandomMovementCooldown());
    }

    public bool CheckIfAtEndOfPath()
    {
        if (nma == null)
            return true;
        if (nma.hasPath)
        {
            if (nma.remainingDistance <= 0.1f)
            {
                return true;
            }
        }
        return false;
    }

    public MovementReason GetCurrentMovementReason() { return currentMovementReason; }

    public virtual void StopMovement()
    {
        nma.ResetPath();
        nma.isStopped = true;
        nma.velocity = Vector3.zero;
    }

    protected virtual void TakenDamage(float damage)
    {
        player.DealtDamage(damage);
        StartCoroutine(ChangeColor());
    }

    protected IEnumerator ChangeColor()
    {
        for (int i = 0; i < activeMaterials.Length; i++)
        {
            activeMaterials[i].SetColor("Color_4500394E", color);
        }
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < activeMaterials.Length; i++)
        {
            activeMaterials[i].SetColor("Color_4500394E", Color.white);
        }
    }

    // Meant for weapons like the shredder and laser which would have no knockback
    public virtual void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        TakenDamage(damageAmount);
        if (health <= 0)
        {
            GetRidOfChildren();
            Death(0);
        }
        UpdateHealthAndLists();
    }

    public virtual void TakeDamage(float damageAmount, float knockbackForce, Transform objTransform)
    {
        health -= damageAmount;
        TakenDamage(damageAmount);
        if (health <= 0)
        {
            GetRidOfChildren();
            Vector3 direction = objTransform.forward;
            direction.y = 0f;
            Death(knockbackForce, direction);
        }
        UpdateHealthAndLists();
    }

    public virtual void TakeDamage(float damageAmount, float exForce, Vector3 exPos, float radius, float upForce, ForceMode frcMode)
    {
        health -= damageAmount;
        TakenDamage(damageAmount);
        if (health <= 0)
        {
            GetRidOfChildren();
            Death(exForce, exPos, radius, upForce, frcMode);
        }
        UpdateHealthAndLists();
    }

    private void Death(float knockback)
    {
        UpdateGameLists();
        OnDeath();
        if (ragdoll != null)
        {
            ragdoll.SetActive(true);
            ragdoll.transform.parent = null;
            ragdoll.GetComponent<RagdollPart>().StartRagdoll(Vector3.zero, knockback);
        }
        Destroy(gameObject);
    }











    // Meant for weapons that you would expect to push things back as they impact
    private void Death(float knockback, Vector3 objectDirection)
    {
        UpdateGameLists();
        OnDeath();
        if (ragdoll != null)
        {
            /*
            gameObject.AddComponent<DirectionAndForce>().SetBoth(objectDirection, knockback);
            var value = GetComponent<DirectionAndForce>();
            ragdoll.SetActive(true);
            BroadcastMessage("ImpactForce", value, SendMessageOptions.DontRequireReceiver);
            ragdoll.transform.parent = null;*/
            ragdoll.SetActive(true);
            ragdoll.transform.parent = null;
            ragdoll.GetComponent<RagdollPart>().StartRagdoll(objectDirection, knockback);
        }
        Destroy(gameObject);
    }

    // The following overloaded methods are to work with the explosion vfx/launched grenade.
    // The explosion would kill the enemyRobotAI, call this overload and pass in its explosion parameters
    // The Robot enables the ragdoll object, unparents it, and creates a new physics overlap with the explosion parameters
    // --------- This took an entire day and a half to figure out and its probably still a bad method ------------ never forget 10/12/2019

    private void Death(float exForce, Vector3 exPos, float radius, float upForce, ForceMode frcMode)
    {
        UpdateGameLists();
        OnDeath();
        if (ragdoll)
        {
            ragdoll.SetActive(true);
            ragdoll.transform.parent = null;
            GetComponent<Collider>().enabled = false;
            Collider[] colliders = Physics.OverlapSphere(exPos, radius);

            foreach (Collider hit in colliders)
            {
                if (hit.GetComponent<Rigidbody>())
                    hit.GetComponent<Rigidbody>().AddExplosionForce(exForce, exPos, radius, upForce, frcMode);
            }
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// Called before being destroyed, will send calls to commanders, protectors, and behavior handlers (active formation)
    /// To remove this enemy from their list and stop their current actions
    /// </summary>
    protected virtual void OnDeath()
    {
        foreach(Commander com in activeCommanders)
        {
            com.TetheredEnemyDied(this);
        }
        foreach(Protector prot in activeProtectors)
        {
            prot.TargetDied();
        }
        if (behaviorHandler != null)
            behaviorHandler.EnemyInFormationDied(this);
    }


    private void GetRidOfChildren()
    {
        BroadcastMessage("Unparent", SendMessageOptions.DontRequireReceiver);
    }

    private void UpdateHealthAndLists()
    {
        if (health <= dangerousHealth && !inDanger)
        {
            inDanger = true;
            LevelController.AddEnemyInDanger(this);
        }
    }

    private void UpdateGameLists()
    {
        LevelController.RemoveEnemy(this);
        if (inDanger)
        {
            LevelController.RemoveEnemyInDanger(this);
        }

    }

    // Methods to control the active commanders that have their target on this enemy
    public void AddCommander(GameObject obj, Commander script, float damageFactor)
    {
        activeCommanders.Add(script);
        AddDamageChangeFactor(obj, damageFactor);
    }
    public void RemoveCommander(GameObject obj, Commander script)
    {
        activeCommanders.Remove(script);
        RevertDamageChangeFactor(obj);
    }


    // Methods to control the active protectors that have their target on this enemy
    public void AddProtector(Protector script) { activeProtectors.Add(script); }
    public void RemoveProtector(Protector script) { activeProtectors.Remove(script);  }
    public List<Protector> GetActiveProtectors() { return activeProtectors; }
    public int GetActiveProtectorCount() { return activeProtectors.Count; }



    public void SpeedUp(float speedUpFactor)
    {
        //originalMoveSpeed = nma.speed;
        //nma.speed = nma.speed * speedUpFactor;
    }

    public void SpeedDown()
    {
        //nma.speed = originalMoveSpeed;
    }

    protected Projectile CreateBasicProjectile()
    {
        Projectile bullet = projPool.GetNextProjectile();
        bullet.transform.position = projectileSpawn.position;
        bullet.transform.rotation = firePointRotation.rotation;
        bullet.ChangeDamage(GetTotalChangedDamage());
        return bullet;
    }

    private float GetTotalChangedDamage()
    {
        float total = 1;
        for (int i = 0; i < activeDamageChanges.Count; i++)
        {
            total *= activeDamageChanges[i].damageFactor;
        }
        return total;
    }

    public void AddDamageChangeFactor(GameObject objectCalling, float factor)
    {
        activeDamageChanges.Add(new DamageBoost(objectCalling, factor));
    }

    public bool RevertDamageChangeFactor(GameObject objectCalling)
    {
        for (int i = 0; i < activeDamageChanges.Count; i++)
        {
            if (activeDamageChanges[i].objectCalled == objectCalling)
            {
                activeDamageChanges.RemoveAt(i);
                return true;
            }
        }
        return false;
    }


    public virtual void StartGroupProtection(Vector3 position)
    {
        inFormation = true;
        activeBehavior = BehaviorController.Behavior.groupProt;
    }

    public virtual void SecondStartGroupProtection()
    {

    }

    public virtual void EndFomation()
    {
        inFormation = false;
        activeBehavior = BehaviorController.Behavior.none;
        LevelController.enemiesWithoutActiveBehavior.Add(this);
    }

    public void SetBehaviorHandler(BehaviorController.BehaviorHandler behaviorHandler) { this.behaviorHandler = behaviorHandler; }
    public bool HasActiveBehavior() { return activeBehavior != BehaviorController.Behavior.none; }
    public BehaviorController.Behavior GetActiveBehavior() { return activeBehavior; }

    protected bool IsOutOfBounds() { return LevelController.CheckTransformOutOfBounds(transform.position); }




    

    public enum MovementReason
    {
        none,
        commanderIntoGroup,
    }
}