using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsSwitcher : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] BasicWeapon activeWeapon;
    public BasicWeapon[] equippedWeapons;
    [SerializeField] BasicWeapon[] allWeapons;
    int activeSlot;
    /*
    AssaultRifle ar; SniperRifle sniper; Shotgun shotty; Deagles deagle; StickyBombLauncher stickyBL; Shredder shredder;
    LaserBeam laser; ChargeRifle cr; RocketLauncher rpg;
    */


    [Range(0f, 1f)] [SerializeField] float timerNotEquippedPercentageSubtraction = 0.10f;
    public float[] weaponTimers;
    bool infniteAmmoCurrently = false;
    Player mainPlayer;
    HeadsUpDisplay hud;

    [Header("Missle Strike")]
    [SerializeField] BasicWeapon deadeyeWeapon;
    public bool inDeadeye = false;
    public Vector3 targetPosition;
    public bool positionChosen = false;
    int lastWeaponEquipped = -1;
    GameObject explosion;


    void Start()
    {
        hud = FindObjectOfType<HeadsUpDisplay>();
        mainPlayer = FindObjectOfType<Player>();
        /*
        ar = allWeapons[0].GetComponent<AssaultRifle>(); sniper = allWeapons[1].GetComponent<SniperRifle>();
        shotty = allWeapons[2].GetComponent<Shotgun>(); deagle = allWeapons[3].GetComponent<Deagles>();
        stickyBL = allWeapons[4].GetComponent<StickyBombLauncher>(); shredder = allWeapons[5].GetComponent<Shredder>();
        laser = allWeapons[6].GetComponent<LaserBeam>(); cr = allWeapons[7].GetComponent<ChargeRifle>();
        rpg = allWeapons[8].GetComponent<RocketLauncher>();
        */
        SetDefaultWeapon();
        ChangeActiveWeapon(0);

        foreach (BasicWeapon weapon in allWeapons)
        {
            weapon.DisableWeapon();
        }
        deadeyeWeapon.DisableWeapon();
        activeWeapon.EnableWeapon();
        weaponTimers = new float[equippedWeapons.Length];
    }

    void Update()
    {
        if (weaponTimers.Length > 0)
        {
            if (!infniteAmmoCurrently)
                WeaponTimers();
        }

        if (!inDeadeye)
            CheckForSwitchWeaponInput();
        else
            CheckForMissleStrikeClick();
    }

    private void WeaponTimers()
    {
        // For each weapon, check if it has a timer, check if it is the equipped weapon else subtract some time,  
        // check if it is the not the ar, subtract the amount of time it is held from timer
        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            if (weaponTimers[i] > 0)
            {
                if (IsActiveWeapon(equippedWeapons[i]))
                {
                    if (!IsActiveWeapon(allWeapons[0]))
                    {
                        weaponTimers[i] -= Time.deltaTime;
                        hud.UpdateAmmo(weaponTimers[i], i);
                    }
                }
                else
                {
                    weaponTimers[i] -= Time.deltaTime * timerNotEquippedPercentageSubtraction;
                    hud.UpdateAmmo(weaponTimers[i], i);
                }
                if (weaponTimers[i] < 0)
                {
                    // If its the sticky bomb launcher, explode all bombs
                    if (IsEquippedWeapon(4))
                        allWeapons[4].SendMessage("DisableWeapon");

                    // Remove weapon. Is it the active weapon?
                    if (IsActiveWeapon(equippedWeapons[i]))
                    {
                        RemoveWeaponFromEquipped(i);
                        ChangeActiveWeapon(0);
                        SwapCloneWeapon();
                    }
                    else
                    {
                        RemoveWeaponFromEquipped(i);
                    }
                    weaponTimers[i] = 0;
                }
            }
        }
    }

    /// <summary> Helper function for weapon timers to determine if the the weapon that has no time left is the currently equipped weapon or not
    private void RemoveWeaponFromEquipped(int equipSlot)
    {
        if (equipSlot == 0)
        {
            SetDefaultWeapon();
            hud.UpdateWeaponHUD(0);
            hud.UpdateWeaponHUD(0, 0, equippedWeapons[0].GetCurrentLevel());
        }
        else
        {
            equippedWeapons[equipSlot] = null;
            hud.UpdateWeaponHUD(equipSlot);
        }
    }

    // Checks for one of the keys pressed, if so it will change to that equipped weapon in the array
    private void CheckForSwitchWeaponInput()
    {
        int weaponNumber = -1;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            weaponNumber = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            weaponNumber = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            weaponNumber = 2;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (activeSlot == 0)
                weaponNumber = 2;
            else
                weaponNumber = activeSlot - 1;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (activeSlot == 2)
                weaponNumber = 0;
            else
                weaponNumber = activeSlot + 1;
        }

        if (weaponNumber != -1 && !WeaponSlotEmpty(weaponNumber))
            ChangeActiveWeapon(weaponNumber);
    }

    /// <summary> Enables and switches to the weapon that is equipped given by slot </summary>
    private void ChangeActiveWeapon(int slot)
    {
        activeWeapon = equippedWeapons[slot];
        activeSlot = slot;

        foreach (BasicWeapon weapon in allWeapons)
        {
            if (weapon != null)
                weapon.DisableWeapon();
        }
        activeWeapon.EnableWeapon();
        SwapCloneWeapon();
        hud.ChangeActiveWeapon(slot);
    }

    //Hold Pickup
    public void CollisionWithWeaponPickup(Pickup weapon)
    {
        float weaponTime = weapon.GetTime();
        int slot = Extra.ConvertFromWeaponTypeToGlobalNumber(weapon.weaponType);
        ChangeEquippedWeapon(slot, weaponTime);
        Destroy(weapon.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<WeaponUpgrade>())
        {
            BasicWeapon.WeaponType type = other.GetComponent<WeaponUpgrade>().GetWeaponType();
            int slot = Extra.ConvertFromWeaponTypeToGlobalNumber(type);
            allWeapons[slot].PermanantUpgradeWeaponLevel();
            hud.ChangeWeaponLevel(slot, allWeapons[slot].GetCurrentLevel());
            SwapCloneWeapon();
            Destroy(other.gameObject);
        }
    }

    // Helper function called by weapon pickups to correctly enter into the weapon slot and
    // Increase the time
    private void ChangeEquippedWeapon(int weaponNumber, float time)
    {
        int weaponSlotToAssign = activeSlot;
        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            if (IsEquippedWeapon(weaponNumber))
            {
                weaponTimers[i] += time / 2;
                hud.UpdateAmmo(weaponTimers[i], i);
                weaponSlotToAssign = i;
                activeWeapon = equippedWeapons[weaponSlotToAssign];
                SwapCloneWeapon();
                return;
            }
        }

        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            if (WeaponSlotEmpty(i))
            {
                weaponSlotToAssign = i;
                i = equippedWeapons.Length;
            }
        }

        if (!WeaponSlotEmpty(weaponSlotToAssign))
            equippedWeapons[weaponSlotToAssign].DisableWeapon();
        equippedWeapons[weaponSlotToAssign] = allWeapons[weaponNumber];

        ChangeActiveWeapon(weaponSlotToAssign);
        weaponTimers[weaponSlotToAssign] = time;
        hud.UpdateAmmo(weaponTimers[weaponSlotToAssign], weaponSlotToAssign);
        hud.UpdateWeaponHUD(weaponNumber, activeSlot, activeWeapon.GetCurrentLevel());
    }

    public BasicWeapon GetActiveWeapon()
    {
        return activeWeapon.GetComponent<BasicWeapon>();
    }

    private void SetDefaultWeapon()
    {
        equippedWeapons[0] = allWeapons[0];
    }

    bool IsActiveWeapon(BasicWeapon weapon)
    {
        return activeWeapon == weapon;
    }

    bool IsEquippedWeapon(BasicWeapon weapon)
    {
        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            if (equippedWeapons[i] == weapon)
                return true;
        }
        return false;
    }

    /// <summary> Uses GLOBAL WEAPON NUMBER to return whether that weapon is equipped or not </summary>
    bool IsEquippedWeapon(int number)
    {
        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            if (equippedWeapons[i] == allWeapons[number])
                return true;
        }
        return false;
    }

    bool WeaponSlotEmpty(int slotToCheck)
    {
        return equippedWeapons[slotToCheck] == null;
    }

    int GetWeaponNumber(BasicWeapon weapon)
    {
        return Extra.ConvertFromWeaponTypeToGlobalNumber(weapon.GetWeaponType());
    }

    /// <summary> Return equip number slot of global weapon number passed in </summary>
    int GetEquipNumberSlot(int wepNum)
    {
        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            if (equippedWeapons[i] == allWeapons[wepNum])
                return i;
        }
        return -1;
    }












    // InfiniteAmmo Pickup
    public void InfiniteAmmo(float timeAmount)
    {
        StartCoroutine("SetInfniteAmmo", timeAmount);
    }

    IEnumerator SetInfiniteAmmo(float timeAmount)
    {
        infniteAmmoCurrently = true;
        yield return new WaitForSeconds(timeAmount);
        infniteAmmoCurrently = false;
    }


    // DeadEye Pickup
    public void MissleStrike(float timeAllowed, GameObject explosionVFX)
    {
        explosion = explosionVFX;
        inDeadeye = true;
        StartCoroutine("MissleStrikeTimer", timeAllowed);
    }

    IEnumerator MissleStrikeTimer(float time)
    {
        Time.timeScale = 0.01f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        lastWeaponEquipped = activeSlot;
        activeWeapon.DisableWeapon();
        deadeyeWeapon.EnableWeapon();
        yield return new WaitForSecondsRealtime(time);
        StartCoroutine("ExecuteMissleStrike");
    }

    private void CheckForMissleStrikeClick()
    {
        if (Input.GetMouseButtonDown(0) && !positionChosen)
        {
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, 0);
            float rayLength;
            if (groundPlane.Raycast(cameraRay, out rayLength))
            {
                targetPosition = cameraRay.GetPoint(rayLength);
            }

            positionChosen = true;
            StopCoroutine("MissleStrikeTimer");
            StartCoroutine("ExecuteMissleStrike");
        }
    }

    IEnumerator ExecuteMissleStrike()
    {
        GlobalClass.player.lookingAtMouse = false;
        transform.LookAt(targetPosition);
        Instantiate(explosion, targetPosition, Quaternion.identity);
        yield return new WaitForSecondsRealtime(0.75f);
        GlobalClass.player.lookingAtMouse = true;
        equippedWeapons[lastWeaponEquipped].EnableWeapon();
        deadeyeWeapon.DisableWeapon();
        positionChosen = false;
        inDeadeye = false;
        Time.timeScale = 1;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }


    // TimeField
    public void SpeedUpFireRates(float factor)
    {
        foreach (BasicWeapon weapon in allWeapons)
        {
            weapon.SpeedUpFireRate(factor);
        }
    }

    public void SpeedDownFireRates(float factor)
    {
        foreach (BasicWeapon weapon in allWeapons)
        {
            weapon.SpeedDownFireRate(factor);
        }
    }

    // Gives current weapon and projectile of active weapon to each clone that exists
    public void SwapCloneWeapon()
    {
        GameObject projectile = activeWeapon.GetProjectile();
        foreach (Clone clone in mainPlayer.allActiveClones)
        {
            clone.SwitchActiveWeapon(GetWeaponNumber(activeWeapon),
                projectile,
                activeWeapon.GetCurrentLevel());
        }
    }


    // Upgrade
    public void UpgradeAllWeapons(float time)
    {
        for (int i = 0; i < allWeapons.Length; i++)
        {
            allWeapons[i].TemporaryUpgradeWeaponLevel(time);
            int equipNum = GetEquipNumberSlot(i);
            if (equipNum != -1)
                hud.ChangeWeaponLevel(equipNum, equippedWeapons[equipNum].GetCurrentLevel());
        }
        SwapCloneWeapon();
        StartCoroutine("UpgradeTimer", time);
    }

    IEnumerator UpgradeTimer(float time)
    {
        yield return new WaitForSeconds(time + 0.05f);
        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            if (!WeaponSlotEmpty(i))
            {
                print(equippedWeapons[i].GetCurrentLevel());
                hud.ChangeWeaponLevel(i, equippedWeapons[i].GetCurrentLevel());
            }
        }
        SwapCloneWeapon();
    }
}