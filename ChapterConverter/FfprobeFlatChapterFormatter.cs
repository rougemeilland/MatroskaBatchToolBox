using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utility;

namespace ChapterConverter
{
    internal class FfprobeFlatChapterFormatter
        : ChapterFormatter
    {
        private static readonly Regex _quotedRowPattern;
        private static readonly Regex _unquotedRowPattern;

        static FfprobeFlatChapterFormatter()
        {
            _quotedRowPattern = new Regex(@"^chapters\.chapter\.(?<index>\d+)\.(?<key>[^=]+)=""(?<value>.*)""$", RegexOptions.Compiled);
            _unquotedRowPattern = new Regex(@"^chapters\.chapter\.(?<index>\d+)\.(?<key>[^=]+)=(?<value>\d+)$", RegexOptions.Compiled);
        }

        public FfprobeFlatChapterFormatter(ChapterFormatterParameter parameter)
            : base(parameter)
        {
        }

        protected override IEnumerable<InternalChapterElement> Parse(string rawText)
        {
            var flatRows = new Dictionary<int, IDictionary<string, string>>();
            foreach (var rowText in rawText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                AddToFlatRows(flatRows, rowText);

            foreach (var flatRow in flatRows.OrderBy(flatRow => flatRow.Key))
            {
                var index = flatRow.Key;
                var keyValue = flatRow.Value;
                var id = GetValue(keyValue, index, "id");
                var timeBase = GetValue(keyValue, index, "time_base");
                if (!timeBase.TryParse(out long timeBaseNumerator, out long timeBaseDenominator))
                    throw new Exception($"The format of \"{"time_base"}\" in the input data is invalid.: \"chapters.chapter.{index}.{"time_base"}={timeBase}\"");
                var startText = GetValue(keyValue, index, "start");
                if (!startText.TryParse(out long start))
                    throw new Exception($"The format of \"{"start"}\" in the input data is invalid.: \"chapters.chapter.{index}.{"start"}={startText}\"");
                var startTime = start.FromTimeCountToTimeSpan(timeBaseNumerator, timeBaseDenominator);
                var endText = GetValue(keyValue, index, "end");
                if (!endText.TryParse(out long end))
                    throw new Exception($"The format of \"{"end"}\" in the input data is invalid.: \"chapters.chapter.{index}.{"end"}={endText}\"");
                var endTime = end.FromTimeCountToTimeSpan(timeBaseNumerator, timeBaseDenominator);
                var title = GetValue(keyValue, index, "tags.title", "");
                yield return new InternalChapterElement($"id#{id}", startTime, endTime, title);
            }

            static string GetValue(IDictionary<string, string> keyValue, int index, string key, string? defaultValue = null)
            {
                return
                    keyValue.TryGetValue(key, out string? value)
                    ? value
                    : defaultValue is not null
                    ? defaultValue
                    : throw new Exception($"The line \"chapters.chapter.{index}.{key}=...\" was not found in the input data.");
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
            var quoted_match = _quotedRowPattern.Match(rowText);
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
                var unquoted_match = _unquotedRowPattern.Match(rowText);
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
                if (!searchResult.TryGetValue(index, out IDictionary<string, string>? keyValueDic))
                {
                    keyValueDic = new Dictionary<string, string>();
                    searchResult.Add(index, keyValueDic);
                }

                keyValueDic.Add(key, value);
            }
        }
    }
}
