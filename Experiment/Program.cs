using System;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Experiment
{

    public static class Program
    {
        public static void Main(string[] args)
        {
            var 実際に存在するファイルのパス = typeof(Program).Assembly.Location;
            var baseDirectoryPath = Path.GetDirectoryName(実際に存在するファイルのパス) ?? ".";
            var 実際に存在しないファイルのパス = Path.Combine(baseDirectoryPath, "hogehoge.txt");
            var 実際に存在しないディレクトリのパス = Path.Combine(baseDirectoryPath, "--directory--");
            var 実際に存在しないディレクトリの配下のファイルのパス = Path.Combine(実際に存在しないディレクトリのパス, "hoehoe.txt");

            if (!File.Exists(実際に存在するファイルのパス))
                throw new Exception();
            if (!new FileInfo(実際に存在するファイルのパス).Exists)
                throw new Exception();

            if (File.Exists(実際に存在しないファイルのパス))
                throw new Exception();
            if (new FileInfo(実際に存在しないファイルのパス).Exists)
                throw new Exception();

            if (Directory.Exists(実際に存在しないディレクトリのパス))
                throw new Exception();
            if (new DirectoryInfo(実際に存在しないディレクトリのパス).Exists)
                throw new Exception();

            if (File.Exists(実際に存在しないディレクトリの配下のファイルのパス))
                throw new Exception();
            if (new FileInfo(実際に存在しないディレクトリの配下のファイルのパス).Exists)
                throw new Exception();

            Console.WriteLine("OK");
            Console.Beep();
            Console.ReadLine();
        }
    }
}
