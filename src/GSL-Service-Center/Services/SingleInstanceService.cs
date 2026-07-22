using System.IO.Pipes;

namespace GSL_Rettungsring.Services;

internal static class SingleInstanceService
{
    public static bool ShouldOpenDashboard(string[] args)
    {
        return args.Any(arg => string.Equals(arg, AppConstants.DashboardArgument, StringComparison.OrdinalIgnoreCase));
    }

    public static bool ShouldUseDevNews(string[] args)
    {
        return args.Any(arg => string.Equals(arg, AppConstants.NewsDevArgument, StringComparison.OrdinalIgnoreCase));
    }

    public static void SendShowDashboardCommand()
    {
        try
        {
            using var client = new NamedPipeClientStream(".", AppConstants.IpcPipeName, PipeDirection.Out);
            client.Connect(1200);

            using var writer = new StreamWriter(client) { AutoFlush = true };
            writer.WriteLine(AppConstants.IpcShowDashboardCommand);
        }
        catch
        {
        }
    }
}
