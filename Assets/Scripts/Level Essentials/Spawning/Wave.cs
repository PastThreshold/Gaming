using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Wave")]
public class Wave : ScriptableObject
{
    [SerializeField] int maxTotal;
    [SerializeField] Vector2[] enemyCounts;
    [SerializeField] int[] maxOf;

    public int GetTotalMax()
    {
        return maxTotal;
    }

    public Vector2[] GetEnemyCounts()
    {
        return enemyCounts;
    }

    public int[] GetMaxOfEachEnemy()
    {
        return maxOf;
    }
}