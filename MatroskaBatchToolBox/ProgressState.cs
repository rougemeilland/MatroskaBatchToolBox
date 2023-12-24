using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using MatroskaBatchToolBox.Properties;
using Palmtree;
using Palmtree.IO;
using Palmtree.Linq;

namespace MatroskaBatchToolBox
{
    internal class ProgressState
    {
        private class SourceFileInfo
        {
            public SourceFileInfo(int uniqueId, FilePath file, ulong fileLength)
            {
                UniqueId = uniqueId;
                File = file;
                FileLength = fileLength;
                Progress = 0;
            }

            public int UniqueId { get; }

            public FilePath File { get; }
            public ulong FileLength { get; }
            public double Progress { get; set; }
        }

        private class ProgressHistoryElement
        {
            public ProgressHistoryElement(DateTime dateTime, double percentage)
            {
                DateTime = dateTime;
                Percentage = percentage;
            }

            public DateTime DateTime { get; }

            public double Percentage { get; }
        }

        /// <summary>
        /// ヒストリーへの最新の項目からこの値以上進捗が変化しない限りヒストリへの新たな追加はできません。
        /// </summary>
        private const double _minimumPercentageDifferenceForValidHistoryItem = 0.01 / 100;

        /// <summary>
        /// 推定終了時刻の計算において、ヒストリの最新の項目からこの時間より前の項目は参考にされません。
        /// </summary>
        private static readonly TimeSpan _maximumHistoryIntervalForValidHistoryItem;

        /// <summary>
        /// 有効な推定終了時刻の計算のためには、ヒストリの最新の項目からこの時間以上以前の項目がヒストリに存在しなければなりません。
        /// </summary>
        private static readonly TimeSpan _minimumHistoryIntervalForFinishTimeCalculation;

        /// <summary>
        /// 有効な推定終了時刻の計算のためには、少なくともこの値以上進捗がなければなりません。
        /// </summary>
        private const double _minimumPercentageForFinishTimeCalculation = 0.01 / 100.0;

        private readonly Queue<SourceFileInfo> _unprocessedSourceFiles;
        private readonly IDictionary<int, SourceFileInfo> _processingSourceFiles;
        private readonly IDictionary<ActionResult, ICollection<SourceFileInfo>> _processedSourceFiles;
        private readonly DateTime _firstDateTime;
        private readonly LinkedList<ProgressHistoryElement> _progressHistory;
        private ulong _totalLengthOfUnprocessedSourceFiles;
        private ulong _totalLengthOfProcessedSourceFiles;

        static ProgressState()
        {
            _maximumHistoryIntervalForValidHistoryItem = TimeSpan.FromDays(1);
            _minimumHistoryIntervalForFinishTimeCalculation = TimeSpan.FromMinutes(1);
        }

        public ProgressState(IEnumerable<FilePath> sourceFiles)
        {
            _unprocessedSourceFiles = new Queue<SourceFileInfo>(EnumerateSourceFileInfo(sourceFiles));
            _totalLengthOfUnprocessedSourceFiles = _unprocessedSourceFiles.Sum(sourceFile => sourceFile.FileLength);
            _processingSourceFiles = new Dictionary<int, SourceFileInfo>();
            _processedSourceFiles = new Dictionary<ActionResult, ICollection<SourceFileInfo>>
            {
                { ActionResult.Success, new List<SourceFileInfo>() },
                { ActionResult.Skipped, new List<SourceFileInfo>() },
                { ActionResult.Failed, new List<SourceFileInfo>() },
                { ActionResult.Cancelled, new List<SourceFileInfo>() }
            };
            _totalLengthOfProcessedSourceFiles = 0;
            _progressHistory = new LinkedList<ProgressHistoryElement>();
            _firstDateTime = DateTime.UtcNow;
            _progressHistory.AddLast(new LinkedListNode<ProgressHistoryElement>(new ProgressHistoryElement(_firstDateTime, 0.0)));
        }

        public bool TryGetNextSourceFile(out int sourceFieId, [NotNullWhen(true)] out FilePath? sourceFile)
        {
            lock (this)
            {
                if (_unprocessedSourceFiles.Count > 0)
                {
                    var item = _unprocessedSourceFiles.Dequeue();
                    item.Progress = 0;
                    _processingSourceFiles.Add(item.UniqueId, item);
                    sourceFieId = item.UniqueId;
                    sourceFile = item.File;
                    checked
                    {
                        _totalLengthOfUnprocessedSourceFiles -= item.FileLength;
                    }

                    return true;
                }
                else
                {
                    sourceFieId = default;
                    sourceFile = default;
                    return false;
                }
            }
        }

        public void UpdateProgress(int sourceFieId, double progress)
        {
            if (!progress.IsBetween(0.0, 1.0))
                throw new Exception($"Invalid {nameof(progress)} value: {nameof(progress)} = {progress}");

            lock (this)
            {
                if (!_processingSourceFiles.TryGetValue(sourceFieId, out SourceFileInfo? item))
                {
                    // sourceFieId が変換中のソールファイルのものではない。
                    // 本来ならエラーとするべきであるが、UpdateProgress は非同期に呼び出される可能性があるため、
                    // 変換が完了して _processingSourceFiles の中に既に存在しないときに UpdateProgress が呼び出されることがある。
                    // そのため、エラーにはせずに何もせずに復帰する。
                    return;
                }

                item.Progress = progress;

                var totalProgress = GetProgressValue();
                var now = DateTime.UtcNow;
                if (_progressHistory.Last is not null &&
                    totalProgress - _progressHistory.Last.Value.Percentage >= _minimumPercentageDifferenceForValidHistoryItem)
                {
                    _progressHistory.AddLast(new LinkedListNode<ProgressHistoryElement>(new ProgressHistoryElement(now, totalProgress)));
                    var historyItemLowerLimit = now - _maximumHistoryIntervalForValidHistoryItem;
                    while (_progressHistory.First is not null && _progressHistory.First.Value.DateTime < historyItemLowerLimit)
                        _progressHistory.RemoveFirst();
                }
            }
        }

        public void CompleteSourceFile(int sourceFieId, ActionResult actionResult)
        {
            lock (this)
            {
                if (!_processingSourceFiles.TryGetValue(sourceFieId, out var item))
                    throw Validation.GetFailErrorException("Failed in '_processingSourceFiles.TryGetValue(sourceFieId, out var item)'");
                _processedSourceFiles[actionResult].Add(item);
                _ = _processingSourceFiles.Remove(sourceFieId);
                switch (actionResult)
                {
                    case ActionResult.Success:
                    case ActionResult.Failed:
                        checked
                        {
                            _totalLengthOfProcessedSourceFiles += item.FileLength;
                        }

                        break;
                    case ActionResult.Skipped:
                    case ActionResult.Cancelled:
                    default:
                        // NOP
                        break;
                }
            }
        }

        public void WriteProgressText(Action<string> consoleWriter)
        {
            double oldPercentage;
            DateTime oldDateTime;
            double latestPercentage;
            DateTime latestDateTime;
            double progressValue;
            var now = DateTime.UtcNow;
            lock (this)
            {
                Validation.Assert(_progressHistory.First is not null, "_progressHistory.First is not null");
                oldDateTime = _progressHistory.First.Value.DateTime;
                oldPercentage = _progressHistory.First.Value.Percentage;
                Validation.Assert(_progressHistory.Last is not null, "_progressHistory.Last is not null");
                latestDateTime = _progressHistory.Last.Value.DateTime;
                latestPercentage = _progressHistory.Last.Value.Percentage;
                progressValue = GetProgressValue();
            }

            var elapsedTime = latestDateTime - _firstDateTime;
            var percentText = (progressValue * 100).ToString("F2");
            var elapsedTimeFromNow = now - _firstDateTime;

            if (elapsedTime < _minimumHistoryIntervalForFinishTimeCalculation || latestPercentage < _minimumPercentageForFinishTimeCalculation)
            {
                consoleWriter(
                    string.Format(
                        Resource.ProgressFormat1Text,
                        percentText,
                        FormatTimeSpanFriendly(elapsedTimeFromNow)));
            }
            else
            {
                var remainTime = (now - oldDateTime).Multiply((1.0 - latestPercentage) / (latestPercentage - oldPercentage));
                var untilDateTime = now + remainTime;
                consoleWriter(
                    string.Format(
                        Resource.ProgressFormat2Text,
                        percentText,
                        FormatTimeSpanFriendly(elapsedTimeFromNow),
                        FormatTimeSpanFriendly(remainTime),
                        FormatDateTimeFriendly(untilDateTime.ToLocalTime(), now.ToLocalTime())));
            }
        }

        private static IEnumerable<SourceFileInfo> EnumerateSourceFileInfo(IEnumerable<FilePath> sourceFiles)
        {
            // Lengthプロパティの参照で例外が発生した場合は、そのファイルは結果のリストからは除く
            foreach (var item in sourceFiles.Select((sourceFile, index) => new { index, sourceFile }))
            {
                FilePath? sourceFile = null;
                int? index = null;
                ulong? sourceFileLength = null;
                try
                {
                    if (item.sourceFile.Exists)
                    {
                        var length = item.sourceFile.Length;
                        if (length > 0)
                        {
                            sourceFile = item.sourceFile;
                            index = item.index;
                            sourceFileLength = length;
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                }
                catch (IOException)
                {
                }

                if (sourceFile is not null && index is not null && sourceFileLength is not null)
                    yield return new SourceFileInfo(index.Value, sourceFile, sourceFileLength.Value);
            }
        }

        private double GetProgressValue()
        {
            var totalOfSourceFileLength =
                checked(
                    _totalLengthOfUnprocessedSourceFiles +
                    _processingSourceFiles.Values.Sum(sourceFile => sourceFile.FileLength) +
                    _totalLengthOfProcessedSourceFiles);

            if (totalOfSourceFileLength <= 0)
                return 0;

            var totalOfProcessedSourceFileLength =
                _processingSourceFiles.Values.Sum(sourceFile => sourceFile.FileLength * sourceFile.Progress) +
                _totalLengthOfProcessedSourceFiles;

            var progress = totalOfProcessedSourceFileLength / totalOfSourceFileLength;
            return progress;
        }

        private static string FormatDateTimeFriendly(DateTime localDateTime, DateTime localNow)
            => localDateTime.Year != localNow.Year
                ? string.Format(Resource.CompletionDateTimeFriendlyFormat1Text, localDateTime.Year, localDateTime.Month, localDateTime.Day, localDateTime.Hour, localDateTime.Minute)
                : localDateTime.Month != localNow.Month || localDateTime.Day != localNow.Day
                ? string.Format(Resource.CompletionDateTimeFriendlyFormat2Text, localDateTime.Month, localDateTime.Day, localDateTime.Hour, localDateTime.Minute)
                : string.Format(Resource.CompletionDateTimeFriendlyFormat3Text, localDateTime.Hour, localDateTime.Minute);

        private static string FormatTimeSpanFriendly(TimeSpan time)
            => time >= TimeSpan.FromDays(1)
                ? string.Format(Resource.TimeSpanFriendlyFormat1Text, time.Days, time.Hours, time.Minutes)
                : time >= TimeSpan.FromHours(1)
                ? string.Format(Resource.TimeSpanFriendlyFormat2Text, time.Hours, time.Minutes)
                : time >= TimeSpan.FromMinutes(1)
                ? string.Format(Resource.TimeSpanFriendlyFormat3Text, time.Minutes)
                : Resource.TimeSpanFriendlyFormat4Text;
    }
}
