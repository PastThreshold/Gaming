using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    RoomData roomData;
    [SerializeField] float minTimeBetweenSpawns = 3f;
    [SerializeField] float maxTimeBetweenSpawns = 5f;
    LevelController level;
    Enemy[] enemiesToSpawn;
    float[] enemyChanceToSpawn;
    bool isWaiting;
    public bool underMax;
    float chance = 0;
    static float robotYSpawnAdd = 0;
    static float walkerYSpawnAdd = 3f;
    static float protectorYSpawnAdd = 0;
    static float rollerYSpawnAdd = 0;
    static float assassinYSpawnAdd = 0;

    void Start()
    {
        level = FindObjectOfType<LevelController>();
        roomData = level.roomData;
        enemiesToSpawn = roomData.GetEnemyTypes();
        enemyChanceToSpawn = roomData.GetEnemyTypeChances();
    }

    void Update()
    {
        if (!isWaiting)
        {
            if (level.CanSpawn())
            {
                float total = 0f;
                chance = Random.Range(0f, 100f);

                for (int i = 0; i < enemyChanceToSpawn.Length; i++)
                {
                    if (chance >= total && chance <= enemyChanceToSpawn[i] + total)
                    {
                        Vector3 spawnpoint = transform.position;
                        switch(enemiesToSpawn[i].enemyType)
                        {
                            case Enemy.EnemyType.robot:
                                spawnpoint.y += robotYSpawnAdd;
                                break;
                            case Enemy.EnemyType.assassin:
                                spawnpoint.y += assassinYSpawnAdd;
                                break;
                            case Enemy.EnemyType.walker:
                                spawnpoint.y += walkerYSpawnAdd;
                                break;
                            case Enemy.EnemyType.protector:
                                spawnpoint.y += protectorYSpawnAdd;
                                break;
                            case Enemy.EnemyType.roller:
                                spawnpoint.y += rollerYSpawnAdd;
                                break;
                        }
                        StartCoroutine("Wait");
                        Instantiate(enemiesToSpawn[i], transform.position, transform.rotation);
                        LevelController.totalEnemies++;
                        LevelController.enemiesLeftToSpawn--;
                        break;
                    }
                    total += enemyChanceToSpawn[i];
                }
            }
        }
    }

    IEnumerator Wait()
    {
        isWaiting = true;
        yield return new WaitForSeconds(Random.Range(minTimeBetweenSpawns, maxTimeBetweenSpawns));
        isWaiting = false;
    }
}
