using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadsUpDisplay : MonoBehaviour
{
    [SerializeField] GameObject container;
    [SerializeField] GameObject slot1;
    GameObject equippedWeaponImage1;
    [SerializeField] GameObject slot2;
    GameObject equippedWeaponImage2;
    [SerializeField] GameObject slot3;
    GameObject equippedWeaponImage3;

    [SerializeField] GameObject[] weaponImages;

    [SerializeField] Slider healthBar;
    [SerializeField] Text ammo1;
    [SerializeField] Text ammo2;
    [SerializeField] Text ammo3;

    [SerializeField] Text fpsCounter;
    int fps;
    float timer = 1f;


    [SerializeField] Image crosshair;

    [Header("Abilities")]
    [SerializeField] GameObject abilitySlot1;
    [SerializeField] GameObject abilitySlot2;
    [SerializeField] GameObject abilitySlot3;
    GameObject AssignedSpriteSlot1;
    GameObject AssignedSpriteSlot2;
    GameObject AssignedSpriteSlot3;
    [SerializeField] GameObject blinkSprite;
    [SerializeField] GameObject autoTargetSprite;
    [SerializeField] GameObject deflectSprite;
    [SerializeField] GameObject shieldSprite;
    [SerializeField] GameObject holdSprite;
    [SerializeField] GameObject grappleSprite;
    [SerializeField] GameObject bulletTimeSprite;
    [SerializeField] GameObject enrageSprite;
    [SerializeField] GameObject pushPullSprite;

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

    public void UpdateWeaponHUD(int weaponSpot, int slot)
    {
        weaponImages[weaponSpot].SetActive(true);
        switch (slot)
        {
            case 0:
                if (equippedWeaponImage1 != null)
                {
                    equippedWeaponImage1.SetActive(false);
                    equippedWeaponImage1.transform.parent = container.transform;
                }

                equippedWeaponImage1 = weaponImages[weaponSpot];
                equippedWeaponImage1.transform.SetParent(slot1.transform);
                equippedWeaponImage1.transform.position = slot1.transform.position;
                break;
            case 1:
                if (equippedWeaponImage2 != null)
                {
                    equippedWeaponImage2.SetActive(false);
                    equippedWeaponImage2.transform.parent = container.transform;
                }

                equippedWeaponImage2 = weaponImages[weaponSpot];
                equippedWeaponImage2.transform.SetParent(slot2.transform);
                equippedWeaponImage2.transform.position = slot2.transform.position;
                break;
            case 2:
                if (equippedWeaponImage3 != null)
                {
                    equippedWeaponImage3.SetActive(false);
                    equippedWeaponImage3.transform.parent = container.transform;
                }

                equippedWeaponImage3 = weaponImages[weaponSpot];
                equippedWeaponImage3.transform.SetParent(slot3.transform);
                equippedWeaponImage3.transform.position = slot3.transform.position;
                break;
        }
    }

    public void UpdateWeaponHUD(int slot)
    {
        switch (slot)
        {
            case 0:
                equippedWeaponImage1.SetActive(false);
                equippedWeaponImage1 = null;
                break;
            case 1:
                equippedWeaponImage2.SetActive(false);
                equippedWeaponImage2 = null;
                break;
            case 2:
                equippedWeaponImage3.SetActive(false);
                equippedWeaponImage3 = null;
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
        switch(type)
        {
            case BasicAbility.abilityType.blink:
                AssignSpriteToSlot(key, blinkSprite);
                break;
            case BasicAbility.abilityType.autoTarget:
                AssignSpriteToSlot(key, autoTargetSprite);
                break;
            case BasicAbility.abilityType.deflect:
                AssignSpriteToSlot(key, deflectSprite);
                break;
            case BasicAbility.abilityType.shield:
                AssignSpriteToSlot(key, shieldSprite);
                break;
            case BasicAbility.abilityType.bulletTime:
                AssignSpriteToSlot(key, bulletTimeSprite);
                break;
            case BasicAbility.abilityType.enrage:
                AssignSpriteToSlot(key, enrageSprite);
                break;
            case BasicAbility.abilityType.grapple:
                AssignSpriteToSlot(key, grappleSprite);
                break;
            case BasicAbility.abilityType.holdPickup:
                AssignSpriteToSlot(key, holdSprite);
                break;
            case BasicAbility.abilityType.pushPull:
                AssignSpriteToSlot(key, pushPullSprite);
                break;
            default:
                Debug.Log("Unimplemented Ability");
                break;
        }
    }

    private void AssignSpriteToSlot(int key, GameObject sprite)
    {
        switch(key)
        {
            case AbilitySwitcher.SPACEINT:
                sprite.transform.position = abilitySlot1.transform.position;
                if (AssignedSpriteSlot1 != null)
                    AssignedSpriteSlot1.SetActive(false);
                AssignedSpriteSlot1 = sprite;
                break;
            case AbilitySwitcher.EINT:
                sprite.transform.position = abilitySlot2.transform.position;
                if (AssignedSpriteSlot2 != null)
                    AssignedSpriteSlot2.SetActive(false);
                AssignedSpriteSlot2 = sprite;
                break;
            case AbilitySwitcher.RMBINT:
                sprite.transform.position = abilitySlot3.transform.position;
                if (AssignedSpriteSlot3 != null)
                    AssignedSpriteSlot3.SetActive(false);
                AssignedSpriteSlot3 = sprite;
                break;
        }

        sprite.SetActive(true);
    }

    public void RemoveAbility(int key)
    {
        switch (key)
        {
            case AbilitySwitcher.SPACEINT:
                AssignedSpriteSlot1.SetActive(false);
                AssignedSpriteSlot1 = null;
                break;
            case AbilitySwitcher.EINT:
                AssignedSpriteSlot2.SetActive(false);
                AssignedSpriteSlot2 = null;
                break;
            case AbilitySwitcher.RMBINT:
                AssignedSpriteSlot3.SetActive(false);
                AssignedSpriteSlot3 = null;
                break;
        }
    }

    /// <summary>
    /// Takes in the ability type that is being used, then gets the sprite, and then the slot where the sprite is located to 
    /// enabled the white flash
    /// </summary>
    private Image FindSpriteFromType(BasicAbility.abilityType type)
    {
        GameObject sprite = null;
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

        Image flashSprite = sprite.transform.GetChild(0).GetComponent<Image>();
        return flashSprite;
    }



    public void AbilityInUse(BasicAbility.abilityType type)
    {
        Image flashSprite = FindSpriteFromType(type);
        print("Sprite: " + flashSprite.name);
        StartCoroutine(FlashAbilityImage(flashSprite, true));
    }

    public void AbilityNotInUse(BasicAbility.abilityType type)
    {
        Image flashSprite = FindSpriteFromType(type);
        print("Sprite: " + flashSprite.name);
        StartCoroutine(FlashAbilityImage(flashSprite, false));
    }

    public void AbilityImmediateUse(BasicAbility.abilityType type)
    {
        Image flashSprite = FindSpriteFromType(type);
        print("Sprite: " + flashSprite.name);
        StartCoroutine(FlashAbilityImage(flashSprite));
    }

    IEnumerator FlashAbilityImage(Image image, bool increase)
    {
        Color imageColor = image.color;
        for (float i = 0; i < 0.35f; i += Time.unscaledDeltaTime)
        {
            yield return new WaitForSecondsRealtime(0.01f);
            if (increase)
                imageColor.a += Time.unscaledDeltaTime * 2.85f;
            else
                imageColor.a -= Time.unscaledDeltaTime * 2.85f;
            image.color = imageColor;
            print(imageColor.ToString());
        }
    }

    IEnumerator FlashAbilityImage(Image image)
    {
        Color imageColor = image.color;
        for (float i = 0; i < 0.1f; i += Time.unscaledDeltaTime)
        {
            yield return new WaitForSecondsRealtime(0.01f);
            imageColor.a += Time.unscaledDeltaTime * 10;

            image.color = imageColor;
        }
        for (float i = 0; i < 0.1f; i += Time.unscaledDeltaTime)
        {
            yield return new WaitForSecondsRealtime(0.01f);
            imageColor.a -= Time.unscaledDeltaTime * 10;
            image.color = imageColor;
        }
        imageColor.a = 0;
        image.color = imageColor;
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
