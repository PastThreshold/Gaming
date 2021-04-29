using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{

    public enum WeaponPickupType
    {
        // Pickup Type
        assaultRifle,
        sniperRifle,
        shotgun,
        deagle,
        stickyBombLauncher,
        shredder,
        laser,
        chargeRifle,
        rpg,
    }

    [SerializeField] WeaponPickupType weaponPickupType;
    [SerializeField] float timeAllowed = 10f;

    public WeaponPickupType GetWeaponPickupType()
    {
        return weaponPickupType;
    }

    public float GetTime()
    {
        return timeAllowed;
    }
}
