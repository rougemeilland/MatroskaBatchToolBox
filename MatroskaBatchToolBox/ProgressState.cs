using MatroskaBatchToolBox.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace MatroskaBatchToolBox
{
    internal class ProgressState
    {
        private class SourceFileInfo
        {
            public SourceFileInfo(int uniqueId, FileInfo file, long fileLength)
            {
                UniqueId = uniqueId;
                File = file;
                FileLength = fileLength;
                Progress = 0;
            }

            public int UniqueId { get; }

            public FileInfo File { get; }
            public long FileLength { get; }
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
        private static TimeSpan _maximumHistoryIntervalForValidHistoryItem;

        /// <summary>
        /// 有効な推定終了時刻の計算のためには、ヒストリの最新の項目からこの時間以上以前の項目がヒストリに存在しなければなりません。
        /// </summary>
        private static TimeSpan _minimumHistoryIntervalForFinishTimeCalculation;

        /// <summary>
        /// 有効な推定終了時刻の計算のためには、少なくともこの値以上進捗がなければなりません。
        /// </summary>
        private const double _minimumPercentageForFinishTimeCalculation = 0.01 / 100.0;

        private Queue<SourceFileInfo> _unprocessedSourceFiles;
        private IDictionary<int, SourceFileInfo> _processingSourceFiles;
        private IDictionary<ActionResult, ICollection<SourceFileInfo>> _processedSourceFiles;
        private long _totalLengthOfUnprocessedSourceFiles;
        private long _totalLengthOfProcessedSourceFiles;
        private DateTime _firstDateTime;
        private LinkedList<ProgressHistoryElement> _progressHistory;

        static ProgressState()
        {
            _maximumHistoryIntervalForValidHistoryItem = TimeSpan.FromDays(1);
            _minimumHistoryIntervalForFinishTimeCalculation = TimeSpan.FromMinutes(1);
        }

        public ProgressState(IEnumerable<FileInfo> sourceFiles)
        {
            _unprocessedSourceFiles
                = new Queue<SourceFileInfo>(
                    sourceFiles
                    .Select((sourceFile, index) => new SourceFileInfo(index, sourceFile, sourceFile.Length)));
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
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"{nameof(MatroskaBatchToolBox)}:INFO: {nameof(ProgressState)} object created. Total length of source files = {_totalLengthOfUnprocessedSourceFiles:N} bytes.");
#endif
        }

        public bool TryGetNextSourceFile(out int sourceFieId, [MaybeNullWhen(false)] out FileInfo sourceFile)
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
                    _totalLengthOfUnprocessedSourceFiles -= item.FileLength;
#if DEBUG
                    {
                        var totalLength1 = _unprocessedSourceFiles.Sum(item => item.FileLength);
                        var totalLength2 = _totalLengthOfUnprocessedSourceFiles;
                        if (totalLength1 != totalLength2)
                            throw new Exception();
                    }
#endif
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"{nameof(MatroskaBatchToolBox)}:INFO: {nameof(ProgressState)}.{nameof(TryGetNextSourceFile)}() => sourceFieId={sourceFieId}, sourceFile=\"{item.File.FullName}\"({item.FileLength:N} bytes)");
#endif
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
            if (progress < 0 || progress > 1)
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

                var now = DateTime.UtcNow;

                if (_progressHistory.Last is not null &&
                    progress - _progressHistory.Last.Value.Percentage >= _minimumPercentageDifferenceForValidHistoryItem)
                {
                    _progressHistory.AddLast(new LinkedListNode<ProgressHistoryElement>(new ProgressHistoryElement(now, GetProgressValue())));
                    var historyItemLowerLimit = now - _maximumHistoryIntervalForValidHistoryItem;
                    while (_progressHistory.First is not null && _progressHistory.First.Value.DateTime < historyItemLowerLimit)
                        _progressHistory.RemoveFirst();
                }

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"{nameof(MatroskaBatchToolBox)}:INFO: {nameof(ProgressState)}.{nameof(UpdateProgress)}({nameof(sourceFieId)}={sourceFieId}, {nameof(progress)}={progress:F6})");
#endif
            }
        }

        public void CompleteSourceFile(int sourceFieId, ActionResult actionResult)
        {
            lock (this)
            {
                if (!_processingSourceFiles.TryGetValue(sourceFieId, out SourceFileInfo? item))
                    throw new Exception($"internal error (invalid {nameof(sourceFieId)})");
                _processedSourceFiles[actionResult].Add(item);
                _processingSourceFiles.Remove(sourceFieId);
                switch (actionResult)
                {
                    case ActionResult.Success:
                    case ActionResult.Failed:
                        _totalLengthOfProcessedSourceFiles += item.FileLength;
                        break;
                    case ActionResult.Skipped:
                    case ActionResult.Cancelled:
                    default:
                        // NOP
                        break;
                }
#if DEBUG
                {
                    var totalLength1 = _processedSourceFiles[ActionResult.Success].Sum(item => item.FileLength);
                    var totalLength2 = _processedSourceFiles[ActionResult.Failed].Sum(item => item.FileLength);
                    var totalLength3 = _totalLengthOfProcessedSourceFiles;
                    if (totalLength1 + totalLength2 != totalLength3)
                        throw new Exception();
                }
#endif
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"{nameof(MatroskaBatchToolBox)}:INFO: {nameof(ProgressState)}.{nameof(CompleteSourceFile)}({nameof(sourceFieId)}={sourceFieId}, {nameof(actionResult)}={actionResult}");
#endif
            }
        }

        public void WriteProgressText(Action<string> consoleWriter)
        {
            double oldPercentage;
            DateTime oldDateTime;
            double latestPercentage;
            DateTime latestDateTime;
            double progressValue;
            lock (this)
            {
                if (_progressHistory.First is null)
                    throw new Exception("internal error");
                oldDateTime = _progressHistory.First.Value.DateTime;
                oldPercentage = _progressHistory.First.Value.Percentage;
                if (_progressHistory.Last is null)
                    throw new Exception("internal error");
                latestDateTime = _progressHistory.Last.Value.DateTime;
                latestPercentage = _progressHistory.Last.Value.Percentage;
                progressValue = GetProgressValue();
            }

            var elapsedTime = latestDateTime - _firstDateTime;
            var percentText = (progressValue * 100).ToString("F2");

            if (elapsedTime < _minimumHistoryIntervalForFinishTimeCalculation || latestPercentage < _minimumPercentageForFinishTimeCalculation)
            {
                consoleWriter(
                    string.Format(
                        Resource.ProgressFormat1Text,
                        percentText,
                        FormatTimeSpanFriendly(elapsedTime)));
            }
            else
            {
                var remainTime = (latestDateTime - oldDateTime).Multiply((1.0 - latestPercentage) / (latestPercentage - oldPercentage));
                var untilDateTime = latestDateTime + remainTime;
                consoleWriter(
                    string.Format(
                        Resource.ProgressFormat2Text,
                        percentText,
                        FormatTimeSpanFriendly(elapsedTime),
                        FormatTimeSpanFriendly(remainTime),
                        FormatDateTimeFriendly(untilDateTime.ToLocalTime(), latestDateTime.ToLocalTime())));
            }
        }

        public void CheckCompletion()
        {

#if DEBUG
            {
                if (_unprocessedSourceFiles.Any())
                    throw new Exception("internal error");
                if (_processingSourceFiles.Any())
                    throw new Exception("internal error");
            }
            {
                var totalLength1 = _processedSourceFiles[ActionResult.Success].Sum(item => item.FileLength);
                var totalLength2 = _processedSourceFiles[ActionResult.Failed].Sum(item => item.FileLength);
                var totalLength3 = _totalLengthOfProcessedSourceFiles;
                if (totalLength1 + totalLength2 != totalLength3)
                    throw new Exception();
            }
#endif
        }

        private double GetProgressValue()
        {
            var totalOfSourceFileLength =
                _totalLengthOfUnprocessedSourceFiles +
                _processingSourceFiles.Values.Sum(sourceFile => sourceFile.FileLength) +
                _totalLengthOfProcessedSourceFiles;

            if (totalOfSourceFileLength <= 0)
                return 0;

            var totalOfProcessedSourceFileLength =
                _processingSourceFiles.Values.Sum(sourceFile => sourceFile.FileLength * sourceFile.Progress) +
                _totalLengthOfProcessedSourceFiles;

#if DEBUG
            if (totalOfProcessedSourceFileLength > totalOfSourceFileLength)
                throw new Exception($"{nameof(totalOfProcessedSourceFileLength)} > {nameof(totalOfSourceFileLength)}: {nameof(totalOfProcessedSourceFileLength)} = {totalOfProcessedSourceFileLength}, {nameof(totalOfSourceFileLength)} = {totalOfSourceFileLength}");
#endif

            var progress = totalOfProcessedSourceFileLength / totalOfSourceFileLength;
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"{nameof(MatroskaBatchToolBox)}:INFO: {nameof(ProgressState)}.{nameof(GetProgressValue)}() => {progress:F6}");
#endif
            return progress;
        }


        private static string FormatDateTimeFriendly(DateTime localDateTime, DateTime localNow)
        {
            if (localDateTime.Year != localNow.Year)
                return string.Format(Resource.CompletionDateTimeFriendlyFormat1Text, localDateTime.Year, localDateTime.Month, localDateTime.Day, localDateTime.Hour, localDateTime.Minute);
            else if (localDateTime.Month != localNow.Month || localDateTime.Day != localNow.Day)
                return string.Format(Resource.CompletionDateTimeFriendlyFormat2Text, localDateTime.Month, localDateTime.Day, localDateTime.Hour, localDateTime.Minute);
            else
                return string.Format(Resource.CompletionDateTimeFriendlyFormat3Text, localDateTime.Hour, localDateTime.Minute);
        }

        private static string FormatTimeSpanFriendly(TimeSpan time)
        {
            if (time >= TimeSpan.FromDays(1))
                return string.Format(Resource.TimeSpanFriendlyFormat1Text, time.Days, time.Hours, time.Minutes);
            else if (time >= TimeSpan.FromHours(1))
                return string.Format(Resource.TimeSpanFriendlyFormat2Text, time.Hours, time.Minutes);
            else if (time >= TimeSpan.FromMinutes(1))
                return string.Format(Resource.TimeSpanFriendlyFormat3Text, time.Minutes);
            else
                return Resource.TimeSpanFriendlyFormat4Text;
        }
    }
}
