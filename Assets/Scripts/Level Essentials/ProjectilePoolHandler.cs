using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePoolHandler : MonoBehaviour
{
    ProjectilePool[] pools;
    [SerializeField] Projectile[] projectileCopies;
    [SerializeField] public bool isAnAltPool = false;
    [SerializeField] AltProjectile[] altProjectileCopies;
    [SerializeField] int[] poolSizes;

    /// <summary>
    /// Creates 4 arrays of type projectile, the sizes and values of these arrays is determined in the inspector
    /// each array should contain enough projectiles so that there can be enough projectiles at one time
    /// </summary>
    void Awake()
    {
        if (!isAnAltPool)
            pools = new ProjectilePool[projectileCopies.Length];
        else
            pools = new ProjectilePool[altProjectileCopies.Length];

        for (int i = 0; i < pools.Length; i++)
        {
            pools[i] = gameObject.AddComponent<ProjectilePool>();
            if (!isAnAltPool) // Are the pools Projectile Type or AltProjectile type?
            {
                pools[i].CreateArray(projectileCopies[i], poolSizes[i]);
            }
            else
            {
                pools[i].CreateArray(altProjectileCopies[i], poolSizes[i]);
            }
        }
    }

    public Projectile GetNextProjectile(int level)
    {
        return pools[level - 1].GetNextProjectile();
    }

    public Projectile GetNextProjectile()
    {
        return pools[0].GetNextProjectile();
    }

    public AltProjectile GetNextAltProjectile(int level)
    {
        return pools[level - 1].GetNextAltProjectile();
    }

    public AltProjectile GetNextAltProjectile()
    {
        return pools[0].GetNextAltProjectile();
    }
}
