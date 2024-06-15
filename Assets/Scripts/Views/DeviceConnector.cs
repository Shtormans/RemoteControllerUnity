using Firebase.Database;
using System;
using System.Collections;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class DeviceConnector : MonoBehaviour
{
    [SerializeField] private ErrorMessageAnimation _errorMessageAnimation;
    [SerializeField] private ConnectionButtonsAnimation _connectionButtonsAnimation;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private UdpController _udpController;
    [SerializeField] private MainSceneStartUpScript _mainSceneStartUpScript;

    private string _deviceId;

    public void TryConnect()
    {
        string tempAddress = _inputField.text;

        FirebaseRepository.Instance.GetDevice(tempAddress, ValidateConnection);
    }

    public void OnInputFieldValueChanged()
    {
        if (_deviceId == null)
        {
            return;
        }

        _deviceId = null;
        _errorMessageAnimation.HideErrorMessage();
        _connectionButtonsAnimation.HideButtons();
    }

    private void ValidateConnection(Result<DeviceModel> deviceResult)
    {
        if (deviceResult.IsFailure)
        {
            _errorMessageAnimation.ShowErrorMessage("Такої адреси не існує");
            _deviceId = "";
            return;
        }

        if (deviceResult.Value.IsCurrentDevice)
        {
            _errorMessageAnimation.ShowErrorMessage("Неможливо підключитися до свого приладу");
            _deviceId = "";
            return;
        }

        _deviceId = deviceResult.Value.Id;
        _connectionButtonsAnimation.ShowButtons();
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

    private IEnumerator SendCommand(string command)
    {
        var dbTask = FirebaseDatabase.DefaultInstance.RootReference.Child("Devices").Child(_deviceId).Child("Command").SetValueAsync(command);

        yield return new WaitUntil(() => dbTask.IsCompleted);
    }
}
