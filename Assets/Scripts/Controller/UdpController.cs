using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UdpController : MonoBehaviour
{
    private UdpModel _receiver;
    private UdpClient _udpClient;

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

        _udpClient = GetUDPClientFromPorts(out string localIp, out int localPort, out string externalIp, out int externalPort);

        model.Ip = localIp;
        model.Port = localPort;

        Debug.Log(localIp);
        Debug.Log(localPort);

        return model;
    }

    public void EstablishConnection(UdpModel receiver)
    {
        _receiver = receiver;
    }

    public void SendImage()
    {

    }

    public async Task<Sprite> ReceiveImage()
    {
        Byte[] receiveBytes = (await _udpClient.ReceiveAsync()).Buffer;
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
                receiveBytes = (await _udpClient.ReceiveAsync()).Buffer;
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
                Socket tempServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                tempServer.Bind(new IPEndPoint(localAddr, ports[i]));
                tempServer.Close();
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
