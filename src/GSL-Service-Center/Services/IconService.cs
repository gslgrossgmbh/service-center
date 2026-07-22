using System.Reflection;

namespace GSL_Rettungsring.Services;

internal static class IconService
{
    public static Icon GetAppIcon()
    {
        try
        {
            using var stream = OpenEmbeddedResource("AppIcon.ico");
            if (stream != null)
            {
                return new Icon(stream);
            }
        }
        catch
        {
            // Fallback unten.
        }

        return SystemIcons.Information;
    }

    public static Image? GetAppImage()
    {
        return GetEmbeddedImage("AppIcon.png");
    }

    public static Image? GetGslLogo()
    {
        return GetEmbeddedImage("GslLogo.png") ?? GetAppImage();
    }

    private static Image? GetEmbeddedImage(string logicalName)
    {
        try
        {
            using var stream = OpenEmbeddedResource(logicalName);
            if (stream == null)
            {
                return null;
            }

            using var temp = Image.FromStream(stream);
            return new Bitmap(temp);
        }
        catch
        {
            return null;
        }
    }

    private static Stream? OpenEmbeddedResource(string logicalName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(logicalName);
        if (stream == null)
        {
            return null;
        }

        var memory = new MemoryStream();
        stream.CopyTo(memory);
        memory.Position = 0;
        stream.Dispose();

        return memory;
    }
}
