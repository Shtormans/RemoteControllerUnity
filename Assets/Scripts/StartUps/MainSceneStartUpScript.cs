using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private UdpController _udpController;

    private int _devicesAmount;
    private List<DeviceView> _deviceViews = new();

    public string MainDeviceId { get; private set; }

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

            if (model.IsCurrentDevice)
            {
                MainDeviceId = model.Id;
                FirebaseRepository.Instance.SendCommand("", MainDeviceId);
            }

            deviceView.Init(model, _udpController, this);
            _deviceViews.Add(deviceView);
        }

        _devicesAmount = deviceModels.Count;


        StartCoroutine(CheckForDevicesUpdate());
    }

    private IEnumerator CheckForDevicesUpdate()
    {
        while (true)
        {
            yield return FirebaseRepository.Instance.GetUserDevicesCoroutine(UpdateDevices);

            yield return new WaitForSeconds(1);
        }
    }

    private void UpdateDevices(List<DeviceModel> devices)
    {
        _devicesAmount = devices.Count;

        if (_deviceViews.Count == _devicesAmount)
        {
            foreach (DeviceModel model in devices)
            {
                DeviceView view = _deviceViews.First(view => view.Id == model.Id);

                if (view.Name != model.Name)
                {
                    view.SetName(model.Name);
                }
            }

            return;
        }

        if (devices.Count > _devicesAmount)
        {
            DeviceModel newModel = devices.First(model => _deviceViews.Count(view => view.Id == model.Id) == 0);

            DeviceView deviceView = Instantiate(_prefab, _devicesParent.transform);
            deviceView.Init(newModel, _udpController, this);

            _deviceViews.Add(deviceView);

            return;
        }

        if (devices.Count < _devicesAmount)
        {
            DeviceView deviceView = _deviceViews.First(view => devices.Count(model => model.Id == view.Id) == 0);

            _deviceViews.Remove(deviceView);
            Destroy(deviceView.gameObject);

            return;
        }
    }
}
