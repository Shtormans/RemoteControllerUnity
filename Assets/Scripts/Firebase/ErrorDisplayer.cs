using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorDisplayer : MonoBehaviour
{
    [SerializeField] private RectTransform _panel;
    [SerializeField] private TextMeshProUGUI _errorTitle;

    public void DisplayError(Error error)
    {
        _panel.gameObject.SetActive(true);
        _errorTitle.text = error.Message;
    }
}
