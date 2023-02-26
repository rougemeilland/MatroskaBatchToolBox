using System.Collections.Generic;
using System.IO;

namespace MatroskaBatchToolBox
{
    internal class LocalSettingsCache
    {
        private readonly IDictionary<string, Settings> _cache;

        public LocalSettingsCache()
        {
            _cache = new Dictionary<string, Settings>();
        }

        public Settings this[DirectoryInfo? sourceFileDirectory]
        {
            get
            {
                lock (this)
                {
                    return GetSettings(sourceFileDirectory);
                }
            }
        }

        private Settings GetSettings(DirectoryInfo? directory)
        {
            // 対象ディレクトリが null ならグローバル設定を返す。
            if (directory is null)
                return Settings.GlobalSettings;

            var key = directory.FullName;

            // 対象ディレクトリに対するローカル設定がキャッシュに既にあればそれを返す。
            if (_cache.TryGetValue(key, out Settings? settings))
            {
#if DEBUG && true
                System.Diagnostics.Debug.WriteLine($"get settings from cache: \"{key}\"");
#endif
                return settings;
            }

            // 対象ディレクトリの親ディレクトリに対するローカル設定に、対象ディレクトリにある設定を上書きして、キャッシュに追加する。(再帰呼び出し)
            var newSettings = GetSettings(directory.Parent).GetLocalSettings(directory);
            _cache.Add(key, newSettings);
#if DEBUG && true
            System.Diagnostics.Debug.WriteLine($"add settings to cache: \"{key}\"");
#endif
            return newSettings;
        }
    }
}
