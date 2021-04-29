using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyBombLauncher : BasicWeapon
{
    const int MAX_STICKIES = 35;
    public List<StickyBomb> allStickyBombsActive;
    [SerializeField] float distanceToConnect = 10f;
    [SerializeField] float timeBetweenExplosion = 0.08f;
    [SerializeField] GameObject connectedLaser;

    private void Start()
    {
        projPool = GlobalClass.stickyblPool;
        allStickyBombsActive = new List<StickyBomb>();
        CheckWeaponLevel();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift))
        {
            if (active)
            {
                Explode();
                StartCoroutine(DisableFiring());
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (canFire && active)
            {
                StartCoroutine(FireBomb());
            }
        }
    }

    private void Explode()
    {
        switch (currentWeaponLevel)
        {
            case 1:
                ExplodeAllBombs();
                break;
            case 2:
                ChainNextBombs();
                break;
            case 3:
                ChainBombsWithinRange();
                break;
            case 4:
                ChainBombsWithinRange();
                break;
            default:
                Debug.Log("Weapon Level firing error");
                break;
        }
    }

    private void ExplodeAllBombs()
    {
        foreach(StickyBomb stickyBomb in allStickyBombsActive)
        {
            stickyBomb.ExplodeBomb();
        }
        allStickyBombsActive.Clear();
    }

    private void ChainBombsWithinRange()
    {
        foreach (StickyBomb bomb in allStickyBombsActive)
        {
            foreach (StickyBomb otherBomb in allStickyBombsActive)
            {
                if (otherBomb == bomb || bomb.CheckIfConnected(otherBomb))
                    continue;

                float distance = (otherBomb.transform.position - bomb.transform.position).magnitude;
                if (distance <= distanceToConnect)
                {
                    var laser = Instantiate(connectedLaser, bomb.transform.position, Quaternion.identity);
                    laser.transform.LookAt(otherBomb.transform.position);
                    laser.GetComponent<LineRenderer>().SetPosition(1, new Vector3(0, 0, distance));
                    bomb.AddConnection(otherBomb);
                    otherBomb.AddConnection(bomb);
                }
            }
        }
        StartCoroutine(WaitAfterActivation());
    }
    
    private void ChainNextBombs()
    {
        for (int i = 0; i < allStickyBombsActive.Count; i++)
        {
            StickyBomb thisBomb = allStickyBombsActive[i];
            StickyBomb theNextBomb;

            if (i == allStickyBombsActive.Count - 1)
            {
                theNextBomb = allStickyBombsActive[0];
            }
            else
            {
                theNextBomb = allStickyBombsActive[i + 1];
            }

            float distance = (theNextBomb.transform.position - thisBomb.transform.position).magnitude;
            if (distance <= distanceToConnect)
            {
                var laser = Instantiate(connectedLaser, allStickyBombsActive[i].transform.position, Quaternion.identity);
                laser.transform.LookAt(theNextBomb.transform.position);
                laser.GetComponent<LineRenderer>().SetPosition(1, new Vector3(0, 0, distance));
                Debug.DrawRay(thisBomb.transform.position, theNextBomb.transform.position - thisBomb.transform.position, Color.red, 10f);
                thisBomb.AddConnection(theNextBomb);
                theNextBomb.AddConnection(thisBomb);
                StartCoroutine(WaitAfterActivation());
            }
        }
    }

    IEnumerator WaitAfterActivation()
    {
        yield return new WaitForSeconds(timeBetweenExplosion);
        foreach (StickyBomb stickyBomb in allStickyBombsActive)
        {
            stickyBomb.CreateRaycasts();
        }
        foreach (StickyBomb stickyBomb in allStickyBombsActive)
        {
            stickyBomb.ExplodeBomb();
        }
        allStickyBombsActive.Clear();
    }

    IEnumerator FireBomb()
    {
        canFire = false;
        Fired();
        Projectile bomb = CreateBasicProjectile();
        SetProjectileTarget(bomb);
        bomb.EnableProjectile();

        yield return new WaitForSeconds(fireRate);
        canFire = true;
    }

    IEnumerator DisableFiring()
    {
        canFire = false;
        yield return new WaitForSeconds(fireRate);
        canFire = true;
    }

    public List<StickyBomb> GetStickyBombsActiveList()
    {
        return allStickyBombsActive;
    }

    protected override Projectile CreateBasicProjectile()
    {
        Projectile bomb = base.CreateBasicProjectile();
        AddSticky(bomb);
        return bomb;
    }

    //Adds a sticky to the list of active ones
    public void AddSticky(Projectile bomb)
    {
        allStickyBombsActive.Add(bomb.GetComponent<StickyBomb>());
    }

    public override void DisableWeapon()
    {
        Explode();
    }
}
