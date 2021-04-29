using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class ClickAndDrag : MonoBehaviour, IPointerClickHandler , IDragHandler, IPointerUpHandler
{
    public BasicAbility.abilityType type;
    static AbilitySwitcher switcherAndCanvas;
    public GameObject slot;
    Vector3 originalPos;
    public bool equipped = false;

    private void Start()
    {
        originalPos = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (switcherAndCanvas.CloseEnoughToSlot(this))
            equipped = true;
        else
        {
            transform.position = originalPos;
            if (equipped)
            {
                switcherAndCanvas.UnAssignAbility(slot);
                equipped = false;
                ForceUnequip();
            }
        }
    }

    public static void AssignSwitcher(AbilitySwitcher switcher)
    {
        switcherAndCanvas = switcher;
    }

    public void AssignSlot(GameObject canvasSlot)
    {
        slot = canvasSlot;
    }

    public void ForceUnequip()
    {
        switcherAndCanvas.UnAssignAbility(slot);
        transform.position = originalPos;
        equipped = false;
        slot = null;
    }
}
