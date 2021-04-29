using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [SerializeField] ParticleSystem[] pSystems;
    [SerializeField] float time = 0f;

    void Start()
    {
        foreach (ParticleSystem system in pSystems)
        {
            system.Play();
        }
        StartCoroutine("Destroy");
    }

    IEnumerator Destroy()
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
