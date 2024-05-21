using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

public class UdpController : MonoBehaviour
{
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

    public async Task SendImage(byte[] bytes, UdpModel other)
    {
        const int chunkSize = 65000;
        int chunks = bytes.Length / chunkSize;

        int remainder = bytes.Length % chunkSize;

        byte[] initialSend = new byte[5];
        initialSend[0] = (byte)(chunks + (remainder != 0 ? 1 : 0));

        Array.Copy(BitConverter.GetBytes(bytes.Length), 0, initialSend, 1, 4);

        await _udpSenderClient.SendAsync(initialSend, initialSend.Length, other.Ip, other.Port);

        byte[] chunk = new byte[chunkSize + 1];
        chunk[0] = 3;
        for (int i = 0; i < chunks; i++)
        {
            Array.Copy(bytes, chunkSize * i, chunk, 1, chunkSize);

            await _udpSenderClient.SendAsync(chunk, chunkSize + 1, other.Ip, other.Port);
        }

        if (remainder != 0)
        {
            Array.Copy(bytes, chunkSize * chunks, chunk, 1, remainder);

            await _udpSenderClient.SendAsync(chunk, remainder + 1, other.Ip, other.Port);
        }
    }

    public async Task<Sprite> ReceiveImage()
    {
        Byte[] receiveBytes = (await _udpReceiverClient.ReceiveAsync()).Buffer;
        if (receiveBytes.Length != 5)
        {
            return null;
        }

        int chunks = receiveBytes[0];

        Debug.Log(chunks);

        Byte[] bytes = new Byte[BitConverter.ToInt32(receiveBytes, 1)];
        int index = 0;

        try
        {
            for (int i = 0; i < chunks; i++)
            {
                receiveBytes = (await _udpReceiverClient.ReceiveAsync()).Buffer;
                if (receiveBytes[0] != 3)
                {
                    return null;
                }

                Array.Copy(receiveBytes, 1, bytes, index, receiveBytes.Length - 1);
                //Array.Copy(receiveBytes, 0, bytes, index, receiveBytes.Length);

                index += receiveBytes.Length - 1;
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }


        Texture2D spriteTexture = new Texture2D(2, 2);
        spriteTexture.LoadImage(bytes);

        Rect rect = new(0, 0, spriteTexture.width, spriteTexture.height);
        Sprite sprite = Sprite.Create(spriteTexture, rect, Vector2.zero, 100);

        return sprite;
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
