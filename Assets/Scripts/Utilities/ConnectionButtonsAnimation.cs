using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionButtonsAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform _fullConnectionButton;
    [SerializeField] private RectTransform _fastConnectionButton;

    [ContextMenu("ShowButtons")]
    public void ShowButtons()
    {
        StartCoroutine(ShowButtonsAnimation());
    }

    private IEnumerator ShowButtonsAnimation()
    {
        _fullConnectionButton.DOLocalMoveY(0, 0.7f);

        yield return null;

        _fastConnectionButton.DOLocalMoveY(0, 0.7f);

        _fullConnectionButton.DOLocalRotate(Vector3.zero, 0.2f);

        yield return null;

        _fastConnectionButton.DOLocalRotate(Vector3.zero, 0.2f);
    }
}
