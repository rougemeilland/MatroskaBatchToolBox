using System.Collections.Generic;
using Palmtree.IO;

namespace MatroskaBatchToolBox
{
    internal sealed class LocalSettingsCache
    {
        private readonly Dictionary<string, Settings> _cache;

        public LocalSettingsCache()
        {
            _cache = [];
        }

        public Settings this[DirectoryPath? sourceFileDirectory]
        {
            get
            {
                lock (this)
                {
                    return GetSettings(sourceFileDirectory);
                }
            }
        }

        private Settings GetSettings(DirectoryPath? directory)
        {
            // 対象ディレクトリが null ならグローバル設定を返す。
            if (directory is null)
                return Settings.GlobalSettings;

            var key = directory.FullName;

            // 対象ディレクトリに対するローカル設定がキャッシュに既にあればそれを返す。
            if (_cache.TryGetValue(key, out var settings))
                return settings;

            // 対象ディレクトリの親ディレクトリに対するローカル設定に、対象ディレクトリにある設定を上書きして、キャッシュに追加する。(再帰呼び出し)
            var newSettings = GetSettings(directory.Parent).GetLocalSettings(directory);
            _cache.Add(key, newSettings);
            return newSettings;
        }
    }
}
