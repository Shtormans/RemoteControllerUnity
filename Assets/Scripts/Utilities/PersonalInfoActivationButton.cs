using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PersonalInfoActivationButton : MonoBehaviour
{
    [SerializeField] private RectTransform _personalInfoPanel;
    [SerializeField] private float _animationTime = 2f;
    [SerializeField] private bool _isActive = false;

    private Button _self;

    private void Awake()
    {
        _self = GetComponent<Button>();
    }

    public void ActivateDeactivate()
    {
        StartCoroutine(ActivateDeactivateCoroutine(_isActive));
    }

    private IEnumerator ActivateDeactivateCoroutine(bool isActive)
    {
        _self.interactable = false;

        float xDestination = isActive ? 0 : -_personalInfoPanel.rect.width;
        _personalInfoPanel.DOLocalMoveX(xDestination, _animationTime);

        yield return new WaitForSeconds(_animationTime);

        _self.interactable = true;
        _isActive = !isActive;
    }
}
