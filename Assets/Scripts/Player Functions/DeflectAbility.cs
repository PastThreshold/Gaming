using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeflectAbility : BasicAbility, AbilityADT
{
    [SerializeField] Deflect shield;
    [SerializeField] Deflect altShield;
    [SerializeField] float timeShieldCanDeflect = 0.1f;
    [SerializeField] float cooldown = 1.5f;
    [SerializeField] float altCooldown = 10f;
    int levelToReset = 3;
    bool canReset = false;
    int levelToAlt = 2;
    bool canAlt = false;
    bool onCooldown = false;
    bool altOnCooldown = false;
    bool willReset = false;
    bool scriptRunning = false;

    public override void CheckAbilityLevel()
    {
        switch (currentAbilityLevel)
        {
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
        }

        if (currentAbilityLevel >= levelToReset)
            canReset = true;
        if (currentAbilityLevel >= levelToAlt)
            canAlt = true;
    }

    private void Start()
    {
        shield.gameObject.SetActive(false);
        altShield.gameObject.SetActive(false);
        CheckAbilityLevel();
    }

    private void EnableProjectileShield()
    {
        shield.gameObject.SetActive(true);
        StartCoroutine("WaitForDisable");
    }

    private void DisableProjectileShield()
    {
        shield.gameObject.SetActive(false);
    }

    IEnumerator WaitForDisable()
    {
        yield return new WaitForSeconds(timeShieldCanDeflect);
        shield.gameObject.SetActive(false);
        if (canReset && willReset)
        {
            onCooldown = false;
            willReset = false;
        }
        else
        {
            StartCoroutine("Cooldown");
        }
    }

    IEnumerator Cooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }

    IEnumerator AltCooldown()
    {
        altOnCooldown = true;
        yield return new WaitForSeconds(altCooldown);
        altOnCooldown = false;
    }

    private void AltFire()
    {
        print("altfire");
        StartCoroutine("AltCooldown");
        altShield.gameObject.SetActive(true);
        altShield.transform.parent = null;
        altShield.SetAltMode();
    }

    public void AltReturned()
    {
        altShield.transform.position = transform.position;
        altShield.transform.parent = transform;
        altShield.transform.rotation = Quaternion.identity;
        altShield.gameObject.SetActive(false);
    }

    public void ResetCooldown()
    {
        if (canReset)
            willReset = true;
    }

    public void InputGetDown()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (canAlt)
            {
                if (!altOnCooldown)
                {
                    AltFire();
                    AbilityUsed();
                }
            }
        }
        else if (!onCooldown)
        {
            EnableProjectileShield();
            AbilityUsed();
        }
    }

    public void InputGet()
    {
        return;
    }

    public void InputGetUp()
    {
        DisableProjectileShield();
    }

    public void SetScriptStatus(bool status)
    {
        scriptRunning = status;
    }
}
