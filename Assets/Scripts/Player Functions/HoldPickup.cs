using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldPickup : BasicAbility, AbilityADT
{
    bool keyHeldDown = false;
    bool scriptRunning = false;
    bool willThrow = false;

    [SerializeField] int levelToExchange = 2;
    bool canExchange = false;
    bool exchangeOnCooldown = false;
    float exchangeCooldown = 10f;

    [SerializeField] int canHoldL1 = 1;
    [SerializeField] int canHoldL2 = 2;
    [SerializeField] int canHoldL3 = 3;
    [SerializeField] int canHoldL4 = 4;
    int canHold = 0;
    int currentIndex = 0;
    public Pickup[] heldPickups;

    bool holdingDown = false;
    bool throwing = false;
    public Pickup thrownPickup;
    float timeForThrown = 0.5f;
    float speed = 0;
    Vector3 direction = Vector3.zero;

    void Start()
    {
        heldPickups = new Pickup[canHoldL4];
        CheckAbilityLevel();
    }

    private void Update()
    {
        if (throwing)
        {
            thrownPickup.transform.position += direction * speed * Time.deltaTime;
        }
    }

    public override void CheckAbilityLevel()
    {
        switch(currentAbilityLevel)
        {
            case 1:
                canHold = canHoldL1;
                break;
            case 2:
                canHold = canHoldL2;
                break;
            case 3:
                canHold = canHoldL3;
                break;
            case 4:
                canHold = canHoldL4;
                break;
        }

        if (currentAbilityLevel >= levelToExchange)
        {

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Pickup>())
        {
            if (keyHeldDown && scriptRunning)
            {
                if (heldPickups[currentIndex] != null)
                {
                    if (willThrow)
                        ThrowPickup();
                    else
                        UsePickup();
                }
                AssignSlot(currentIndex, other.GetComponent<Pickup>());
            }
            else
            {
                other.GetComponent<Pickup>().SpawnPickup();
            }
        }
    }

    private void AssignSlot(int index, Pickup obj)
    {
        heldPickups[index] = obj;
        heldPickups[index].gameObject.SetActive(false);
    }

    private void UsePickup()
    {
        heldPickups[currentIndex].GetComponent<Pickup>().SpawnPickup();
        heldPickups[currentIndex] = null;
    }

    private void ThrowPickup()
    {
        thrownPickup = heldPickups[currentIndex];
        thrownPickup.transform.position = transform.position;
        thrownPickup.gameObject.SetActive(true);
        heldPickups[currentIndex] = null;

        direction = ((transform.forward + transform.position) - transform.position).normalized;
        speed = 5f / timeForThrown;

        StartCoroutine("ThrownObjectTimer");
    }

    IEnumerator ThrownObjectTimer()
    {
        throwing = true;
        yield return new WaitForSeconds(timeForThrown);
        throwing = false;
    }

    public void InputGetDown()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentIndex++;

            if (currentIndex >= canHold)
                currentIndex = 0;

            string name;
            if (heldPickups[currentIndex] != null)
                name = heldPickups[currentIndex].name;
            else
                name = "Null";
            print("Current spot: " + currentIndex + ", Which contains a " + name);
        }
        else if (heldPickups[currentIndex] != null)
        {
            holdingDown = true;
        }

        keyHeldDown = true;
    }

    public void InputGet()
    {

    }

    private bool IsPickupConsumable()
    {
        return heldPickups[currentIndex].type != Pickup.Type.obj;
    }

    public void InputGetUp()
    {
        if (holdingDown)
        {
            if (IsPickupConsumable())
                UsePickup();
            else
                ThrowPickup();

            holdingDown = false;
        }
        keyHeldDown = false;
    }

    public void SetScriptStatus(bool status)
    {
        scriptRunning = status;
    }
}
