using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainSceneStartUpScript : MonoBehaviour
{
    [SerializeField] private TMP_InputField _keyText;
    [SerializeField] private TMP_InputField _emailText;
    [SerializeField] private TMP_InputField _passwordText;

    private void Awake()
    {
        FirebaseAuth authenticationInstance = FirebaseAuth.DefaultInstance;

        _emailText.text = authenticationInstance.CurrentUser.Email;
        _passwordText.text = PlayerPrefsRepository.GetUserPassword();
    }

    private void SetTempKey()
    {
        string tempKey = Guid.NewGuid().ToString("N")[..12];
    }
}
