using System.Diagnostics;
using UnityEngine;

public static class DeviceCommandHandler
{
    public const string ShutdownCommand = "shutdown";
    public const string RestartCommand = "restart";
    public const string CloseApplicationCommand = "closeApplication";
    public const string ConnectAsControllerCommand = "connectAsController";
    public const string ConnectAsReceiverCommand = "connectAsReceiver";

    public static string CreateConnectAsControllerCommand(string ip, int port)
    {
        return $"{ConnectAsControllerCommand} {ip}:{port}";
    }

    public static string CreateConnectAsReceiverCommand(string ip, int port, string mainDeviceId)
    {
        return $"{ConnectAsReceiverCommand} {ip}:{port} {mainDeviceId}";
    }

    public static void Handle(string command)
    {
        string commandPart = command.Split(' ')[0];

        if (commandPart == ShutdownCommand)
        {
            Shutdown();
        }
        else if (commandPart == RestartCommand)
        {
            Restart();
        }
        else if (commandPart == CloseApplicationCommand)
        {
            CloseApplication();
        }
        else if (commandPart == ConnectAsControllerCommand)
        {
            string[] config = command.Split(' ')[1].Split(':');
            string ip = config[0];
            int port = int.Parse(config[1]);

            Connector.Instance.StartConnectionAsController(ip, port);
        }
        else if (commandPart == ConnectAsReceiverCommand)
        {
            string[] config = command.Split(' ')[1].Split(':');
            string ip = config[0];
            int port = int.Parse(config[1]);

            string deviceId = command.Split(' ')[2];

            UdpModel udpModel = Connector.Instance.UdpController.GetModel();

            string senderCommand = CreateConnectAsControllerCommand(udpModel.Ip, udpModel.Port);
            FirebaseRepository.Instance.SendCommand(senderCommand, deviceId);

            Connector.Instance.StartConnectionAsReceiver(ip, port);
        }
    }

    private static void Shutdown()
    {
        string strCmdText = "/C shutdown /s";

        System.Diagnostics.Process.Start("CMD.exe", strCmdText);
    }

    private static void Restart()
    {
        string strCmdText = "/C shutdown /r";

        System.Diagnostics.Process.Start("CMD.exe", strCmdText);
    }

    private static void CloseApplication()
    {
        Application.Quit();
    }
}
