using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseAbility : MonoBehaviour, AbilityADT
{
    [SerializeField] int pulseAmmo = 1;
    [SerializeField] GameObject pulse;
    bool pulsed = false;
    bool scriptRunning = false;

    private void SpawnPulse()
    {
        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
        var newPulse = Instantiate(pulse, spawnPos, Quaternion.identity);
        newPulse.transform.Rotate(new Vector3(90, 0, 0));
        pulsed = true;
        pulseAmmo--;
        StartCoroutine("Pulsed");
    }

    IEnumerator Pulsed()
    {
        yield return new WaitForSeconds(10f);
        pulsed = false;
    }

    public void InputGetDown()
    {
        if (pulseAmmo >= 1 && !pulsed)
            SpawnPulse();
    }

    public void InputGet()
    {
        return;
    }

    public void InputGetUp()
    {
        return;
    }

    public void SetScriptStatus(bool status)
    {
        scriptRunning = status;
    }
}
