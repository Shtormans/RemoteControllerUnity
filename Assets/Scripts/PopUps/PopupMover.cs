using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PopupMover : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private readonly float _blindZone = 20;

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 mousePosition = Input.mousePosition;
        if (mousePosition.x < _blindZone || Screen.width - mousePosition.x < _blindZone)
        {
            return;
        }
        if (mousePosition.y < _blindZone || Screen.height - mousePosition.y < _blindZone)
        {
            return;
        }

        transform.position += (Vector3)eventData.delta;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }
}
