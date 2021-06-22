using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Purpose is to avoid numerous calls to the player in different scripts whether in update or start or serialized
/// Some of the names for the player variable have differed
/// Can also put other data like the clones list, and other lists here as well
/// </summary>
public class GlobalClass : MonoBehaviour
{
    public static Player player;
    public static Vector3 playerPos;
    public static Vector3 playerPosPrev;
    public static WeaponsSwitcher weaponSwitcher;
    public static AbilitySwitcher abilitySwitcher;
    public static Transform firePointRotation;
    public static LevelController levelController;
    public static ExtraData exD; //xD


    [SerializeField] GameObject allPoolsPrefab;
    public static ProjectilePoolHandler bulletPool; //Player direct weapon pools
    public static ProjectilePoolHandler arPool;
    public static ProjectilePoolHandler sniperPool;
    public static ProjectilePoolHandler deaglePool;
    public static ProjectilePoolHandler stickyblPool;
    public static ProjectilePoolHandler shredderPool;
    public static ProjectilePoolHandler crPool;
    public static ProjectilePoolHandler rpgPool;
    public static ProjectilePoolHandler rpgAltPool;


    public static ProjectilePoolHandler hookShotPool; //Player indirect pools
    public static ProjectilePoolHandler deflectPool;

    public static ProjectilePoolHandler basicEnemyPool; //Enemy pools
    public static ProjectilePoolHandler timedBombPool;


    public const string DEFAULT_TAG = "Untagged";
    public const string ENEMY_TAG = "Enemy";
    public const string PLAYER_TAG = "Player";
    public const string PROJECTILE_TAG = "Projectile";
    public const string ROOM_TAG = "Room";
    public const string PICKUP_TAG = "Pickup";
    public const string SPECIAL_TAG = "Special";
    public const string SHIELD_TAG = "Shield";
    public const string DETECT_BULLETS_TAG = "Detect Bullet";
    public const string DEFLECT_TAG = "Deflect";
    public const string PROJECTILE_GATE_TAG = "ProjectileGate";

    public const int PROJECTILE_LAYER = 9;
    public const int ENEMY_PROJECTILE_LAYER = 13;


    private void Awake()
    {
        player = FindObjectOfType<Player>();
        weaponSwitcher = FindObjectOfType<WeaponsSwitcher>();
        abilitySwitcher = FindObjectOfType<AbilitySwitcher>();
        exD = GetComponent<ExtraData>();

        arPool = GameObject.Find("Assault Rifle Projectile Pool").GetComponent<ProjectilePoolHandler>();
        if (arPool == null)
        {
            Instantiate(allPoolsPrefab, transform.position, Quaternion.identity);
            arPool = GameObject.Find("Assault Rifle Projectile Pool").GetComponent<ProjectilePoolHandler>();
        }

        sniperPool = GameObject.Find("Sniper Rifle Projectile Pool").GetComponent<ProjectilePoolHandler>();

        deaglePool = GameObject.Find("Deagle Projectile Pool").GetComponent<ProjectilePoolHandler>();

        stickyblPool = GameObject.Find("Sticky Bomb Launcher Projectile Pool").GetComponent<ProjectilePoolHandler>();

        shredderPool = GameObject.Find("Shredder Projectile Pool").GetComponent<ProjectilePoolHandler>();

        crPool = GameObject.Find("Charge Rifle Projectile Pool").GetComponent<ProjectilePoolHandler>();

        rpgPool = GameObject.Find("Rocket Launcher Projectile Pool").GetComponent<ProjectilePoolHandler>();

        rpgAltPool = GameObject.Find("Rocket Launcher Alt Projectile Pool").GetComponent<ProjectilePoolHandler>();

        hookShotPool = GameObject.Find("HookShot Projectile Pool").GetComponent<ProjectilePoolHandler>();

        deflectPool = GameObject.Find("Deflected Projectile Pool").GetComponent<ProjectilePoolHandler>();

        basicEnemyPool = GameObject.Find("Enemy Projectile Pool").GetComponent<ProjectilePoolHandler>();

        timedBombPool = GameObject.Find("Timed Bomb Projectile Pool").GetComponent<ProjectilePoolHandler>();
    }

    /*  Basic projectile collision statement
        switch (other.tag)
        {
            case GlobalClass.DEFAULT_TAG:
                DisableProjectile();
                break;
            case GlobalClass.PLAYER_TAG:
                other.GetComponent<Player>().TakeDamage(damage);
                break;
            case GlobalClass.PROJECTILE_TAG:
                break;
            case GlobalClass.ENEMY_TAG:
                other.GetComponent<Enemy>().TakeDamage(damage);
                DisableProjectile();
                break;
            case GlobalClass.PICKUP_TAG:
                break;
            case GlobalClass.ROOM_TAG:
                break;
            case GlobalClass.SPECIAL_TAG:
                if (!other.GetComponent<SphereWeapon>())
                    DisableProjectile();
                break;
            case GlobalClass.SHIELD_TAG:
                break;
            case GlobalClass.DETECT_BULLETS_TAG:
                break;
        }
    */

    void Start()
    {
        playerPos = player.GetCurrentPos();
    }

    void Update()
    {
        playerPosPrev = player.GetPreviousPos();
        playerPos = player.GetCurrentPos();
    }
}