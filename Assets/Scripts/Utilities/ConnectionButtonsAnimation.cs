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

    [ContextMenu("HideButtons")]
    public void HideButtons()
    {
        StartCoroutine(HideButtonsAnimation());
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

    private IEnumerator HideButtonsAnimation()
    {
        _fastConnectionButton.DOLocalRotate(new Vector3(0, 0, -6.24f), 0.2f);

        yield return null;

        _fastConnectionButton.DOLocalMoveY(-102.1136f, 0.7f);

        _fullConnectionButton.DOLocalRotate(new Vector3(0, 0, -6.24f), 0.2f);

        yield return null;

        _fullConnectionButton.DOLocalMoveY(-102.1136f, 0.7f);
    }
}
