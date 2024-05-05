using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDropdownButton : MonoBehaviour
{
    [SerializeField] private GameObject _variants;

    public void ShowSettings()
    {
        _variants.SetActive(true);
        PopupManager.PopupCover.Enable(_variants);
    }
}
