using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class Connector : MonoBehaviour
{
    [SerializeField] private UdpController _udpController;
    [SerializeField] private Canvas _mainScreen;
    [SerializeField] private Canvas _controllerScreen;
    [SerializeField] private Image _screen;

    private CancellationTokenSource _cancellationTokenSource = new();

    public UdpController UdpController => _udpController;

    public static Connector Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void StartScreenAsSender()
    {
        _mainScreen.gameObject.SetActive(false);
        _controllerScreen.gameObject.SetActive(true);
    }

    public void Exit()
    {
        _mainScreen.gameObject.SetActive(true);
        _controllerScreen.gameObject.SetActive(false);

        _cancellationTokenSource.Cancel();
    }

    public void StartConnectionAsController(string ip, int port)
    {
        UdpModel other = new UdpModel()
        {
            Ip = ip,
            Port = port
        };

        _cancellationTokenSource = new();

        StartScreenAsSender();

        ListenForImages(_cancellationTokenSource);
        SendKeys(other, _cancellationTokenSource);
    }

    public void StartConnectionAsReceiver(string ip, int port)
    {
        UdpModel other = new UdpModel()
        {
            Ip = ip,
            Port = port
        };

        _cancellationTokenSource = new();

        SendImages(other, _cancellationTokenSource);
        ReceiveKeys(_cancellationTokenSource);
    }

    private async Task ListenForImages(CancellationTokenSource cancellationToken)
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

    private async Task SendKeys(UdpModel other, CancellationTokenSource cancellationToken)
    {
    }

    private async Task SendImages(UdpModel other, CancellationTokenSource cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var bytes = ScreenCapture.CaptureScreen();
            await _udpController.SendImage(bytes, other);
        }

    }

    private async Task ReceiveKeys(CancellationTokenSource cancellationTokenSource)
    {

    }
}
