using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyBomb : Projectile
{
    bool stuck = false;

    [Header("Bullet Level and Values")]
    [SerializeField] float connectionDamage = 15f;
    [SerializeField] float perfectWaitTime = 0.5f;
    [SerializeField] float perfectTimeWindow = 0.5f;
    bool perfectTime = false;
    [SerializeField] GameObject explosionVFX;
    [SerializeField] GameObject perfectExplosion;
    GameObject stuckTo;
    StickyBombLauncher gunFiredFrom;
    [SerializeField] MeshRenderer mesh;

    public List<StickyBomb> allConnectedBombs;
    bool linkedAll;

    private void Start()
    {
        gunFiredFrom = FindObjectOfType<StickyBombLauncher>();
        rb = GetComponent<Rigidbody>();
        DisableProjectile();
    }

    void Update()
    {
        // (moving)
           //transform.rotation = Quaternion.LookRotation(rb.velocity);
    }

    private void FixedUpdate()
    {
        BaseFixedUpdate();
    }

    void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case GlobalClass.DEFAULT_TAG:
                DisableProjectile();
                break;
            case GlobalClass.PLAYER_TAG:
                other.GetComponentInParent<Player>().TakeDamage(currentDamage);
                DisableProjectile();
                break;
            case GlobalClass.PROJECTILE_TAG:
                break;
            case GlobalClass.ENEMY_TAG:
                moving = false;
                stuck = true;
                rb.isKinematic = true;
                transform.parent = other.transform;
                stuckTo = other.gameObject;
                break;
            case GlobalClass.PICKUP_TAG:
                break;
            case GlobalClass.ROOM_TAG:
                moving = false;
                stuck = true;
                rb.isKinematic = true;
                transform.parent = other.transform;
                stuckTo = other.gameObject;
                break;
            case GlobalClass.SPECIAL_TAG:
                if (!other.GetComponent<SphereWeapon>())
                {
                    if (!other.GetComponent<TimeField>())
                        DisableProjectile();
                }
                else
                    StartCoroutine("WaitUntilPerfectTime");
                break;
            case GlobalClass.SHIELD_TAG:
                break;
            case GlobalClass.DETECT_BULLETS_TAG:
                break;
            default:
                Debug.Log("Different String Collision");
                moving = false;
                stuck = true;
                rb.isKinematic = true;
                transform.parent = other.transform;
                stuckTo = other.gameObject;
                break;
        }
    }

    IEnumerator WaitUntilPerfectTime()
    {
        yield return new WaitForSeconds(perfectWaitTime);
        StartCoroutine("PerfectTimeWindow");
    }

    IEnumerator PerfectTimeWindow()
    {
        perfectTime = true;
        yield return new WaitForSeconds(perfectTimeWindow);
        perfectTime = false;
    }

    public void ExplodeBomb()
    {
        print(perfectTime);
        if (perfectTime)
        {
            Instantiate(perfectExplosion, transform.position, transform.rotation);
        }
        else
        {
            Instantiate(explosionVFX, transform.position, transform.rotation);
        }
        DisableProjectile();
    }

    public void CreateRaycasts()
    {
        for (int i = 0; i < allConnectedBombs.Count; i++)
        {
            if (allConnectedBombs[i].linkedAll)
            {
                print(name + " Already connected with: " + allConnectedBombs[i].name);
                continue;
            }

            print(name + " Connecting with: " + allConnectedBombs[i].name);
            Vector3 direction = allConnectedBombs[i].transform.position - transform.position;
            RaycastHit[] hits = Physics.RaycastAll(transform.position, Extra.SetYToZero(direction).normalized, direction.magnitude, GlobalClass.exD.enemiesOnlyLM);
            List<GameObject> enemiesHit = CollisionHandler.TestSingleTriggerArray(hits);
            foreach(GameObject hit in enemiesHit)
            {
                if (hit.tag == GlobalClass.ENEMY_TAG)
                {
                    hit.GetComponent<Enemy>().TakeDamage(connectionDamage);
                }
                print(hit.transform.name);
            }
        }
        linkedAll = true;
    }

    public void AddConnection(StickyBomb bomb)
    {
        allConnectedBombs.Add(bomb);
    }

    public bool CheckIfConnected(StickyBomb bomb)
    {
        return allConnectedBombs.Contains(bomb);
    }

    public override void StopMoving()
    {
        base.StopMoving();
    }

    public override void ContinueMoving()
    {
        base.ContinueMoving();
    }

    public override void DisableProjectile()
    {
        moving = false;
        linkedAll = false;
        perfectTime = false;
        StopAllCoroutines();
        allConnectedBombs.Clear();
        mesh.enabled = false;
        base.DisableProjectile();
        rb.velocity = Vector3.zero;
        enabled = false;
    }

    public override void EnableProjectile()
    {
        rb.isKinematic = false;
        mesh.enabled = true;
        base.EnableProjectile();
    }
}
