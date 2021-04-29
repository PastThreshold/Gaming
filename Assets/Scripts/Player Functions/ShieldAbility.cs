using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldAbility : BasicAbility, AbilityADT
{
    [SerializeField] Shield shield;
    Vector3 shieldStartPos;

    [Tooltip("This is the minimum amount of shield time in order to activate the ability")]
    [Range(0, 2f)] [SerializeField] float minShieldTimeActivate;
    [SerializeField] float minShieldHealthActivate = 50f;

    float timeToActivate = 1f;
    bool canActivate = true;
    bool inUse = false;

    float shieldStartHealth = 0;                             
    [Header("Level Values")]
    [SerializeField] float shieldStartHealthL1 = 300f;
    [SerializeField] float shieldStartHealthL2 = 400f;
    [SerializeField] float shieldStartHealthL3 = 500f;
    [SerializeField] float shieldStartHealthL4 = 600f;

    float shieldHealthAdd = 0;
    [SerializeField] float shieldHealthAddL1 = 30f;
    [SerializeField] float shieldHealthAddL2 = 35f;
    [SerializeField] float shieldHealthAddL3 = 40f;
    [SerializeField] float shieldHealthAddL4 = 45f;

    float altDamageFactor = 0;
    [SerializeField] float altDamageFactorL1 = 1.2f;
    [SerializeField] float altDamageFactorL2 = 1.3f;
    [SerializeField] float altDamageFactorL3 = 1.4f;
    [SerializeField] float altDamageFactorL4 = 1.5f;

    float maxAltTime = 0;
    [SerializeField] float maxAltTimeL1 = 10f;
    [SerializeField] float maxAltTimeL2 = 14f;
    [SerializeField] float maxAltTimeL3 = 18f;
    [SerializeField] float maxAltTimeL4 = 22f;

    float altTime = 0;
    bool altActive;

    bool scriptRunning = false;

    private void Start()
    {
        BaseStart();
        shieldStartPos = shield.transform.position;
        shield.gameObject.SetActive(false);
        CheckAbilityLevel();
    }

    public override void CheckAbilityLevel()
    {
        switch (currentAbilityLevel)
        {
            case 1:
                shieldStartHealth = shieldStartHealthL1;
                shieldHealthAdd = shieldHealthAddL1;
                altDamageFactor = altDamageFactorL1;
                maxAltTime = maxAltTimeL1;
                break;
            case 2:
                shieldStartHealth = shieldStartHealthL2;
                shieldHealthAdd = shieldHealthAddL2;
                altDamageFactor = altDamageFactorL2;
                maxAltTime = maxAltTimeL2;
                break;
            case 3:
                shieldStartHealth = shieldStartHealthL3;
                shieldHealthAdd = shieldHealthAddL3;
                altDamageFactor = altDamageFactorL3;
                maxAltTime = maxAltTimeL3;
                break;
            case 4:
                shieldStartHealth = shieldStartHealthL4;
                shieldHealthAdd = shieldHealthAddL4;
                altDamageFactor = altDamageFactorL4;
                maxAltTime = maxAltTimeL4;
                break;
            default:
                Debug.Log("Leveling error");
                break;
        }
        altTime = maxAltTime;
        shield.SetValues(shieldStartHealth, altDamageFactor);
    }

    void Update()
    {
        if (scriptRunning)
        {
            if (altActive)
            {
                altTime -= 1f * Time.deltaTime;
                if (altTime <= 0)
                {
                    DisableProjectileShield();
                }
            }
            else if (!shield.isActiveAndEnabled)
            {
                Mathf.Clamp(altTime += .5f * Time.deltaTime, 0, maxAltTime);
                shield.ChangeHealth(shieldHealthAdd * Time.deltaTime);
            }
        }
    }

    private void DisableProjectileShield()
    {
        shield.Disabled();
        shield.gameObject.SetActive(false);
        shield.transform.parent = transform;
        shield.transform.position = transform.position;
        shield.transform.rotation = transform.rotation;
        inUse = false;
    }

    private void EnableProjectileShield()
    {
        shield.gameObject.SetActive(true);
        inUse = true;
    }

    public void ShieldDestroyed()
    {
        DisableProjectileShield();
    }

    public void InputGetDown()
    {
        if (!inUse)
        {
            if (canActivate)
            {
                if (Input.GetKey(KeyCode.LeftShift) && altTime >= minShieldTimeActivate)
                {
                    EnableProjectileShield();
                    shield.AltForm();
                    altActive = true;
                    AbilityInUse();
                }
                else if (shield.GetHealth() >= minShieldHealthActivate)
                {
                    EnableProjectileShield();
                    shield.Enabled();
                    AbilityInUse();
                }
            }
        }
        else
        {
            DisableProjectileShield();
            StartCoroutine(Cooldown());
            AbilityNotInUse();
            altActive = false;
        }
        /*
        if ((Input.GetKey(KeyCode.LeftShift) && altActive) || altActive)
        {
            DisableProjectileShield();
            altActive = false;
            StartCoroutine("Cooldown");
            AbilityNotInUse();
        }
        else if (canActivate)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                EnableProjectileShield();
                shield.AltForm();
                altActive = true;
                AbilityInUse();
            }
            else if (altTime >= minShieldTimeActivate && shield.GetHealth() >= minShieldHealthActivate && canActivate)
            {
                EnableProjectileShield();
                shield.Enabled();
                AbilityInUse();
            }
        }
        else if (shield.gameObject.activeSelf && !altActive)
        {
            DisableProjectileShield();
            StartCoroutine("Cooldown");
            AbilityNotInUse();
        }*/
    }

    IEnumerator Cooldown()
    {
        canActivate = false;
        yield return new WaitForSeconds(timeToActivate);
        canActivate = true;
    }

    public void InputGet()
    {

    }

    public void InputGetUp()
    {

    }

    public void SetScriptStatus(bool status)
    {
        scriptRunning = status;
    }
}
