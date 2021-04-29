using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    ShieldAbility mainScript;
    float startHealth = 0;
    float currentHealth = 0;
    bool altForm = false;
    float altFormDamageFactor = 0;
    [SerializeField] Collider shieldCollider;
    [SerializeField] Collider altCollider;
    [SerializeField] GameObject shieldPS;
    [SerializeField] GameObject altPS;

    private void Start()
    {
        mainScript = FindObjectOfType<ShieldAbility>();
    }

    public void SetValues(float startingHealth, float damageFactor)
    {
        startHealth = currentHealth = startingHealth;
        altFormDamageFactor = damageFactor;
    }

    public void ChangeHealth(float addValue)
    {
        currentHealth = Mathf.Clamp(currentHealth + addValue, 0, startHealth);
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    public void Enabled()
    {
        tag = GlobalClass.SHIELD_TAG;
        shieldCollider.enabled = true;
        shieldPS.SetActive(true);
        altCollider.enabled = false;
        altPS.SetActive(false);
    }

    public void AltForm()
    {
        altForm = true;
        transform.parent = null;
        tag = GlobalClass.PROJECTILE_GATE_TAG;

        shieldCollider.enabled = false;
        shieldPS.SetActive(false);
        altCollider.enabled = true;
        altPS.SetActive(true);
    }

    public void Disabled()
    {
        if (altForm)
            altForm = false;
    }

    private void OnTriggerEnter(Collider other)
    { 
        if (other.CompareTag(GlobalClass.PROJECTILE_TAG))
        {
            if (!altForm)
            {
                TakeDamage(other.GetComponent<Projectile>().GetDamage());
            }
            else
            {
                other.GetComponent<Projectile>().ChangeDamage(altFormDamageFactor);
            }
        }
    }

    public void TriggeredByRaycast(Projectile other)
    {
        if (!altForm)
        {
            TakeDamage(other.GetDamage());
        }
        else
        {
            other.ChangeDamage(altFormDamageFactor);
        }
    }

    private void TakeDamage(float damage)
    {
        currentHealth -= damage;
        print("Health is now: " + currentHealth);
        if (currentHealth <= 0)
        {
            mainScript.ShieldDestroyed();
            currentHealth = 0;
        }
    }

    public bool IgnoreThis()
    {
        return altForm;
    }
}
