using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Essentially like global class but with data that can not easily be declared in script because its static
/// </summary>
public class ExtraData : MonoBehaviour
{
    public LayerMask bulletLayerMask;
    public LayerMask enemiesOnlyLM;
    public LayerMask playerOnlyLM;
    public LayerMask proctilesOnlyLM;
    public LayerMask bulletsNoDefault;
    public LayerMask wallsAndEnemyShieldsLayerMask;

    public Enemy[] enemyPrefabs;
    public Pickup[] weaponPrefabs;
    public Pickup[] pickupPrefabs;
}
