using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class PasswordToggle : MonoBehaviour
{
    [SerializeField] private TMP_InputField _passwordInputField;

    private void Start()
    {
        GetComponent<Toggle>().onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDestroy()
    {
        GetComponent<Toggle>().onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(bool value)
    {
        _passwordInputField.contentType = value ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        _passwordInputField.ForceLabelUpdate();
    }
}
