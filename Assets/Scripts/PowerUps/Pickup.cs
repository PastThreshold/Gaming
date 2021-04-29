using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The pickup class includes all three types, the power ups such as infinite ammo  and the turret,
// the weapon pickups for the player
// and the already instantiated objects such as the sphere and the gravity sphere
public class Pickup : MonoBehaviour
{
    public enum PickupType
    {
        turret,
        infiniteAmmo,
        deadeye,
        timeField,
        clone,
        upgrade,
        nullVal,
    }

    public enum ObjectType
    {
        sphere,
        gravityWell,
        portal,
        nullVal,
    }

    public enum Type
    {
        weapon,
        pickup,
        obj
    }

    [SerializeField] float timeUntilSpawnerDeath = 15f;
    [SerializeField] float timeAllowed = 15f;
    bool timerPaused = false;

    public Type type = Type.pickup;

    [Header("WEAPON")]
    public BasicWeapon.WeaponType weaponType = BasicWeapon.WeaponType.nullVal;

    [Header("OBJECTS")]
    public ObjectType objType = ObjectType.nullVal;

    [Header("POWERUPS")]
    public PickupType pickupType = PickupType.nullVal;

    [Header("Only required if an object is instantiated")]
    [SerializeField] GameObject thisPickup;
    [SerializeField] float correctYSpawnValue = 0;
    bool needsToBeInstaniated = false;
    bool spawnAtSpawner = true;

    private void Start()
    {
        if (type != Type.pickup)
            return;

        switch(pickupType)
        {
            case PickupType.turret:
                needsToBeInstaniated = true;
                break;
            case PickupType.infiniteAmmo:
                needsToBeInstaniated = false;
                break;
            case PickupType.deadeye:
                needsToBeInstaniated = false;
                break;
            case PickupType.timeField:
                needsToBeInstaniated = true;
                break;
            case PickupType.clone:
                needsToBeInstaniated = true;
                spawnAtSpawner = false;
                break;
            case PickupType.upgrade:
                needsToBeInstaniated = false;
                spawnAtSpawner = false;
                break;

            default:
                Debug.Log("Pickup Type Error");
                break;
        }
    }

    private void Update()
    {
        if (!timerPaused)
        {
            timeUntilSpawnerDeath -= 1 * Time.deltaTime;

            if (timeUntilSpawnerDeath <= 0)
                Destroy(gameObject);
        }
        transform.Rotate(new Vector3(0, 20 * Time.deltaTime, 0));
    }

    public void SpawnPickup()
    {
        if (type == Type.obj)
            return;

        if (type == Type.weapon)
        {
            GlobalClass.weaponSwitcher.CollisionWithWeaponPickup(this);
            return;
        }

        if (needsToBeInstaniated)
        {
            GameObject pickupObject;
            if (spawnAtSpawner)
            {
                pickupObject = Instantiate(thisPickup, transform.position, Quaternion.identity);
                pickupObject.transform.position += new Vector3(0, correctYSpawnValue, 0);
            }
            else
            {
                pickupObject = Instantiate(thisPickup, FindObjectOfType<Player>().transform.position + 
                    new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized * 4
                    , Quaternion.identity);
            }
            pickupObject.BroadcastMessage("SetTimeUntilDeath", timeAllowed, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
        else
        {
            switch(pickupType)
            {
                case PickupType.infiniteAmmo:
                    FindObjectOfType<WeaponsSwitcher>().InfiniteAmmo(timeAllowed);
                    break;
                case PickupType.deadeye:
                    FindObjectOfType<WeaponsSwitcher>().MissleStrike(timeAllowed, thisPickup);
                    break;
                case PickupType.upgrade:
                    FindObjectOfType<WeaponsSwitcher>().UpgradeAllWeapons(timeAllowed);
                    FindObjectOfType<AbilitySwitcher>().TempUpgradeAbilities(timeAllowed);
                    break;
                default:
                    Debug.Log("Pickup Type Error");
                    break;
            }

            Destroy(gameObject);
        }
    }

    public float GetTime()
    {
        return timeAllowed;
    }

    public void PauseTimer()
    {
        timerPaused = true;
    }
    
    public void UnpauseTimer()
    {
        timerPaused = false;
    }
}
