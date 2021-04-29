using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionVFX : MonoBehaviour
{
    [SerializeField] bool isEnemyExplosion = false;
    [SerializeField] int damage = 300;
    [SerializeField] float explosionForce = 10f;
    [SerializeField] float radius = 2.5f;
    [SerializeField] float upForce = 1.0f;
    [SerializeField] float timeToDestroy = 0.5f;
    [SerializeField] LayerMask layerMask;

    [SerializeField] bool immediate = true;
    [SerializeField] float timeBeforeStart = 0f;

    Vector3 explosionPos;

    private void Start()
    {
        explosionPos = transform.position;

        if (!immediate)
        {
            StartCoroutine("WaitToExplode");
        }
        else
        {
            StartCoroutine("PlayAndDestroy");
            Explode();
        }
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius, layerMask);
        List<GameObject> uniqueObjects = CollisionHandler.TestSingleTriggerArray(colliders);

        if (isEnemyExplosion)
        {
            foreach (GameObject obj in uniqueObjects)
            {
                if (obj.CompareTag(GlobalClass.PLAYER_TAG))
                {
                    obj.GetComponentInParent<Player>().TakeDamage(damage);
                }
                else
                {
                    if (obj.GetComponent<Rigidbody>())
                        obj.GetComponent<Rigidbody>().AddExplosionForce(
                            explosionForce, explosionPos, radius, upForce, ForceMode.VelocityChange);
                }
            }
        }
        else
        {
            foreach (GameObject obj in uniqueObjects)
            {
                if (obj.CompareTag(GlobalClass.ENEMY_TAG))
                {
                    obj.GetComponentInParent<Enemy>().TakeDamage(
                        damage, explosionForce, explosionPos, radius, upForce, ForceMode.VelocityChange);
                }
                else if (obj.CompareTag(GlobalClass.SHIELD_TAG))
                {
                    if (obj.GetComponent<EnemyShield>())
                    {
                        obj.GetComponent<EnemyShield>().TakeDamage(damage);
                    }
                }
                else
                {
                    if (obj.GetComponent<Rigidbody>())
                        obj.GetComponent<Rigidbody>().AddExplosionForce(
                            explosionForce, explosionPos, radius, upForce, ForceMode.VelocityChange);
                }
            }
        }
    }

    IEnumerator PlayAndDestroy()
    {
        yield return new WaitForSeconds(timeToDestroy);
        Destroy(gameObject);
    }

    IEnumerator WaitToExplode()
    {
        yield return new WaitForSeconds(timeBeforeStart);
        StartCoroutine("PlayAndDestroy");
        Explode();
    }
}
