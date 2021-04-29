using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponUpgrade : MonoBehaviour
{
    public enum WeaponUpgradeType
    {
        // Upgrade Type
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

    [SerializeField] WeaponUpgradeType weaponUpgradeType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            Destroy(gameObject);
    }

    public WeaponUpgradeType GetWeaponUpgradeType()
    {
        return weaponUpgradeType;
    }
}
