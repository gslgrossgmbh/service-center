namespace GSL_Rettungsring;

internal static class AppConstants
{
    public const string AppName = "GSL Service Center";
    public const string CompanyName = "GSL Computer GmbH";
    public const string AppVersion = "0.5.13";

    public const string InstallPath = @"C:\GSL-Support\GSL-Service-Center";
    public const string LogPath = @"C:\GSL-Support\GSL-Service-Center\logs";
    public const string ScreenshotPath = @"C:\GSL-Support\GSL-Service-Center\screenshots";

    public const string SupportEmail = "support@gsl-computer.de";
    public const string MailSubjectPrefix = "Supportanfrage";

    public const string RemoteSupportUrl = "https://gsl-computer.de/fernwartung";
    public const string CollaborationInfoUrl = "https://www.gsl-computer.de/gsl-support/";
    public const string NewsUrl = "https://www.gsl-computer.de/aktuelles/";
    public const string NewsMarkdownUrl = "https://api.github.com/repos/gslgrossgmbh/service-center/contents/news.md?ref=main";
    public const string NewsDevMarkdownUrl = "https://api.github.com/repos/gslgrossgmbh/service-center/contents/news_dev.md?ref=main";

    public const string CloudProtectionMailPortalUrl = "https://cloudprotect.gsl-computer.de";
    public const string WebsiteUrl = "https://www.gsl-computer.de/";
    public const string TermsUrl = "https://www.gsl-computer.de/agb/";
    public const string PrivacyUrl = "https://www.gsl-computer.de/datenschutz/";

    public const string AutostartName = "GSL Service Center";

    public const string DashboardArgument = "--dashboard";
    public const string NewsDevArgument = "--news-dev";
    public const string IpcPipeName = "GSL-Service-Center-DashboardPipe";
    public const string IpcShowDashboardCommand = "SHOW_DASHBOARD";
    public const string PublicDesktopShortcutName = "GSL Service Center.lnk";
}
