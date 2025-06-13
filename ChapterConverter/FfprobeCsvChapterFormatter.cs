using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MatroskaBatchToolBox.Utility;
using Palmtree;
using Palmtree.IO.Serialization;
using Palmtree.Numerics;

namespace ChapterConverter
{
    internal sealed class FfprobeCsvChapterFormatter
        : ChapterFormatter
    {
        private const string _firstColumnValue = "chapter";

        public FfprobeCsvChapterFormatter(ChapterFormatterParameter parameter)
            : base(parameter)
        {
        }

        protected override IEnumerable<InternalChapterElement> Parse(string rawText)
        {
            try
            {
                var chapters =
                    CsvSerializer.Deserialize(rawText, new CsvSerializerOption { })
                    .Select(row => row.ToArray())
                    .Where(row => row.Length > 0 && row[0] == _firstColumnValue)
                    .Select((row, rowIndex) =>
                    {
                        if (!row.Length.IsBetween(7, 8))
                            throw new ApplicationException("The format of the input CSV data is invalid. (Invalid number of columns in row)");
                        var header = row[0];
                        var id = row[1];
                        var timeBase = row[2];
                        if (!timeBase.TryParse(out long timeBaseNumerator, out var timeBaseDenominator))
                            throw new ApplicationException($"The format of the input CSV data is invalid. (The 3rd column of the \"{header}.{id}\" row is not in the format that represents \"TIMEBASE\" (eg 1/100000000)): \"{timeBase}\"");
                        var startText = row[3];
                        if (!startText.TryParse(out long start))
                            throw new ApplicationException($"The format of the input CSV data is invalid. (The 4th column of the \"{header}.{id}\" line is not an integer representing \"START\"): \"{startText}\"");
                        var startTime = start.FromTimeCountToTimeSpan(timeBaseNumerator, timeBaseDenominator);
                        var endText = row[5];
                        if (!endText.TryParse(out long end))
                            throw new ApplicationException($"The format of the input CSV data is invalid. (the 6th column of the \"{header}.{id}\" line is not an integer representing \"END\"): \"{endText}\"");
                        var endTime = end.FromTimeCountToTimeSpan(timeBaseNumerator, timeBaseDenominator);
                        var title = row.Length >= 8 ? row[7] : "";
                        return new { header, id, startTime, endTime, title };
                    })
                    .ToArray();

                return
                    chapters
                    .Select(chapter => new InternalChapterElement($"{chapter.header}.{chapter.id}", chapter.startTime, chapter.endTime, chapter.title));
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"The input string is not compatible with the format displayed by the command \"ffprobe -show_chapters -print_format csv\".: \"{rawText}\"", ex);
            }
        }

        protected override string Render(IEnumerable<InternalChapterElement> chapters)
            => CsvSerializer.Serialize(
                chapters
                .Select((chapter, index) =>
                {
                    var row = new[]
                    {
                        _firstColumnValue,
                        (index + 1).ToString(CultureInfo.InvariantCulture),
                        $"{DefaultTimeBaseNumerator}/{DefaultTimeBaseDenominator}",
                        chapter.StartTime.FromTimeSpanToTimeCount(DefaultTimeBaseNumerator, DefaultTimeBaseDenominator).ToString(CultureInfo.InvariantCulture),
                        chapter.StartTime.TotalSeconds.ToString("F6", CultureInfo.InvariantCulture),
                        chapter.EndTime.FromTimeSpanToTimeCount(DefaultTimeBaseNumerator, DefaultTimeBaseDenominator).ToString(CultureInfo.InvariantCulture),
                        chapter.EndTime.TotalSeconds.ToString("F6", CultureInfo.InvariantCulture),
                    }.AsEnumerable();
                    if (!string.IsNullOrEmpty(chapter.Title))
                        row = row.Append(chapter.Title);
                    return row;
                }));
    }
}
