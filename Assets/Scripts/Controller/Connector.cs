using System;
using System.Collections;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Connector : MonoBehaviour
{
    [SerializeField] private UdpController _udpController;
    [SerializeField] private Canvas _mainScreen;
    [SerializeField] private Canvas _controllerScreen;
    [SerializeField] private Image _screen;

    private CancellationTokenSource _cancellationTokenSource = new();
    private bool _isReceiver = false;

    public UdpController UdpController => _udpController;

    public static Connector Instance { get; private set; }

    private static Mutex _mutex = new();

    private void Awake()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        _cancellationTokenSource?.Cancel();
    }

    private void OnApplicationQuit()
    {
        _cancellationTokenSource?.Cancel();
    }

    private byte[] _buffer;

    private byte[] GetSetBytes(byte[] buffer, bool set)
    {
        _mutex.WaitOne();

        if (set)
        {
            _buffer = buffer;

            _mutex.ReleaseMutex();

            return null;
        }
        else
        {
            byte[] temp = _buffer;
            _buffer = null;

            _mutex.ReleaseMutex();

            return temp;
        }
    }

    private void StartScreen()
    {
        _screen.sprite = null;

        _mainScreen.gameObject.SetActive(false);
        _controllerScreen.gameObject.SetActive(true);
    }

    public void Exit()
    {
        _mainScreen.gameObject.SetActive(true);
        _controllerScreen.gameObject.SetActive(false);

        _isReceiver = false;
        _udpController.ClosePorts();
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

        StartScreen();

        SendKeys(other, _cancellationTokenSource);
        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            ListenForImages(_cancellationTokenSource);
        }).Start();

        StartCoroutine(ConstructImages(_cancellationTokenSource));
    }

    public void StartConnectionAsReceiver(string ip, int port)
    {
        UdpModel other = new UdpModel()
        {
            Ip = ip,
            Port = port
        };

        _isReceiver = true;
        _cancellationTokenSource = new();

        StartScreen();

        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            SendImages(other, _cancellationTokenSource);
        }).Start();

        _lastReceivedKeysTime = DateTime.UtcNow;

        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            ReceiveKeys(_cancellationTokenSource);
        }).Start();
    }

    private DateTime _lastReceivedKeysTime;

    private IEnumerator ConstructImages(CancellationTokenSource cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            DateTime time = DateTime.Now;

            byte[] buffer = GetSetBytes(null, false);

            if (buffer != null)
            {
                Texture2D spriteTexture = new Texture2D(2, 2);
                spriteTexture.LoadImage(buffer);

                Rect rect = new(0, 0, spriteTexture.width, spriteTexture.height);
                Sprite sprite = Sprite.Create(spriteTexture, rect, Vector2.zero, 100);

                _screen.sprite = sprite;
            }

            yield return null;
        }
    }

    private void ListenForImages(CancellationTokenSource cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            GetSetBytes(_udpController.ReceiveImage(), true);
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

            if (_screen.sprite != null)
            {
                Vector2 mousePosition = Mouse.current.position.value;

                mousePosition.x /= _controllerScreen.transform.localScale.x * _screen.rectTransform.rect.width / _screen.mainTexture.width;
                mousePosition.y /= _controllerScreen.transform.localScale.y * _screen.rectTransform.rect.height / _screen.mainTexture.height;

                mousePosition.y = _screen.mainTexture.height - mousePosition.y;

                await _udpController.SendKeys(mousePosition, other);
            }
            else
            {
                await Task.Yield();
            }
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

            _lastReceivedKeysTime = DateTime.UtcNow;
        }
    }

    private void Update()
    {
        if (_isReceiver && (DateTime.UtcNow - _lastReceivedKeysTime).TotalSeconds > 5)
        {
            Exit();
        }
    }
}
