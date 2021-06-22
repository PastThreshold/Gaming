using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadsUpDisplay : MonoBehaviour
{
    [SerializeField] GameObject container;

    [SerializeField] Image[] weaponImages;

    [SerializeField] Slider healthBar;
    [SerializeField] Text ammo1;
    [SerializeField] Text ammo2;
    [SerializeField] Text ammo3;

    [SerializeField] Text fpsCounter;
    int fps;
    float timer = 1f;


    [SerializeField] Image crosshair;

    [Header("Abilities")]
    [SerializeField] HUDSlot abilitySlot1;
    [SerializeField] HUDSlot abilitySlot2;
    [SerializeField] HUDSlot abilitySlot3;
    [SerializeField] HUDSlot wepSlot1;
    [SerializeField] HUDSlot wepSlot2;
    [SerializeField] HUDSlot wepSlot3;
    [SerializeField] Image blinkSprite;
    [SerializeField] Image autoTargetSprite;
    [SerializeField] Image deflectSprite;
    [SerializeField] Image shieldSprite;
    [SerializeField] Image holdSprite;
    [SerializeField] Image grappleSprite;
    [SerializeField] Image bulletTimeSprite;
    [SerializeField] Image enrageSprite;
    [SerializeField] Image pushPullSprite;

    [SerializeField] GameObject autoTargetCrosshair;
    float crosshairRotationSpeed = 125f;

    private void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        crosshair.transform.position = Input.mousePosition;
        UpdateFPS();
    }

    public void UpdateAutoTargetCrosshair(Vector3 position) 
    {
        if (!autoTargetCrosshair.activeSelf)
            autoTargetCrosshair.SetActive(true);
        position = Camera.main.WorldToScreenPoint(position);
        autoTargetCrosshair.transform.position = position;
        autoTargetCrosshair.transform.Rotate(new Vector3(0, 0, crosshairRotationSpeed) * Time.deltaTime);
    }


    public void UpdateAutoTargetCrosshair()
    {
        if (autoTargetCrosshair.activeSelf)
            autoTargetCrosshair.SetActive(false);
    }

    public void UpdateWeaponHUD(int weaponSpot, int slot, int level)
    {
        switch (slot)
        {
            case 0:
                wepSlot1.AssignWeapon(weaponImages[weaponSpot], level);
                break;
            case 1:
                wepSlot2.AssignWeapon(weaponImages[weaponSpot], level);
                break;
            case 2:
                wepSlot3.AssignWeapon(weaponImages[weaponSpot], level);
                break;
        }
    }

    public void UpdateWeaponHUD(int slot)
    {
        switch (slot)
        {
            case 0:
                wepSlot1.UnassignWeapon();
                break;
            case 1:
                wepSlot2.UnassignWeapon();
                break;
            case 2:
                wepSlot3.UnassignWeapon();
                break;
        }
    }

    public void ChangeWeaponLevel(int slot, int level)
    {
        print("slot: " + slot + " level: " + level);
        switch (slot)
        {
            case 0:
                wepSlot1.ChangeBorderColor(level);
                break;
            case 1:
                wepSlot2.ChangeBorderColor(level);
                break;
            case 2:
                wepSlot3.ChangeBorderColor(level);
                break;
        }
    }

    public void ChangeActiveWeapon(int slot)
    {
        wepSlot1.ChangeBorderOpacity(false);
        wepSlot2.ChangeBorderOpacity(false);
        wepSlot3.ChangeBorderOpacity(false);
        switch (slot)
        {
            case 0:
                wepSlot1.ChangeBorderOpacity(true);
                break;
            case 1:
                wepSlot2.ChangeBorderOpacity(true);
                break;
            case 2:
                wepSlot3.ChangeBorderOpacity(true);
                break;
        }
                
    }

    public void UpdateHealth(float newHealth)
    {
        healthBar.value = newHealth;
    }

    public void UpdateAmmo(float ammo, int slot)
    {
        ammo = (int)ammo;
        switch(slot)
        {
            case 0:
                ammo1.text = ammo.ToString();
                break;
            case 1:
                ammo2.text = ammo.ToString();
                break;
            case 2:
                ammo3.text = ammo.ToString();
                break;
        }
    }

    public void UpdateAbilites(int key, BasicAbility.abilityType type)
    {
        print(key + " + " + type);
        switch(type)
        {
            case BasicAbility.abilityType.blink:
                AssignSpriteToSlot(key, blinkSprite, type);
                break;
            case BasicAbility.abilityType.autoTarget:
                AssignSpriteToSlot(key, autoTargetSprite, type);
                break;
            case BasicAbility.abilityType.deflect:
                AssignSpriteToSlot(key, deflectSprite, type);
                break;
            case BasicAbility.abilityType.shield:
                AssignSpriteToSlot(key, shieldSprite, type);
                break;
            case BasicAbility.abilityType.bulletTime:
                AssignSpriteToSlot(key, bulletTimeSprite, type);
                break;
            case BasicAbility.abilityType.enrage:
                AssignSpriteToSlot(key, enrageSprite, type);
                break;
            case BasicAbility.abilityType.grapple:
                AssignSpriteToSlot(key, grappleSprite, type);
                break;
            case BasicAbility.abilityType.holdPickup:
                AssignSpriteToSlot(key, holdSprite, type);
                break;
            case BasicAbility.abilityType.pushPull:
                AssignSpriteToSlot(key, pushPullSprite, type);
                break;
            default:
                Debug.Log("Unimplemented Ability");
                break;
        }
    }

    private void AssignSpriteToSlot(int key, Image sprite, BasicAbility.abilityType type)
    {
        switch(key)
        {
            case AbilitySwitcher.SPACEINT:
                abilitySlot1.AssignAbility(sprite, type);
                break;
            case AbilitySwitcher.EINT:
                abilitySlot2.AssignAbility(sprite, type);
                break;
            case AbilitySwitcher.RMBINT:
                abilitySlot3.AssignAbility(sprite, type);
                break;
        }
    }

    public void RemoveAbility(int key)
    {
        switch (key)
        {
            case AbilitySwitcher.SPACEINT:
                abilitySlot1.UnassignAbility();
                break;
            case AbilitySwitcher.EINT:
                abilitySlot2.UnassignAbility();
                break;
            case AbilitySwitcher.RMBINT:
                abilitySlot3.UnassignAbility();
                break;
        }
    }

    /// <summary>
    /// Takes in the ability type that is being used, then gets the sprite, and then the slot where the sprite is located to 
    /// enabled the white flash
    /// </summary>
    private Image FindSpriteFromType(BasicAbility.abilityType type)
    {
        Image sprite = null;
        switch (type)
        {
            case BasicAbility.abilityType.blink:
                sprite = blinkSprite;
                break;
            case BasicAbility.abilityType.autoTarget:
                sprite = autoTargetSprite;
                break;
            case BasicAbility.abilityType.deflect:
                sprite = deflectSprite;
                break;
            case BasicAbility.abilityType.shield:
                sprite = shieldSprite;
                break;
            case BasicAbility.abilityType.bulletTime:
                sprite = bulletTimeSprite;
                break;
            case BasicAbility.abilityType.enrage:
                sprite = enrageSprite;
                break;
            case BasicAbility.abilityType.grapple:
                sprite = grappleSprite;
                break;
            case BasicAbility.abilityType.holdPickup:
                sprite = holdSprite;
                break;
            case BasicAbility.abilityType.pushPull:
                sprite = pushPullSprite;
                break;
            default:
                Debug.Log("Unimplemented Ability");
                break;
        }
        return sprite;
    }

    private HUDSlot FindCurrentlyEquippedSlot(BasicAbility.abilityType type)
    {
        if (abilitySlot1.IsAssigned(type)) return abilitySlot1;
        if (abilitySlot2.IsAssigned(type)) return abilitySlot2;
        if (abilitySlot3.IsAssigned(type)) return abilitySlot3;
        return null;
    }

    public void AbilityInUse(BasicAbility.abilityType type)
    {
        print("called this");
        HUDSlot slotToCall = FindCurrentlyEquippedSlot(type);
        if (slotToCall)
            slotToCall.ChangeBorderOpacity(true);
    }

    public void AbilityNotInUse(BasicAbility.abilityType type)
    {
        HUDSlot slotToCall = FindCurrentlyEquippedSlot(type);
        if (slotToCall)
            slotToCall.ChangeBorderOpacity(false);
    }

    public void AbilityImmediateUse(BasicAbility.abilityType type)
    {
        HUDSlot slotToCall = FindCurrentlyEquippedSlot(type);
        if (slotToCall)
            slotToCall.FlashBorderOpacity();
    }

    public void UpdateFPS()
    {
        fps++;
        timer -= 1f * Time.deltaTime;
        if (timer <= 0)
        {
            fpsCounter.text = fps.ToString();
            timer = 1f;
            fps = 0;
        }
    }
}
