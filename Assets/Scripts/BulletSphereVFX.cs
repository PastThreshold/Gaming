using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSphereVFX : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine("PlayAndDestroy");
    }

    IEnumerator PlayAndDestroy()
    {
        GetComponent<ParticleSystem>().Play();
        yield return new WaitForSeconds(.5f);
        Destroy(gameObject); 
    }
}
