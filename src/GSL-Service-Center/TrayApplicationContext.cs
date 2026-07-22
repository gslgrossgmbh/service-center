using System.IO.Pipes;
using GSL_Rettungsring.Forms;
using GSL_Rettungsring.Services;

namespace GSL_Rettungsring;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    private readonly MailService _mailService = new();
    private readonly LinkService _linkService = new();
    private readonly ScreenshotService _screenshotService = new();
    private readonly SystemInfoService _systemInfoService = new();
    private readonly NewsService _newsService;
    private readonly SynchronizationContext _uiContext;
    private readonly CancellationTokenSource _ipcCancellation = new();

    private MainPopupForm? _popupForm;

    public TrayApplicationContext(bool showDashboardOnStartup = false, bool useDevNews = false)
    {
        _uiContext = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
        _newsService = new NewsService(useDevNews);

        _contextMenu = BuildMenu();

        _notifyIcon = new NotifyIcon
        {
            Icon = IconService.GetAppIcon(),
            Text = $"{AppConstants.AppName} {AppConstants.AppVersion}",
            ContextMenuStrip = _contextMenu,
            Visible = true
        };

        _notifyIcon.MouseClick += NotifyIcon_MouseClick;

        StartIpcServer();
        _ = _newsService.RefreshAsync();

        if (showDashboardOnStartup)
        {
            BeginShowDashboard();
        }
    }

    private void NotifyIcon_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ShowDashboard();
        }
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();

        menu.Items.Add(CreateHeaderItem());
        menu.Items.Add(new ToolStripSeparator());

        menu.Items.Add("Dashboard öffnen", null, (_, _) => ShowDashboard());
        menu.Items.Add("Systeminformationen anzeigen", null, (_, _) => ShowSystemInfo());

        menu.Items.Add(new ToolStripSeparator());

        menu.Items.Add("Screenshot-Ordner öffnen", null, (_, _) => _linkService.OpenFolder(AppConstants.ScreenshotPath));

        menu.Items.Add(new ToolStripSeparator());

        menu.Items.Add("Zusammenarbeit mit dem GSL-Support", null, (_, _) => _linkService.OpenUrl(AppConstants.CollaborationInfoUrl));
        menu.Items.Add("AGB", null, (_, _) => _linkService.OpenUrl(AppConstants.TermsUrl));
        menu.Items.Add("Datenschutz", null, (_, _) => _linkService.OpenUrl(AppConstants.PrivacyUrl));

        menu.Items.Add(new ToolStripSeparator());

        menu.Items.Add("Info", null, (_, _) => ShowAbout());
        menu.Items.Add("Beenden", null, (_, _) => ExitApplication());

        return menu;
    }

    private static ToolStripMenuItem CreateHeaderItem()
    {
        return new ToolStripMenuItem($"{AppConstants.CompanyName} - {AppConstants.AppName} {AppConstants.AppVersion}")
        {
            Enabled = false,
            Font = new Font(SystemFonts.MenuFont ?? SystemFonts.DefaultFont, FontStyle.Bold)
        };
    }

    private void BeginShowDashboard()
    {
        var timer = new System.Windows.Forms.Timer
        {
            Interval = 300
        };

        timer.Tick += (_, _) =>
        {
            timer.Stop();
            timer.Dispose();
            ShowDashboard();
        };

        timer.Start();
    }

    private void StartIpcServer()
    {
        Task.Run(async () =>
        {
            while (!_ipcCancellation.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(
                        AppConstants.IpcPipeName,
                        PipeDirection.In,
                        1,
                        PipeTransmissionMode.Message,
                        PipeOptions.Asynchronous);

                    await server.WaitForConnectionAsync(_ipcCancellation.Token);

                    using var reader = new StreamReader(server);
                    var command = await reader.ReadLineAsync();

                    if (string.Equals(command, AppConstants.IpcShowDashboardCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        _uiContext.Post(_ => ShowDashboard(), null);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch
                {
                    await Task.Delay(500, _ipcCancellation.Token).ContinueWith(_ => { });
                }
            }
        }, _ipcCancellation.Token);
    }

    private void ShowDashboard()
    {
        if (_popupForm is { IsDisposed: false })
        {
            if (!_popupForm.Visible)
            {
                PositionDashboardNearTray(_popupForm);
                _popupForm.Show();
            }

            _popupForm.WindowState = FormWindowState.Normal;
            _popupForm.Activate();
            return;
        }

        _popupForm = new MainPopupForm(
            _mailService,
            _linkService,
            _screenshotService,
            _systemInfoService,
            _newsService);

        PositionDashboardNearTray(_popupForm);
        _popupForm.Show();
        _popupForm.Activate();
    }

    private static void PositionDashboardNearTray(Form form)
    {
        var workingArea = Screen.PrimaryScreen?.WorkingArea ?? Screen.GetWorkingArea(Cursor.Position);

        var x = Math.Max(workingArea.Left + 12, workingArea.Right - form.Width - 18);
        var y = Math.Max(workingArea.Top + 12, workingArea.Bottom - form.Height - 18);

        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(x, y);
    }

    private void ShowSystemInfo()
    {
        MessageBox.Show(
            _systemInfoService.GetSystemInfo(),
            "Systeminformationen",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void ShowAbout()
    {
        var devText = _newsService.IsDevMode ? "\nDEV-Modus aktiv" : string.Empty;

        MessageBox.Show(
            $"{AppConstants.AppName}\nVersion {AppConstants.AppVersion}{devText}\n\n{AppConstants.CompanyName}",
            "Info",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void ExitApplication()
    {
        _notifyIcon.Visible = false;
        _ipcCancellation.Cancel();
        _popupForm?.Close();
        _popupForm?.Dispose();
        _contextMenu.Dispose();
        _notifyIcon.Dispose();
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _ipcCancellation.Cancel();
            _ipcCancellation.Dispose();
            _popupForm?.Dispose();
            _contextMenu.Dispose();
            _notifyIcon.Dispose();
        }

        base.Dispose(disposing);
    }
}
