using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    const int BREAK_LOOP_SAFETY = 1000;
    const int OUT_OF_SCENE_VALUE = 150;
    Vector3 outOfScene = new Vector3(0, OUT_OF_SCENE_VALUE, 0);

    Projectile[] projectiles;
    AltProjectile[] alternateProjectiles;
    public bool notAltType = true;
    int index;

    public void CreateArray(Projectile projectile, int size)
    {
        notAltType = true;
        projectiles = new Projectile[size];
        Projectile current = null;
        index = 0;
        for (int i = 0; i < size; i++)
        {
            projectiles[i] = Instantiate(projectile, transform.position, Quaternion.identity);
            current = projectiles[i];
            current.gameObject.SetActive(true);
            current.transform.parent = transform;
            current.transform.position = outOfScene;
            current.spotInArray = i;
            current.belongsTo = this;
            current.name += " Spot #" + i;
        }
    }

    /// <summary>
    /// Grabs a projectile from array, called by the weapons currentLevel value
    /// </summary>
    /// <returns></returns>
    public Projectile GetNextProjectile()
    {
        Projectile proj;
        int endIndex = projectiles.Length;
        int safetyBreak = 0;
        while (projectiles[index].beingUsed)
        {
            index++;
            safetyBreak++;
            if (index >= endIndex)
            {
                index = 0;
                if (safetyBreak >= BREAK_LOOP_SAFETY)
                {
                    Debug.Log("Infinite Loop Safety Break: Projectile Handler. " + projectiles[0].name);
                    break;
                }
            }
        }
        proj = projectiles[index];
        proj.beingUsed = true;

        index++;
        if (index >= endIndex)
            index = 0;

        proj.gameObject.SetActive(true);
        return proj;
    }



    public void ReturnBullet(int spotInProjArray)
    {
        if (notAltType)
        {
            Projectile proj = projectiles[spotInProjArray];
            proj.beingUsed = false;
            proj.transform.parent = this.transform;
            proj.transform.position = outOfScene;
            proj.gameObject.SetActive(false);
        }
        else
        {
            AltProjectile proj = alternateProjectiles[spotInProjArray];
            proj.beingUsed = false;
            proj.transform.parent = this.transform;
            proj.transform.position = outOfScene;
            proj.gameObject.SetActive(false);
        }

    }

    public Projectile[] GetPool()
    {
        return projectiles;
    }








    /*
     * 
     * 
     * 
     * 
     * Used for alternate projectiles
     * 
     * 
     * 
     * */



    public void CreateArray(AltProjectile projectile, int size)
    {
        notAltType = false;
        alternateProjectiles = new AltProjectile[size];
        index = 0;
        for (int i = 0; i < size; i++)
        {
            alternateProjectiles[i] = Instantiate(projectile, transform.position, Quaternion.identity);
            alternateProjectiles[i].transform.parent = transform;
            alternateProjectiles[i].transform.position = outOfScene;
            alternateProjectiles[i].spotInArray = i;
            alternateProjectiles[i].belongsTo = this;
            alternateProjectiles[i].name = alternateProjectiles[i].name + " Spot #" + i;
        }
    }

    public AltProjectile GetNextAltProjectile()
    {
        AltProjectile proj;
        int endIndex = alternateProjectiles.Length;
        int safetyBreak = 0;
        while (alternateProjectiles[index].beingUsed)
        {
            index++;
            safetyBreak++;
            if (index >= endIndex)
            {
                index = 0;
                if (safetyBreak >= BREAK_LOOP_SAFETY)
                {
                    Debug.Log("Infinite Loop Safety Break: Projectile Handler. " + projectiles[0].name);
                    break;
                }
            }
        }
        proj = alternateProjectiles[index];
        proj.beingUsed = true;

        index++;
        if (index >= endIndex)
            index = 0;

        proj.gameObject.SetActive(true);
        return proj;
    }

    public AltProjectile[] GetAltPool()
    {
        return alternateProjectiles;
    }
}
