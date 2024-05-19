using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RenameDevicePopup : PopupMover
{
    [SerializeField] private TMP_InputField _inputField;
    private string _deviceId;

    public void Init(string deviceId, string deviceName)
    {
        PopupManager.PopupCover.Enable(gameObject);
        gameObject.SetActive(true);

        _inputField.text = deviceName;
        _deviceId = deviceId;
    }

    public void Save()
    {
        StartCoroutine(SaveCoroutine());
    }

    private IEnumerator SaveCoroutine()
    {
        yield return FirebaseRepository.Instance.RenameDeviceCoroutine(_deviceId, _inputField.text);

        PopupManager.PopupCover.Disable();
    }
}
