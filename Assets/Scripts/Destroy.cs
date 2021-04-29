using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{

    [SerializeField] float time = 5f;

    void Start()
    {
        StartCoroutine("DestroyObject");
    }

    IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    public void ResetTime(float newTime)
    {
        time = newTime;
        StopCoroutine("DestroyObject");
        StartCoroutine("DestroyObject");
    }
}
