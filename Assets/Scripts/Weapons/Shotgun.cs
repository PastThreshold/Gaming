using System.Collections;
using UnityEngine;

public class Shotgun : BasicWeapon
{
    [Header("Fire")]
    [SerializeField] GameObject muzzleFlashVFX;

    private void Awake()
    {
        BaseAwake();
    }

    private void Start()
    {
        BaseStart();
        CheckWeaponLevel();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (canFire && active)
                StartCoroutine("Fire");
        }
    }

    IEnumerator Fire()
    {
        
        canFire = false;
        int angle = -1;

        for (int i = 0; i < 3; i++)
        {
            Projectile bullet = CreateBasicProjectile();
            bullet.transform.Rotate(0, angle, 0);
            bullet.enabled = true;
            bullet.EnableProjectile();
            angle += 1;
        }
        Fired();
        //Instantiate(muzzleFlashVFX, projectileSpawn.transform.position, firePointRotation.transform.rotation);

        yield return new WaitForSeconds(fireRate);
        canFire = true;
    }
}
