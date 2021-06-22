using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponUpgrade : MonoBehaviour
{


    [SerializeField] BasicWeapon.WeaponType weaponType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            Destroy(gameObject);
    }

    public BasicWeapon.WeaponType GetWeaponType()
    {
        return weaponType;
    }
}
