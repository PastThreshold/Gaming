using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deagles : BasicWeapon
{
    [Header("Fire")]
    [SerializeField] Transform rightBarrel;
    [SerializeField] Transform leftBarrel;
    [SerializeField] GameObject firstGun;
    [SerializeField] GameObject secondGun;
    [SerializeField] GameObject muzzleFlashVFX;
    public bool bothGunsActive;
    [SerializeField] AudioClip audioClip;
    [SerializeField] float pitchRandomization;

    private void Awake()
    {
        BaseAwake();
    }

    private void Start()
    {
        BaseStart();
        shotSound = GetComponent<AudioSource>();
        CheckWeaponLevel();
    }

    public override void CheckWeaponLevel()
    {
        base.CheckWeaponLevel();

        
        switch(currentWeaponLevel)
        {
            case 1:
                bothGunsActive = false;
                break;
            case 2:
                bothGunsActive = true;
                break;
            case 3:
                bothGunsActive = true;
                break;
            case 4:
                bothGunsActive = true;
                break;
            default:
                Debug.Log("Weapon Level Error");
                break;
        }
        SetSecondWeapon();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (canFire && active)
            {
                StartCoroutine(FireLevelOne());
                
                switch (currentWeaponLevel)
                {
                    case 1:
                        StartCoroutine("FireLevelOne");
                        break;
                    case 2:
                        StartCoroutine("FireLevelTwo");
                        break;
                    case 3:
                        StartCoroutine("FireLevelThree");
                        break;
                    case 4:
                        StartCoroutine("FireLevelThree");
                        break;
                    default:
                        Debug.Log("Weapon Level Error");
                        break;
                }
            }
        }
    }

    IEnumerator FireLevelOne()
    {
        canFire = false;
        Fired();
        Projectile bullet1 = CreateProjectile(rightBarrel.transform, rightBarrel.transform);
        SetProjectileTarget(bullet1);
        bullet1.EnableProjectile();
        var muzzleFlash1 = Instantiate(muzzleFlashVFX, rightBarrel.transform.position, leftBarrel.transform.rotation);
        muzzleFlash1.transform.parent = rightBarrel.gameObject.transform;

        Projectile bullet2 = null;
        if (currentWeaponLevel >= 2)
        {
            bullet2 = CreateProjectile(leftBarrel.transform, leftBarrel.transform);
            SetProjectileTarget(bullet2);
            bullet2.EnableProjectile();
            var muzzleFlash2 = Instantiate(muzzleFlashVFX, leftBarrel.transform.position, leftBarrel.transform.rotation);
            muzzleFlash2.transform.parent = leftBarrel.gameObject.transform;
        }
        if (currentWeaponLevel >= 3)
        {
            bullet1.GetComponent<HighCaliber>().SetBulletPair(bullet2.gameObject);
            bullet2.GetComponent<HighCaliber>().SetBulletPair(bullet1.gameObject);
        }

        //shotSound.clip = audioClip;
        //shotSound.pitch = 1 - pitchRandomization + Random.Range(-pitchRandomization, pitchRandomization);
        //shotSound.PlayOneShot(audioClip);
        yield return new WaitForSeconds(fireRate);
        canFire = true;
    }

    IEnumerator FireLevelTwo()
    {
        canFire = false;
        Fired();
        Projectile bullet1 = CreateProjectile(leftBarrel.transform, leftBarrel.transform);
        bullet1.EnableProjectile();
        Projectile bullet2 = CreateProjectile(rightBarrel.transform, rightBarrel.transform);
        bullet2.EnableProjectile();

        var muzzleFlash = Instantiate(muzzleFlashVFX, leftBarrel.transform.position, leftBarrel.transform.rotation);
        muzzleFlash.transform.parent = leftBarrel.gameObject.transform;
        var muzzleFlash2 = Instantiate(muzzleFlashVFX, rightBarrel.transform.position, rightBarrel.transform.rotation);
        muzzleFlash2.transform.parent = rightBarrel.gameObject.transform;

        yield return new WaitForSeconds(fireRate);
        canFire = true;
    }

    IEnumerator FireLevelThree()
    {
        canFire = false;
        Fired();
        Projectile bullet1 = CreateProjectile(leftBarrel.transform, leftBarrel.transform);
        Projectile bullet2 = CreateProjectile(rightBarrel.transform, rightBarrel.transform);
        bullet1.GetComponent<HighCaliber>().SetBulletPair(bullet2.gameObject);
        bullet2.GetComponent<HighCaliber>().SetBulletPair(bullet1.gameObject);
        bullet1.EnableProjectile(); 
        bullet2.EnableProjectile();

        var muzzleFlash = Instantiate(muzzleFlashVFX, leftBarrel.transform.position, leftBarrel.transform.rotation);
        muzzleFlash.transform.parent = leftBarrel.gameObject.transform;
        var muzzleFlash2 = Instantiate(muzzleFlashVFX, rightBarrel.transform.position, rightBarrel.transform.rotation);
        muzzleFlash2.transform.parent = rightBarrel.gameObject.transform;


        yield return new WaitForSeconds(fireRate);
        canFire = true;
    }

    public void SetSecondWeapon()
    {
        if (bothGunsActive)
            secondGun.SetActive(true);
        else
            secondGun.SetActive(false);
    }

    protected Projectile CreateProjectile(Transform spawn, Transform rotation)
    {
        Projectile bullet = projPool.GetNextProjectile(currentWeaponLevel);
        bullet.transform.position = spawn.position;
        bullet.transform.rotation = rotation.transform.rotation;
        bullet.enabled = true;
        return bullet;
    }

    public override void EnableWeapon()
    {
        base.EnableWeapon();
        SetSecondWeapon();
    }
}
