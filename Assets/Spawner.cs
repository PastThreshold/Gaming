using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    int ROBOT = 0; int ASSASSIN = 1; int WALKER = 2; int PROTECTOR = 3; int ROLLERMINE = 4; int COMMANDER = 5; 
    [SerializeField] Transform[] spawnPoints;
    RoomData roomData;
    Wave currentWave;
    int maxEnemies;
    EnemySpawner[] spawners;
    [SerializeField] float minTimeBetweenSpawns = 3f;
    [SerializeField] float maxTimeBetweenSpawns = 5f;
    bool waitingToSpawn = false;

    void Start()
    {
        roomData = LevelController.roomData;
        currentWave = roomData.GetNextWave();
        print(currentWave.GetEnemyCounts()[5]);
        maxEnemies = currentWave.GetTotalMax();
        Vector2[] counts = currentWave.GetEnemyCounts();
        int[] maxes = currentWave.GetMaxOfEachEnemy();
        spawners = new EnemySpawner[6];
        for (int i = 0; i < spawners.Length; i++)
        {
            spawners[i] = new EnemySpawner(counts[i], maxes[i]);
        }
    }

    void Update()
    {
        if (!waitingToSpawn)
            StartCoroutine(Spawn());
    }

    IEnumerator SpawnCooldown()
    {
        yield return new WaitForSeconds(Random.Range(minTimeBetweenSpawns, maxTimeBetweenSpawns));
        waitingToSpawn = false;
    }

    IEnumerator Spawn()
    {
        if (CanSpawn())
        {
            
            List<int> availableSpawners = new List<int>();
            for (int i = 0; i < spawners.Length; i++)
            {
                int enemiesActive = LevelController.GetEnemyCountOfType(i);
                print("Num: " + i + " Enemy count " + LevelController.GetEnemyCountOfType(i));
                if (spawners[i].CanSpawn(enemiesActive))
                {
                    print("Spawner: " + i + " is able to spawn");
                    availableSpawners.Add(i);
                }
            }
            if (availableSpawners.Count < 1)
            {
                print("No spawners left");
                yield break;
            }
            waitingToSpawn = true;
            do
            {
                int enemyNumber = availableSpawners[Random.Range(0, availableSpawners.Count - 1)];
                print("Num: " + enemyNumber + " Enemy count " + LevelController.GetEnemyCountOfType(enemyNumber));
                if (!spawners[enemyNumber].CanSpawn(LevelController.GetEnemyCountOfType(enemyNumber)))
                {
                    availableSpawners.Remove(enemyNumber);
                    print("yuh");
                    continue;
                }
                SpawnEnemy(enemyNumber);
                yield return new WaitForSeconds(0.55f);
            } while (Extra.RollChance(75f) && availableSpawners.Count > 0);

            StartCoroutine(SpawnCooldown());
        }
    }

    private void SpawnEnemy(int enemyNum)
    {
        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Enemy enemy = GlobalClass.exD.enemyPrefabs[enemyNum];
        Instantiate(enemy, spawn);
        spawners[enemyNum].SpawnedOne();
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
            //print("left: " + leftToSpawn + " and " + enemyCount + " " + (leftToSpawn - 1 >= 0 && enemyCount < maxAtOnce));
            return leftToSpawn - 1 >= 0 && (enemyCount < maxAtOnce || maxAtOnce == 0);
        }
    }
}
