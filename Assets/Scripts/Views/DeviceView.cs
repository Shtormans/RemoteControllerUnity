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
    public DateTime LastUpdated;
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

    private bool _isSet = false;
    private bool _isCurrentDevice = false;

    private DatabaseReference _dbReference;
    private UdpController _udpController;
    private MainSceneStartUpScript _mainSceneStartUpScript;

    public string Id => _id;
    public string Name => _name;

    private void Awake()
    {
        _dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void Init(DeviceModel deviceModel, UdpController udpController, MainSceneStartUpScript mainSceneStartUpScript)
    {
        _id = deviceModel.Id;
        _name = deviceModel.Name;
        _nameText.text = _name;
        _mainSceneStartUpScript = mainSceneStartUpScript;
        _udpController = udpController;

        _isSet = true;

        if (deviceModel.IsCurrentDevice)
        {
            _isCurrentDevice = true;
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

    private void OnEnable()
    {
        if (_isSet)
        {
            if (_isCurrentDevice)
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
    }

    public void SetName(string newName)
    {
        _name = newName;
        _nameText.text = _name;
    }

    public void SendShutodownCommand()
    {
        StartCoroutine(SendCommand(DeviceCommandHandler.ShutdownCommand));
    }

    public void SendRestartCommand()
    {
        StartCoroutine(SendCommand(DeviceCommandHandler.RestartCommand));
    }

    public void SendCloseApplicationCommand()
    {
        StartCoroutine(SendCommand(DeviceCommandHandler.CloseApplicationCommand));
    }

    public void SendConnectAsReceiverCommand()
    {
        var model = _udpController.GetModel();
        string command = DeviceCommandHandler.CreateConnectAsReceiverCommand(model.Ip, model.Port, _mainSceneStartUpScript.MainDeviceId);

        StartCoroutine(SendCommand(command));
    }

    public void SendConnectAsControllerCommand()
    {
        var model = _udpController.GetModel();
        string command = DeviceCommandHandler.CreateConnectAsControllerCommand(model.Ip, model.Port);

        StartCoroutine(SendCommand(command));
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
        DateTime lastSeenTime = DateTime.UtcNow;

        try
        {
            lastSeenTime = DateTime.ParseExact(result.GetValue(false).ToString(), "M.d.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Can't parse: {result.GetValue(false)} to \"M.d.yyyy HH: mm:ss\" format");

            yield break;
        }

        DateTime now = DateTime.UtcNow;
        Action<DateTime> action = (value) =>
        {
            now = value;
        };

        yield return DateComparer.GetCurrentDateAndTime(action);

        TimeSpan difference = now - lastSeenTime;

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
        DateTime now = DateTime.UtcNow;
        Action<DateTime> action = (value) =>
        {
            now = value;
        };

        yield return DateComparer.GetCurrentDateAndTime(action);

        var dbTask = _dbReference.Child("Devices").Child(_id).Child("Enabled").SetValueAsync(now.ToString("M.d.yyyy HH:mm:ss"));

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

    public void RenameDevice()
    {
        PopupManager.RenameDevice.Init(Id, Name);
    }
}
