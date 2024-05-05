using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public enum DeviceStatus
{
    Enabled,
    Disabled,
    CurrentDevice
}

public class DeviceModel
{
    public Guid Id;
    public string Name;
    public string PublicTempKey;
    public DeviceStatus DeviceStatus;
}

public class DeviceView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private CanvasGroup _disabledGroup;
    [SerializeField] private RectTransform _buttonsPanel;
    [SerializeField] private RectTransform _yourDevicePanel;

    private Guid _id;
    private string _name;
    private string _publicTempKey;
    private DeviceStatus _status;

    private DatabaseReference _dbReference;
    private FirebaseAuth _auth;

    private void Awake()
    {
        _dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        _auth = FirebaseAuth.DefaultInstance;
    }

    public void Init(DeviceModel deviceModel)
    {
        _id = deviceModel.Id;
        _name = deviceModel.Name;
        _publicTempKey = deviceModel.PublicTempKey;
        _status = deviceModel.DeviceStatus;

        ChangeViewStatus(_status);

        if (_status != DeviceStatus.CurrentDevice)
        {
            StartCoroutine(GetDeviceStatusLoop());
        }
    }

    private void ChangeViewStatus(DeviceStatus deviceStatus)
    {
        switch (deviceStatus)
        {
            case DeviceStatus.Enabled:
                _disabledGroup.enabled = false;
                _buttonsPanel.gameObject.SetActive(true);
                _yourDevicePanel.gameObject.SetActive(false);
                break;
            case DeviceStatus.Disabled:
                _disabledGroup.enabled = true;
                _buttonsPanel.gameObject.SetActive(true);
                _yourDevicePanel.gameObject.SetActive(false);
                break;
            case DeviceStatus.CurrentDevice:
                _disabledGroup.enabled = false;
                _buttonsPanel.gameObject.SetActive(false);
                _yourDevicePanel.gameObject.SetActive(true);
                break;
        }
    }

    private IEnumerator GetDeviceStatusLoop()
    {
        while (true)
        {
            yield return GetDeviceStatus();
        }
    }

    private IEnumerator GetDeviceStatus()
    {
        var dbTask = _dbReference.Child("Devices").Child(_id.ToString()).Child("LastTimeSeen").GetValueAsync();

        yield return new WaitUntil(() => dbTask.IsCompleted);

        if (dbTask.Exception == null)
        {
            yield break;
        }

        DataSnapshot result = dbTask.Result;
        DateTime lastSeenTime = DateTime.Parse(result.GetValue(false).ToString());

        TimeSpan difference = DateTime.Now - lastSeenTime;

        if (difference.TotalSeconds > Constants.FirebaseConstants.DeviceEnabledTime)
        {
            ChangeViewStatus(DeviceStatus.Disabled);
        }
        else
        {
            ChangeViewStatus(DeviceStatus.Enabled);
        }
    }
}
