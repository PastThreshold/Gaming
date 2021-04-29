﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

/// <summary>
/// Responsible for keeping track of enemies in scene and scene setup
/// </summary>
public class LevelController : MonoBehaviour
{
    public static List<Enemy> allEnemiesInScene;
    public static List<Enemy> enemiesInDanger;
    public static EnemyList enemiesWithoutActiveBehavior;
    public static List<Protector> allProtectorsInScene;
    public static Projectile[] activeProjectiles;
    private int creationIndex = 0; // only used on start for activeProjectiles
    public static AltProjectile[] activeAlternateProjectiles;
    private int secondCreationIndex = 0;
    public static List<GravityWell> activeGravityWells;

    LevelProgression levelProgression;
    [SerializeField] public RoomData roomData;
    [SerializeField] GameObject player;
    [SerializeField] Transform playerSpawn;
    [SerializeField] GameObject hud;
    static CameraFollow mainCamera;

    [Header("Enemy Spawning")]
    public static int totalEnemiesToSpawn;
    public static int enemiesLeftToSpawn;
    static int maxEnemiesAtOnce;
    static public int totalEnemies = 0;
    static public bool canSpawn;

    [Header("Pickup Spawning")]
    [SerializeField] Vector3 positiveSpawnBoundaries = Vector3.zero;
    [SerializeField] Vector3 negativeSpawnBoundaries = Vector3.zero;
    GameObject[] powerUps;
    float[] chancesOfPowerUps;
    [SerializeField] float minTimeBetweenPowerUpRolls = 5f;
    [SerializeField] float maxTimeBetweenPowerUpRolls = 15f;
    [SerializeField] float chanceForPowerUp = 20f;
    [Tooltip("After a succesful roll for a powerup, a second chance will be rolled against (chance / this value), and then third and so on.")]
    [Range(1f, 10f)] [SerializeField] float subsequentRollDivisorPenalty = 2f;

    float newChanceValue;
    bool rolling;
    int previousScene;

    // DEBUGGING/TESTING
    [SerializeField] bool spawnPickups;

    private void Awake()
    {
        if (FindObjectOfType<Player>())
            FindObjectOfType<Player>().transform.position = playerSpawn.transform.position;
        else
            Instantiate(player, playerSpawn.transform.position, Quaternion.identity);

        if (!FindObjectOfType<HeadsUpDisplay>())
            Instantiate(hud, Vector3.zero, Quaternion.identity);

        allEnemiesInScene = new List<Enemy>();
        enemiesInDanger = new List<Enemy>();
        enemiesWithoutActiveBehavior = new EnemyList();
        allProtectorsInScene = new List<Protector>();
        activeGravityWells = new List<GravityWell>();
    }

    private void Start()
    {
        levelProgression = FindObjectOfType<LevelProgression>();
        mainCamera = Camera.main.GetComponent<CameraFollow>();
        rolling = false;
        canSpawn = true;
        totalEnemies = 0;
        totalEnemiesToSpawn = roomData.GetTotalEnemies();
        enemiesLeftToSpawn = totalEnemiesToSpawn;
        maxEnemiesAtOnce = roomData.GetMaxEnemiesAtTime();
        powerUps = roomData.GetPowerUps();
        chancesOfPowerUps = roomData.GetPowerUpChances();
        GetProjectiles();
    }

    private void Update()
    {
        if (totalEnemies >= totalEnemiesToSpawn || enemiesLeftToSpawn <= 0)
        {
            canSpawn = false;
        }

        if (totalEnemies <= 0 && enemiesLeftToSpawn <= 0)
        {
            Door[] doors = FindObjectsOfType<Door>();
            foreach (Door door in doors)
            {
                door.SetActive();
            }
        }

        if (!rolling && spawnPickups)
        {
            StartCoroutine("RollChanceForPowerUp");
        }
    }

    public bool CanSpawn()
    {
        return canSpawn;
    }

    public void NextLevel()
    {
        int chance = Random.Range(0, 2);
        if (chance == 0)
        {
            if (SceneManager.GetActiveScene().buildIndex == 3)
                SceneManager.LoadScene(2);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            if (SceneManager.GetActiveScene().buildIndex == 3)
                SceneManager.LoadScene(levelProgression.previousLevel + 1);
            else
                SceneManager.LoadScene(3);
        }
    }

    IEnumerator RollChanceForPowerUp()
    {
        rolling = true;
        float roll = Random.Range(0f, 100f);
        float newX, newY, newZ;
        Vector3 spawnPos = Vector3.zero;
        NavMeshHit hit;
        newChanceValue = chanceForPowerUp;
        int safetyBreak = 0;
        while (roll <= newChanceValue)
        {
            newX = Random.Range(negativeSpawnBoundaries.x, positiveSpawnBoundaries.x);
            newY = Random.Range(negativeSpawnBoundaries.y, positiveSpawnBoundaries.y);
            newZ = Random.Range(negativeSpawnBoundaries.z, positiveSpawnBoundaries.z);
            NavMesh.SamplePosition(new Vector3(newX, newY, newZ), out hit, 10, NavMesh.AllAreas);
            spawnPos = hit.position;
            spawnPos.y += 1.2f;
            Instantiate(powerUps[Random.Range(0, powerUps.Length)], spawnPos, Quaternion.identity);
            newChanceValue /= subsequentRollDivisorPenalty;
            roll = Random.Range(0f, 100f);
            if (Extra.CheckSafetyBreak(safetyBreak, 25))
            {
                Debug.Log("Infinite Loop Safety Break Triggered");
                break;
            }
            safetyBreak++;
        }
        yield return new WaitForSeconds(Random.Range(minTimeBetweenPowerUpRolls, maxTimeBetweenPowerUpRolls));
        rolling = false;
    }

    public static void AddEnemy(Enemy enem)
    {
        allEnemiesInScene.Add(enem);
        enemiesWithoutActiveBehavior.Add(enem);
        if (enem.GetComponent<Protector>())
            allProtectorsInScene.Add(enem.GetComponent<Protector>());
    }

    public static void RemoveEnemy(Enemy enem)
    {
        allEnemiesInScene.Remove(enem);
        enemiesWithoutActiveBehavior.Remove(enem);
        if (enem.GetComponent<Protector>())
            allProtectorsInScene.Remove(enem.GetComponent<Protector>());
        totalEnemies--;
        mainCamera.Shake();
    }

    public static void AddEnemyInDanger(Enemy enem)
    {
        enemiesInDanger.Add(enem);
    }

    public static void RemoveEnemyInDanger(Enemy enem)
    {
        enemiesInDanger.Remove(enem);
    }


    /// <summary>
    /// Called at start to add all the projectiles that are instantiated by the pools into a array
    /// meant to work with bullet time alt ability
    /// </summary>
    private void GetProjectiles()
    {
        ProjectilePool[] pools = FindObjectsOfType<ProjectilePool>();
        int totalLengthRequired = 0;
        int totalAltLengthRequired = 0;
        foreach (ProjectilePool pool in pools)
        {
            if (pool.notAltType)
                totalLengthRequired += pool.GetPool().Length;
            else
                totalAltLengthRequired += pool.GetAltPool().Length;
        }
        activeProjectiles = new Projectile[totalLengthRequired];
        activeAlternateProjectiles = new AltProjectile[totalAltLengthRequired];
        foreach (ProjectilePool pool in pools)
        {
            if (pool.notAltType)
                AddToProjectileArray(pool.GetPool());
            else
                AddToAlternateProjectileArray(pool.GetAltPool());
        }
    }

    /// <summary>
    /// Called for every projectile pool in the scene, creationIndex is the index of the activeProjectiles array
    /// The next function called continues where the last left off based on creationIndex
    /// </summary>
    private void AddToProjectileArray(Projectile[] array)
    {
        int index;
        for (index = 0; index < array.Length; index++)
        {
            activeProjectiles[creationIndex] = array[index];
            creationIndex++;
        }
    }

    private void AddToAlternateProjectileArray(AltProjectile[] array)
    {
        int index;
        for (index = 0; index < array.Length; index++)
        {
            activeAlternateProjectiles[secondCreationIndex] = array[index];
            secondCreationIndex++;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static void FreezeAllProjectiles()
    {
        foreach(Projectile proj in activeProjectiles)
        {
            if (proj.beingUsed)
                proj.StopMoving();
        }
    }

    public static void UnFreezeAllProjectiles()
    {
        foreach (Projectile proj in activeProjectiles)
        {
            if (proj.beingUsed)
                proj.ContinueMoving();
        }
    }
}