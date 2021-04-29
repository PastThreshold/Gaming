using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeField : MonoBehaviour
{
    [SerializeField] float projectileSpeedUpFactor = 1.2f;
    [SerializeField] float playerSpeedUpFactor = 1.2f;
    [SerializeField] float enemySpeedUpFactor = 1.2f;
    [SerializeField] float fireRateSpeedUpFactor = 1.2f;
    public List<Collider> collidersHit;
    public List<GameObject> gameObjectsHit;
    [SerializeField] LayerMask layerMask;
    CollisionHandler colHandler;

    void Start()
    {
        colHandler = gameObject.AddComponent<CollisionHandler>();
        collidersHit = new List<Collider>();
        gameObjectsHit = new List<GameObject>();
    }

    // In order to account for enemies made of multiple colliders and the problem of giving speed for each collider,
    // the collider is added to the list, if the root object of the collider is the same as one in gameObjectsHit
    // nothing will happen, if not it adds that root to the game objects list and executes the rest of ontriggerenter
    // All of this is handled by the Collision Handler script
    private void OnTriggerEnter(Collider other)
    {
        switch(other.tag)
        {
            case GlobalClass.PROJECTILE_TAG:
                other.GetComponentInParent<Projectile>().SpeedUp(gameObject, projectileSpeedUpFactor);
                break;
            case GlobalClass.ENEMY_TAG:
                if (colHandler.CheckIfAlreadyBeenHitOnEnter(other))
                    return;
                other.GetComponentInParent<Enemy>().SpeedUp(enemySpeedUpFactor);
                break;
            case GlobalClass.PLAYER_TAG:
                if (colHandler.CheckIfAlreadyBeenHitOnEnter(other))
                    return;
                other.GetComponentInParent<Player>().SpeedUp(playerSpeedUpFactor);
                other.GetComponentInParent<WeaponsSwitcher>().SpeedUpFireRates(fireRateSpeedUpFactor);
                break;
        }
    }

    // Similar principle to enter tests colliders root object
    private void OnTriggerExit(Collider other)
    {
        switch (other.tag)
        {
            case GlobalClass.PROJECTILE_TAG:
                other.GetComponentInParent<Projectile>().SpeedDown(gameObject);
                break;
            case GlobalClass.ENEMY_TAG:
                if (colHandler.CheckIfAlreadyHitOnExit(other))
                    return;
                other.GetComponentInParent<Enemy>().SpeedDown();
                break;
            case GlobalClass.PLAYER_TAG:
                if (colHandler.CheckIfAlreadyHitOnExit(other))
                    return;
                other.GetComponentInParent<Player>().SpeedDown();
                other.GetComponentInParent<WeaponsSwitcher>().SpeedDownFireRates(fireRateSpeedUpFactor);
                break;
        }
    }
}
