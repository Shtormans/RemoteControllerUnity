using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InfoSceneController : MonoBehaviour
{
    [SerializeField] private List<RectTransform> _panels;

    public UnityEvent<bool> CanMoveRight;
    public UnityEvent<bool> CanMoveLeft;

    private int _index = 0;

    public void MoveNext()
    {
        if (_index + 1 >= _panels.Count - 1)
        {
            CanMoveRight.Invoke(false);
        }
        else
        {
            CanMoveRight.Invoke(true);
        }

        _panels[_index].gameObject.SetActive(false);

        _index++;
        _panels[_index].gameObject.SetActive(true);

        CanMoveLeft.Invoke(true);
    }

    public void MovePrevious()
    {
        if (_index - 1 <= 0)
        {
            CanMoveLeft.Invoke(false);
        }
        else
        {
            CanMoveLeft.Invoke(true);
        }

        _panels[_index].gameObject.SetActive(false);

        _index--;
        _panels[_index].gameObject.SetActive(true);
        
        CanMoveRight.Invoke(true);
    }
}
