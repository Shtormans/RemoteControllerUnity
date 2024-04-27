using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorMessageAnimation : MonoBehaviour
{
    [SerializeField] private Image _image;

    [ContextMenu("ShowErrorMessage")]
    public void ShowErrorMessage()
    {
        _image.DOFillAmount(1, 0.7f).SetEase(Ease.Linear);
    }

    [ContextMenu("HideErrorMessage")]
    public void HideErrorMessage()
    {
        _image.DOFillAmount(0, 0.7f).SetEase(Ease.Linear);
    }
}
