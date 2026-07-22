using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GSL_Rettungsring.Services;

internal sealed class NewsService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(8)
    };

    private readonly string _markdownUrl;

    public NewsService(bool isDevMode)
    {
        IsDevMode = isDevMode;
        _markdownUrl = isDevMode ? AppConstants.NewsDevMarkdownUrl : AppConstants.NewsMarkdownUrl;
        CurrentNews = NewsContent.Default;
    }

    public bool IsDevMode { get; }

    public NewsContent CurrentNews { get; private set; }

    public event EventHandler<NewsContent>? NewsUpdated;

    public async Task<bool> RefreshAsync(bool forceRefresh = false)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, BuildRequestUrl(forceRefresh));
            request.Headers.UserAgent.ParseAdd("GSL-Service-Center/" + AppConstants.AppVersion);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true
            };
            request.Headers.Pragma.ParseAdd("no-cache");

            using var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                return HandleRefreshFailure();
            }

            var payload = await response.Content.ReadAsStringAsync();
            var markdown = ExtractMarkdownFromGitHubContentsResponse(payload);
            if (string.IsNullOrWhiteSpace(markdown))
            {
                return HandleRefreshFailure();
            }

            var parsed = NewsContent.FromMarkdown(markdown);
            if (!parsed.HasBody)
            {
                return HandleRefreshFailure();
            }

            CurrentNews = parsed;
            NewsUpdated?.Invoke(this, CurrentNews);
            return true;
        }
        catch
        {
            return HandleRefreshFailure();
        }
    }

    private bool HandleRefreshFailure()
    {
        if (IsDevMode)
        {
            CurrentNews = NewsContent.DevError;
            NewsUpdated?.Invoke(this, CurrentNews);
        }

        // Im Kundenbetrieb bleibt der eingebaute Standardtext sichtbar.
        return false;
    }

    private string BuildRequestUrl(bool forceRefresh)
    {
        if (!forceRefresh)
        {
            return _markdownUrl;
        }

        var separator = _markdownUrl.Contains('?') ? "&" : "?";
        return _markdownUrl + separator + "nocache=" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    private static string? ExtractMarkdownFromGitHubContentsResponse(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!root.TryGetProperty("content", out var contentElement))
        {
            return null;
        }

        var content = contentElement.GetString();
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var encoding = root.TryGetProperty("encoding", out var encodingElement)
            ? encodingElement.GetString()
            : null;

        if (string.Equals(encoding, "base64", StringComparison.OrdinalIgnoreCase))
        {
            var cleanBase64 = Regex.Replace(content, @"\s+", string.Empty);
            var bytes = Convert.FromBase64String(cleanBase64);
            return Encoding.UTF8.GetString(bytes);
        }

        return content;
    }
}

internal sealed class NewsContent
{
    private const int MaxVisibleContentLines = 6;

    public static NewsContent Default { get; } = new(
        "Aktuelles von GSL",
        "Microsoft 365 Backup: Daten zusätzlich absichern\n" +
        "• Security Awareness: Mitarbeitende gegen Phishing schulen\n" +
        "• CloudProtect: Erweiterter Schutz für Microsoft 365\n" +
        "• Managed Monitoring: Systeme proaktiv überwachen",
        AppConstants.NewsUrl,
        "Mehr erfahren");

    public static NewsContent DevError { get; } = new(
        "DEV-News konnten nicht geladen werden",
        "Die Datei news_dev.md konnte nicht von GitHub abgerufen werden.\n" +
        "Bitte Internetverbindung, Repository und Dateipfad prüfen.",
        string.Empty,
        string.Empty);

    private NewsContent(string title, string body, string moreUrl, string buttonText)
    {
        Title = title;
        Body = body;
        MoreUrl = moreUrl;
        ButtonText = buttonText;
    }

    public string Title { get; }

    public string Body { get; }

    public string MoreUrl { get; }

    public string ButtonText { get; }

    public bool HasBody => !string.IsNullOrWhiteSpace(Body);

    public static NewsContent FromMarkdown(string markdown)
    {
        var normalized = markdown.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var body = normalized;

        if (normalized.StartsWith("---\n", StringComparison.Ordinal))
        {
            var endIndex = normalized.IndexOf("\n---", 4, StringComparison.Ordinal);
            if (endIndex > 0)
            {
                var metaBlock = normalized[4..endIndex];
                body = normalized[(endIndex + 4)..].TrimStart('\n').Trim();

                foreach (var line in metaBlock.Split('\n'))
                {
                    var separatorIndex = line.IndexOf(':');
                    if (separatorIndex <= 0)
                    {
                        continue;
                    }

                    var key = line[..separatorIndex].Trim();
                    var value = line[(separatorIndex + 1)..].Trim().Trim('"');
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        metadata[key] = value;
                    }
                }
            }
        }

        var title = ReadValue(metadata, "title", Default.Title);
        var moreUrl = ReadValue(metadata, "moreUrl", Default.MoreUrl);
        var buttonText = ReadValue(metadata, "buttonText", Default.ButtonText);
        var renderedBody = RenderBody(body);

        return new NewsContent(title, renderedBody, moreUrl, buttonText);
    }

    private static string ReadValue(Dictionary<string, string> metadata, string key, string fallback)
    {
        return metadata.TryGetValue(key, out var value) ? value.Trim() : fallback;
    }

    private static string RenderBody(string markdownBody)
    {
        // Erlaubt bewusst einfache manuelle Umbrueche in der News-MD:
        // <br>, <br/> und <br /> werden wie echte Zeilenumbrueche behandelt.
        // Steht ein <br> direkt am Anfang des MD-Inhalts, erzeugt es bewusst
        // Abstand zwischen Frontmatter-title und dem ersten Inhaltstext.
        var allowLeadingBlank = Regex.IsMatch(
            markdownBody.TrimStart(),
            @"^(<br\s*/?>\s*)+",
            RegexOptions.IgnoreCase);

        var normalizedBody = Regex.Replace(
            markdownBody,
            @"<br\s*/?>",
            "\n",
            RegexOptions.IgnoreCase);

        var renderedLines = new List<string>();
        var visibleContentLines = 0;
        var previousWasBlank = false;

        foreach (var rawLine in normalizedBody.Split('\n'))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                if ((renderedLines.Count > 0 || allowLeadingBlank) && !previousWasBlank)
                {
                    renderedLines.Add("[blank]");
                    previousWasBlank = true;
                }

                allowLeadingBlank = false;
                continue;
            }

            allowLeadingBlank = false;
            previousWasBlank = false;

            if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                continue;
            }

            var renderedLine = line.StartsWith("## ", StringComparison.Ordinal)
                ? "[heading]" + StripInlineMarkdown(line[3..])
                : line.StartsWith("### ", StringComparison.Ordinal)
                    ? "[heading]" + StripInlineMarkdown(line[4..])
                    : line.StartsWith("- ", StringComparison.Ordinal) || line.StartsWith("* ", StringComparison.Ordinal)
                        ? "[bullet]" + StripInlineMarkdown(line[2..])
                        : "[text]" + StripInlineMarkdown(line);

            visibleContentLines++;
            if (visibleContentLines > MaxVisibleContentLines)
            {
                while (renderedLines.Count > 0 && renderedLines[^1] == "[blank]")
                {
                    renderedLines.RemoveAt(renderedLines.Count - 1);
                }

                if (renderedLines.Count > 0)
                {
                    renderedLines[^1] = renderedLines[^1] + " …";
                }

                break;
            }

            renderedLines.Add(renderedLine);
        }

        while (renderedLines.Count > 0 && renderedLines[^1] == "[blank]")
        {
            renderedLines.RemoveAt(renderedLines.Count - 1);
        }

        return string.Join(Environment.NewLine, renderedLines);
    }

    private static string StripInlineMarkdown(string value)
    {
        var result = value.Trim();
        result = Regex.Replace(result, @"\[([^\]]+)\]\(([^\)]+)\)", "$1");
        result = result.Replace("**", string.Empty).Replace("__", string.Empty).Replace("`", string.Empty);
        return result;
    }
}
