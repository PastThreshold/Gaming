using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShield : MonoBehaviour
{
    Protector protectorScript;
    [Header("Health")]
    [SerializeField] float maxHealth = 1000;
    [SerializeField] float currentHealth = 0;
    float originalMaxHealth = 0;

    [Header("Other")]
    [SerializeField] protected bool largeShield = false;
    [SerializeField] bool damageGate = false;
    [SerializeField] float damageGateDamageFactor = 1.2f;
    [SerializeField] float addedLength = 3f;
    bool combined = false;

    [Header("Rendering")]
    [SerializeField] MeshRenderer mainRenderer;
    [GradientUsage(true)] [SerializeField] Gradient colors;
    [GradientUsage(true)] [SerializeField] Gradient hitColors;
    float dissolveValue = 0;
    [SerializeField] float dissolveValueNone = 0;
    [SerializeField] float dissolveValueFull = 0;
    [SerializeField] float timeToDissolve = 0.5f;

    Vector3 originalScale;
    [SerializeField] float subsequentShieldSizeFactor = 0.9f;


    public void SetCaller(Protector protector)
    {
        protectorScript = protector;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        originalScale = transform.localScale;
        mainRenderer.material.SetColor("Color_B9E25027", colors.Evaluate(1f));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(GlobalClass.PROJECTILE_TAG))
        {
            if (!damageGate)
            {
                TakeDamage(other.GetComponent<Projectile>().GetDamage());
            }
            else
            {
                other.GetComponent<Projectile>().ChangeDamage(damageGateDamageFactor);
            }
        }
    }

    public void TriggeredByRaycast(Projectile other)
    {
        if (!damageGate)
        {
            TakeDamage(other.GetDamage());
        }
        else
        {
            other.ChangeDamage(damageGateDamageFactor);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            BroadcastMessage("Unparent", SendMessageOptions.DontRequireReceiver);
            protectorScript.ShieldDestroyed(this);
        }
        else
        {
            protectorScript.ShieldDamaged(this, damage);
            StartCoroutine(HitColorChange());
        }
    }

    public void TakeDamageCombined(float damage)
    {
        currentHealth -= damage;
        StartCoroutine(HitColorChange());
    }

    public void SetCombined(float health, float max)
    {
        combined = true;
        maxHealth = max;
        currentHealth = health;
        float colorVal = Mathf.InverseLerp(0, maxHealth, currentHealth);
        mainRenderer.material.SetColor("Color_B9E25027", colors.Evaluate(colorVal));
    }
    public void Uncombine()
    {
        currentHealth = originalMaxHealth * (currentHealth / originalMaxHealth);
        maxHealth = originalMaxHealth;
        combined = false;
        float colorVal = Mathf.InverseLerp(0, maxHealth, currentHealth);
        mainRenderer.material.SetColor("Color_B9E25027", colors.Evaluate(colorVal));
    }

    IEnumerator HitColorChange()
    {
        float colorVal = Mathf.InverseLerp(0, maxHealth, currentHealth);
        mainRenderer.material.SetColor("Color_B9E25027", hitColors.Evaluate(colorVal));
        yield return new WaitForSeconds(0.1f);
        ChangeColor();
    }

    /// <summary> Takes in flat health value to heal (Does not account for frame diffence or time) </summary>
    public void Heal(float health)
    {
        currentHealth += health;
        if (currentHealth >= maxHealth && !combined)
        {
            currentHealth = maxHealth;
            protectorScript.ShieldFull(this);
        }
        ChangeColor();
    }

    public void DisableShield()
    {
        BroadcastMessage("Unparent", SendMessageOptions.DontRequireReceiver);
        protectorScript.DisableShield(this);
    }

    public bool IgnoreThis()
    {
        return damageGate;
    }

    private void ChangeColor()
    {
        float colorVal = Mathf.InverseLerp(0, maxHealth, currentHealth);
        mainRenderer.material.SetColor("Color_B9E25027", colors.Evaluate(colorVal));
    }

    public void ResetSize() { transform.localScale = originalScale; }
    public void ChangeEntireSize(int previousShields)
    {
        transform.localScale = Extra.SubtractValueFromVector(transform.localScale, subsequentShieldSizeFactor * previousShields);
    }
    public void ChangeSize(float width)
    {
        transform.localScale = new Vector3(width + addedLength, transform.localScale.y, transform.localScale.z);
    }

    public float PhaseIn()
    {
        StartCoroutine(DissolveInMaterial());
        return timeToDissolve;
    }

    IEnumerator DissolveInMaterial()
    {
        float timeStep = 0.001f;
        float incrementValue = (dissolveValueFull - dissolveValueNone) / (timeToDissolve / timeStep);
        dissolveValue = dissolveValueNone;
        for (float i = 0; i < timeToDissolve; i += timeStep)
        {
            mainRenderer.material.SetFloat("Vector1_43D91402", dissolveValue);
            dissolveValue += incrementValue;
            yield return new WaitForSeconds(timeStep);
        }
    }


    public float GetHealth() { return currentHealth; }
    public float GetMaxHealth() { return maxHealth; }
    public float GetOrigMaxHealth() { return originalMaxHealth; }
}
