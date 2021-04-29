using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTime : BasicAbility, AbilityADT
{
    [SerializeField] float bulletTimeLeft;

    [Tooltip("This is the minimum amount of bullet time in order to activate the ability")]
    [Range(0f, 2f)] [SerializeField] float minBulletTimeActivate;
    [SerializeField] float maxBulletTime;
    [Range(0f, 1f)] [SerializeField] float slowedTimeScale = 0.5f;
    [SerializeField] float timeBetweenActivations = 1f;

    bool canActivate = true;
    int levelForFreeze = 2;
    bool canFreeze = false;
    bool scriptRunning = false;
    bool inUse = false;
    public static bool isFrozen = false;

    private void Start()
    {
        BaseStart();
        CheckAbilityLevel();
    }

    public override void CheckAbilityLevel()
    {
        switch(currentAbilityLevel)
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

        if (currentAbilityLevel >= levelForFreeze)
            canFreeze = true;
    }

    void Update()
    {
        if (scriptRunning)
        {
            if (inUse)
            {
                if (bulletTimeLeft > 0)
                {
                    bulletTimeLeft -= 1f * Time.unscaledDeltaTime;
                    if (bulletTimeLeft <= 0)
                    {
                        TurnOffAbility();
                    }
                }
            }
            else
            {
                bulletTimeLeft += .5f * Time.deltaTime;
                Mathf.Clamp(bulletTimeLeft, 0, maxBulletTime);
            }
        }
    }

    public void InputGetDown()
    {
        if (canActivate)
        {
            TurnOnAbility();
        }
        else if (inUse)
        {
            TurnOffAbility();
        }
    }

    private void TurnOnAbility()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (bulletTimeLeft > minBulletTimeActivate)
            {
                isFrozen = true;
                LevelController.FreezeAllProjectiles();
            }
        }
        else
        {
            if (bulletTimeLeft > minBulletTimeActivate)
            {
                Time.timeScale = slowedTimeScale;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
            }
        }
        AbilityInUse();
        inUse = true;
        canActivate = false;
    }

    private void TurnOffAbility()
    {
        if (isFrozen)
        {
            LevelController.UnFreezeAllProjectiles();
            isFrozen = false;
        }
        else
        {
            Time.timeScale = 1;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
        }
        AbilityNotInUse();
        StartCoroutine("WaitBetweenActivates");
        inUse = false;
    }

    public void InputGet()
    {

    }

    public void InputGetUp()
    {

    }

    IEnumerator WaitBetweenActivates()
    {
        yield return new WaitForSeconds(timeBetweenActivations);
        canActivate = true;
    }

    public void SetScriptStatus(bool status)
    {
        scriptRunning = status;
    }
}
