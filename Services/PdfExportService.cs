using JournalApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace JournalApp.Services;

/// <summary>
/// PdfExportService:
/// - Reads journal entries from SQLite using EF Core
/// - Generates a PDF file (QuestPDF)
/// - Saves it inside the app storage folder (AppDataDirectory/Exports)
/// </summary>
public class PdfExportService
{
    private readonly AppDbContext _db;

    public PdfExportService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Export entries for one user between a date range as a single PDF.
    /// Returns the full file path of the created PDF.
    /// </summary>
    public async Task<string> ExportEntriesAsync(int userId, DateTime from, DateTime to)
    {
        // Normalize dates (date only)
        from = from.Date;
        to = to.Date;

        // 1) Read data from DB
        var entries = await _db.JournalEntries
            .Where(e => e.UserId == userId && e.EntryDate >= from && e.EntryDate <= to)
            .OrderBy(e => e.EntryDate)
            .ToListAsync();

        // 2) Create export folder in AppDataDirectory
        var exportDir = Path.Combine(FileSystem.AppDataDirectory, "Exports");
        Directory.CreateDirectory(exportDir);

        // 3) Create file name (date range included)
        var fileName = $"JournalExport_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf";
        var filePath = Path.Combine(exportDir, fileName);

        // 4) Build the PDF document using QuestPDF
        var accent = "#E16B7A";
        var accentSoft = "#FFE7EA";
        var border = "#F3B9C1";
        var grey = "#6B7280";

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(35);

                // Header
                page.Header().Column(col =>
                {
                    col.Item().Text("Journal Export")
                        .FontSize(26)
                        .SemiBold();

                    col.Item().Text($"{from:dd MMM yyyy}  →  {to:dd MMM yyyy}")
                        .FontSize(12)
                        .FontColor(grey);

                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor("#E5E7EB");
                });

                // Content
                page.Content().PaddingTop(14).Column(col =>
                {
                    if (entries.Count == 0)
                    {
                        col.Item()
                            .Background(accentSoft)
                            .Border(1).BorderColor(border)
                            .Padding(14)
                            .Text("No journal entries found in this date range.")
                            .FontColor(grey);

                        return;
                    }

                    foreach (var e in entries)
                    {
                        var secondary = SplitCsv(e.SecondaryMoodsCsv);
                        var tags = SplitCsv(e.TagsCsv);

                        col.Item().Border(1)
                            .BorderColor(border)
                            .Background(accentSoft)
                            .Padding(16)
                            .Column(card =>
                            {
                                // Row: Date + Mood
                                card.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(e.EntryDate.ToString("dddd, dd MMM yyyy", CultureInfo.InvariantCulture))
                                        .FontSize(13)
                                        .SemiBold();

                                    row.ConstantItem(160).AlignRight().Text($"Mood: {e.Mood}")
                                        .FontColor(accent)
                                        .SemiBold();
                                });

                                // Title
                                card.Item().PaddingTop(6).Text(e.Title).FontSize(18).SemiBold();

                                // Content (your DB stores HTML, so PDF will show raw HTML tags)
                                // Simple approach: strip tags so it looks clean in PDF
                                var plain = StripHtml(e.Content ?? "");
                                card.Item().PaddingTop(6).Text(plain).FontSize(11);

                                // ✅ Secondary moods
                                if (secondary.Count > 0)
                                {
                                    card.Item().PaddingTop(8).Text(t =>
                                    {
                                        t.DefaultTextStyle(s => s.FontSize(10));   // ✅ apply font size to whole line

                                        t.Span("Secondary moods: ").SemiBold().FontColor(grey);
                                        t.Span(string.Join(", ", secondary));
                                    });
                                }

                                // ✅ Tags
                                if (tags.Count > 0)
                                {
                                    card.Item().PaddingTop(6).Text(t =>
                                    {
                                        t.DefaultTextStyle(s => s.FontSize(10));   // ✅ apply font size to whole line

                                        t.Span("Tags: ").SemiBold().FontColor(grey);
                                        t.Span(string.Join(", ", tags));
                                    });
                                }

                                // Updated timestamp
                                card.Item().PaddingTop(8).Text($"Updated: {e.UpdatedAt.ToLocalTime():g}")
                                    .FontSize(9)
                                    .FontColor(grey);
                            });

                        col.Item().PaddingBottom(12);
                    }
                });

                // Footer
                page.Footer()
                    .AlignCenter()
                    .Text(t =>
                    {
                        t.DefaultTextStyle(x => x.FontSize(9).FontColor(grey));

                        t.Span("Generated by JournalApp • ");
                        t.CurrentPageNumber();
                        t.Span(" / ");
                        t.TotalPages();
                    });
            });
        })
        .GeneratePdf(filePath);

        // 5) Return the file path so UI can show it
        return filePath;
    }

    // -------------------------
    // Helper Methods
    // -------------------------

    // Convert CSV string -> list
    private static List<string> SplitCsv(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return new();
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(x => x.Trim())
                  .Where(x => x.Length > 0)
                  .Distinct()
                  .ToList();
    }

    // Remove HTML tags from RichText before putting into PDF
    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return "";

        var noTags = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        return System.Net.WebUtility.HtmlDecode(noTags).Trim();
    }
}
