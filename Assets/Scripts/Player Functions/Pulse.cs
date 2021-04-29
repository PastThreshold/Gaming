using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulse : MonoBehaviour
{

    [SerializeField] SphereCollider pulse;
    [SerializeField] float addFactor;

    void Start()
    {
        pulse.radius = 0.1f;
    }

    void Update()
    {
        //The particle system takes ~1.8 seconds to complete. Time.deltaTime makes is perfect over one second Add factor should be about half the full radius of the particle system.
        pulse.radius += addFactor * Time.deltaTime;
        if (pulse.radius >= 32.5f)
        {
            Destroy(gameObject); 
        }
    }
}
