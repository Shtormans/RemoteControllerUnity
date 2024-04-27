using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionAnimations : MonoBehaviour
{
    [SerializeField] private Text _connectText;
    [SerializeField] private RectMask2D _connectMask;
    [SerializeField] private RectTransform _loadingPanel;
    [SerializeField] private Image _loadingImage;

    [ContextMenu("LoadingAnimation")]
    public void StartLoadingAnimation()
    {
        StartCoroutine(ShowLoadingAnimation());
    }

    [ContextMenu("Debug")]
    public void D()
    {
        Debug.Log(_loadingPanel.localPosition.x);
    }

    private IEnumerator ShowLoadingAnimation()
    {
        DOTween.To(() => _connectMask.padding.x, x =>
        {
            var padding = _connectMask.padding;
            padding.x = x;
            _connectMask.padding = padding;
        }, 144.6f, 2f).SetEase(Ease.Linear);

        _connectText.DOFade(0, 1f);

        _loadingPanel.DOLocalMoveX(-76.7f, 2);

        yield return null;
    }
}
