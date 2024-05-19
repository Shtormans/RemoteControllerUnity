using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownVariants : MonoBehaviour
{
    public void Disable()
    {
        PopupManager.PopupCover.Disable();
    }
}
