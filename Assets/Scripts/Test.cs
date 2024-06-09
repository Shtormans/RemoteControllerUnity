using UnityEngine;
using UnityEngine.Device;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Image _image;

    private void Update()
    {
        Vector2 mousePosition = Mouse.current.position.value;

        mousePosition.x /= _canvas.transform.localScale.x * _image.rectTransform.rect.width / _image.mainTexture.width;
        mousePosition.y = (UnityEngine.Screen.height - mousePosition.y) / _canvas.transform.localScale.y * _image.rectTransform.rect.height / _image.mainTexture.height;

        Debug.Log(mousePosition);
    }
}
