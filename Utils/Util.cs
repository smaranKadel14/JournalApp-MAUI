using System.Net;
using System.Text.RegularExpressions;

namespace JournalApp.Utils;

public static class Util
{
    // I use this to remove HTML tags so the app can show a clean preview/snippet
    // instead of displaying raw tags like <h2> or <b>.
    public static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var noTags = Regex.Replace(html, "<.*?>", string.Empty);
        return WebUtility.HtmlDecode(noTags).Trim();
    }

    // I use this to safely force DateTime to act like "date-only" for comparisons,
    // so time portion never causes bugs.
    public static DateTime DateOnly(DateTime dt) => dt.Date;

    // I use this for entry previews (Dashboard/Entries list)
    public static string SnippetFromHtml(string html, int maxLen = 90)
    {
        var text = StripHtml(html).Replace("\r", "").Replace("\n", " ").Trim();
        return text.Length > maxLen ? text.Substring(0, maxLen) + "..." : text;
    }

    // I use this to validate rich text input properly (prevents saving "<br>" as content)
    public static bool HasRealText(string html) => !string.IsNullOrWhiteSpace(StripHtml(html));
}
