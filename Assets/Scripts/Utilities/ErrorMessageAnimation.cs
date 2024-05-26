using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ErrorMessageAnimation : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _text;

    [ContextMenu("ShowErrorMessage")]
    public void ShowErrorMessage(string message)
    {
        _text.text = message;
        _image.DOFillAmount(1, 0.2f).SetEase(Ease.Linear);
    }

    [ContextMenu("HideErrorMessage")]
    public void HideErrorMessage()
    {
        _image.DOFillAmount(0, 0.2f).SetEase(Ease.Linear);
    }
}
