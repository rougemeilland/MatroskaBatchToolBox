using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MatroskaBatchToolBox.Utility;
using Palmtree.Numerics;

namespace ChapterConverter
{
    internal sealed partial class FfprobeFlatChapterFormatter
        : ChapterFormatter
    {
        private static readonly char[] _textLineSeparators = ['\r', '\n'];

        public FfprobeFlatChapterFormatter(ChapterFormatterParameter parameter)
            : base(parameter)
        {
        }

        protected override IEnumerable<InternalChapterElement> Parse(string rawText)
        {
            var flatRows = new Dictionary<int, IDictionary<string, string>>();
            foreach (var rowText in rawText.Split(_textLineSeparators, StringSplitOptions.RemoveEmptyEntries))
                AddToFlatRows(flatRows, rowText);

            foreach (var flatRow in flatRows.OrderBy(flatRow => flatRow.Key))
            {
                var index = flatRow.Key;
                var keyValue = flatRow.Value;
                var id = GetValue(keyValue, index, "id");
                var timeBase = GetValue(keyValue, index, "time_base");
                if (!timeBase.TryParse(out var timeBaseNumerator, denominator: out long timeBaseDenominator))
                    throw new ApplicationException($"The format of \"{"time_base"}\" in the input data is invalid.: \"chapters.chapter.{index}.{"time_base"}={timeBase}\"");
                var startText = GetValue(keyValue, index, "start");
                if (!startText.TryParse(value: out long start))
                    throw new ApplicationException($"The format of \"{"start"}\" in the input data is invalid.: \"chapters.chapter.{index}.{"start"}={startText}\"");
                var startTime = start.FromTimeCountToTimeSpan(timeBaseNumerator, timeBaseDenominator);
                var endText = GetValue(keyValue, index, "end");
                if (!endText.TryParse(value: out long end))
                    throw new ApplicationException($"The format of \"{"end"}\" in the input data is invalid.: \"chapters.chapter.{index}.{"end"}={endText}\"");
                var endTime = end.FromTimeCountToTimeSpan(timeBaseNumerator, timeBaseDenominator);
                var title = GetValue(keyValue, index, "tags.title", "");
                yield return new InternalChapterElement($"id#{id}", startTime, endTime, title);
            }

            static string GetValue(IDictionary<string, string> keyValue, int index, string key, string? defaultValue = null)
            {
                return
                    keyValue.TryGetValue(key, out var value)
                    ? value
                    : defaultValue is not null
                    ? defaultValue
                    : throw new ApplicationException($"The line \"chapters.chapter.{index}.{key}=...\" was not found in the input data.");
            }
        }

        protected override string Render(IEnumerable<InternalChapterElement> chapters)
            => string.Join(
                "\r\n",
                chapters
                .SelectMany((chapter, index) =>
                {
                    var rows =
                        new[]
                        {
                            $"chapters.chapter.{index}.id={index + 1}",
                            $"chapters.chapter.{index}.time_base=\"{DefaultTimeBaseNumerator}/{DefaultTimeBaseDenominator}\"",
                            $"chapters.chapter.{index}.start={chapter.StartTime.FromTimeSpanToTimeCount(DefaultTimeBaseNumerator, DefaultTimeBaseDenominator)}",
                            $"chapters.chapter.{index}.start_time=\"{chapter.StartTime.TotalSeconds:F6}\"",
                            $"chapters.chapter.{index}.end={chapter.EndTime.FromTimeSpanToTimeCount(DefaultTimeBaseNumerator, DefaultTimeBaseDenominator)}",
                            $"chapters.chapter.{index}.end_time=\"{chapter.EndTime.TotalSeconds:F6}\"",
                        }
                        .AsEnumerable();
                    if (!string.IsNullOrEmpty(chapter.Title))
                        rows = rows.Append($"chapters.chapter.{index}.tags.title=\"{chapter.Title.Replace(@"\", @"\\").Replace(@"""", @"\""")}\"");
                    return rows;
                })
                .Append(""));

        private static void AddToFlatRows(IDictionary<int, IDictionary<string, string>> searchResult, string rowText)
        {
            var quoted_match = GetQuotedRowPattern().Match(rowText);
            if (quoted_match.Success)
            {
                Register(
                    searchResult,
                    quoted_match.Groups["index"].Value.ParseAsInt32(),
                    quoted_match.Groups["key"].Value,
                    quoted_match.Groups["value"].Value.Replace(@"\""", @"""").Replace(@"\\", @"\"));
            }
            else
            {
                var unquoted_match = GetUnquotedRowPattern().Match(rowText);
                if (unquoted_match.Success)
                {
                    Register(
                        searchResult,
                        unquoted_match.Groups["index"].Value.ParseAsInt32(),
                        unquoted_match.Groups["key"].Value,
                        unquoted_match.Groups["value"].Value);
                }
            }

            static void Register(IDictionary<int, IDictionary<string, string>> searchResult, int index, string key, string value)
            {
                if (!searchResult.TryGetValue(index, out var keyValueDic))
                {
                    keyValueDic = new Dictionary<string, string>();
                    searchResult.Add(index, keyValueDic);
                }

                keyValueDic.Add(key, value);
            }
        }

        [GeneratedRegex(@"^chapters\.chapter\.(?<index>\d+)\.(?<key>[^=]+)=""(?<value>.*)""$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
        private static partial Regex GetQuotedRowPattern();

        [GeneratedRegex(@"^chapters\.chapter\.(?<index>\d+)\.(?<key>[^=]+)=(?<value>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
        private static partial Regex GetUnquotedRowPattern();
    }
}
