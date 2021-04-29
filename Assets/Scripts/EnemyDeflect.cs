using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeflect : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(GlobalClass.PROJECTILE_TAG))
        {
            Projectile proj = other.GetComponent<Projectile>();
            if (!proj.isEnemyBullet)
            {
                proj.transform.Rotate(0, 180f, 0);
                proj.EnableProjectile();
                proj.FlipSide();
            }
        }
    }

    public void TriggeredByRaycast(Projectile other)
    {
        other.transform.Rotate(0, 180, 0);
        other.FlipSide();
        other.EnableProjectile();
    }
}
