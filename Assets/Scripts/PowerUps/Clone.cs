using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clone : MonoBehaviour
{
    Animator anim;
    Rigidbody rb;
    static Player mainPlayer;
    public static bool playerControlling;
    public static bool lookingAtMouse = true;
    [SerializeField] float addFactor = -1.2265f;
    [SerializeField] GameObject[] allCloneWeapons;
    [SerializeField] Transform firePointRotation;
    public GameObject activeWep;
    public WeaponClone activeWepCloneScript;

    bool blinking = false;
    Vector3 blinkDirAndDist = Vector3.zero;


    public static void ControlledByPlayer()
    {
        playerControlling = !playerControlling;
    }

    public static void MouseDirectionOrPlayer()
    {
        lookingAtMouse = !lookingAtMouse;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (mainPlayer == null)
            mainPlayer = GlobalClass.player;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        mainPlayer.allActiveClones.Add(this);
        if (mainPlayer.allActiveClones.Count == 1)
            playerControlling = true;

        GlobalClass.weaponSwitcher.SwapCloneWeapon();
    }

    private void Update()
    {
        if (playerControlling)
        {
            if (blinking)
            {
                transform.position += blinkDirAndDist * Time.deltaTime;
            }
        }

        if (lookingAtMouse)
            LookAtMouse();
        else
            KeepPlayerDirection();
    }


    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (playerControlling)
            Move(h, v);

        float rotationY = transform.rotation.eulerAngles.y;
        Vector3 Val = new Vector3(h, 0f, v);
        Val = Quaternion.Euler(0f, -rotationY, 0f) * Val;
        if (playerControlling)
        {
            anim.SetFloat("Forward", Val.z);
            anim.SetFloat("Turn", Val.x);
        }
        else
        {
            anim.SetFloat("Forward", 0);
            anim.SetFloat("Turn", 0);
        }
    }

    private void KeepPlayerDirection()
    {
        transform.rotation = mainPlayer.transform.rotation;
        firePointRotation.rotation = transform.rotation;
    }

    private void LookAtMouse()
    {
        Vector3 pointToLook = mainPlayer.mouseLocationConverted;
        firePointRotation.transform.LookAt(new Vector3(pointToLook.x, firePointRotation.position.y, pointToLook.z));
        transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
    }

    private void Move(float h, float v)
    {
        Vector3 movement = Vector3.zero;
        movement.x = h;
        movement.z = v;
        movement = movement.normalized * mainPlayer.movementSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + movement);
    }

    public void SwitchActiveWeapon(int weaponSlot, GameObject projectile, int weaponLevel)
    {
        activeWep = allCloneWeapons[weaponSlot];
        activeWepCloneScript = activeWep.GetComponent<WeaponClone>();

        foreach(GameObject weapon in allCloneWeapons)
        {
            weapon.SetActive(false);
        }
        activeWep.SetActive(true);

        activeWepCloneScript.UpdateValues(projectile, weaponLevel);
    }

    public void FireWeapon()
    {
        activeWepCloneScript.Fire();
    }

    public void AltFireWeapon()
    {
        activeWepCloneScript.AltFire();
    }

    public void HoldFireWeapon()
    {
        activeWepCloneScript.HoldFire();
    }

    public void StopFireWeapon()
    {
        activeWepCloneScript.StopFire();
    }

    public void SetTimeUntilDeath(float time)
    {
        StartCoroutine("Despawn", time);
    }

    IEnumerator Despawn(float timer)
    {
        yield return new WaitForSeconds(timer);
        mainPlayer.allActiveClones.Remove(this);
        Destroy(gameObject);
    }

    public void Blink(Vector3 directionAndSpeed, float time)
    {
        blinking = true;
        blinkDirAndDist = directionAndSpeed;
        print(blinkDirAndDist);
        StartCoroutine("BlinkOK", time);
    }

    IEnumerator BlinkOK(float timeHere)
    {
        yield return new WaitForSeconds(timeHere);
        blinking = false;
    }
}
