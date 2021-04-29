using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : BasicAbility, AbilityADT
{
    ProjectilePoolHandler projPool;
    [SerializeField] GameObject grapple;
    [SerializeField] GameObject hookShotPrefab;
    Hook hook;
    HookShot hookShot;

    [SerializeField] GameObject grappleStartPos;
    [SerializeField] GameObject firePointRotation;

    public bool firing = false;
    bool canAlt = false;
    public bool altOnCooldown = false;

    [SerializeField] int levelForAlt = 2;
    [SerializeField] int levelForReset = 3;
    [SerializeField] int levelForContinue = 4;
    bool willReset = false;
    bool willContinue = false;
    [SerializeField] float hookShotCooldown = 5f;
    [SerializeField] int canGrabL1 = 1;
    [SerializeField] int canGrabL2 = 2;
    [SerializeField] int canGrabL3 = 3;
    [SerializeField] int canGrabL4 = 10;
    int canGrab = 1;
    [SerializeField] float speedL1 = 20;
    [SerializeField] float speedL2 = 30;
    [SerializeField] float speedL3 = 40;
    [SerializeField] float speedL4 = 50;
    float speed = 20;
    [SerializeField] float damageL1 = 0;
    [SerializeField] float damageL2 = 15;
    [SerializeField] float damageL3 = 20;
    [SerializeField] float damageL4 = 25;
    float damage = 0;

    GameObject firedGrapple;
    [SerializeField] float groundPlaneYAxis = -1.2265f;
    bool scriptRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        projPool = GlobalClass.hookShotPool;
        firedGrapple = Instantiate(grapple, grappleStartPos.transform.position, Quaternion.identity);
        firedGrapple.transform.parent = transform;
        hook = firedGrapple.GetComponent<Hook>();
        hook.SetGrapple(this);
        //var shot = Instantiate(hookShotPrefab, grappleStartPos.transform.position, Quaternion.identity);
       // hookShot = shot.GetComponent<HookShot>();
       // hookShot.transform.parent = transform;
       // hookShot.SetGrapple(this);
        CheckAbilityLevel();
    }

    public override void CheckAbilityLevel()
    {
        switch (currentAbilityLevel)
        {
            case 1:
                canGrab = canGrabL1;
                speed = speedL1;
                damage = damageL1;
                break;
            case 2:
                canGrab = canGrabL2;
                speed = speedL2;
                damage = damageL2;
                break;
            case 3:
                canGrab = canGrabL3;
                speed = speedL3;
                damage = damageL3;
                break;
            case 4:
                canGrab = canGrabL4;
                speed = speedL4;
                damage = damageL4;
                break;
        }

        if (currentAbilityLevel >= levelForAlt)
        {
            canAlt = true;
            if (currentAbilityLevel >= levelForReset)
                willReset = true;
            if (currentAbilityLevel >= levelForContinue)
                willContinue = true;
        }
        hook.UpdateLevelValues(canGrab, speed);
    }

    public void HookReturned()
    {
        firing = false;
    }

    public void InputGetDown()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (canAlt)
            {
                if (!altOnCooldown)
                {
                    HookShot hook = projPool.GetNextAltProjectile().GetComponent<HookShot>();
                    hook.transform.position = grappleStartPos.transform.position;
                    hook.transform.rotation = transform.rotation;
                    hook.SetValues(damage, willReset, willContinue);
                    hook.EnableProjectile();
                    StartCoroutine(HookShotCooldown());
                    altOnCooldown = true;
                    AbilityUsed();
                }
            }
        }
        else if (!firing)
        {
            hook.Fire(GlobalClass.player.mouseLocationConverted);
            firing = true;
            AbilityUsed();
        }
    }

    IEnumerator HookShotCooldown()
    {
        yield return new WaitForSeconds(hookShotCooldown);
        altOnCooldown = false;
    }

    public void ResetHookShotCooldown()
    {
        altOnCooldown = false;
        StopCoroutine(HookShotCooldown());
    }

    public void InputGet()
    {
        return;
    }

    public void InputGetUp()
    {
        return;
    }

    public void SetScriptStatus(bool status)
    {
        scriptRunning = status;
    }
}
