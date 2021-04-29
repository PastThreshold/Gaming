using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookShot : AltProjectile
{
    [SerializeField] TrailRenderer trail;
    bool trailFrozen = false;
    Vector3[] positions;
    [SerializeField] MeshRenderer mesh;
    static Grapple grapple;

    bool willReset = false;
    bool willContinue = false;
    List<SphereWeapon> spheresHit;

    Vector3 lastPos;
    float timeUntilDeath;

    const float RAY_LENGTH = 50f;
    const float MAX_LIFE = 3f;
    const float LAST_FRAME_WAIT = 0.001f;

    bool canBounce = false;
    bool willBounce = false;
    int startBounces = 2;
    int bounces = 0;
    Vector3 bounceDirection = Vector3.zero;


    public void SetValues(float damage, bool willReset, bool willContinue)
    {
        currentDamage = damage;
        this.willReset = willReset;
        this.willContinue = willContinue;
    }

    void Start()
    {
        if (grapple == null)
            grapple = FindObjectOfType<Grapple>();
        DisableProjectile();
    }

    void Update()
    {
        if (moving)
            transform.position += transform.forward * speed * Time.deltaTime;
    }
    
    private void RaycastForEnemy(bool sphereIncluded)
    {
        Enemy enemyHit = null;
        // Called with true so it may hit the sphere, but since it wont disappear after hitting
        // it calls this again with false if it hits it
        LayerMask layer;
        if (sphereIncluded)
            layer = GlobalClass.exD.bulletLayerMask;
        else
            layer = GlobalClass.exD.bulletsNoDefault;

        if (Physics.Raycast(transform.position, transform.forward,
            out RaycastHit hit, RAY_LENGTH, layer))
        {
            switch (hit.transform.tag)
            {
                case GlobalClass.ENEMY_TAG:
                    enemyHit = hit.transform.GetComponent<Enemy>();
                    break;
                case GlobalClass.ROOM_TAG:
                    if (canBounce && bounces > 0)
                    {
                        willBounce = true;
                        bounces--;
                        bounceDirection = Vector3.Reflect(transform.forward, hit.normal).normalized;
                        Debug.DrawLine(hit.point, hit.point + bounceDirection, Color.red, 10f);
                    }
                    break;
                case GlobalClass.SPECIAL_TAG:
                    if (hit.collider.GetComponent<SphereWeapon>())
                    {
                        spheresHit.Add(hit.collider.GetComponent<SphereWeapon>());
                        canBounce = true;
                        bounces = startBounces;
                        RaycastForEnemy(false);
                        return;
                    }
                    break;
                default:
                    Debug.Log("Different String Collision by: " + hit.transform.name + ", tag: " + hit.transform.tag);
                    break;
            }
        }
        else
            timeUntilDeath = MAX_LIFE;

        CalculateLastPosition(hit, enemyHit);
    }


    /// <summary>
    /// Calculates the time, distance, and speed relationship for coroutine
    /// Will Start different coroutines whether and enemy was hit or not, and if the object will bounce off the wall or not
    /// </summary>
    /// <param name="hit">Collider hit</param>
    /// <param name="enemyHit">Passed if an enemy was hit, null otherwise</param>
    private void CalculateLastPosition(RaycastHit hit, Enemy enemyHit)
    {
        lastPos = hit.point;
        timeUntilDeath = hit.distance / speed;
        if (willBounce)
            StartCoroutine("Bounce");
        else if (enemyHit != null)
            StartCoroutine("DamageEnemy", enemyHit);
        else
            StartCoroutine("Disable");
    }

    /// <summary>
    /// Things are weird when objects move at such high speeds, slowed down, objects correctly die at
    /// calculated speed / time / distance values, but at high speeds, they render past where they should
    /// To combat this the function sets the transform to the end of the ray and makes the object wait for
    /// 0.001 seconds to render for that last frame
    /// </summary>
    IEnumerator Disable()
    {
        yield return new WaitForSeconds(timeUntilDeath);
        moving = false;
        transform.position = lastPos;
        yield return new WaitForSeconds(LAST_FRAME_WAIT);
        DisableProjectile();
    }

    IEnumerator DamageEnemy(Enemy enemy)
    {
        yield return new WaitForSeconds(timeUntilDeath);
        transform.position = lastPos;
        if (enemy.health - currentDamage <= 0)
        {
            enemy.TakeDamage(currentDamage);
            yield return null;
            if (enemy != null)
            {
                Debug.Log("Enemy did not correctly die for next raycast");
                StopCoroutine("DamageEnemy");
            }

            if (willReset)
            {
                grapple.ResetHookShotCooldown();
                if (willContinue)
                    RaycastForEnemy(true);
            }
        }
        else
        {
            enemy.TakeDamage(currentDamage);
            DisableProjectile();
        }
    }

    IEnumerator Bounce()
    {
        yield return new WaitForSeconds(timeUntilDeath);
        transform.position = lastPos;
        transform.LookAt(Extra.SetYToTransform(transform.position + bounceDirection, transform));
        willBounce = false;
        RaycastForEnemy(true);
    }

    public override void StopMoving()
    {
        moving = false;
        StopCoroutine("Disable");

        trail.time = Mathf.Infinity;
        positions = new Vector3[trail.positionCount];
        trail.GetPositions(positions);
        trailFrozen = true;
    }

    public override void ContinueMoving()
    {
        moving = true;
        if (trailFrozen)
        {
            trail.time = 0.175f;
            for (int i = 0; i < positions.Length; i++)
            {
                trail.AddPosition(positions[i]);
            }
            trailFrozen = false;
        }
        RaycastForEnemy(true);
    }

    public override void EnableProjectile()
    {
        trail.enabled = true;
        if (!BulletTime.isFrozen)
        {
            moving = true;
            RaycastForEnemy(true);
        }
    }

    public override void DisableProjectile()
    {
        trail.enabled = false;
        currentDamage = startDamage;
        canBounce = false;
        willBounce = false;
        bounces = startBounces;
        base.DisableProjectile();
    }
}
