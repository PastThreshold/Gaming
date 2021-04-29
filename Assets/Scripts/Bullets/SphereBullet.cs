using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereBullet : MonoBehaviour
{
    [SerializeField] int damage = 20;
    [SerializeField] float speed = 20f;
    [SerializeField] Enemy target;
    [SerializeField] float rotationAmount = 500f;
    [Tooltip("This is how much to increase per second on the bullet's speed after its target has been destroyed")]
    [SerializeField] float increase = 1f;
    [Tooltip("This is factor to match the increase speed of the rotation after the target is destroyed")]
    [SerializeField] float rotationIncreaseFactor = 2f;

    [SerializeField] float speedToFire = 20f;
    [SerializeField] float speedToDestroy = 30f;

    [SerializeField] GameObject fireParticleSystem;
    float distanceBetween;
    Vector3 targetPos;
    public int randomDirection;

    Rigidbody rb;

    void Awake()
    {
        fireParticleSystem.SetActive(false);
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        distanceBetween = Mathf.Infinity;
        foreach (Enemy enemy in enemies)
        {
                float distance = (enemy.transform.position - transform.position).sqrMagnitude;
                if (distanceBetween > distance)
                {
                    target = enemy;
                    distanceBetween = distance;
                }
        }
        targetPos = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
        randomDirection = Random.Range(0, 2);
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (target != null)
        {
            targetPos = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
            rb.velocity = transform.forward * speed;
            transform.LookAt(targetPos);
        }
        else
        {
            if (randomDirection == 0)
            {
                transform.Rotate(Vector3.up * rotationAmount * Time.deltaTime);
            }
            if (randomDirection == 1)
            {
                transform.Rotate(Vector3.down * rotationAmount * Time.deltaTime);
            }
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            speed += increase * Time.deltaTime;
            rotationAmount += increase * rotationIncreaseFactor * Time.deltaTime;
            if (speed >= speedToFire)
            {
                fireParticleSystem.SetActive(true);
            }
            if (speed >= speedToDestroy)
            {
                Destroy(gameObject);
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        
        if(other.GetComponentInParent<Enemy>())
        {
            Vector3 backwardsVisualized = transform.position - -transform.forward;
            Vector3 rotation = (backwardsVisualized * 5 - other.transform.position).normalized;
            rotation.y = 0f;
            other.GetComponentInParent<Enemy>().TakeDamage(damage);

            Destroy(gameObject);
            
        }
        else if (other.GetComponent<TimeField>())
            return;
        else if (other.GetComponent<GravityWell>())
            Destroy(gameObject);
        else if (other.tag == "Detect Bullet")
            return;
        else if(other.tag != "Sphere Weapon")
            Destroy(gameObject);
    }

    public void SetSpeed(int factor)
    {
        speed = speed / factor;
    }

}
