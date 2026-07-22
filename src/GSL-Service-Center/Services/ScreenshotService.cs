using System.Drawing.Imaging;

namespace GSL_Rettungsring.Services;

internal sealed class ScreenshotService
{
    public string CaptureAllScreens()
    {
        Directory.CreateDirectory(AppConstants.ScreenshotPath);

        var fileName = $"support-screenshot-{Environment.MachineName}-{DateTime.Now:yyyy-MM-dd-HHmmss}.png";
        var filePath = Path.Combine(AppConstants.ScreenshotPath, fileName);

        var bounds = GetVirtualScreenBounds();

        using var bitmap = new Bitmap(bounds.Width, bounds.Height);
        using var graphics = Graphics.FromImage(bitmap);

        graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
        bitmap.Save(filePath, ImageFormat.Png);

        return filePath;
    }

    private static Rectangle GetVirtualScreenBounds()
    {
        if (Screen.AllScreens.Length == 0)
        {
            throw new InvalidOperationException("Kein Bildschirm gefunden.");
        }

        var left = Screen.AllScreens.Min(screen => screen.Bounds.Left);
        var top = Screen.AllScreens.Min(screen => screen.Bounds.Top);
        var right = Screen.AllScreens.Max(screen => screen.Bounds.Right);
        var bottom = Screen.AllScreens.Max(screen => screen.Bounds.Bottom);

        return Rectangle.FromLTRB(left, top, right, bottom);
    }
    public int DeleteExistingScreenshots()
    {
        Directory.CreateDirectory(AppConstants.ScreenshotPath);

        var deleted = 0;
        var files = Directory.GetFiles(AppConstants.ScreenshotPath, "*.png", SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            try
            {
                File.Delete(file);
                deleted++;
            }
            catch
            {
            }
        }

        return deleted;
    }

}
