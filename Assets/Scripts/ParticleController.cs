using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{




    [Header("ChargeShot")]
    [SerializeField] ParticleSystem startSystem;
    [SerializeField] ParticleSystem midSystem;
    [SerializeField] ParticleSystem endSystem;

    public void ChargeShotStop()
    {
        startSystem.Stop();
        midSystem.Stop();
        endSystem.Stop();
    }

    public void ChargeShotStart() { StartCoroutine(ChargeShotPlay()); }
    IEnumerator ChargeShotPlay()
    {
        startSystem.Play();
        yield return new WaitForSeconds(ChargeShotStartTime());
        startSystem.Stop();
        midSystem.Play();
        yield return new WaitForSeconds(ChargeShotMidTime());
        midSystem.Stop();
        endSystem.Play();
    }

    public float ChargeShotStartTime() { return startSystem.main.duration; }
    public float ChargeShotMidTime() { return midSystem.main.startLifetime.constant; }
    public float ChargeShotEndTime() { return endSystem.main.startLifetime.constant; }
    public float ChargeShotTotalTime() { return ChargeShotMidTime() + ChargeShotStartTime(); }

}
