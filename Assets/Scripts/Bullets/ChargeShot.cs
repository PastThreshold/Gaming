using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeShot : Projectile
{
    [SerializeField] float minimumScale = 0.07f;
              public float maximumScale = 0.75f;
    [SerializeField] float minDamage = 10;
    [SerializeField] float maxDamage = 20;
    [SerializeField] float scaleAddPerSecSphere = 1f;
    bool charging = false; 
    bool atLastState = false;
    bool maxScale = false;
    bool hitSphere = false;
    bool fired = false;

    GameObject currentState;
    [SerializeField] GameObject[] damageStates;
    [SerializeField] GameObject tendril;
    [SerializeField] float[] statePoints;
    float scale;
    int index = 0;
    bool started = false;
    [SerializeField] ParticleController pSystem;
    [SerializeField] ParticleSystem startP;
    float startTime;
    [SerializeField] ParticleSystem midP;
    float midTime;
    [SerializeField] ParticleSystem endP;
    float endTime;

    //[Header("Disable Components")]
    //[SerializeField] MeshRenderer mesh;

    void Start()
    {
        BaseStart();


        if (damageStates.Length != statePoints.Length)
            Debug.Log("Arrays not the same length");
        if (damageStates.Length <= 1)
            atLastState = true;
        foreach (GameObject state in damageStates)
        {
            state.SetActive(false);
        }
        currentState = damageStates[0];

        DisableProjectile();

        startTime = startP.main.startLifetime.constant;
        midTime = midP.main.startLifetime.constant;
        endTime = endP.main.startLifetime.constant;
    }

    void Update()
    {
        if (moving)
        {
            if (hitSphere)
            {
                Charge(scaleAddPerSecSphere);
            }
        }
    }

    private void FixedUpdate()
    {
        BaseFixedUpdate();
    }


    /// <summary>
    /// Charges the projectile, increases it scale if it has not already, and at certain points denoted by
    /// statePoints[], the look of the projectile will change
    /// TODO - make the state changes actually do something
    /// </summary>
    /// <param name="scalePerSecond">scale to add per second, called by chargeRifle</param>
    public void Charge(float scalePerSecond)
    {
        if (!maxScale)
        {
            transform.localScale += new Vector3(scalePerSecond, scalePerSecond, scalePerSecond) * Time.deltaTime;
            tendril.transform.localScale = transform.localScale;
            if (transform.localScale.x >= maximumScale)
                maxScale = true;
        }

        if (!atLastState)
        {
            scale += scalePerSecond * Time.deltaTime;
            if (!started)
            {
                print("Scale: " + scale + " added: " + (scale + pSystem.ChargeShotTotalTime()));
                if (scale + pSystem.ChargeShotTotalTime() >= statePoints[index])
                {
                    started = true;
                    pSystem.ChargeShotStart();
                }
            }
            if (scale >= statePoints[index])
            {
                ChangeState(index);

                if (index < statePoints.Length - 1)
                    index++;
                else
                    atLastState = true;
            }
        }
    }

    /// <summary>
    /// Disables current state, sets it to next one, and enables that one, this way only the required ones are disabled
    /// unlike weaponswitcher which uses a foreach to disable all weapons
    /// </summary>
    /// <param name="index"></param>
    private void ChangeState(int index)
    {
        currentState.SetActive(false);
        currentState = damageStates[index];
        currentState.SetActive(true);
    }

    /// <summary>
    /// Calculates the damage based on its scale currently (subject to change)
    /// </summary>
    /// <param name="rotation"></param>
    public void FireShot()
    {
        float dmgInterpolant = Mathf.InverseLerp(minimumScale, maximumScale, transform.localScale.x);
        currentDamage = Mathf.Lerp(minDamage, maxDamage, dmgInterpolant);
        pSystem.ChargeShotStop();
        charging = false;
        fired = true;
        if (!BulletTime.isFrozen)// Normally on enableProjectile(), but this does not move until released
        {
            moving = true;
            rb.velocity = transform.forward * currentSpeed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case GlobalClass.DEFAULT_TAG:
                DisableProjectile();
                break;
            case GlobalClass.PLAYER_TAG:
                other.GetComponentInParent<Player>().TakeDamage(currentDamage);
                DisableProjectile();
                break;
            case GlobalClass.PROJECTILE_TAG:
                break;
            case GlobalClass.ENEMY_TAG:
                other.GetComponentInParent<Enemy>().TakeDamage(currentDamage);
                DisableProjectile();
                break;
            case GlobalClass.PICKUP_TAG:
                break;
            case GlobalClass.ROOM_TAG:
                DisableProjectile();
                break;
            case GlobalClass.SPECIAL_TAG:
                if (!other.GetComponent<SphereWeapon>())
                {
                    if (!other.GetComponent<TimeField>())
                        DisableProjectile();
                }
                else
                {
                    if (fired)
                    hitSphere = true;
                }
                break;
            case GlobalClass.SHIELD_TAG:
                break;
            case GlobalClass.DEFLECT_TAG:
                break;
            case GlobalClass.PROJECTILE_GATE_TAG:
                break;
            case GlobalClass.DETECT_BULLETS_TAG:
                break;
            default:
                Debug.Log("Different String Collision");
                DisableProjectile();
                break;
        }
    }

    public override void StopMoving()
    {
        moving = false;
    }

    public override void ContinueMoving()
    {
        moving = true;
    }

    public override void DisableProjectile()
    {
        currentState.SetActive(false);
        currentState = damageStates[0];
        index = 1;
        moving = false;
        transform.localScale = new Vector3(minimumScale, minimumScale, minimumScale);
        base.DisableProjectile();
        enabled = false;
    }

    public override void EnableProjectile()
    {
        currentState.SetActive(true);
        charging = true;
            startP.Stop();
        midP.Stop();
        endP.Stop();
    }
}
