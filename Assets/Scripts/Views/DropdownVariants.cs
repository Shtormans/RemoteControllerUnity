using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownVariants : MonoBehaviour, IDragHandler
{
    public void Disable()
    {
        PopupManager.PopupCover.Disable();
        gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
    }
}
