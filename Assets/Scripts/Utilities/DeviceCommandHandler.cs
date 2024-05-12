using System.Diagnostics;

public static class DeviceCommandHandler
{
    public const string ShutdownCommand = "shutdown";

    public static void Handle(string command)
    {
        string commandPart = command.Split(' ')[0];

        if (commandPart == ShutdownCommand)
        {
            OpenCalculator();
        }
    }

    private static void OpenCalculator()
    {
        var ps = new ProcessStartInfo()
        {
            FileName = "cmd.exe",
            WindowStyle = ProcessWindowStyle.Normal,
            Arguments = @"/k start calc"
        };

        Process.Start(ps);
    }
}
