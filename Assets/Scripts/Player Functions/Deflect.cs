using System.Collections;
using UnityEngine;

public class Deflect : MonoBehaviour
{
    [SerializeField] GameObject deflectedBullet;
    [SerializeField] ParticleSystem pSystem;
    DeflectAbility mainScript;
    ProjectilePoolHandler projPool;

    [SerializeField] bool altObject;
    bool altMode;
    Vector3 direction;
    Vector3 startingPosition;
    float endDistance = 7.5f;
    float timeToEnd = 0.4f;
    float speed = 0;
    [SerializeField] BoxCollider altCollider;

    private void Start()
    {
        mainScript = FindObjectOfType<DeflectAbility>();
        speed = endDistance / timeToEnd;
        projPool = GlobalClass.deflectPool;
    }

    private void Update()
    {
        if (altMode)
        {
            print("in alt");
            transform.position += direction * speed * Time.deltaTime;
            float distance = (transform.position - startingPosition).magnitude;
            altCollider.size = new Vector3(altCollider.size.x, altCollider.size.y, distance);
            altCollider.center = new Vector3(altCollider.center.x, altCollider.center.y, -distance / 2);
            Debug.DrawLine(transform.position, startingPosition, Color.black, 10f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        print("col");
        if (other.CompareTag(GlobalClass.PROJECTILE_TAG))
        {
            Projectile script = other.GetComponent<Projectile>();
            if (script != null)
            {
                if (script.isEnemyBullet)
                {
                    DeflectBulletBack(script);
                }
            }
        }
    }

    public void TriggeredByRaycast(Projectile other)
    {
        DeflectBulletBack(other);
    }

    public void DeflectBulletBack(Projectile proj)
    {
        if (GlobalClass.player.HasTarget())
            proj.SetTarget(GlobalClass.player.GetTargetTransform());
        proj.transform.Rotate(0, 180f, 0);
        proj.FlipSide();
        proj.EnableProjectile();
        if (!altObject)
            mainScript.ResetCooldown();
    }

    public void SetAltMode()
    {
        altMode = true;
        transform.LookAt(Extra.SetYToTransform(GlobalClass.player.mouseLocationConverted, transform));
        direction = transform.forward * endDistance;
        direction = direction.normalized;
        startingPosition = transform.position;
        StartCoroutine("AltTimer");
    }

    IEnumerator AltTimer()
    {
        yield return new WaitForSeconds(timeToEnd);
        altCollider.size = new Vector3(altCollider.size.x, altCollider.size.y, 0);
        altMode = false;
        mainScript.AltReturned();
    }
}
