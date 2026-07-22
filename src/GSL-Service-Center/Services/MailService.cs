using System.Diagnostics;
using System.Text;

namespace GSL_Rettungsring.Services;

internal sealed class MailService
{
    public bool IsClassicOutlookAvailable()
    {
        return Type.GetTypeFromProgID("Outlook.Application") is not null;
    }

    public void OpenSupportMail(string? attachmentPath = null)
    {
        var subject = $"{AppConstants.MailSubjectPrefix} - {Environment.MachineName}";
        var body = BuildBody();

        try
        {
            OpenClassicOutlookMail(subject, body, attachmentPath);
        }
        catch
        {
            OpenMailToFallback(subject, body);
        }
    }

    private static string BuildBody()
    {
        var builder = new StringBuilder();

        builder.AppendLine("Hallo GSL-Support,");
        builder.AppendLine();
        builder.AppendLine("bitte unterstützen Sie mich bei folgendem Anliegen:");
        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine($"Hostname: {Environment.MachineName}");
        builder.AppendLine($"Benutzer: {Environment.UserDomainName}\\{Environment.UserName}");

        return builder.ToString();
    }

    private static void OpenClassicOutlookMail(string subject, string body, string? attachmentPath)
    {
        var outlookType = Type.GetTypeFromProgID("Outlook.Application")
            ?? throw new InvalidOperationException("Classic Outlook ist nicht verfügbar.");

        dynamic outlook = Activator.CreateInstance(outlookType)
            ?? throw new InvalidOperationException("Classic Outlook konnte nicht gestartet werden.");

        dynamic mail = outlook.CreateItem(0);
        mail.To = AppConstants.SupportEmail;
        mail.Subject = subject;
        mail.Body = body;

        if (!string.IsNullOrWhiteSpace(attachmentPath) && File.Exists(attachmentPath))
        {
            mail.Attachments.Add(attachmentPath);
        }

        mail.Display(false);
    }

    private static void OpenMailToFallback(string subject, string body)
    {
        var url =
            $"mailto:{AppConstants.SupportEmail}" +
            $"?subject={EscapeMailToValue(subject)}" +
            $"&body={EscapeMailToValue(body)}";

        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    private static string EscapeMailToValue(string value)
    {
        return Uri.EscapeDataString(value)
            .Replace("%0A", "%0D%0A");
    }
}
