using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingCircleAnimation : MonoBehaviour
{
    [SerializeField] private List<Image> _images;
    [SerializeField] private float _time;

    private void Start()
    {
        StartCoroutine(AnimationCoroutine());
    }

    private IEnumerator AnimationCoroutine()
    {
        int i = 0;
        while (true)
        {
            _images[i].color = Color.white;
            _images[i].DOFade(0, _time * (_images.Count - 1));

            i = (i + 1) % _images.Count;
            yield return new WaitForSeconds(_time);
        }
    }
}
