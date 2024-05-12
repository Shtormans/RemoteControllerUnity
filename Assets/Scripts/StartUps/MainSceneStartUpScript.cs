using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneStartUpScript : MonoBehaviour
{
    [SerializeField] private TMP_InputField _keyText;
    [SerializeField] private TMP_InputField _emailText;
    [SerializeField] private TMP_InputField _passwordText;
    [SerializeField] private DeviceView _prefab;
    [SerializeField] private GridLayoutGroup _devicesParent;

    private void Start()
    {
        if (FirebaseRepository.Instance.IsDatabaseLoaded)
        {
            InitData();
        }
        else
        {
            FirebaseRepository.Instance.DatabaseLoaded += InitData;
        }
    }

    private void InitData()
    {
        FirebaseAuth authenticationInstance = FirebaseAuth.DefaultInstance;

        _emailText.text = authenticationInstance.CurrentUser.Email;
        _passwordText.text = PlayerPrefsRepository.GetUserPassword();

        SetTempKey();

        FirebaseRepository.Instance.GetUserDevices(GenerateDevices);
    }    

    private void SetTempKey()
    {
        string tempKey = string.Join("", Guid.NewGuid().ToByteArray())[..12];
        _keyText.text = tempKey;

        FirebaseRepository.Instance.SetTempKey(tempKey);
    }

    private void GenerateDevices(List<DeviceModel> deviceModels)
    {
        foreach (DeviceModel model in deviceModels)
        {
            DeviceView deviceView = Instantiate(_prefab, _devicesParent.transform);
            deviceView.Init(model);
        }
    }
}
