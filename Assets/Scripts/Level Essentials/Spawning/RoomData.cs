using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Room Data")]
public class RoomData : ScriptableObject
{
    [SerializeField] Wave[] waves;
    [SerializeField] int levelNumber;
    [SerializeField] int totalEnemies = 100;
    [SerializeField] int maxEnemiesAtTime = 10;
    [SerializeField] float chanceForWeaponOrPowerup = 50f;
    [SerializeField] float[] chanceForEachWeapon = new float[2];
    [SerializeField] float[] chanceForEachPowerup = new float[2];
    [SerializeField] float chanceForExtraPowerup;
    [SerializeField] float minBtPowerup;
    [SerializeField] float maxBtPowerup;

    public int GetTotalEnemies() { return totalEnemies; }
    public int GetMaxEnemiesAtTime() { return maxEnemiesAtTime; }
    public float GetWeaponPowerupChance() { return chanceForWeaponOrPowerup;  }
    public float[] GetWeaponChances() { return chanceForEachWeapon;  }
    public float[] GetPowerupChances() { return chanceForEachPowerup;  }
    public float GetExtraPowerupChance() { return chanceForExtraPowerup;  }
    public float GetMinBtPowerup() { return minBtPowerup;  }
    public float GetMaxBtPowerup() { return maxBtPowerup;  }

    public Wave GetNextWave(int index)
    {
        return waves[index];
    }

    public bool HasNextWave(int index)
    {
        return waves[index] != null;
    }
}


