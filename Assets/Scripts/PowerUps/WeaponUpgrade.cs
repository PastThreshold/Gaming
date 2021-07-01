using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponUpgrade : MonoBehaviour
{
    [SerializeField] BasicWeapon.WeaponType weaponType;

    public BasicWeapon.WeaponType GetWeaponType()
    {
        return weaponType;
    }
}
