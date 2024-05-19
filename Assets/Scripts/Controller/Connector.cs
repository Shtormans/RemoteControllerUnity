using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Connector : MonoBehaviour
{
    [SerializeField] private UdpController _udpController;
    [SerializeField] private Image _screen;

    private CancellationTokenSource _cancellationTokenSource = new();

    [ContextMenu("StartConnection")]
    public void StartConnection()
    {
        _udpController.GetModel();

        Listen(_cancellationTokenSource);
        SendKeys(_cancellationTokenSource);
    }

    private async Task Listen(CancellationTokenSource cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            float time = Time.time;

            Sprite sprite = await _udpController.ReceiveImage();

            if (sprite != null)
            {
                _screen.sprite = sprite;
            }

            Debug.Log(1 / (Time.time - time));
        }
    }

    private async Task SendKeys(CancellationTokenSource cancellationToken)
    {
        while (true)
        {

        }
    }
}
