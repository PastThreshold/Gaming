using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsSwitcher : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] GameObject activeWeapon;
    public GameObject[] equippedWeapons;
    [SerializeField] GameObject[] allWeapons;
    int activeNumber;

    AssaultRifle ar; SniperRifle sniper; Shotgun shotty; Deagles deagle; StickyBombLauncher stickyBL; Shredder shredder;
    LaserBeam laser; ChargeRifle cr; RocketLauncher rpg;

    [Header("Default Weapon")]
    [Range(1, 4)] [SerializeField] int permanantArWeaponLevel = 1;
                                     int currentArWeaponLevel = 1;
                                   bool temporaryLevelUpgrade = false;

    GameObject defaultWeapon;
    [SerializeField] GameObject assualtRifleL1;
    [SerializeField] GameObject assualtRifleL2;
    [SerializeField] GameObject assualtRifleL3;
    [SerializeField] GameObject assualtRifleL4;

    [Range(0f, 1f)] [SerializeField] float timerNotEquippedPercentageSubtraction = 0.10f;
    public float[] weaponTimers;
    bool infniteAmmoCurrently = false;
    Player mainPlayer;
    HeadsUpDisplay hud;

    [Header("Missle Strike")]
    [SerializeField] GameObject deadeyeWeapon;
    public bool inDeadeye = false;
    public Vector3 targetPosition;
    public bool positionChosen = false;
    int lastWeaponEquipped = -1;

    GameObject explosion;

    /* Called upon by the onTriggerEnter() when colliding with an upgrade script */
    public void PermanantUpgradeDefaultWeapon()
    {
        if (permanantArWeaponLevel < 3)
        {
            permanantArWeaponLevel++;

            if (currentArWeaponLevel < 4)
            {
                currentArWeaponLevel++;
            }
            CheckDefaultWepEquipAndLevel();
        }
    }

    /* Called upon by upgrade pickup, keeps track of permant level and temporary */
    public void TemporaryUpgradeDefaultWeapon(float time)
    {
        if (currentArWeaponLevel < 4)
        {
            if (!temporaryLevelUpgrade)
            {
                currentArWeaponLevel = permanantArWeaponLevel + 1;
                temporaryLevelUpgrade = true;
            }
            else
            {
                currentArWeaponLevel++;
            }

            CheckDefaultWepEquipAndLevel();
            StartCoroutine("DowngradeDefaultWeapon", time);
        }
    }

    IEnumerator DowngradeDefaultWeapon(float time)
    {
        yield return new WaitForSeconds(time);

        if (currentArWeaponLevel > permanantArWeaponLevel)
        {
            currentArWeaponLevel--;

            if (currentArWeaponLevel == permanantArWeaponLevel)
                temporaryLevelUpgrade = false;
        }
        else if (temporaryLevelUpgrade)
            temporaryLevelUpgrade = false;

        CheckDefaultWepEquipAndLevel();
    }

    /* This checks for the default weapon being equipped, and also sets the default weapon 
     * if it was a different level */
    private void CheckDefaultWepEquipAndLevel()
    {
        if (activeWeapon == defaultWeapon)
        {
            CheckDefaultWeapon();
            SetEquippedWeaponToDefault();
            EnableDefaultWeapon();
        }
        else if (activeWeapon != defaultWeapon && defaultWeapon == equippedWeapons[0])
        {
            CheckDefaultWeapon();
            SetEquippedWeaponToDefault();
        }
        else
        {
            CheckDefaultWeapon();
        }
    }

    /* Set default weapon based on level */
    private void CheckDefaultWeapon()
    {
        switch(currentArWeaponLevel)
        {
            case 1: 
                defaultWeapon = assualtRifleL1;
                break;
            case 2:
                defaultWeapon = assualtRifleL2;
                break;
            case 3:
                defaultWeapon = assualtRifleL3;
                break;
            case 4:
                defaultWeapon = assualtRifleL4;
                break;
        }


        DisableWeaponMesh(assualtRifleL1);
        DisableWeaponMesh(assualtRifleL2);
        DisableWeaponMesh(assualtRifleL3);
        DisableWeaponMesh(assualtRifleL4);
    }

    private void SetEquippedWeaponToDefault()
    {
        equippedWeapons[0] = defaultWeapon;
    }

    private void EnableDefaultWeapon()
    {
        activeWeapon = equippedWeapons[0];
        EnableWeaponMesh(activeWeapon);
        SwapCloneWeapon();
        hud.UpdateWeaponHUD(FindInAllWeaponsSlot(activeWeapon), 0);
    }

    // Returns the index of weapon entered
    private int FindInAllWeaponsSlot(GameObject weapon)
    {
        int result = 0;
        for (int i = 0; i < allWeapons.Length; i++)
        {
            if (allWeapons[i] == weapon)
                result = i;
        }
        return result;
    }

    void Start()
    {
        hud = FindObjectOfType<HeadsUpDisplay>();
        mainPlayer = FindObjectOfType<Player>();

        ar = allWeapons[0].GetComponent<AssaultRifle>(); sniper = allWeapons[1].GetComponent<SniperRifle>();
        shotty = allWeapons[2].GetComponent<Shotgun>(); deagle = allWeapons[3].GetComponent<Deagles>();
        stickyBL = allWeapons[4].GetComponent<StickyBombLauncher>(); shredder = allWeapons[5].GetComponent<Shredder>();
        laser = allWeapons[6].GetComponent<LaserBeam>(); cr = allWeapons[7].GetComponent<ChargeRifle>();
        rpg = allWeapons[8].GetComponent<RocketLauncher>();

        currentArWeaponLevel = permanantArWeaponLevel;
        CheckDefaultWeapon();
        SetEquippedWeaponToDefault();
        EnableDefaultWeapon();

        foreach (GameObject weapon in allWeapons)
        {
            DisableWeaponMesh(weapon);
        }
        DisableWeaponMesh(deadeyeWeapon);
        EnableWeaponMesh(activeWeapon);
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
            ChangeWeapon();
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
                if (activeWeapon == equippedWeapons[i])
                {
                    if (activeWeapon != defaultWeapon)
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
                    if (equippedWeapons[i] == allWeapons[4])
                        allWeapons[4].SendMessage("DisableWeapon");

                    //remove weapon
                    // Is is the equipped?
                    if (activeWeapon == equippedWeapons[i])
                    {
                        RemoveWeaponFromEquipped(i);
                        activeWeapon = equippedWeapons[0];
                        activeNumber = 0;
                        EnableWeaponMesh(activeWeapon);
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

    /// <summary>
    /// Helper function for weapon timers to determine if the the weapon that has no time left is
    /// the currently equipped weapon or not
    /// </summary>
    /// <param name="i"> the index of the weapon in timers </param>
    private void RemoveWeaponFromEquipped(int i)
    {
        DisableWeaponMesh(equippedWeapons[i]);
        if (i == 0)
        {
            equippedWeapons[0] = defaultWeapon;
            hud.UpdateWeaponHUD(FindInAllWeaponsSlot(defaultWeapon), 0);
        }
        else
        {
            equippedWeapons[i] = null;
            hud.UpdateWeaponHUD(i);
        }
    }

    // Checks for one of the keys pressed, if so it will change to that equipped weapon in the array
    private void ChangeWeapon()
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
            if (activeNumber == 0)
                weaponNumber = 2;
            else
                weaponNumber = activeNumber - 1;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (activeNumber == 2)
                weaponNumber = 0;
            else
                weaponNumber = activeNumber + 1;
        }

        if (weaponNumber != -1)
        {
            int wN = weaponNumber;
            activeNumber = wN;

            if (equippedWeapons[wN] != null)
            {
                EnableWeaponMesh(equippedWeapons[wN]);

                activeWeapon = equippedWeapons[wN];
                SwapCloneWeapon();

                foreach (GameObject weapon in equippedWeapons)
                {
                    if (weapon != activeWeapon)
                        DisableWeaponMesh(weapon);
                }
            }
        }
    }

    //Hold Pickup
    public void CollisionWithWeaponPickup(Pickup weapon)
    {
        float weaponTime = weapon.GetTime();
        int slot = -1;
        // The following code will find the enum that the weapon pickup was set to
        // it will then assign the active weapon and the weapon slot it is currently on into those
        // This is based on a static assignment value for the array of allWeapons
        // That being so far 
        //assaultRifle = 0, 
        //sniperRifle = 1, 
        //shotgun = 2, 
        //deagle = 3
        //grenadeLauncher = 4
        //shredder = 5
        //laser = 6
        //chargeRifle = 7
        //rpg = 8
        //polygun = 9

        BasicWeapon.WeaponType wep = weapon.weaponType;
        switch (wep)
        {
            case BasicWeapon.WeaponType.assaultRifle:
                slot = 0;
                break;
            case BasicWeapon.WeaponType.sniperRifle:
                slot = 1;
                break;
            case BasicWeapon.WeaponType.shotgun:
                slot = 2;
                break;
            case BasicWeapon.WeaponType.deagle:
                slot = 3;
                break;
            case BasicWeapon.WeaponType.stickyBombLauncher:
                slot = 4;
                break;
            case BasicWeapon.WeaponType.shredder:
                slot = 5;
                break;
            case BasicWeapon.WeaponType.laser:
                slot = 6;
                break;
            case BasicWeapon.WeaponType.chargeRifle:
                slot = 7;
                break;
            case BasicWeapon.WeaponType.rpg:
                slot = 8;
                break;
            default:
                Debug.Log("Weapon Switch Error");
                break;
        }

        SwitchActiveWeaponAndSlot(slot, weaponTime);
        Destroy(weapon.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<WeaponUpgrade>())
        {
            WeaponUpgrade.WeaponUpgradeType upgradeWeapon = other.GetComponent<WeaponUpgrade>().GetWeaponUpgradeType();
            switch (upgradeWeapon)
            {
                case WeaponUpgrade.WeaponUpgradeType.assaultRifle:
                    PermanantUpgradeDefaultWeapon();
                    break;
                case WeaponUpgrade.WeaponUpgradeType.sniperRifle:
                    allWeapons[1].GetComponent<BasicWeapon>().PermanantUpgradeWeaponLevel();
                    break;
                case WeaponUpgrade.WeaponUpgradeType.shotgun:
                    allWeapons[2].GetComponent<BasicWeapon>().PermanantUpgradeWeaponLevel();
                    break;
                case WeaponUpgrade.WeaponUpgradeType.deagle:
                    allWeapons[3].GetComponent<BasicWeapon>().PermanantUpgradeWeaponLevel();
                    break;
                case WeaponUpgrade.WeaponUpgradeType.stickyBombLauncher:
                    allWeapons[4].GetComponent<BasicWeapon>().PermanantUpgradeWeaponLevel();
                    break;
                case WeaponUpgrade.WeaponUpgradeType.shredder:
                    allWeapons[5].GetComponent<BasicWeapon>().PermanantUpgradeWeaponLevel();
                    break;
                case WeaponUpgrade.WeaponUpgradeType.laser:
                    allWeapons[6].GetComponent<BasicWeapon>().PermanantUpgradeWeaponLevel();
                    break;
                case WeaponUpgrade.WeaponUpgradeType.chargeRifle:
                    allWeapons[7].GetComponent<BasicWeapon>().PermanantUpgradeWeaponLevel();
                    break;
                case WeaponUpgrade.WeaponUpgradeType.rpg:
                    allWeapons[8].GetComponent<BasicWeapon>().PermanantUpgradeWeaponLevel();
                    break;
                default:
                    Debug.Log("Weapon Upgrade Error");
                    break;
            }
        }
    }

    // Helper function called by weapon pickups to correctly enter into the weapon slot and
    // Increase the time
    private void SwitchActiveWeaponAndSlot(int slot, float time)
    {
        int weaponSlotToAssign = activeNumber;
        bool alreadyEquipped = false;
        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            if (equippedWeapons[i] == allWeapons[slot])
            {
                weaponTimers[i] += time / 2;
                hud.UpdateAmmo(weaponTimers[i], i);
                alreadyEquipped = true;
                weaponSlotToAssign = i;
                activeWeapon = equippedWeapons[weaponSlotToAssign];
                SwapCloneWeapon();
            }
        }

        if (!alreadyEquipped)
        {
            for (int i = 0; i < equippedWeapons.Length; i++)
            {
                if (equippedWeapons[i] == null)
                {
                    weaponSlotToAssign = i;
                    i = equippedWeapons.Length;
                }
            }

            equippedWeapons[weaponSlotToAssign] = allWeapons[slot];


            DisableWeaponMesh(activeWeapon);
            Debug.Log("Disabling: " + activeWeapon.name);

            activeWeapon = equippedWeapons[weaponSlotToAssign];
            SwapCloneWeapon();

            weaponTimers[weaponSlotToAssign] = time;
            hud.UpdateAmmo(weaponTimers[weaponSlotToAssign], weaponSlotToAssign);

            EnableWeaponMesh(activeWeapon);
            hud.UpdateWeaponHUD(slot, weaponSlotToAssign);
        }
    }

    // These 2 fuctions enable and disable the equippedWeapons meshs and set their active bool to true/false which prevents them from firing
    // This allows for the firerate to still be in a couroutine as my previous solution was to disable the gameobject entirely.
    // Now because of polymorphism the only object that are in here are special cases like lights, lasers, and second weapons
    private void EnableWeaponMesh(GameObject weapon)
    {
        BasicWeapon basicWep = weapon.GetComponent<BasicWeapon>();
        BasicWeapon.WeaponType wepType = basicWep.GetWeaponType();
        switch (wepType)
        {
            case BasicWeapon.WeaponType.sniperRifle:
                Light[] sniperLights = weapon.GetComponentsInChildren<Light>();
                foreach (Light light in sniperLights)
                {
                    light.enabled = true;
                }
                LineRenderer[] lines = weapon.GetComponentsInChildren<LineRenderer>();
                foreach (LineRenderer line in lines)
                {
                    line.enabled = true;
                }
                break;
            case BasicWeapon.WeaponType.deagle:
                deagle.SetSecondWeapon();
                break;
            case BasicWeapon.WeaponType.laser:
                Light[] laserLights = weapon.GetComponentsInChildren<Light>();
                foreach (Light light in laserLights)
                {
                    light.enabled = true;
                }
                break;
        }
        basicWep.active = true;

        MeshRenderer[] objects = weapon.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer obj in objects)
        {
            obj.enabled = true;
        }
    }

    private void DisableWeaponMesh(GameObject weapon)
    {
        if (weapon != null)
        {
            BasicWeapon basicWep = weapon.GetComponent<BasicWeapon>();
            BasicWeapon.WeaponType wepType = basicWep.GetWeaponType();
            switch (wepType)
            {
                case BasicWeapon.WeaponType.sniperRifle:
                    Light[] sniperLights = weapon.GetComponentsInChildren<Light>();
                    foreach (Light light in sniperLights)
                    {
                        light.enabled = false;
                    }
                    LineRenderer[] lines = weapon.GetComponentsInChildren<LineRenderer>();
                    foreach (LineRenderer line in lines)
                    {
                        line.enabled = false;
                    }
                    break;
                case BasicWeapon.WeaponType.deagle:
                    break;
                case BasicWeapon.WeaponType.laser:
                    laser.DisableLaser();
                    Light[] laserLights = weapon.GetComponentsInChildren<Light>();
                    foreach (Light light in laserLights)
                    {
                        light.enabled = false;
                    }
                    break;
                case BasicWeapon.WeaponType.chargeRifle:
                    cr.StartCoroutine("FireCharge");
                    break;
            }

            MeshRenderer[] objects = weapon.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer obj in objects)
            {
                obj.enabled = false;
            }
            basicWep.active = false;
        }
    }

    public BasicWeapon GetActiveWeapon()
    {
        return activeWeapon.GetComponent<BasicWeapon>();
    }


    // InfiniteAmmo Pickup
    public void InfiniteAmmo(float timeAmount)
    {
        StartCoroutine("SetInfniteAmmo", timeAmount);
    }

    IEnumerator SetInfniteAmmo(float timeAmount)
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
        lastWeaponEquipped = activeNumber;
        DisableWeaponMesh(activeWeapon);
        EnableWeaponMesh(deadeyeWeapon);
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
        FindObjectOfType<Player>().lookingAtMouse = false;
        transform.LookAt(targetPosition);
        Instantiate(explosion, targetPosition, Quaternion.identity);
        print("spawn");
        yield return new WaitForSecondsRealtime(0.75f);
        FindObjectOfType<Player>().lookingAtMouse = true;
        EnableWeaponMesh(equippedWeapons[lastWeaponEquipped]);
        DisableWeaponMesh(deadeyeWeapon);
        positionChosen = false;
        inDeadeye = false;
        Time.timeScale = 1;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }
    

    // TimeField
    public void SpeedUpFireRates(float factor)
    {
        foreach (GameObject weapon in allWeapons)
        {
            weapon.GetComponent<BasicWeapon>().SpeedUpFireRate(factor);
        }
    }

    public void SpeedDownFireRates(float factor)
    {
        foreach (GameObject weapon in allWeapons)
        {
            weapon.GetComponent<BasicWeapon>().SpeedDownFireRate(factor);
        }
    }

    // Gives current weapon and projectile of active weapon to each clone that exists
    public void SwapCloneWeapon()
    {
        GameObject projectile = activeWeapon.GetComponent<BasicWeapon>().GetProjectile();
        foreach (Clone clone in mainPlayer.allActiveClones)
        {
            clone.SwitchActiveWeapon(FindInAllWeaponsSlot(activeWeapon), 
                projectile, 
                activeWeapon.GetComponent<BasicWeapon>().GetCurrentLevel());
        }
    }


    // Upgrade
    public void UpgradeAllWeapons(float time)
    {
        TemporaryUpgradeDefaultWeapon(time);
        foreach(GameObject wep in allWeapons)
        {
            if (wep != assualtRifleL1 && wep != assualtRifleL2 && wep != assualtRifleL3 && wep != assualtRifleL4)
                wep.GetComponent<BasicWeapon>().TemporaryUpgradeWeaponLevel(time);
        }
        SwapCloneWeapon();
        StartCoroutine("UpgradeTimer", time);
    }

    IEnumerator UpgradeTimer(float time)
    {
        yield return new WaitForSeconds(time + 0.05f);
        SwapCloneWeapon();
    }
}