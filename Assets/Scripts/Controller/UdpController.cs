using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class UdpController : MonoBehaviour
{
    enum PressedStatus : byte
    {
        None = 0,
        Pressed = 1,
        Released = 2
    }

    private UdpModel _receiver;
    private UdpModel _sender;
    private UdpClient _udpReceiverClient;
    private UdpClient _udpSenderClient;

    static int[] ports = new int[]
    {
      5283,
      5284,
      5285,
      5286,
      5287,
      5288,
      5289,
      5290,
      5291,
      5292,
      5293,
      5294,
      5295,
      5296,
      5297
    };

    public UdpModel GetModel()
    {
        if (_receiver != null && _receiver.InUse)
        {
            return _receiver;
        }

        _udpReceiverClient = GetUDPClientFromPorts(out string localReceiverIp, out int localReceiverPort, out string externalReceiverIp, out int externalReceiverPort);
        _receiver = new UdpModel()
        {
            Ip = localReceiverIp,
            Port = localReceiverPort,
            InUse = true
        };

        _udpSenderClient = GetUDPClientFromPorts(out string localSenderIp, out int localSenderPort, out string externalSenderIp, out int externalSenderPort);
        _sender = new()
        {
            Ip = localSenderIp,
            Port = localSenderPort,
            InUse = true
        };

        return _receiver;
    }

    private void OnDisable()
    {
        ClosePorts();
    }

    public void ClosePorts()
    {
        _receiver.InUse = false;
        _sender.InUse = false;
    }

    public async Task SendKeys(Vector2 mousePosition, UdpModel other)
    {
        byte leftMousePressedStatus = (byte)PressedStatus.None;
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            leftMousePressedStatus = (byte)PressedStatus.Pressed;
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            leftMousePressedStatus = (byte)PressedStatus.Released;
        }

        byte rightMousePressedStatus = (byte)PressedStatus.None;
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            rightMousePressedStatus = (byte)PressedStatus.Pressed;
        }
        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            rightMousePressedStatus = (byte)PressedStatus.Released;
        }

        byte scroll = (byte)(Mouse.current.scroll.ReadValue().y / 120);

        byte[] bytes = new byte[sizeof(int) * 3 + 5];

        Array.Copy(BitConverter.GetBytes((int)mousePosition.x), 0, bytes, 0, sizeof(int));
        Array.Copy(BitConverter.GetBytes((int)mousePosition.y), 0, bytes, sizeof(int), sizeof(int));

        bytes[sizeof(int) * 3 + 2] = leftMousePressedStatus;
        bytes[sizeof(int) * 3 + 3] = rightMousePressedStatus;
        bytes[sizeof(int) * 3 + 4] = scroll;


        Key keyCode = Key.None;
        PressedStatus keyPressedStatus = PressedStatus.None;

        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            foreach (var key in Keyboard.current.allKeys)
            {
                if (key.wasPressedThisFrame)
                {
                    keyCode = key.keyCode;
                    keyPressedStatus = PressedStatus.Pressed;
                }
            }
        }

        if (Keyboard.current.anyKey.wasReleasedThisFrame)
        {
            foreach (var key in Keyboard.current.allKeys)
            {
                if (key.wasReleasedThisFrame)
                {
                    keyCode = key.keyCode;
                    keyPressedStatus = PressedStatus.Released;
                }
            }
        }

        Array.Copy(BitConverter.GetBytes((int)keyCode), 0, bytes, sizeof(int) * 2, sizeof(int));
        bytes[sizeof(int) * 3 + 1] = (byte)keyPressedStatus;

        await _udpSenderClient.SendAsync(bytes, bytes.Length, other.Ip, other.Port);
    }

    public void ReceiveKeys()
    {
        IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        Byte[] receiveBytes = _udpReceiverClient.Receive(ref remoteIpEndPoint);

        Vector2Int mousePosition = new()
        {
            x = BitConverter.ToInt32(receiveBytes, 0),
            y = BitConverter.ToInt32(receiveBytes, sizeof(int))
        };

        MouseImpersonator.SetCursorPos(mousePosition.x, mousePosition.y);

        Key key = (Key)BitConverter.ToInt32(receiveBytes, sizeof(int) * 2);
        PressedStatus keyPressedStatus = (PressedStatus)receiveBytes[sizeof(int) * 3 + 1];

        PressedStatus leftButtonStatus = (PressedStatus)receiveBytes[sizeof(int) * 3 + 2];
        PressedStatus rightButtonStatus = (PressedStatus)receiveBytes[sizeof(int) * 3 + 3];
        byte scroll = receiveBytes[sizeof(int) * 3 + 4];

        if (keyPressedStatus == PressedStatus.Pressed)
        {
            MouseImpersonator.SimualteKeyboardPress(key);
        }
        else if (keyPressedStatus == PressedStatus.Pressed)
        {
            MouseImpersonator.SimualteKeyboardRelease(key);
        }

        switch (leftButtonStatus)
        {
            case PressedStatus.Pressed:
                MouseImpersonator.SimualteMousePress(0);
                break;
            case PressedStatus.Released:
                MouseImpersonator.SimualteMouseRelease(0);
                break;
        }
        switch (rightButtonStatus)
        {
            case PressedStatus.Pressed:
                MouseImpersonator.SimualteMousePress(1);
                break;
            case PressedStatus.Released:
                MouseImpersonator.SimualteMouseRelease(1);
                break;
        }
        
        MouseImpersonator.Scroll(scroll);
    }

    public void SendImage(byte[] bytes, UdpModel other)
    {
        const int chunkSize = 65500;
        int chunks = bytes.Length / (chunkSize - sizeof(int));

        int remainder = bytes.Length % (chunkSize - sizeof(int)) + sizeof(int);

        byte[] initialSend = new byte[5];
        initialSend[0] = (byte)(chunks + (remainder != 0 ? 1 : 0));

        Array.Copy(BitConverter.GetBytes(bytes.Length), 0, initialSend, 1, sizeof(int));

        _udpSenderClient.Send(initialSend, initialSend.Length, other.Ip, other.Port);

        byte[] chunk = new byte[chunkSize];
        Array.Copy(BitConverter.GetBytes(chunk.Length), 0, chunk, 0, sizeof(int));

        for (int i = 0; i < chunks; i++)
        {
            Array.Copy(bytes, (chunkSize - sizeof(int)) * i, chunk, sizeof(int), chunkSize - sizeof(int));

            _udpSenderClient.Send(chunk, chunkSize, other.Ip, other.Port);

            Thread.Sleep(5);
        }

        if (remainder != 0)
        {
            Array.Copy(BitConverter.GetBytes(remainder), 0, chunk, 0, sizeof(int));
            Array.Copy(bytes, (chunkSize - sizeof(int)) * chunks, chunk, sizeof(int), remainder - sizeof(int));

            _udpSenderClient.Send(chunk, remainder, other.Ip, other.Port);
        }

        Thread.Sleep(5);

    }

    public byte[] ReceiveImage()
    {
        try
        {
            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            byte[] receiveBytes = null;
            do
            {
                receiveBytes = _udpReceiverClient.Receive(ref remoteIpEndPoint);
            } while (receiveBytes.Length != 5);

            int chunksAmount = receiveBytes[0];
            int bufferSize = BitConverter.ToInt32(receiveBytes, 1);

            byte[][] chunks = new byte[chunksAmount][];

            for (int i = 0; i < chunksAmount; i++)
            {
                receiveBytes = _udpReceiverClient.Receive(ref remoteIpEndPoint);
                chunks[i] = receiveBytes;

                int chunkSize = BitConverter.ToInt32(receiveBytes, 0);
                if (receiveBytes.Length != chunkSize)
                {
                    return null;
                }
            }


            byte[] buffer = new byte[bufferSize];

            int index = 0;
            for (int i = 0; i < chunksAmount; i++)
            {
                receiveBytes = chunks[i];

                Array.Copy(receiveBytes, sizeof(int), buffer, index, receiveBytes.Length - sizeof(int));
                index += receiveBytes.Length - sizeof(int);
            }

            return buffer;
        }
        catch (Exception ex)
        {
            int i = 0;
            return null;
        }
    }

    private static UdpClient GetUDPClientFromPorts(out string localIp, out int localPort, out string externalIp, out int externalPort)
    {
        localIp = GetLocalIp();
        //externalIp = GetExternalIp();
        externalIp = "";

        IPAddress localAddr = IPAddress.Parse(localIp);
        int workingPort = -1;
        for (int i = 0; i < ports.Length; i++)
        {
            try
            {
                // You can alternatively test tcp with  nc -vz externalip 5293 in linux and
                // udp with  nc -vz -u externalip 5293 in linux
                UdpClient testUdpClient = new UdpClient(ports[i]);
                testUdpClient.Close();
                workingPort = ports[i];
                break;
            }
            catch
            {
                // Binding failed, port is in use, try next one
            }
        }


        if (workingPort == -1)
        {
            throw new Exception("Failed to connect to a port");
        }


        localPort = workingPort;

        // You could try a different external port if the below code doesn't work
        externalPort = workingPort;


        // Make a UDP Client that will use that port
        UdpClient udpClient = new UdpClient(localPort);
        return udpClient;
    }

    static string GetExternalIp()
    {
        for (int i = 0; i < 2; i++)
        {
            string res = GetExternalIpWithTimeout(400);
            if (res != "")
            {
                return res;
            }
        }
        throw new Exception("Failed to get external IP");
    }

    static string GetExternalIpWithTimeout(int timeoutMillis)
    {
        string[] sites = new string[] {
          "http://ipinfo.io/ip",
          "http://icanhazip.com/",
          "http://ipof.in/txt",
          "http://ifconfig.me/ip",
          "http://ipecho.net/plain"
        };

        foreach (string site in sites)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(site);
                request.Timeout = timeoutMillis;

                using var webResponse = (HttpWebResponse)request.GetResponse();
                using Stream responseStream = webResponse.GetResponseStream();
                using StreamReader responseReader = new StreamReader(responseStream, Encoding.UTF8);

                return responseReader.ReadToEnd().Trim();
            }
            catch
            {
                continue;
            }
        }

        return "";
    }

    static string GetLocalIp()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        throw new Exception("Failed to get local IP");
    }
}