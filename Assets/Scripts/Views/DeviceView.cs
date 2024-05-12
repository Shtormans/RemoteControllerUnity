using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;
using System.Globalization;
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
    public string Id;
    public string Name;
    public bool IsCurrentDevice;
}

public class DeviceView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private CanvasGroup _disabledGroup;
    [SerializeField] private RectTransform _buttonsPanel;
    [SerializeField] private RectTransform _yourDevicePanel;

    private string _id;
    private string _name;
    private DeviceStatus _status;

    private DatabaseReference _dbReference;

    private void Awake()
    {
        _dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void Init(DeviceModel deviceModel)
    {
        _id = deviceModel.Id;
        _name = deviceModel.Name;
        _nameText.text = _name;

        if (deviceModel.IsCurrentDevice)
        {
            _status = DeviceStatus.CurrentDevice;
            ChangeViewStatus(_status);

            StartCoroutine(CurrentDeviceStatusLoop());
        }
        else
        {
            _status = DeviceStatus.Disabled;
            ChangeViewStatus(_status);

            StartCoroutine(NotCurrentDeviceStatusLoop());
        }
    }

    public void SendShutodownCommand()
    {
        Debug.LogError("Here");
        StartCoroutine(SendCommand(DeviceCommandHandler.ShutdownCommand));
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

    private IEnumerator NotCurrentDeviceStatusLoop()
    {
        while (true)
        {
            yield return NotCurrentDeviceStatusCoroutine();

            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator CurrentDeviceStatusLoop()
    {
        while (true)
        {
            yield return CurrentDeviceStatusCoroutine();

            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator NotCurrentDeviceStatusCoroutine()
    {
        var dbTask = _dbReference.Child("Devices").Child(_id.ToString()).Child("Enabled").GetValueAsync();

        yield return new WaitUntil(() => dbTask.IsCompleted);

        DataSnapshot result = dbTask.Result;
        DateTime lastSeenTime = DateTime.ParseExact(result.GetValue(false).ToString(), "M.d.yyyy HH:mm:ss", CultureInfo.InvariantCulture);


        TimeSpan difference = DateTime.UtcNow - lastSeenTime;

        if (difference.TotalSeconds > Constants.FirebaseConstants.DeviceEnabledTime)
        {
            ChangeViewStatus(DeviceStatus.Disabled);
        }
        else
        {
            ChangeViewStatus(DeviceStatus.Enabled);
        }
    }

    private IEnumerator CurrentDeviceStatusCoroutine()
    {
        yield return UpdateLastSeenTime();

        yield return ReceiveCommand();
    }

    private IEnumerator UpdateLastSeenTime()
    {
        var dbTask = _dbReference.Child("Devices").Child(_id).Child("Enabled").SetValueAsync(DateTime.UtcNow.ToString("M.d.yyyy HH:mm:ss"));

        yield return new WaitUntil(() => dbTask.IsCompleted);
    }

    private IEnumerator ReceiveCommand()
    {
        var dbTask = _dbReference.Child("Devices").Child(_id).Child("Command").GetValueAsync();

        yield return new WaitUntil(() => dbTask.IsCompleted);

        DataSnapshot result = dbTask.Result;

        object command = result.GetValue(false);

        if (command == null || string.IsNullOrEmpty(command.ToString()))
        {
            yield break;
        }

        yield return SendCommand("");

        DeviceCommandHandler.Handle(command.ToString());
    }

    private IEnumerator SendCommand(string command)
    {
        var dbTask = _dbReference.Child("Devices").Child(_id).Child("Command").SetValueAsync(command);

        yield return new WaitUntil(() => dbTask.IsCompleted);
    }
}
