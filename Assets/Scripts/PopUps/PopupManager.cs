using UnityEngine;

public class PopupManager : MonoBehaviour
{
    [SerializeField] private PopupCover _popupCover;

    public static PopupCover PopupCover => _instance._popupCover;

    private static PopupManager _instance;

    private void Awake()
    {
        _instance = this;
    }
}
