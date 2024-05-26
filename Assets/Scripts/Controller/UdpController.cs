using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

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
        UdpModel model = new();

        _udpReceiverClient?.Close();
        _udpSenderClient?.Close();

        _udpReceiverClient = GetUDPClientFromPorts(out string localReceiverIp, out int localReceiverPort, out string externalReceiverIp, out int externalReceiverPort);
        _udpSenderClient = GetUDPClientFromPorts(out string localSenderIp, out int localSenderPort, out string externalSenderIp, out int externalSenderPort);

        model.Ip = localReceiverIp;
        model.Port = localReceiverPort;

        return model;
    }

    private void OnDisable()
    {
        ClosePorts();
    }

    public void ClosePorts()
    {
        _udpReceiverClient.Close();
        _udpSenderClient.Close();
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

        byte[] bytes = new byte[sizeof(int) * 2 + 2];

        bytes[0] = 3;
        Array.Copy(BitConverter.GetBytes((int)mousePosition.x), 0, bytes, 1, sizeof(int));
        Array.Copy(BitConverter.GetBytes((int)mousePosition.y), 0, bytes, sizeof(int) + 1, sizeof(int));

        bytes[^1] = leftMousePressedStatus;

        await _udpSenderClient.SendAsync(bytes, bytes.Length, other.Ip, other.Port);
    }

    public void ReceiveKeys()
    {
        IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        Byte[] receiveBytes = _udpReceiverClient.Receive(ref remoteIpEndPoint);

        if (receiveBytes[0] != 3)
        {
            return;
        }

        Vector2Int mousePosition = new()
        {
            x = BitConverter.ToInt32(receiveBytes, 1),
            y = BitConverter.ToInt32(receiveBytes, sizeof(int) + 1)
        };

        Debug.Log(mousePosition);

        MouseImpersonator.SetCursorPos(mousePosition.x, mousePosition.y);

        PressedStatus leftMousePressedStatus = (PressedStatus)receiveBytes[^1];
        switch (leftMousePressedStatus)
        {
            case PressedStatus.None:
                break;
            case PressedStatus.Pressed:
                MouseImpersonator.Press(mousePosition.x, mousePosition.y);
                break;
            case PressedStatus.Released:
                MouseImpersonator.Release(mousePosition.x, mousePosition.y);
                break;
        }
    }

    public void SendImage(byte[] bytes, UdpModel other)
    {
        const int chunkSize = 65000;
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
        }

        if (remainder != 0)
        {
            Array.Copy(BitConverter.GetBytes(remainder), 0, chunk, 0, sizeof(int));
            Array.Copy(bytes, (chunkSize - sizeof(int)) * chunks, chunk, sizeof(int), remainder - sizeof(int));

            _udpSenderClient.Send(chunk, remainder, other.Ip, other.Port);
        }
    }

    public async Task<Sprite> ReceiveImage()
    {
        try
        {
            byte[] receiveBytes = null;
            do
            {
                receiveBytes = (await _udpReceiverClient.ReceiveAsync()).Buffer;
            } while (receiveBytes.Length != 5);

            int chunksAmount = receiveBytes[0];
            int bufferSize = BitConverter.ToInt32(receiveBytes, 1);

            byte[][] chunks = new byte[chunksAmount][];

            for (int i = 0; i < chunksAmount; i++)
            {
                receiveBytes = (await _udpReceiverClient.ReceiveAsync()).Buffer;
                chunks[i] = receiveBytes;
            }


            byte[] buffer = new byte[bufferSize];

            int index = 0;
            for (int i = 0; i < chunksAmount; i++)
            {
                receiveBytes = chunks[i];

                int chunkSize = BitConverter.ToInt32(receiveBytes, 0);
                if (receiveBytes.Length != chunkSize)
                {
                    return null;
                }

                Array.Copy(receiveBytes, sizeof(int), buffer, index, receiveBytes.Length - sizeof(int));
                index += receiveBytes.Length - sizeof(int);
            }

            Texture2D spriteTexture = new Texture2D(2, 2);
            spriteTexture.LoadImage(buffer);

            Rect rect = new(0, 0, spriteTexture.width, spriteTexture.height);
            Sprite sprite = Sprite.Create(spriteTexture, rect, Vector2.zero, 100);

            return sprite;
        }
        catch (Exception ex)
        {
            int i = 0;
            return null;
        }
    }

    static UdpClient GetUDPClientFromPorts(out string localIp, out int localPort, out string externalIp, out int externalPort)
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