﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Utility;
using Utility.Serialization;

namespace ChapterConverter
{
    internal class CsvChapterFormatter
        : ChapterFormatter
    {
        public CsvChapterFormatter(ChapterFormatterParameter parameter)
            : base(parameter)
        {
        }

        protected override IEnumerable<InternalChapterElement> Parse(string rawText)
        {
            var chapters =
                CsvSerializer.Deserialize(rawText, new CsvSerializerOption { ColumnDelimiterChar = '\t' })
                .Select(row => row.ToArray())
                .Select((row, index) =>
                    row.Length.IsInRange(1, 2)
                    ? (new
                    {
                        startTime = row[0].TryParse(false, out TimeSpan time) ? time : throw new Exception($"Invalid row format in input data.: {row[0]}"),
                        title = row.Length >= 2 ? row[1] : "",
                    })
                    : throw new Exception("The format of the input CSV data is invalid. (not enough columns in row)"))
                .ToArray();

            for (var index = 0; index < chapters.Length; ++index)
            {
                var startTime = chapters[index].startTime;
                var endTime = index + 1 < chapters.Length ? chapters[index + 1].startTime : SimpleChapterElement.DefaultMaximumDuration;
                var title = chapters[index].title;
                yield return new InternalChapterElement($"#{index}", startTime, endTime, title);
            }
        }

        protected override string Render(IEnumerable<InternalChapterElement> chapters)
            => CsvSerializer.Serialize(
                chapters
                .Select((chapter, index) =>
                {
                    var row = new[] { chapter.StartTime.FormatTime(6), }.AsEnumerable();
                    if (!string.IsNullOrEmpty(chapter.Title))
                        row = row.Append(chapter.Title);
                    return row;
                }),
                new CsvSerializerOption { ColumnDelimiterChar = '\t' });
    }
}
