using System.Diagnostics;
using GSL_Rettungsring.Services;

namespace GSL_Rettungsring.Forms;

internal sealed class MainPopupForm : Form
{
    private readonly MailService _mailService;
    private readonly LinkService _linkService;
    private readonly ScreenshotService _screenshotService;
    private readonly SystemInfoService _systemInfoService;
    private readonly NewsService _newsService;

    private Label? _newsTitle;
    private RichTextBox? _newsText;
    private Button? _moreButton;
    private Button? _refreshButton;
    private string _currentMoreUrl = AppConstants.NewsUrl;

    private const int BaseClientWidth = 414;
    private const int BaseClientHeight = 546;
    private float _uiScale = 1F;

    private int ScaleValue(int value) => (int)Math.Round(value * _uiScale, MidpointRounding.AwayFromZero);
    private Size ScaleSize(int width, int height) => new(ScaleValue(width), ScaleValue(height));
    private Point ScalePoint(int x, int y) => new(ScaleValue(x), ScaleValue(y));
    private Padding ScalePadding(int all) => new(ScaleValue(all));
    private Padding ScalePadding(int left, int top, int right, int bottom) => new(ScaleValue(left), ScaleValue(top), ScaleValue(right), ScaleValue(bottom));

    public MainPopupForm(
        MailService mailService,
        LinkService linkService,
        ScreenshotService screenshotService,
        SystemInfoService systemInfoService,
        NewsService newsService)
    {
        _mailService = mailService;
        _linkService = linkService;
        _screenshotService = screenshotService;
        _systemInfoService = systemInfoService;
        _newsService = newsService;

        BuildForm();
        ApplyNews(_newsService.CurrentNews);
        _newsService.NewsUpdated += NewsService_NewsUpdated;
    }

    private float DetermineDpiScale()
    {
        try
        {
            using var graphics = CreateGraphics();
            return Math.Max(1F, graphics.DpiX / 96F);
        }
        catch
        {
            return 1F;
        }
    }

    private void EnforceDpiStableSize()
    {
        var currentScale = DetermineDpiScale();
        if (currentScale > _uiScale + 0.01F)
        {
            var ratio = currentScale / _uiScale;
            Scale(new SizeF(ratio, ratio));
            _uiScale = currentScale;
        }

        var targetClientSize = ScaleSize(BaseClientWidth, BaseClientHeight);
        if (ClientSize.Width < targetClientSize.Width || ClientSize.Height < targetClientSize.Height)
        {
            ClientSize = new Size(
                Math.Max(ClientSize.Width, targetClientSize.Width),
                Math.Max(ClientSize.Height, targetClientSize.Height));
        }

        var targetWindowSize = SizeFromClientSize(targetClientSize);
        MinimumSize = targetWindowSize;
        MaximumSize = targetWindowSize;
    }

    private void BuildForm()
    {
        _uiScale = DetermineDpiScale();

        Text = _newsService.IsDevMode ? $"{AppConstants.AppName} - DEV-Modus" : AppConstants.AppName;
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = ScaleSize(BaseClientWidth, BaseClientHeight);
        var targetWindowSize = SizeFromClientSize(ClientSize);
        MinimumSize = targetWindowSize;
        MaximumSize = targetWindowSize;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        TopMost = true;
        Icon = IconService.GetAppIcon();
        BackColor = Color.FromArgb(246, 248, 252);
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 1,
            Padding = ScalePadding(18),
            BackColor = BackColor
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, ScaleValue(124)));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, ScaleValue(178)));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildActionPanel(), 0, 1);
        root.Controls.Add(BuildNewsPanel(), 0, 2);

        Controls.Add(root);
        AddDevRefreshButtonOverlay();
    }

    private void AddDevRefreshButtonOverlay()
    {
        if (!_newsService.IsDevMode)
        {
            return;
        }

        _refreshButton = new Button
        {
            Text = "↻",
            Width = ScaleValue(34),
            Height = ScaleValue(30),
            Location = new Point(ClientSize.Width - ScaleValue(46), ScaleValue(8)),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(0, 104, 190),
            Font = new Font("Segoe UI Symbol", 14F, FontStyle.Bold, GraphicsUnit.Point),
            Cursor = Cursors.Hand,
            TabStop = false
        };

        _refreshButton.FlatAppearance.BorderColor = Color.FromArgb(172, 198, 226);
        _refreshButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(233, 243, 255);
        _refreshButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(215, 233, 255);
        _refreshButton.Click += async (_, _) => await RefreshDevNewsAsync();

        var toolTip = new ToolTip();
        toolTip.SetToolTip(_refreshButton, "DEV-News aktualisieren");

        Controls.Add(_refreshButton);
        _refreshButton.BringToFront();
    }

    private Control BuildHeader()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = BackColor
        };

        var logo = new PictureBox
        {
            Width = ScaleValue(330),
            Height = ScaleValue(92),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = IconService.GetGslLogo(),
            Location = ScalePoint(22, 6)
        };

        var client = new Label
        {
            Text = $"Client-ID: {Environment.MachineName}    Version {AppConstants.AppVersion}",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
            ForeColor = Color.FromArgb(88, 101, 122),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Location = ScalePoint(0, 101),
            Size = ScaleSize(374, 20)
        };

        panel.Controls.Add(logo);
        panel.Controls.Add(client);

        return panel;
    }

    private Control BuildActionPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            BackColor = BackColor,
            Padding = ScalePadding(0, 6, 0, 6)
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));

        panel.Controls.Add(CreateActionButton("Ticket erstellen", "", () => _mailService.OpenSupportMail()), 0, 0);
        if (_mailService.IsClassicOutlookAvailable())
        {
            panel.Controls.Add(CreateActionButton("Ticket mit Screenshot erstellen", "", OpenMailWithScreenshot), 1, 0);
        }
        else
        {
            panel.Controls.Add(CreateActionButton("Screenshot vorbereiten", "Datei anzeigen", PrepareScreenshot), 1, 0);
        }
        panel.Controls.Add(CreateActionButton("Fernwartung starten", "", () => _linkService.OpenUrl(AppConstants.RemoteSupportUrl)), 0, 1);
        panel.Controls.Add(CreateActionButton("Cloudprotect Portal", "", () => _linkService.OpenUrl(AppConstants.CloudProtectionMailPortalUrl)), 1, 1);
        panel.Controls.Add(CreateActionButton("Systeminformationen", "", ShowSystemInfo), 0, 2);
        panel.Controls.Add(CreateActionButton("Zusammenarbeit mit dem GSL-Support", "", () => _linkService.OpenUrl(AppConstants.CollaborationInfoUrl)), 1, 2);

        return panel;
    }

    private Button CreateActionButton(string title, string subtitle, Action action)
    {
        var buttonText = string.IsNullOrWhiteSpace(subtitle)
            ? title
            : $"{title}\n{subtitle}";

        var button = new Button
        {
            Text = buttonText,
            Dock = DockStyle.Fill,
            Margin = ScalePadding(5),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(22, 55, 96),
            Font = new Font("Segoe UI", 8.4F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderColor = Color.FromArgb(214, 224, 238);
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(233, 243, 255);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(215, 233, 255);

        button.Click += (_, _) => SafeRunAndHide(action);
        return button;
    }

    private Control BuildNewsPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = ScalePadding(14),
            Margin = ScalePadding(5, 8, 5, 8)
        };

        // Hidden state holder only. The visible title is rendered together with the body
        // inside the RichTextBox so both start on exactly the same left edge.
        _newsTitle = new Label
        {
            Text = "Aktuelles von GSL",
            Visible = false
        };

        _newsText = new RichTextBox
        {
            Text = string.Empty,
            Font = new Font("Segoe UI", 8.55F, FontStyle.Regular),
            ForeColor = Color.FromArgb(66, 78, 96),
            BackColor = Color.White,
            BorderStyle = BorderStyle.None,
            ReadOnly = true,
            TabStop = false,
            ShortcutsEnabled = false,
            ScrollBars = RichTextBoxScrollBars.None,
            Location = ScalePoint(14, 12),
            Size = ScaleSize(360, 142)
        };

        _moreButton = new Button
        {
            Text = "Mehr erfahren",
            Width = ScaleValue(130),
            Height = ScaleValue(30),
            Location = ScalePoint(14, 162),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 104, 190),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _moreButton.FlatAppearance.BorderSize = 0;
        _moreButton.Click += (_, _) => SafeRunAndHide(() => _linkService.OpenUrl(_currentMoreUrl));

        panel.Controls.Add(_newsText);
        panel.Controls.Add(_moreButton);


        panel.Resize += (_, _) => PositionNewsControls(panel);
        PositionNewsControls(panel);

        return panel;
    }

    private void PositionNewsControls(Panel panel)
    {
        if (_moreButton is not null)
        {
            var buttonTop = Math.Max(ScaleValue(142), panel.ClientSize.Height - _moreButton.Height - ScaleValue(18));
            _moreButton.Location = new Point(ScaleValue(14), buttonTop);
        }

        if (_newsText is not null && _moreButton is not null)
        {
            var textHeight = Math.Max(ScaleValue(78), _moreButton.Top - _newsText.Top - ScaleValue(12));
            _newsText.Size = new Size(Math.Max(ScaleValue(280), panel.ClientSize.Width - ScaleValue(28)), textHeight);
        }
    }

    private Control BuildFooter()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = BackColor
        };

        var closeButton = new Button
        {
            Text = "Schließen",
            Width = ScaleValue(94),
            Height = ScaleValue(30),
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
            Location = ScalePoint(282, 2),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(231, 236, 244),
            ForeColor = Color.FromArgb(45, 57, 75),
            Cursor = Cursors.Hand
        };
        closeButton.FlatAppearance.BorderSize = 0;
        closeButton.Click += (_, _) => HideToTray();

        panel.Controls.Add(closeButton);
        return panel;
    }

    private void NewsService_NewsUpdated(object? sender, NewsContent news)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ApplyNews(news)));
            return;
        }

        ApplyNews(news);
    }

    private void RenderNewsBody(NewsContent news)
    {
        if (_newsText is null)
        {
            return;
        }

        _newsText.SuspendLayout();
        _newsText.Clear();

        AppendNewsLine(news.Title.Trim(), new Font("Segoe UI", 10.2F, FontStyle.Bold), Color.FromArgb(21, 42, 74));

        var contentStarted = false;
        foreach (var rawLine in news.Body.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n'))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.Equals("[blank]", StringComparison.Ordinal))
            {
                // Ein fuehrendes <br> direkt nach dem Frontmatter soll bewusst
                // Abstand nach dem Haupttitel erzeugen. Deshalb nicht erst auf
                // contentStarted pruefen: Der Titel steht bereits im Textfeld.
                if (_newsText.TextLength > 0)
                {
                    _newsText.AppendText(Environment.NewLine);
                }

                continue;
            }

            var isHeading = false;
            var isBullet = false;

            if (line.StartsWith("[heading]", StringComparison.Ordinal))
            {
                isHeading = true;
                line = line[9..].Trim();
            }
            else if (line.StartsWith("[bullet]", StringComparison.Ordinal))
            {
                isBullet = true;
                line = line[8..].Trim();
            }
            else if (line.StartsWith("[text]", StringComparison.Ordinal))
            {
                line = line[6..].Trim();
            }
            else if (line.StartsWith("•", StringComparison.Ordinal))
            {
                isBullet = true;
                line = line[1..].Trim();
            }

            AppendNewsLine(
                isBullet ? "• " + line : line,
                isHeading ? new Font("Segoe UI", 8.75F, FontStyle.Bold) : new Font("Segoe UI", 8.45F, FontStyle.Regular),
                Color.FromArgb(66, 78, 96));

            contentStarted = true;
        }

        _newsText.SelectionStart = 0;
        _newsText.SelectionLength = 0;
        _newsText.ResumeLayout();
    }

    private void AppendNewsLine(string text, Font font, Color color)
    {
        if (_newsText is null || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        if (_newsText.TextLength > 0)
        {
            _newsText.AppendText(Environment.NewLine);
        }

        _newsText.SelectionIndent = 0;
        _newsText.SelectionHangingIndent = 0;
        _newsText.SelectionRightIndent = 0;
        _newsText.SelectionColor = color;
        _newsText.SelectionFont = font;
        _newsText.AppendText(text);
    }

    private void ApplyNews(NewsContent news)
    {
        if (_newsTitle is null || _newsText is null || _moreButton is null)
        {
            return;
        }

        _newsTitle.Text = news.Title;
        RenderNewsBody(news);
        _currentMoreUrl = news.MoreUrl;
        _moreButton.Text = string.IsNullOrWhiteSpace(news.ButtonText) ? "Mehr erfahren" : news.ButtonText;
        _moreButton.Visible = !string.IsNullOrWhiteSpace(_currentMoreUrl);
        _newsTitle.Refresh();
        _newsText.Refresh();
        _moreButton.Refresh();
    }

    private async Task RefreshDevNewsAsync()
    {
        if (_refreshButton is null)
        {
            return;
        }

        try
        {
            _refreshButton.Enabled = false;
            _refreshButton.Text = "…";

            var success = await _newsService.RefreshAsync(forceRefresh: true);
            ApplyNews(_newsService.CurrentNews);

            if (!_refreshButton.IsDisposed)
            {
                _refreshButton.Text = success ? "✓" : "!";
                await Task.Delay(650);
            }
        }
        finally
        {
            if (!_refreshButton.IsDisposed)
            {
                _refreshButton.Text = "⟳";
                _refreshButton.Enabled = true;
            }
        }
    }

    private void OpenMailWithScreenshot()
    {
        _screenshotService.DeleteExistingScreenshots();

        var screenshotPath = _screenshotService.CaptureAllScreens();
        _mailService.OpenSupportMail(screenshotPath);
    }

    private void ShowSystemInfo()
    {
        MessageBox.Show(
            _systemInfoService.GetSystemInfo(),
            "Systeminformationen",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void SafeRunAndHide(Action action)
    {
        try
        {
            action();
            HideToTray();
        }
        catch
        {
            MessageBox.Show(
                "Die Aktion konnte nicht ausgeführt werden.",
                AppConstants.AppName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void HideToTray()
    {
        Hide();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        EnforceDpiStableSize();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            HideToTray();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            HideToTray();
            return;
        }

        base.OnFormClosing(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _newsService.NewsUpdated -= NewsService_NewsUpdated;
        }

        base.Dispose(disposing);
    }

    private void PrepareScreenshot()
    {
        try
        {
            _screenshotService.DeleteExistingScreenshots();
            var screenshotPath = _screenshotService.CaptureAllScreens();

            MessageBox.Show(
                this,
                "Der Screenshot wurde erstellt und wird jetzt im Explorer angezeigt.",
                "Screenshot vorbereitet",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{screenshotPath}\"",
                UseShellExecute = true
            });

            Hide();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                "Der Screenshot konnte nicht erstellt werden." + Environment.NewLine + Environment.NewLine + ex.Message,
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
