using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AbilitySwitcher : MonoBehaviour
{
    public const int SPACEINT = 0;
    public const int EINT = 1;
    public const int RMBINT = 2;
    [SerializeField] Canvas equipMenu;
    [SerializeField] Canvas hud;
    HeadsUpDisplay hudScript;
    [SerializeField] ShieldAbility shield;
    [SerializeField] PulseAbility pulse;
    [SerializeField] BulletTime bulletTime;
    [SerializeField] Grapple grapple;
    [SerializeField] HoldPickup holdPickup;
    [SerializeField] Blink blink;
    [SerializeField] PushAndPull pushAndPull;
    [SerializeField] DeflectAbility deflect;
    [SerializeField] Enrage enrage;
    [SerializeField] AutoTarget autoTarget;

    BasicAbility[] allAbilities;

    [Header("Equip Menu Canvas")]
    [SerializeField] GameObject[] canvasSlots;
    public bool[] slotsTaken;
    public ClickAndDrag[] abilitySpritesEquipped;

    [SerializeField] float canvasSlotCloseEnough = 10f;

    AbilityADT spaceKeyAbility;
    AbilityADT eKeyAbility;
    AbilityADT rightClickAbility;

    bool menuActive = false;

    void Start()
    {
        ClickAndDrag.AssignSwitcher(this);
        slotsTaken = new bool[canvasSlots.Length];
        abilitySpritesEquipped = new ClickAndDrag[canvasSlots.Length];
        allAbilities = GetComponents<BasicAbility>();
        hudScript = hud.GetComponent<HeadsUpDisplay>();
    }

    // Update is called once per frame
    void Update()
    {
        if (menuActive)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                equipMenu.gameObject.SetActive(false);
                hud.gameObject.SetActive(true);
                menuActive = false;
                Time.timeScale = 1f;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                hud.gameObject.SetActive(false);
                equipMenu.gameObject.SetActive(true);
                menuActive = true;
                Time.timeScale = 0f;
            }

            if (spaceKeyAbility != null)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                    spaceKeyAbility.InputGetDown();

                else if (Input.GetKey(KeyCode.Space))
                    spaceKeyAbility.InputGet();

                else if (Input.GetKeyUp(KeyCode.Space))
                    spaceKeyAbility.InputGetUp();
            }
            if (eKeyAbility != null)
            {
                if (Input.GetKeyDown(KeyCode.E))
                    eKeyAbility.InputGetDown();

                else if (Input.GetKey(KeyCode.E))
                    eKeyAbility.InputGet();

                else if (Input.GetKeyUp(KeyCode.E))
                    eKeyAbility.InputGetUp();
            }
            if (rightClickAbility != null)
            {
                if (Input.GetMouseButtonDown(1))
                    rightClickAbility.InputGetDown();

                else if (Input.GetMouseButton(1))
                    rightClickAbility.InputGet();

                else if (Input.GetMouseButtonUp(1))
                    rightClickAbility.InputGetUp();
            }
        }
    }

    public bool CloseEnoughToSlot(ClickAndDrag obj)
    {
        if (AssignAbility(canvasSlots[0], obj))
            return true;
        else if (AssignAbility(canvasSlots[1], obj))
            return true;
        else if (AssignAbility(canvasSlots[2], obj))
            return true;
        else
            return false;
    }

    public bool AssignAbility(GameObject canvasSlot, ClickAndDrag obj)
    {
        AbilityADT tempKey = null;
        if ((obj.transform.position - canvasSlot.transform.position).sqrMagnitude < canvasSlotCloseEnough)
        {
            obj.transform.position = canvasSlot.transform.position;
            switch (obj.type)
            {
                case BasicAbility.abilityType.blink:
                    tempKey = blink;
                    break;
                case BasicAbility.abilityType.bulletTime:
                    tempKey = bulletTime;
                    break;
                case BasicAbility.abilityType.deflect:
                    tempKey = deflect;
                    break;
                case BasicAbility.abilityType.pushPull:
                    tempKey = pushAndPull;
                    break;
                case BasicAbility.abilityType.grapple:
                    tempKey = grapple;
                    break;
                case BasicAbility.abilityType.holdPickup:
                    tempKey = holdPickup;
                    break;
                case BasicAbility.abilityType.shield:
                    tempKey = shield;
                    break;
                case BasicAbility.abilityType.enrage:
                    tempKey = enrage;
                    break;
                case BasicAbility.abilityType.autoTarget:
                    tempKey = autoTarget;
                    break;
            }

            int place = -1; // 0 = space, 1 = e, 2 = rmb  
            if (canvasSlot == canvasSlots[SPACEINT])
            {
                place = SPACEINT;
                spaceKeyAbility = tempKey;
                spaceKeyAbility.SetScriptStatus(true);
            }
            else if (canvasSlot == canvasSlots[EINT])
            {
                place = EINT;
                eKeyAbility = tempKey;
                eKeyAbility.SetScriptStatus(true);
            }
            else if (canvasSlot == canvasSlots[RMBINT])
            {
                place = RMBINT;
                rightClickAbility = tempKey;
                rightClickAbility.SetScriptStatus(true);
            }

            if (place != -1)
            {
                if (slotsTaken[place] == true && abilitySpritesEquipped[place] != obj)
                    abilitySpritesEquipped[place].ForceUnequip();
                obj.AssignSlot(canvasSlots[place]);
                abilitySpritesEquipped[place] = obj;
                slotsTaken[place] = true;
                hudScript.UpdateAbilites(place, obj.type);
            }

            return true;
        }
        else
            return false;
    }

    public void UnAssignAbility(GameObject canvasSlot)
    {
        int count = -1;
        if (canvasSlot == canvasSlots[SPACEINT])
        {
            count = SPACEINT;
            spaceKeyAbility.SetScriptStatus(false);
            spaceKeyAbility = null;
            abilitySpritesEquipped[count] = null;
            slotsTaken[count] = false;
        }
        else if (canvasSlot == canvasSlots[EINT])
        {
            count = EINT;
            eKeyAbility.SetScriptStatus(false);
            eKeyAbility = null;
            abilitySpritesEquipped[count] = null;
            slotsTaken[count] = false;
        }
        else if (canvasSlot == canvasSlots[RMBINT])
        {
            count = RMBINT;
            rightClickAbility.SetScriptStatus(false);
            rightClickAbility = null;
            abilitySpritesEquipped[count] = null;
            slotsTaken[count] = false;
        }
        hudScript.RemoveAbility(count);
    }

    public void TempUpgradeAbilities(float timeAllowed)
    {
        foreach(BasicAbility abil in allAbilities)
        {
            abil.TemporaryUpgradeAbilityLevel(timeAllowed);
        }
    }
}
