using GSL_Rettungsring.Services;

namespace GSL_Rettungsring.Forms;

internal sealed class SystemInfoForm : Form
{
    public SystemInfoForm(Dictionary<string, string> systemInfo)
    {
        Text = "Systeminformationen";
        Width = 620;
        Height = 380;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Icon = IconService.GetAppIcon();

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = systemInfo.Count,
            Padding = new Padding(12),
            AutoScroll = true
        };

        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

        foreach (var item in systemInfo)
        {
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var keyLabel = new Label
            {
                Text = item.Key,
                AutoSize = true,
                Font = new Font(SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont, FontStyle.Bold),
                Padding = new Padding(0, 6, 8, 6)
            };

            var valueLabel = new Label
            {
                Text = item.Value,
                AutoSize = true,
                Padding = new Padding(0, 6, 0, 6)
            };

            table.Controls.Add(keyLabel);
            table.Controls.Add(valueLabel);
        }

        var closeButton = new Button
        {
            Text = "Schließen",
            Dock = DockStyle.Right,
            Width = 110
        };
        closeButton.Click += (_, _) => Close();

        var bottomPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            Padding = new Padding(12)
        };
        bottomPanel.Controls.Add(closeButton);

        Controls.Add(table);
        Controls.Add(bottomPanel);
    }
}
