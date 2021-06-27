using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    int ROBOT = 0; int ASSASSIN = 1; int WALKER = 2; int PROTECTOR = 3; int ROLLERMINE = 4; int COMMANDER = 5; 
    [SerializeField] Transform[] spawnPoints;
    RoomData roomData;
    Wave currentWave;
    int waveIndex = 0;
    int maxEnemies;
    EnemySpawner[] spawners;
    [SerializeField] float minTimeBetweenSpawns = 3f;
    [SerializeField] float maxTimeBetweenSpawns = 5f;
    bool waitingToSpawnEnemies = false;
    bool spawningMode = true;
    bool waitingMode = false;

    [Header("Pickup Spawning")]
    bool waitingToSpawnPowerup = false;
    [SerializeField] Vector3 positiveSpawnBoundaries = Vector3.zero;
    [SerializeField] Vector3 negativeSpawnBoundaries = Vector3.zero;
    float minTimeBetweenPowerUpRolls;
    float maxTimeBetweenPowerUpRolls;
    float chanceForWeaponOrPowerup;
    float[] chancesOfPowerups;
    float[] chancesOfWeapons;
    float chancePerExtraPowerup;

    void Start()
    {
        roomData = LevelController.roomData;
        spawners = new EnemySpawner[6];
        SetPowerupData();
        SetNextWave();
    }

    void Update()
    {
        if (!waitingToSpawnEnemies && spawningMode)
            StartCoroutine(SpawnEnemies());
        else if (waitingMode)
        {
            if (LevelController.allEnemiesInScene.Count <= 0)
                SetNextWave();
        }

        if (!waitingToSpawnPowerup)
            StartCoroutine(SpawnPowerups());
    }


    IEnumerator SpawnPowerupsCooldown()
    {
        yield return new WaitForSeconds(Random.Range(minTimeBetweenPowerUpRolls, maxTimeBetweenPowerUpRolls));
        waitingToSpawnPowerup = false;
    }

    IEnumerator SpawnEnemiesCooldown()
    {
        yield return new WaitForSeconds(Random.Range(minTimeBetweenSpawns, maxTimeBetweenSpawns));
        waitingToSpawnEnemies = false;
    }

    public void SetNextWave()
    {
        if (!roomData.HasNextWave(waveIndex))
        {
            print("no more");
            return;
        }
        currentWave = roomData.GetNextWave(waveIndex);
        maxEnemies = currentWave.GetTotalMax();
        Vector2[] counts = currentWave.GetEnemyCounts();
        int[] maxes = currentWave.GetMaxOfEachEnemy();
        for (int i = 0; i < spawners.Length; i++)
        {
            if (spawners[i] == null)
                spawners[i] = new EnemySpawner(counts[i], maxes[i]);
            else
                spawners[i].Reinitalize(counts[i], maxes[i]);
        }
        waitingMode = false;
        spawningMode = true;
        waitingToSpawnEnemies = false;
        waveIndex++;
    }

    private void SetPowerupData()
    {
        chanceForWeaponOrPowerup = roomData.GetWeaponPowerupChance();
        chancesOfWeapons = roomData.GetWeaponChances();
        chancesOfPowerups = roomData.GetPowerupChances();
        chancePerExtraPowerup = roomData.GetExtraPowerupChance();
        minTimeBetweenPowerUpRolls = roomData.GetMinBtPowerup();
        maxTimeBetweenPowerUpRolls = roomData.GetMaxBtPowerup();
    }

    IEnumerator SpawnEnemies()
    {
        if (CanSpawn())
        {
            List<int> availableSpawners = new List<int>();
            for (int i = 0; i < spawners.Length; i++)
            {
                int enemiesActive = LevelController.GetEnemyCountOfType(i);
                if (spawners[i].CanSpawn(enemiesActive))
                {
                    availableSpawners.Add(i);
                }
            }
            if (availableSpawners.Count <= 0)
            {
                waitingToSpawnEnemies = true;
                spawningMode = false;
                waitingMode = true;
                yield break;
            }
            waitingToSpawnEnemies = true;
            do
            {
                int enemyNumber = availableSpawners[Random.Range(0, availableSpawners.Count - 1)];
                if (!spawners[enemyNumber].CanSpawn(LevelController.GetEnemyCountOfType(enemyNumber)))
                {
                    availableSpawners.Remove(enemyNumber);
                    continue;
                }
                SpawnEnemy(enemyNumber);
                yield return new WaitForSeconds(0.55f);
            } while (Extra.RollChance(75f) && availableSpawners.Count > 0);

            StartCoroutine(SpawnEnemiesCooldown());
        }
    }

    IEnumerator SpawnPowerups()
    {
        waitingToSpawnPowerup = true;
        if (LevelController.allEnemiesInScene.Count >= 4)
        {
            float[] list;
            do
            {
                bool isWeapon = Extra.RandomBoolean();
                if (isWeapon)
                    list = chancesOfWeapons;
                else
                    list = chancesOfPowerups;
                int pickupNum = GetFromPickupList(list);
                if (pickupNum == -1)
                    Debug.LogError("RoomData doesnt have fully defined powerup percentages, idiot. Weapon List? - " + isWeapon);
                SpawnPowerup(isWeapon, pickupNum);
                yield return new WaitForSeconds(0.55f);
            } while (Extra.RollChance(chancePerExtraPowerup));
        }


        StartCoroutine(SpawnPowerupsCooldown());

        yield return new WaitForSeconds(0.55f);

        int GetFromPickupList(float[] chances)
        {
            float numberRoll = Random.Range(0f, 100f);
            int pickupNum = -1;
            float totalAdded = 0;
            for (int i = 0; i < chances.Length; i++)
            {
                if (numberRoll <= chances[i] + totalAdded)
                {
                    pickupNum = i;
                    break;
                }
                totalAdded += chances[i];
            }

            return pickupNum;
        }
    }


    private void SpawnEnemy(int enemyNum)
    {
        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Enemy enemy = GlobalClass.exD.enemyPrefabs[enemyNum];
        enemy = Instantiate(enemy, spawn);
        enemy.transform.parent = null;
        spawners[enemyNum].SpawnedOne();
    }

    private void SpawnPowerup(bool isWeapon, int powerupNum)
    {
        Vector3 spawnLocation = new Vector3(
            Random.Range(negativeSpawnBoundaries.x, positiveSpawnBoundaries.x),
            Random.Range(negativeSpawnBoundaries.y, positiveSpawnBoundaries.y),
            Random.Range(negativeSpawnBoundaries.z, positiveSpawnBoundaries.z));
        Pickup powerup;
        if (isWeapon)
            powerup = GlobalClass.exD.weaponPrefabs[powerupNum];
        else
            powerup = GlobalClass.exD.pickupPrefabs[powerupNum];
        Instantiate(powerup, spawnLocation, Quaternion.identity);
    }



    private bool CanSpawn()
    {
        return maxEnemies >= LevelController.totalEnemies;
    }











    private class EnemySpawner
    {
        int totalToSpawn;
        int leftToSpawn;
        int maxAtOnce;

        
        public EnemySpawner(Vector2 count, int max)
        {
            totalToSpawn = Random.Range((int)count.x, (int)count.y);
            leftToSpawn = totalToSpawn;
            maxAtOnce = max;
        }

        public void Reinitalize(Vector2 count, int max)
        {
            totalToSpawn = Random.Range((int)count.x, (int)count.y);
            leftToSpawn = totalToSpawn;
            maxAtOnce = max;
        }

        public void SpawnedOne()
        {
            leftToSpawn--;
        }

        public bool CanSpawn(int enemyCount)
        {
            return leftToSpawn - 1 >= 0 && (enemyCount < maxAtOnce || maxAtOnce == 0);
        }
    }
}
