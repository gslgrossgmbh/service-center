using GSL_Rettungsring.Services;

namespace GSL_Rettungsring;

internal static class Program
{
    private const string SingleInstanceMutexName = @"Global\GSL-Service-Center-SingleInstance";

    [STAThread]
    private static void Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        ApplicationConfiguration.Initialize();

        var openDashboard = SingleInstanceService.ShouldOpenDashboard(args);
        var useDevNews = SingleInstanceService.ShouldUseDevNews(args);

        using var mutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var isFirstInstance);

        if (!isFirstInstance)
        {
            if (openDashboard)
            {
                SingleInstanceService.SendShowDashboardCommand();
                return;
            }

            MessageBox.Show(
                "Das GSL Service Center läuft bereits im Hintergrund.",
                AppConstants.AppName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        Directory.CreateDirectory(AppConstants.InstallPath);
        Directory.CreateDirectory(AppConstants.ScreenshotPath);

        using var context = new TrayApplicationContext(openDashboard, useDevNews);
        Application.Run(context);
    }
}
