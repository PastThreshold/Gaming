using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Room Data")]
public class RoomData : ScriptableObject
{
    [SerializeField] int levelNumber;
    [SerializeField] int totalEnemies = 100;
    [SerializeField] int maxEnemiesAtTime = 10;

    [Header("These two must be the same size")]
    [SerializeField] Enemy[] enemiesAllowedToSpawn;
    [SerializeField] float[] chancesOfEnemiesSpawning;

    [SerializeField] GameObject[] powerUps;
    [SerializeField] float[] chancesOfPowerUps;

    public int GetTotalEnemies() { return totalEnemies; }

    public int GetMaxEnemiesAtTime() { return maxEnemiesAtTime; }

    public Enemy[] GetEnemyTypes() { return enemiesAllowedToSpawn; }

    public float[] GetEnemyTypeChances() { return chancesOfEnemiesSpawning; }

    public GameObject[] GetPowerUps() { return powerUps; }

    public float[] GetPowerUpChances() { return chancesOfPowerUps; }
}
