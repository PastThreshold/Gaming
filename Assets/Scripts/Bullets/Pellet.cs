using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pellet : Projectile
{
    [SerializeField] GameObject pellet;
    [SerializeField] float timeTilSplit = 1f;

    Vector3 prevFramPos;
    Vector3 Pos;
    int currentBounces = 0;
    [SerializeField] int bounces = 2;
    public bool splitAlready;

    [Header("Disable Components")]
    [SerializeField] TrailRenderer trail;
    [SerializeField] ParticleSystem pSystem;
    [SerializeField] Collider collider;

    private void Start()
    {
        //rb = GetComponent<Rigidbody>();
        //Pos = rb.position;
        currentBounces = bounces;
        DisableProjectile();
        enabled = false;
    }
    /*
    void FixedUpdate()
    {
        //prevFramPos = Pos;
        //Pos = rb.position;
        //transform.rotation = Quaternion.LookRotation(rb.velocity);
    }*/

    private void Update()
    {
        //transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        /*
        if (other.tag == "Shield")
            return;
        else if (other.GetComponent<TimeField>())
            return;
        else if (other.tag == "Detect Bullet")
            return;
        else if (other.GetComponent<GravityWell>())
            DisableProjectile();

        else if (other.tag == "Sphere Weapon")
        {
            if (!splitAlready)
                StartCoroutine("Split", other);
            else
                return;
        }
        else  if (other.GetComponentInParent<Enemy>())
        {
            print("enem");
            Vector3 backwardsVisualized = rb.position - -transform.forward;
            Vector3 rotation = (backwardsVisualized * 5 - other.transform.position).normalized;
            rotation.y = 0f;
            other.GetComponentInParent<Enemy>().TakeDamage(damage, force, rotation);
            DisableProjectile();
        }
        else
        {
            if (level == 1)
                DisableProjectile();
            else if ((level == 2 || level == 3 || level == 4) && currentBounces > 0)
            {
                RaycastHit ray;
                if (Physics.Raycast(prevFramPos, rb.velocity.normalized, out ray))
                {
                    Vector3 point = Vector3.Reflect(Pos - prevFramPos, ray.normal);
                    point += transform.position;
                    point.y = transform.position.y;
                    transform.LookAt(point);
                    rb.velocity = transform.forward * (speed + 5f);
                }
                currentBounces--;
            }
            else
                DisableProjectile();
        }*/
        DisableProjectile();
    }

    IEnumerator Split(Collider other)
    {
        yield return new WaitForSeconds(timeTilSplit);

        var newPellet = Instantiate(pellet, transform.position, transform.rotation);
        newPellet.transform.Rotate(0, 8, 0);
        newPellet.GetComponent<Pellet>().splitAlready = true;

        newPellet = Instantiate(pellet, transform.position, transform.rotation);
        newPellet.transform.Rotate(0, -8, 0);
        newPellet.GetComponent<Pellet>().splitAlready = true;
    }

    public void SetSpeed(int factor)
    {
        speed = speed / factor;
    }

    public void SetDamage(int factor)
    {
        currentDamage = currentDamage / factor;
    }

    public int GetLevel()
    {
        return level;
    }

    public override void DisableProjectile()
    {
        trail.enabled = false;
        pSystem.Stop();
        collider.enabled = false;
        currentBounces = bounces;
        base.DisableProjectile();
        rb.isKinematic = true;
        enabled = false;
    }

    public override void EnableProjectile()
    {
        base.EnableProjectile();
        rb.isKinematic = false;
        rb.velocity = transform.forward * speed;
        trail.enabled = true;
        pSystem.Play();
        collider.enabled = true;
    }
}

