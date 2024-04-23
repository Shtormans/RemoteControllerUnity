using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUpScript : MonoBehaviour
{
    [SerializeField] private Vector2Int _startUpScreenResolution = new(400, 500);

    void Start()
    {
        Application.targetFrameRate = 60;
        
        Screen.SetResolution(_startUpScreenResolution.x, _startUpScreenResolution.y, false);
    }
}
