using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enrage : BasicAbility, AbilityADT
{
    Player playerScript;
    bool scriptRunning = false;
    [SerializeField] float maxTime = 10f;
    [SerializeField] float timeToActivate = 5;
    float damageFactor = 0;
    [SerializeField] float damageFactorL1 = 1.2f;
    [SerializeField] float damageFactorL2 = 1.3f;
    [SerializeField] float damageFactorL3 = 1.4f;
    [SerializeField] float damageFactorL4 = 1.5f;
    float healPerSecond = 0;
    [SerializeField] float healPerSecondL2 = 2f;
    [SerializeField] float healPerSecondL3 = 4f;
    [SerializeField] float healPerSecondL4 = 8f;
    [SerializeField] float cooldownTimer = 1.5f;

    int levelToHeal = 2;
    bool canHeal = false;
    float damageToTimeConversion = 0.04f; // Time += damageTaken * conversion (ex. 20 * .04 = 0.8 seconds)
    bool onCooldown = false;
    bool enraged = false;
    bool healing = false;

    [SerializeField] GameObject enrageEffects;
    [SerializeField] GameObject enrageEffects2;
    [SerializeField] GameObject healEffects;
    [SerializeField] GameObject healEffects2;
    public float totalTime = 0f;

    public override void CheckAbilityLevel()
    {
        switch (currentAbilityLevel)
        {
            case 1:
                damageFactor = damageFactorL1;
                break;
            case 2:
                damageFactor = damageFactorL2;
                healPerSecond = healPerSecondL2;
                break;
            case 3:
                damageFactor = damageFactorL3;
                healPerSecond = healPerSecondL3;
                break;
            case 4:
                damageFactor = damageFactorL4;
                healPerSecond = healPerSecondL4;
                break;
        }

        if (currentAbilityLevel >= levelToHeal)
            canHeal = true;
    }

    private void Start()
    {
        playerScript = GlobalClass.player;
        CheckAbilityLevel();
    }

    public void TookDamage(float damageTaken)
    {
        if (scriptRunning)
        {
            float timeAcquired = damageTaken * damageToTimeConversion;
            totalTime += timeAcquired;
            if (totalTime > maxTime)
                totalTime = maxTime;
        }
    }

    private void Update()
    {
        if (enraged || healing)
        {
            if (healing)
            {
                playerScript.Heal(healPerSecond * Time.deltaTime);
            }
            totalTime -= 1f * Time.deltaTime;
            if (totalTime <= 0)
            {
                Deactivate();
            }
        }
    }

    public void InputGet()
    {
        if (!onCooldown)
        {
            if (enraged || healing)
            {
                Deactivate();
            }
        }
    }

    public void InputGetDown() 
    {
        // Is there enough time
            // Is it already active
                // Is it on cooldown
                    // Is it the alt or not // healing unlocked?
        if (totalTime >= timeToActivate)
        {
            if (!enraged && !healing)
            {
                if (!onCooldown)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        if (canHeal)
                        {
                            healing = true;
                            healEffects.SetActive(true);
                            healEffects2.SetActive(true);
                        }
                    }
                    else
                    {
                        enraged = true;
                        playerScript.enraged = true;
                        enrageEffects.SetActive(true);
                        enrageEffects2.SetActive(true);
                    }
                    AbilityInUse();
                    StartCoroutine("Cooldown");
                }
            }
        }
    }

    public void InputGetUp()
    {

    }

    public  void Deactivate()
    {
        playerScript.enraged = false;
        healing = false;
        enraged = false;
        healEffects.SetActive(false);
        healEffects2.SetActive(false);
        enrageEffects.SetActive(false);
        enrageEffects2.SetActive(false);
        AbilityNotInUse();
        StartCoroutine("Cooldown");
    }

    IEnumerator Cooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(cooldownTimer);
        onCooldown = false;
    }

    public void SetScriptStatus(bool status)
    {
        scriptRunning = status;
    }

    public float GetDamageFactor()
    {
        return damageFactor;
    }

    public bool isHealing()
    {
        return healing;
    }
}
