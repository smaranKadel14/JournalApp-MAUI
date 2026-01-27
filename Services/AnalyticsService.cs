using JournalApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.RegularExpressions;

namespace JournalApp.Services
{
    /// <summary>
    /// AnalyticsService (Student-friendly)
    /// -----------------------------------
    /// This service reads data from SQLite (EF Core) and calculates dashboard analytics:
    ///
    /// Date-range filterable analytics:
    /// - Mood Distribution (Positive/Neutral/Negative) with percentage
    /// - Most Frequent Mood
    /// - Daily Streak (within selected range ending at "To" date)
    /// - Longest Streak (within selected range)
    /// - Missed Days (dates with no entries between From/To)
    /// - Most Used Tags (top tags)
    /// - Tag Breakdown (entries per tag-category: Work/Health/Travel...)
    /// - Word Count Trends (average words per entry per day)
    ///
    /// Notes:
    /// - No DB schema changes required here.
    /// - We parse SecondaryMoodsCsv and TagsCsv (comma-separated).
    /// - We strip HTML from Content to calculate word counts correctly.
    /// </summary>
    public class AnalyticsService
    {
        private readonly AppDbContext _db;

        public AnalyticsService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<AnalyticsResult> GetInsightsAsync(int userId, DateTime from, DateTime to)
        {
            var fromDate = from.Date;
            var toDate = to.Date;

            // Load entries for the selected date range (ordered)
            var entries = await _db.JournalEntries
                .AsNoTracking()
                .Where(e => e.UserId == userId && e.EntryDate >= fromDate && e.EntryDate <= toDate)
                .OrderBy(e => e.EntryDate)
                .ToListAsync();

            // Keep a fast lookup set of dates that have entries
            var entryDates = entries
                .Select(e => e.EntryDate.Date)
                .Distinct()
                .ToHashSet();

            // -------------------------
            // Mood Distribution
            // -------------------------
            // Your app uses primary mood groups: Positive / Neutral / Negative.
            // But we keep it safe in case any entry has something else.
            var moodCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Positive"] = 0,
                ["Neutral"] = 0,
                ["Negative"] = 0
            };

            foreach (var e in entries)
            {
                var mood = NormalizePrimaryMood(e.Mood);

                if (!moodCounts.ContainsKey(mood))
                    moodCounts[mood] = 0;

                moodCounts[mood]++;
            }

            // Most frequent mood (primary)
            var mostFrequentMood = moodCounts
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key)
                .Select(kv => kv.Key)
                .FirstOrDefault() ?? "—";

            // -------------------------
            // Missed Days (From..To)
            // -------------------------
            var missedDays = new List<DateTime>();
            for (var d = fromDate; d <= toDate; d = d.AddDays(1))
            {
                if (!entryDates.Contains(d))
                    missedDays.Add(d);
            }

            // -------------------------
            // Streaks (within range)
            // - Current streak: consecutive days ending at ToDate
            // - Longest streak: maximum consecutive days in range
            // -------------------------
            var currentStreak = CalculateCurrentStreakEndingAt(toDate, entryDates);
            var longestStreak = CalculateLongestStreak(fromDate, toDate, entryDates);

            // -------------------------
            // Tags (Most used)
            // -------------------------
            var tagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var e in entries)
            {
                foreach (var t in SplitCsv(e.TagsCsv))
                {
                    tagCounts[t] = tagCounts.TryGetValue(t, out var c) ? c + 1 : 1;
                }
            }

            var topTags = tagCounts
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key)
                .Take(12)
                .Select(kv => new NameCount(kv.Key, kv.Value))
                .ToList();

            // -------------------------
            // Tag Breakdown (Category)
            // Example categories requested: Work, Health, Travel ...
            // We map tags to a "category group" and compute:
            //   % of entries that contain at least one tag from that category.
            // NOTE: Categories can overlap (an entry may count in multiple).
            // -------------------------
            var tagCategoryMap = BuildTagCategoryMap();

            var categoryCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in entries)
            {
                var tags = SplitCsv(e.TagsCsv);
                if (tags.Count == 0) continue;

                // Which categories does this entry belong to?
                var categoriesForEntry = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var t in tags)
                {
                    if (tagCategoryMap.TryGetValue(t, out var cat))
                        categoriesForEntry.Add(cat);
                }

                foreach (var cat in categoriesForEntry)
                {
                    categoryCounts[cat] = categoryCounts.TryGetValue(cat, out var c) ? c + 1 : 1;
                }
            }

            // Convert to % of entries
            var totalEntries = entries.Count;
            var tagBreakdown = categoryCounts
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key)
                .Select(kv =>
                {
                    var pct = totalEntries == 0 ? 0 : (int)Math.Round(kv.Value * 100.0 / totalEntries);
                    return new NameCountPct(kv.Key, kv.Value, pct);
                })
                .ToList();

            // -------------------------
            // Word Count Trends
            // "Average words per entry over time"
            // We'll calculate per day:
            //   avgWords = (total words that day) / (entries that day)
            // Then we plot it as a line.
            // -------------------------
            var dayTotals = new Dictionary<DateTime, (int totalWords, int entries)>();

            foreach (var e in entries)
            {
                var day = e.EntryDate.Date;

                var plain = StripHtml(e.Content ?? "");
                var words = CountWords(plain);

                if (!dayTotals.ContainsKey(day))
                    dayTotals[day] = (0, 0);

                var prev = dayTotals[day];
                dayTotals[day] = (prev.totalWords + words, prev.entries + 1);
            }

            var avgWordsByDay = new SortedDictionary<DateTime, int>();
            foreach (var kv in dayTotals.OrderBy(k => k.Key))
            {
                var avg = kv.Value.entries == 0 ? 0 : (int)Math.Round(kv.Value.totalWords * 1.0 / kv.Value.entries);
                avgWordsByDay[kv.Key] = avg;
            }

            // Prepare mood percentages
            var moodPct = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in moodCounts)
            {
                var pct = totalEntries == 0 ? 0 : (int)Math.Round(kv.Value * 100.0 / totalEntries);
                moodPct[kv.Key] = pct;
            }

            return new AnalyticsResult
            {
                From = fromDate,
                To = toDate,

                TotalEntries = totalEntries,
                MoodCounts = moodCounts,
                MoodPercentages = moodPct,
                MostFrequentMood = mostFrequentMood,

                CurrentStreak = currentStreak,
                LongestStreak = longestStreak,
                MissedDays = missedDays,

                TopTags = topTags,
                TagBreakdown = tagBreakdown,

                AvgWordsByDay = avgWordsByDay
            };
        }

        // -------------------------
        // Helper: Normalize mood
        // -------------------------
        private static string NormalizePrimaryMood(string? mood)
        {
            var m = (mood ?? "").Trim();

            if (m.Equals("Positive", StringComparison.OrdinalIgnoreCase)) return "Positive";
            if (m.Equals("Neutral", StringComparison.OrdinalIgnoreCase)) return "Neutral";
            if (m.Equals("Negative", StringComparison.OrdinalIgnoreCase)) return "Negative";

            // If something else is stored, keep it (but your charts focus on 3 main moods)
            return string.IsNullOrWhiteSpace(m) ? "Neutral" : m;
        }

        // -------------------------
        // Helper: streak ending at a date
        // -------------------------
        private static int CalculateCurrentStreakEndingAt(DateTime endDate, HashSet<DateTime> datesWithEntries)
        {
            var streak = 0;
            var check = endDate.Date;

            while (datesWithEntries.Contains(check))
            {
                streak++;
                check = check.AddDays(-1);
            }

            return streak;
        }

        // -------------------------
        // Helper: longest streak in range
        // -------------------------
        private static int CalculateLongestStreak(DateTime from, DateTime to, HashSet<DateTime> datesWithEntries)
        {
            var longest = 0;
            var current = 0;

            for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
            {
                if (datesWithEntries.Contains(d))
                {
                    current++;
                    if (current > longest) longest = current;
                }
                else
                {
                    current = 0;
                }
            }

            return longest;
        }

        // -------------------------
        // Helper: CSV split
        // -------------------------
        private static List<string> SplitCsv(string? csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return new();

            return csv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(x => x.Trim())
                      .Where(x => x.Length > 0)
                      .Distinct(StringComparer.OrdinalIgnoreCase)
                      .ToList();
        }

        // -------------------------
        // Helper: Strip HTML from rich text
        // -------------------------
        private static string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return "";
            var noTags = Regex.Replace(html, "<.*?>", string.Empty);
            return WebUtility.HtmlDecode(noTags).Trim();
        }

        private static int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            return Regex.Matches(text, @"\b[\p{L}\p{N}']+\b").Count;
        }

        /// <summary>
        /// Maps your tags into categories for "Tag Breakdown".
        /// Edit this mapping if you want different grouping.
        /// </summary>
        private static Dictionary<string, string> BuildTagCategoryMap()
        {
            // Category names
            const string Work = "Work";
            const string Health = "Health";
            const string Travel = "Travel";
            const string Relationships = "Relationships";
            const string Studies = "Studies";
            const string Finance = "Finance";
            const string SelfCare = "Self-care";
            const string Hobbies = "Hobbies";
            const string Personal = "Personal";

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Work
                ["Work"] = Work,
                ["Career"] = Work,
                ["Projects"] = Work,
                ["Planning"] = Work,

                // Health
                ["Health"] = Health,
                ["Fitness"] = Health,
                ["Exercise"] = Health,
                ["Yoga"] = Health,

                // Travel
                ["Travel"] = Travel,
                ["Vacation"] = Travel,
                ["Holiday"] = Travel,
                ["Nature"] = Travel,

                // Relationships
                ["Family"] = Relationships,
                ["Friends"] = Relationships,
                ["Relationships"] = Relationships,
                ["Parenting"] = Relationships,

                // Studies
                ["Studies"] = Studies,
                ["Reading"] = Studies,
                ["Writing"] = Studies,
                ["Reflection"] = Studies,

                // Finance
                ["Finance"] = Finance,
                ["Shopping"] = Finance,

                // Self-care
                ["Self-care"] = SelfCare,
                ["Meditation"] = SelfCare,
                ["Personal Growth"] = SelfCare,
                ["Spirituality"] = SelfCare,

                // Hobbies
                ["Hobbies"] = Hobbies,
                ["Music"] = Hobbies,
                ["Cooking"] = Hobbies,

                // Personal / life events
                ["Birthday"] = Personal,
                ["Celebration"] = Personal
            };

            return map;
        }
    }

    // UI-friendly results
    public class AnalyticsResult
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public int TotalEntries { get; set; }

        // Mood distribution
        public Dictionary<string, int> MoodCounts { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> MoodPercentages { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public string MostFrequentMood { get; set; } = "—";

        // Streaks + missed days
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public List<DateTime> MissedDays { get; set; } = new();

        // Tags
        public List<NameCount> TopTags { get; set; } = new();
        public List<NameCountPct> TagBreakdown { get; set; } = new();

        // Word trends
        public SortedDictionary<DateTime, int> AvgWordsByDay { get; set; } = new();
    }

    public record NameCount(string Name, int Count);
    public record NameCountPct(string Name, int Count, int Percent);
}
