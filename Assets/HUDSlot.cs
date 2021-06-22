using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDSlot : MonoBehaviour
{
    [SerializeField] bool isAbility = true;
    [SerializeField] Image slotBorder;
    static Color levelOne = new Color(255, 255, 255);
    static Color levelTwo = new Color(0, 255, 0);
    static Color levelThree = new Color(0, 50, 234);
    static Color levelFour = new Color(186, 0, 255);
    Image sprite;
    BasicAbility.abilityType abilityType;
    float abilityTime;
    bool ableToUse;
    float currentOpacity;
    float minOpacity;
    bool atFullOpacity = false;
    bool atMinimumOpacity = true;

    const float timeToFullOpacity = 0.05f;
    const float timeToMinOpacity = 0.25f;

    private void Start()
    {
        minOpacity = slotBorder.color.a;
    }

    /// <summary>
    /// If the border's color is not at its minimum opacity or at full opacity then start the corutine to make it 
    /// more opaque or less depending on if the ability is in use or not
    /// </summary>
    public void ChangeBorderOpacity(bool inUse)
    {
        if (!atFullOpacity || !atMinimumOpacity)
        {
            StopAllCoroutines();
            StartCoroutine(FlashBorder(inUse, true));
        }
    }

    public void FlashBorderOpacity()
    {
        if (!atFullOpacity || !atMinimumOpacity)
        {
            StopAllCoroutines();
            StartCoroutine(FlashBorder(true, false));
        }
    }

    IEnumerator FlashBorder(bool increasing, bool persist)
    {
        Color imageColor = slotBorder.color;
        currentOpacity = imageColor.a;
        float timeLeft = increasing ? timeToFullOpacity : timeToMinOpacity;
        float increasePerInterval;
        if (increasing)
            increasePerInterval = (1f - currentOpacity) / timeToFullOpacity;
        else
            increasePerInterval = (currentOpacity - minOpacity) / timeToMinOpacity;
        for (float i = 0; i < timeLeft; i += Time.unscaledDeltaTime)
        {
            yield return new WaitForSecondsRealtime(0.01f);
            if (increasing)
                imageColor.a += Time.unscaledDeltaTime * increasePerInterval;
            else
                imageColor.a -= Time.unscaledDeltaTime * increasePerInterval;
            slotBorder.color = imageColor;
            if ((imageColor.a >= 1 && increasing) || (imageColor.a <= minOpacity && !increasing))
                i = timeLeft;
        }
        if (increasing)
        {
            atFullOpacity = true;
            atMinimumOpacity = false;
        }
        else
        {
            atMinimumOpacity = true;
            atFullOpacity = false;
        }

        if (!persist) 
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(FlashBorder(false, true));
        }
    }

    public void UpdateAbilityTime(float timeLeft)
    {
        abilityTime = timeLeft;
    }

    public bool IsAssigned(BasicAbility.abilityType type)
    {
        return abilityType == type;
    }

    public void AssignAbility(Image sprite, BasicAbility.abilityType type)
    {
        print(sprite.name);
        sprite.transform.position = transform.position;
        if (sprite != null)
            sprite.gameObject.SetActive(true);
        this.sprite = sprite;
        abilityType = type;
    }

    public void UnassignAbility() 
    {
        sprite.gameObject.SetActive(false);
        sprite = null;
        abilityTime = 0f;
        ableToUse = false;
    }

    public void AssignWeapon(Image sprite, int level)
    {
        sprite.transform.position = transform.position;
        sprite.gameObject.SetActive(true);
        this.sprite = sprite;
        ChangeBorderColor(level);
    }

    public void UnassignWeapon()
    {
        sprite.gameObject.SetActive(false);
        sprite = null;
        ChangeBorderColor(1);
    }

    public void ChangeBorderColor(int level)
    {
        float savedAlpha = slotBorder.color.a;
        switch(level)
        {
            case 1:
                levelOne.a = savedAlpha;
                slotBorder.color = levelOne;
                break;
            case 2:
                levelTwo.a = savedAlpha;
                slotBorder.color = levelTwo;
                break;
            case 3:
                levelThree.a = savedAlpha;
                slotBorder.color = levelThree;
                break;
            case 4:
                levelFour.a = savedAlpha;
                slotBorder.color = levelFour;
                break;
        }
        print("This color: " + slotBorder.color);
    }
}
