using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupCover : MonoBehaviour, IPointerClickHandler
{
    private GameObject _hideObject;

    public void Enable(GameObject hideObject)
    {
        gameObject.SetActive(true);
        _hideObject = hideObject;
    }
    
    public void Disable() 
    { 
        gameObject.SetActive(false);
        _hideObject = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        gameObject.SetActive(false);
        _hideObject?.SetActive(false);
    }
}
