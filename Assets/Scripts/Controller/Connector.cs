using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.LowLevel;
using System;

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

        SendKeys(other, _cancellationTokenSource);
        ListenForImages(_cancellationTokenSource);
    }

    public void StartConnectionAsReceiver(string ip, int port)
    {
        UdpModel other = new UdpModel()
        {
            Ip = ip,
            Port = port
        };

        _cancellationTokenSource = new();

        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            SendImages(other, _cancellationTokenSource);
        }).Start();

        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            ReceiveKeys(_cancellationTokenSource);
        }).Start();
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

            Debug.LogError(1 / (Time.time - time));
        }
    }

    private async Task SendKeys(UdpModel other, CancellationTokenSource cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Vector2 mousePosition = Mouse.current.position.value;

            mousePosition.x /= _controllerScreen.transform.localScale.x;
            mousePosition.y = (Screen.height - mousePosition.y) / _controllerScreen.transform.localScale.y;

            await _udpController.SendKeys(mousePosition, other);
        }
    }

    private void SendImages(UdpModel other, CancellationTokenSource cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var bytes = ScreenCapture.CaptureScreen();
            _udpController.SendImage(bytes, other);
        }
    }

    private void ReceiveKeys(CancellationTokenSource cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _udpController.ReceiveKeys();
        }
    }
}
