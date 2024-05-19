using UnityEngine;

public class PopupManager : MonoBehaviour
{
    [SerializeField] private PopupCover _popupCover;
    [SerializeField] private RenameDevicePopup _renameDevicePopup;

    public static PopupCover PopupCover => _instance._popupCover;
    public static RenameDevicePopup RenameDevice => _instance._renameDevicePopup;

    private static PopupManager _instance;

    private void Awake()
    {
        _instance = this;
    }
}
