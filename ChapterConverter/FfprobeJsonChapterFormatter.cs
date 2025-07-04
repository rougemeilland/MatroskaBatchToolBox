﻿using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MatroskaBatchToolBox.Utility;
using MatroskaBatchToolBox.Utility.Models.Json;
using MatroskaBatchToolBox.Utility.Movie;

namespace ChapterConverter
{
    internal sealed class FfprobeJsonChapterFormatter
        : ChapterFormatter
    {
        public FfprobeJsonChapterFormatter(ChapterFormatterParameter parameter)
            : base(parameter)
        {
        }

        protected override IEnumerable<InternalChapterElement> Parse(string rawText)
            => MovieInformation.ParseFromJson(rawText).Chapters
                .Select(chapter => new InternalChapterElement($"id#{chapter.Id}", chapter.StartTime, chapter.EndTime, chapter.Title));

        protected override string Render(IEnumerable<InternalChapterElement> chapters)
        {
            var movieInfo = new MovieInformationContainer
            {
                Chapters =
                    [..
                        chapters
                        .Select((chapter, index) =>
                            new MovieChapterContainer
                            {
                                Id = index + 1,
                                TimeBase = $"{DefaultTimeBaseNumerator}/{DefaultTimeBaseDenominator}",
                                Start = chapter.StartTime.FromTimeSpanToTimeCount(DefaultTimeBaseNumerator, DefaultTimeBaseDenominator),
                                StartTime = chapter.StartTime.FormatTime(TimeFormatType.LongFormat, 6),
                                End = chapter.EndTime.FromTimeSpanToTimeCount(DefaultTimeBaseNumerator, DefaultTimeBaseDenominator),
                                EndTime = chapter.EndTime.FormatTime(TimeFormatType.LongFormat, 6),
                                Tags = !string.IsNullOrEmpty(chapter.Title) ? new MovieChapterTagContainer { Title = chapter.Title } : null,
                            })
                    ],
            };
            return
                JsonSerializer.Serialize(
                    movieInfo,
                    MovieModelSourceGenerator.Default.MovieInformationContainer);
        }
    }
}
