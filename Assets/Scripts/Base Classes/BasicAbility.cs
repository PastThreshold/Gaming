using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAbility : MonoBehaviour
{
    public enum abilityType
    {
        blink,
        shield,
        deflect,
        bulletTime,
        grapple,
        holdPickup,
        pushPull,
        enrage,
        autoTarget
    }

    protected static Player player;
    protected static HeadsUpDisplay hud;
    const int MAX_LEVEL = 4;
    const int PERM_MAX_LEVEL = 3;

    [Header("Basic Ability Data")]
    [SerializeField] protected abilityType type;
    [Range(1, 4)] [SerializeField] protected int permanantAbilityLevel = 1;
                                   protected int currentAbilityLevel = 1;
    bool temporaryLevelUpgrade = false;

    protected void BaseStart()
    {
        if (player == null)
            player = FindObjectOfType<Player>();
        if (hud == null)
            hud = FindObjectOfType<HeadsUpDisplay>();

        currentAbilityLevel = permanantAbilityLevel;
    }

    public virtual void CheckAbilityLevel()
    {
        // This is empty to let the abilities overide it.
        // It would be part of AbilityADT the interface but this script calls this fuction
    }

    public int GetAbilityLevel()
    {
        return permanantAbilityLevel;
    }

    public void PermanantUpgradeAbilityLevel()
    {
        if (permanantAbilityLevel < PERM_MAX_LEVEL)
        {
            permanantAbilityLevel++;

            if (currentAbilityLevel < MAX_LEVEL)
                currentAbilityLevel++;
        }

        CheckAbilityLevel();
    }

    public void TemporaryUpgradeAbilityLevel(float time)
    {
        if (currentAbilityLevel < MAX_LEVEL)
        {
            if (!temporaryLevelUpgrade)
            {
                currentAbilityLevel = permanantAbilityLevel + 1;
                temporaryLevelUpgrade = true;
            }
            else
            {
                currentAbilityLevel++;
            }

            CheckAbilityLevel();
            StartCoroutine("DowngradeAbilityLevel", time);
        }
    }

    protected virtual IEnumerator DowngradeAbilityLevel(float time)
    {
        yield return new WaitForSeconds(time);

        if (currentAbilityLevel > permanantAbilityLevel)
        {
            currentAbilityLevel--;

            if (currentAbilityLevel == permanantAbilityLevel)
                temporaryLevelUpgrade = false;
        }
        else if (temporaryLevelUpgrade)
            temporaryLevelUpgrade = false;

        CheckAbilityLevel();
    }

    protected void AbilityInUse()
    {
        hud.AbilityInUse(type);
    }

    protected void AbilityNotInUse()
    {
        hud.AbilityNotInUse(type);
    }

    protected void AbilityUsed()
    {
        hud.AbilityImmediateUse(type);
    }
}
