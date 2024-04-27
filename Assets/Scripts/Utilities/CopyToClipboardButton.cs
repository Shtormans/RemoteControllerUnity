using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CopyToClipboardButton : MonoBehaviour
{
    [SerializeField] private TMP_InputField _textField;

    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = _textField.text;
    }
}
