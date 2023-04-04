using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Palmtree;
using Palmtree.IO;
using Palmtree.Terminal.StringExpansion;

namespace Palmtree.Terminal
{
    internal class TerminalInfoDatabase
    {
        private const short _legacyMagicNumber = 0x011a;
        private const short _32bitMagicNumber = 0x021e;
        private const string _pseudoCapabilityNameSetTitle = "__set_title";
        private const string _pseudoCapabilityNameResetColor = "__reset_color";
        private const string _pseudoCapabilityNameClearBuffer = "__clear_buffer";
        private const string _pseudoCapabilityNameEraseScrollBuffer = "__erase_scroll_buffer";
        private const string _pseudoCapabilityNameCursorPositionReport = "__cursor_position_report";
        private const string _pseudoCapabilityNameEraseInDisplay1 = "__erase_in_display_1";
        private const string _pseudoCapabilityNameEraseInLine2 = "__erase_in_line_2";
        private const string _terminalNameWindowsLocalConsole = "windows-terminal";
        private static readonly Regex _resetColorPattern;
        private static readonly Regex _deviceStatusReportPattern;
        private readonly IDictionary<TermInfoBooleanCapabilities, bool> _booleanCapabilities;
        private readonly IDictionary<TermInfoNumberCapabilities, int> _numberCapabilities;
        private readonly IDictionary<TermInfoStringCapabilities, string> _stringCapabilities;
        private readonly IDictionary<string, bool> _extendedBooleanCapabilities;
        private readonly IDictionary<string, int> _extendedNumberCapabilities;
        private readonly IDictionary<string, string> _extendedStringCapabilities;
        private readonly ICollection<string> _warnings;

        static TerminalInfoDatabase()
        {
            _resetColorPattern = new Regex(@"\u001b\[(39:49|49:39)m", RegexOptions.Compiled);
            _deviceStatusReportPattern = new Regex(@"^\e\[6n$", RegexOptions.Compiled);
        }

        private TerminalInfoDatabase(
            string termInfoFilePath,
            IEnumerable<string> terminalNames,
            IDictionary<TermInfoBooleanCapabilities, bool> booleanCapabilities,
            IDictionary<TermInfoNumberCapabilities, int> numberCapabilities,
            IDictionary<TermInfoStringCapabilities, string> stringCapabilities,
            IDictionary<string, bool> extendedBooleanCapabilities,
            IDictionary<string, int> extendedNumberCapabilities,
            IDictionary<string, string> extendedStringCapabilities,
            bool includeUniqueCapabilities)
        {
            TermInfoFilePath = termInfoFilePath;
            TerminalNames = terminalNames.ToReadOnlyArray();
            _booleanCapabilities = booleanCapabilities;
            _numberCapabilities = numberCapabilities;
            _stringCapabilities = stringCapabilities;
            _extendedBooleanCapabilities = extendedBooleanCapabilities;
            _extendedNumberCapabilities = extendedNumberCapabilities;
            _extendedStringCapabilities = extendedStringCapabilities;
            _warnings = new List<string>();
            if (includeUniqueCapabilities)
            {
                #region Add pseudo-capability "__set_title"
                {
                    // Add pseudo-capability "__set_title"

                    // From System.TerminalFormatStrings.GetTitle()
                    //   (https://github.com/dotnet/runtime/blob/main/src/libraries/System.Console/src/System/TerminalFormatStrings.cs)
                    //
                    // + The extended capability "TS" should be used in preference to the capability "to_status_line" and the capability "from_status_line".
                    //   (https://invisible-island.net/ncurses/terminfo.src.html#tic-xterm-utf8)
                    //

                    if (_extendedStringCapabilities.TryGetValue("TS", out var ts))
                    {
                        _extendedStringCapabilities.Add(_pseudoCapabilityNameSetTitle, $"{ts}%p1%s\u0007");
                    }
                    else if (
                        _stringCapabilities.TryGetValue(TermInfoStringCapabilities.ToStatusLine, out var tsl) &&
                        _stringCapabilities.TryGetValue(TermInfoStringCapabilities.FromStatusLine, out var fsl))
                    {
                        _extendedStringCapabilities.Add(_pseudoCapabilityNameSetTitle, $"{tsl}%p1%s{fsl}");
                    }
                    else if (
                        TerminalNames.Any(name => name == "aixterm") ||
                        TerminalNames.Any(name => name == "dtterm") ||
                        TerminalNames.Any(name => name == "linux") ||
                        TerminalNames.Any(name => name == "rxvt") ||
                        TerminalNames.Any(name => name.StartsWith("xterm", StringComparison.Ordinal)))
                    {
                        _extendedStringCapabilities.Add(_pseudoCapabilityNameSetTitle, "\u001b]0;%p1%s\u0007");
                    }
                    else if (TerminalNames.Any(name => name == "cygwin"))
                    {
                        _extendedStringCapabilities.Add(_pseudoCapabilityNameSetTitle, "\u001b];%p1%s\u0007");
                    }
                    else if (TerminalNames.Any(name => name == "konsole"))
                    {
                        _extendedStringCapabilities.Add(_pseudoCapabilityNameSetTitle, "\u001b]30;%p1%s\u0007");
                    }
                    else if (TerminalNames.Any(name => name.StartsWith("screen", StringComparison.Ordinal)))
                    {
                        _extendedStringCapabilities.Add(_pseudoCapabilityNameSetTitle, "\u001bk%p1%s\u001b\\");
                    }
                }
                #endregion

                #region Add pseudo-capability "__reset_color"
                {
                    // Add pseudo-capability "__reset_color"

                    // If the terminal understands the ANSI escape code for resetting foreground/background colors, register the pseudocapability.
                    // The terminal is considered to understand ANSI escape codes for resetting foreground/background colors if any of the following conditions are met:
                    // + If the extended capability "AX" is defined and its value is true.
                    // + If the capabilities "orig_pair" or "orig_color" are defined and the value of at least one of them contains an ANSI escape code to reset the foreground/background color.

                    // The extended capability "AX" is a boolean value indicating whether the terminal understands ANSI escape codes for default foreground/background colors.
                    // See the "Special Terminal Capabilities" entry in "man screen(1)" for more information.

                    if (_extendedBooleanCapabilities.TryGetValue("AX", out var ax) && ax == true)
                    {
                        _extendedStringCapabilities.Add(_pseudoCapabilityNameResetColor, "\u001b[39;49m");
                    }
                    else if (_stringCapabilities.TryGetValue(TermInfoStringCapabilities.OrigPair, out var orig_pair) && _resetColorPattern.IsMatch(orig_pair))
                    {
                        _extendedStringCapabilities.Add(_pseudoCapabilityNameResetColor, orig_pair);
                    }
                    else if (_stringCapabilities.TryGetValue(TermInfoStringCapabilities.OrigColors, out var orig_color) && _resetColorPattern.IsMatch(orig_color))
                    {
                        _extendedStringCapabilities.Add(_pseudoCapabilityNameResetColor, orig_color);
                    }
                }
                #endregion

                #region Add pseudo-capability "__clear_buffer"
                {
                    // Add pseudo-capability "__clear_buffer"
                    //
                    // See the document below for the extended capability "E3".
                    // + https://man7.org/linux/man-pages/man1/clear.1.html#OPTIONS
                    // + https://man7.org/linux/man-pages/man1/clear.1.html#HISTORY
                    // 
                    // See the following documentation for the "xterm" control sequence "\u001b[3J".
                    // + https://www.xfree86.org/current/ctlseqs.html#VT100%20Mode ( search "CSI P s J")
                    // 

                    if (terminalNames.Any(name => name.StartsWith("xterm", StringComparison.Ordinal)))
                    {
                        // If the terminal is "xterm" or a derivative of it, the terminal is assumed to understand "\u001b[3J".
                        _extendedStringCapabilities.Add(_pseudoCapabilityNameEraseScrollBuffer, "\u001b[3J");
                        _extendedStringCapabilities.Add(_pseudoCapabilityNameClearBuffer, "\u001b[2J\u001b[H\u001b[3J");
                    }
                    else if (_extendedStringCapabilities.TryGetValue("E3", out var e3))
                    {
                        // If the terminal has the machine capability "E3" defined, its value is taken as an escape code that clears the console's scroll buffer.

                        _extendedStringCapabilities.Add(_pseudoCapabilityNameEraseScrollBuffer, e3);

                        // Most terminals define E3 as "\u001b[3J".
                        // This escape code does not necessarily clear the entire console buffer,
                        // on some terminals it only clears the scroll buffer outside the console window.
                        // Therefore, E3 must be used in combination with the escape code for erasing the console window
                        // in the following order.
                        //
                        // 1. Value of the capability "clear_screen"
                        // 2. Value of the capability "E3"
                        //
                        // * "clear_screen" is defined as "\u001bH\u001b[2J" in most terminals.
                        //   Experiments have shown that sending "\u001b[3J" before "\u001b[2J" does not completely clear the scroll buffer.
                        //   Therefore, "E3" is written after "clear_screen" in this capability.
                        // * Regarding the difference in behavior of clearing the screen depending on the order of "\u001b[2J" and "\u001b[3J" in the above,
                        //   it is possible to easily check with the linux shell command.
                        //   (e.g. "echo $'\e[H\e[2J\e[3J'")
                        //   See also the example documented at https://github.com/xtermjs/xterm.js/issues/3315#issuecomment-833251913.

                        if (_stringCapabilities.TryGetValue(TermInfoStringCapabilities.ClearScreen, out var clearScreen))
                            _extendedStringCapabilities.Add(_pseudoCapabilityNameClearBuffer, $"{clearScreen}{e3}");
                    }
                }
                #endregion

                #region Add pseudo-capability "__cursor_position_report"
                {
                    // Add pseudo-capability "__cursor_position_report"

                    // Register "\u001b[6n" as a pseudo-capability value if at least one of the following conditions is met:
                    // + The name of the terminal indicates that the terminal understands "\u001b[6n".
                    //   + aixterm: https://sites.ualberta.ca/dept/chemeng/AIX-43/share/man/info/C/a_doc_lib/cmds/aixcmds1/aixterm.htm (search "dsr")
                    //   + dtterm: https://man.freebsd.org/cgi/man.cgi?query=dtterm&sektion=5&manpath=FreeBSD+12.1-RELEASE+and+Ports (search "DSR")
                    //   + screen: https://www.gnu.org/software/screen/manual/html_node/Control-Sequences.html (seach "Cursor Position Report")
                    //   + xterm: https://www.xfree86.org/current/ctlseqs.html (search "DSR")
                    //   + windows-local-console: https://learn.microsoft.com/ja-jp/windows/console/console-virtual-terminal-sequences#query-state (search "DECXCPR")
                    // + Since "\u001b[6n" is used as the value of other capabilities, the terminal should be assumed to understand "\u001b[6n".

                    if (_stringCapabilities.TryGetValue(TermInfoStringCapabilities.User7, out var user7) && _deviceStatusReportPattern.IsMatch(user7) ||
                        terminalNames.Any(name => name == "aixterm") ||
                        terminalNames.Any(name => name == "dtterm") ||
                        terminalNames.Any(name => name.StartsWith("screen", StringComparison.Ordinal)) ||
                        terminalNames.Any(name => name.StartsWith("xterm", StringComparison.Ordinal)) ||
                        terminalNames.Any(name => name == _terminalNameWindowsLocalConsole))
                    {
                        _extendedStringCapabilities.Add(_pseudoCapabilityNameCursorPositionReport, "\u001b[6n");
                    }
                }
                #endregion

                #region Add pseudo-capability "__erase_in_display_1"
                {
                    // Add pseudo-capability "__erase_in_display_1"

                    // The following terminals understand "\u001b[1J".:
                    // + aixterm: https://sites.ualberta.ca/dept/chemeng/AIX-43/share/man/info/C/a_doc_lib/cmds/aixcmds1/aixterm.htm
                    // + dtterm: https://man.freebsd.org/cgi/man.cgi?query=dtterm&sektion=5&manpath=FreeBSD+12.1-RELEASE+and+Ports
                    // + screen: https://www.gnu.org/software/screen/manual/html_node/Control-Sequences.html
                    // + xterm: https://www.xfree86.org/current/ctlseqs.html

                    if (terminalNames.Any(name => name == "aixterm") ||
                        terminalNames.Any(name => name == "dtterm") ||
                        terminalNames.Any(name => name.StartsWith("screen", StringComparison.Ordinal)) ||
                        terminalNames.Any(name => name.StartsWith("xterm", StringComparison.Ordinal)) ||
                        terminalNames.Any(name => name == _terminalNameWindowsLocalConsole))
                    {
                        _extendedStringCapabilities.Add(_pseudoCapabilityNameEraseInDisplay1, "\u001b[1J");
                    }
                }
                #endregion

                #region Add pseudo-capability "__erase_in_line_2"
                {
                    // Add pseudo-capability "__erase_in_line_2"

                    // The following terminals understand "\u001b[2K".:
                    // + aixterm: https://sites.ualberta.ca/dept/chemeng/AIX-43/share/man/info/C/a_doc_lib/cmds/aixcmds1/aixterm.htm
                    // + dtterm: https://man.freebsd.org/cgi/man.cgi?query=dtterm&sektion=5&manpath=FreeBSD+12.1-RELEASE+and+Ports
                    // + screen: https://www.gnu.org/software/screen/manual/html_node/Control-Sequences.html
                    // + xterm: https://www.xfree86.org/current/ctlseqs.html

                    if (terminalNames.Any(name => name == "aixterm") ||
                        terminalNames.Any(name => name == "dtterm") ||
                        terminalNames.Any(name => name.StartsWith("screen", StringComparison.Ordinal)) ||
                        terminalNames.Any(name => name.StartsWith("xterm", StringComparison.Ordinal)) ||
                        terminalNames.Any(name => name == _terminalNameWindowsLocalConsole))
                    {
                        _extendedStringCapabilities.Add(_pseudoCapabilityNameEraseInLine2, "\u001b[2K");
                    }
                }
                #endregion

                #region Detect Capability Conflicts
                {
                    // Detect Capability Conflicts

                    if ((_stringCapabilities.ContainsKey(TermInfoStringCapabilities.SetABackground) ||
                        _stringCapabilities.ContainsKey(TermInfoStringCapabilities.SetAForeground) ||
                        _stringCapabilities.ContainsKey(TermInfoStringCapabilities.SetBackground) ||
                        _stringCapabilities.ContainsKey(TermInfoStringCapabilities.SetForeground)) &&
                        !_extendedStringCapabilities.ContainsKey(_pseudoCapabilityNameResetColor))
                    {
                        _warnings.Add("Either a capability is defined to set the foreground or background color, but no capability is defined to reset them.");
                    }

                    if ((_stringCapabilities.ContainsKey(TermInfoStringCapabilities.SetABackground) ||
                        _stringCapabilities.ContainsKey(TermInfoStringCapabilities.SetAForeground)) &&
                        (!_numberCapabilities.TryGetValue(TermInfoNumberCapabilities.MaxColors, out var maxColors16) ||
                        maxColors16 < 16))
                    {
                        _warnings.Add($"The \"set_a_background\" or \"set_a_foreground\" capabilities are defined, but the \"max_colors\" capability is not defined or the value of the \"max_colors\" capability is less than 16.: max_colors={maxColors16}");
                    }

                    if ((_stringCapabilities.ContainsKey(TermInfoStringCapabilities.SetBackground) ||
                        _stringCapabilities.ContainsKey(TermInfoStringCapabilities.SetForeground)) &&
                        (!_numberCapabilities.TryGetValue(TermInfoNumberCapabilities.MaxColors, out var maxColors8) ||
                        maxColors8 < 8))
                    {
                        _warnings.Add($"The \"set_background\" or \"set_foreground\" capabilities are defined, but the \"max_colors\" capability is not defined or the value of the \"max_colors\" capability is less than 8.: max_colors={maxColors8}");
                    }

                    if (!_extendedStringCapabilities.ContainsKey(_pseudoCapabilityNameSetTitle))
                    {
                        _warnings.Add("The capability to set the window title is not defined.");
                    }

                    if (!_extendedStringCapabilities.ContainsKey(_pseudoCapabilityNameClearBuffer))
                    {
                        _warnings.Add("The capability to clear console buffer is not defined.");
                    }

                    if (!_extendedStringCapabilities.ContainsKey(_pseudoCapabilityNameCursorPositionReport))
                    {
                        _warnings.Add("The capability to report cursor position is not defined.");
                    }
                }
                #endregion
            }
        }

        public string TermInfoFilePath { get; }
        public IReadOnlyArray<string> TerminalNames { get; }
        public bool? GetBooleanCapabilityValue(TermInfoBooleanCapabilities name) => _booleanCapabilities.TryGetValue(name, out var value) ? value : null;
        public bool? GetBooleanCapabilityValue(string name) => _extendedBooleanCapabilities.TryGetValue(name, out var value) ? value : null;
        public int? GetNumberCapabilityValue(TermInfoNumberCapabilities name) => _numberCapabilities.TryGetValue(name, out var value) ? value : null;
        public int? GetNumberCapabilityValue(string name) => _extendedNumberCapabilities.TryGetValue(name, out var value) ? value : null;

        public string? GetStringCapabilityValue(TermInfoStringCapabilities name, params object[] args)
            => !_stringCapabilities.TryGetValue(name, out var value)
                ? null
                : args.Length <= 0
                ? value
                : Expansion.ExpandArguments(value, args);

        public string? GetStringCapabilityValue(string name, params object[] args)
            => !_extendedStringCapabilities.TryGetValue(name, out var value)
                ? null
                : args.Length <= 0
                ? value
                : Expansion.ExpandArguments(value, args);

        public void WriteTerminalnfo(TextWriter writer, int indent)
        {
            var outerIndentSpace = new string(' ', indent);
            writer.WriteLine($"{outerIndentSpace}{{");

            var innerIndentSpace = new string(' ', indent + 2);
            if (!string.IsNullOrEmpty(TermInfoFilePath))
                writer.WriteLine($"{innerIndentSpace}\"terminfo_file_path\" : \"{TermInfoFilePath}\",");
            writer.WriteLine($"{innerIndentSpace}\"terminal_names\" : [ {string.Join(", ", TerminalNames.Select(terminalName => $"\"{terminalName.JsonEncode()}\""))} ],");
            writer.WriteLine($"{innerIndentSpace}\"boolean_capabilities_count\" : {_booleanCapabilities.Count},");
            writer.WriteLine($"{innerIndentSpace}\"number_capabilities_count\" : {_numberCapabilities.Count},");
            writer.WriteLine($"{innerIndentSpace}\"string_capabilities_count\" : {_stringCapabilities.Count},");
            writer.WriteLine($"{innerIndentSpace}\"extended_boolean_capabilities_count\" : {_extendedBooleanCapabilities.Count},");
            writer.WriteLine($"{innerIndentSpace}\"extended_number_capabilities_count\" : {_extendedNumberCapabilities.Count},");
            writer.Write($"{innerIndentSpace}\"extended_string_capabilities_count\" : {_extendedStringCapabilities.Count}");

            foreach (var name in Enum.GetValues<TermInfoBooleanCapabilities>())
            {
                if (_booleanCapabilities.TryGetValue(name, out var value))
                {
                    writer.WriteLine(",");
                    writer.Write(@$"{innerIndentSpace}""{name.ToJsonPropertyName().JsonEncode()}"" : {(value ? "true" : "false")}");
                }
            }

            foreach (var name in Enum.GetValues<TermInfoNumberCapabilities>())
            {
                if (_numberCapabilities.TryGetValue(name, out var value))
                {
                    writer.WriteLine(",");
                    writer.Write(@$"{innerIndentSpace}""{name.ToJsonPropertyName().JsonEncode()}"" : {value}");
                }
            }

            foreach (var name in Enum.GetValues<TermInfoStringCapabilities>())
            {
                if (_stringCapabilities.TryGetValue(name, out var value))
                {
                    writer.WriteLine(",");
                    writer.Write(@$"{innerIndentSpace}""{name.ToJsonPropertyName().JsonEncode()}"" : ""{value.JsonEncode()}""");
                }
            }

            foreach (var item in _extendedBooleanCapabilities.OrderBy(item => item.Key))
            {
                writer.WriteLine(",");
                writer.Write(@$"{innerIndentSpace}""{item.Key.JsonEncode()}"" : {(item.Value ? "true" : "false")}");
            }

            foreach (var item in _extendedNumberCapabilities.OrderBy(item => item.Key))
            {
                writer.WriteLine(",");
                writer.Write(@$"{innerIndentSpace}""{item.Key.JsonEncode()}"" : {item.Value}");
            }

            foreach (var item in _extendedStringCapabilities.OrderBy(item => item.Key))
            {
                writer.WriteLine(",");
                writer.Write(@$"{innerIndentSpace}""{item.Key.JsonEncode()}"" : ""{item.Value.JsonEncode()}""");
            }

            if (_warnings.Any())
            {
                writer.WriteLine(",");
                writer.WriteLine(@$"{innerIndentSpace}""{"warnings".JsonEncode()}"" : [");
                var innerIndentSpace2 = new string(' ', indent + 4);
                var isFirst = true;
                foreach (var warning in _warnings)
                {
                    if (!isFirst)
                        writer.WriteLine(",");

                    writer.Write(@$"{innerIndentSpace2}""{warning.JsonEncode()}""");

                    isFirst = false;
                }

                writer.WriteLine("");
                writer.Write(@$"{innerIndentSpace}]");
            }

            writer.WriteLine("");
            writer.Write($"{outerIndentSpace}}}");
        }

        public static TerminalInfoDatabase? GetTerminalDatabase(bool includeUniqueCapabilities)
        {
            var terminalName = GetTerminalName();
            var terminalInfoResolvers = new[]
            {
                () => terminalName is not null ? FindTerminalDatabale(terminalName, includeUniqueCapabilities) : null,
                () => GetTerminalDatabaleForWindows(includeUniqueCapabilities),
            };
            return terminalInfoResolvers.Select(resolver => resolver()).Where(terminalInfo => terminalInfo is not null).FirstOrDefault();
        }

        public static TerminalInfoDatabase? FindTerminalDatabale(string terminalName, bool includeUniqueCapabilities)
        {
            foreach (var termInfoFile in EnumerateTermInfoFiles(terminalName))
            {
                var database = TryReadTerminalDatabale(termInfoFile, includeUniqueCapabilities);
                if (database is not null)
                    return database;
            }

            return null;
        }

        public static TerminalInfoDatabase ReadTerminalDatabale(FileInfo terminfoFile, bool includeUniqueCapabilities)
        {
            using var inStream = new FileStream(terminfoFile.FullName, FileMode.Open, FileAccess.Read);

            // ヘッダーの読み込み

            var headerValue = inStream.ReadInt16Le();
            var is32bitInteger =
                headerValue switch
                {
                    _legacyMagicNumber => false,
                    _32bitMagicNumber => true,
                    _ => throw new Exception($"Invalid file format (Bad header value: 0x{headerValue:x4})"),
                };
            var terminalNameSectionBytes = inStream.ReadInt16Le();
            if (terminalNameSectionBytes < 0)
                throw new Exception("Bad file format.");
            var booleanSectionCount = inStream.ReadInt16Le();
            if (booleanSectionCount < 0)
                throw new Exception("Bad file format.");
            var numberSectionCount = inStream.ReadInt16Le();
            if (numberSectionCount < 0)
                throw new Exception("Bad file format.");
            var stringSectionOffsetCount = inStream.ReadInt16Le();
            if (stringSectionOffsetCount < 0)
                throw new Exception("Bad file format.");
            var stringSectionTableBytes = inStream.ReadInt16Le();
            if (stringSectionTableBytes < 0)
                throw new Exception("Bad file format.");

            // レガシーデータの読み込み

            var terminalNames =
                ReadTerminalNames(inStream, terminalNameSectionBytes)
                .ToList();
            var booleanCapabilityValues =
                ReadBooleanCapabilityValues(inStream, booleanSectionCount)
                .ToDictionary(item => item.index, item => item.value);

            // 2 バイトバウンダリに調整
            if (inStream.Position % 2 != 0)
                _ = inStream.ReadByte();

            var numberCapabilityValues =
                ReadNumberCapabilityValues(inStream, numberSectionCount, is32bitInteger)
                .ToDictionary(item => item.index, item => item.value);

            var stringSectionOffsets =
                ReadStringSectionOffsets(inStream, stringSectionOffsetCount)
                .ToArray();
            var stringCapabilityValues =
                ReadStringCapabilityValues(inStream, stringSectionTableBytes, stringSectionOffsets)
                .ToDictionary(item => item.index, item => item.value);

            // 2 バイトバウンダリに調整
            if (inStream.Position % 2 != 0)
                _ = inStream.ReadByte();

            // 拡張データヘッダの読み込み

            var extendedBooleanCapabilitesCount = inStream.ReadInt16Le();
            if (extendedBooleanCapabilitesCount < 0)
                throw new Exception("Bad file format.");
            var extendedNumberCapabilitesCount = inStream.ReadInt16Le();
            if (extendedNumberCapabilitesCount < 0)
                throw new Exception("Bad file format.");
            var extendedStringCapabilitesCount = inStream.ReadInt16Le();
            if (extendedStringCapabilitesCount < 0)
                throw new Exception("Bad file format.");
            var extendedStringOffsetCount = inStream.ReadInt16Le();
            if (extendedStringOffsetCount < 0)
                throw new Exception("Bad file format.");
            var extendedStringTableBytes = inStream.ReadInt16Le();
            if (extendedStringTableBytes < 0)
                throw new Exception("Bad file format.");

            // 拡張データの読み込み

            var extendedBooleanCapabilityValues =
                ReadExtendedBooleanCapabilityValues(inStream, extendedBooleanCapabilitesCount)
                .ToList();

            // 2 バイトバウンダリに調整
            if (inStream.Position % 2 != 0)
                _ = inStream.ReadByte();

            var extendedNumberCapabilityValues =
                ReadExtendedNumberCapabilityValues(inStream, extendedNumberCapabilitesCount, is32bitInteger)
                .ToList();

            var extendedStringOffsets = ReadExtendedStringSectionOffsets(inStream, extendedStringOffsetCount).ToArray();
            var extendedStringCapabilityValueOffsets = extendedStringOffsets.AsMemory(0, extendedStringCapabilitesCount);

            var extendedCapabilityNameOffsets = extendedStringOffsets.AsMemory(extendedStringCapabilitesCount);

            var extendedStringTableBuffer = new byte[extendedStringTableBytes].AsMemory();
            inStream.ReadBytes(extendedStringTableBuffer.Span);

            var extendedStringCapabilityValueItems =
                ReadExtendedStringValues(
                    extendedStringTableBuffer,
                    extendedStringCapabilityValueOffsets).ToList();
            var extendedStringCapabilityValues =
                extendedStringCapabilityValueItems
                .Select(item => item.value)
                .ToList();

            var extendedStringCapabilityValuesLength =
                extendedStringCapabilityValueItems
                .Sum(item => item.length);
            var extendedCapabilityNamesBuffer =
                extendedStringTableBuffer[extendedStringCapabilityValuesLength..];

            var extendedBooleanCapabilityNames =
                ReadExtendedStringValues(
                    extendedCapabilityNamesBuffer,
                    extendedCapabilityNameOffsets[..extendedBooleanCapabilityValues.Count]);
            extendedCapabilityNameOffsets =
                extendedCapabilityNameOffsets[extendedBooleanCapabilityValues.Count..];

            var extendedNumberCapabilityNames =
                ReadExtendedStringValues(
                    extendedCapabilityNamesBuffer,
                    extendedCapabilityNameOffsets[..extendedNumberCapabilityValues.Count]);
            extendedCapabilityNameOffsets =
                extendedCapabilityNameOffsets[extendedNumberCapabilityValues.Count..];

            var extendedStringCapabilityNames =
                ReadExtendedStringValues(
                    extendedCapabilityNamesBuffer,
                    extendedCapabilityNameOffsets[..extendedStringCapabilityValues.Count]);
            extendedCapabilityNameOffsets =
                extendedCapabilityNameOffsets[extendedStringCapabilityValues.Count..];

            var extendedBooleanCapabilities =
                extendedBooleanCapabilityNames
                .Zip(extendedBooleanCapabilityValues, (name, value) => (name, value))
                .ToDictionary(item => item.name.value, item => item.value);
            var extendedNumberCapabilities =
                extendedNumberCapabilityNames
                .Zip(extendedNumberCapabilityValues, (name, value) => (name, value))
                .ToDictionary(item => item.name.value, item => item.value);
            var extendedStringCapabilities =
                extendedStringCapabilityNames
                .Zip(extendedStringCapabilityValues, (name, value) => (name, value))
                .ToDictionary(item => item.name.value, item => item.value);

            return new TerminalInfoDatabase(
                terminfoFile.FullName,
                includeUniqueCapabilities ? terminalNames.Where(name => name != terminfoFile.Name).Prepend(terminfoFile.Name) : terminalNames,
                booleanCapabilityValues,
                numberCapabilityValues,
                stringCapabilityValues,
                extendedBooleanCapabilities,
                extendedNumberCapabilities,
                extendedStringCapabilities,
                includeUniqueCapabilities);
        }

        public static void WriteAllTerminalInfos(TextWriter writer, int indent, bool includeUniqueCapabilities)
        {
            var indentSpace = new string(' ', indent);

            writer.WriteLine($"{indentSpace}[");
            var shouldBeNewlineAtLast = false;
            var isFirstItem = true;
            foreach (var termInfo in EnumerateTerminalInfoDatabase(includeUniqueCapabilities))
            {
                if (termInfo is not null)
                {
                    if (!isFirstItem)
                        writer.WriteLine(",");
                    termInfo.WriteTerminalnfo(writer, indent + 2);
                    shouldBeNewlineAtLast = true;
                    isFirstItem = false;
                }
            }

            if (shouldBeNewlineAtLast)
                writer.WriteLine("");
            writer.Write($"{indentSpace}]");
        }

        public static IEnumerable<TerminalInfoDatabase> EnumerateTerminalInfoDatabase(bool includeUniqueCapabilities)
        {
            var termInfoDirectories = GetTermInfoDirectories().ToList();
            var termInfoFullPathNames
                = termInfoDirectories
                .SelectMany(termInfoDirectory =>
                {
                    try
                    {
                        return Directory.EnumerateFiles(termInfoDirectory, "*", SearchOption.AllDirectories);
                    }
                    catch (Exception)
                    {
                        return Array.Empty<string>();
                    }
                });
            foreach (var path in termInfoFullPathNames)
            {
                if (File.Exists(path))
                {
                    var terminalInfo = TryReadTerminalDatabale(new FileInfo(path), includeUniqueCapabilities);
                    if (terminalInfo is not null)
                        yield return terminalInfo;
                }
            }

            var windowsTerminalInfo = GetTerminalDatabaleForWindows(includeUniqueCapabilities);
            if (windowsTerminalInfo is not null)
                yield return windowsTerminalInfo;
        }

        private static string? GetTerminalName()
            => Environment.GetEnvironmentVariable("TERM");

        private static IEnumerable<FileInfo> EnumerateTermInfoFiles(string terminalName)
        {
            var termInfoDirectories = GetTermInfoDirectories().ToList();
            var termInfoRelativePathNames = GetTerminalInfoRelativePathNames(terminalName).ToList();
            var termInfoFullPathNames = termInfoDirectories.SelectMany(termInfoDirectory => termInfoRelativePathNames, Path.Combine);
            foreach (var path in termInfoFullPathNames)
            {
                if (File.Exists(path))
                    yield return new FileInfo(path);
            }
        }

        private static IEnumerable<string> GetTermInfoDirectories()
        {
            var termInfoDirectories = Array.Empty<string>().AsEnumerable();
            var termInfoDir = Environment.GetEnvironmentVariable("TERMINFO");
            if (termInfoDir is not null)
                termInfoDirectories = termInfoDirectories.Append(termInfoDir);
            var homeEnvironmentValue = Environment.GetEnvironmentVariable("HOME");
            if (homeEnvironmentValue is not null)
                termInfoDirectories = termInfoDirectories.Append(Path.Combine(homeEnvironmentValue, ".terminfo"));
            var homeDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrEmpty(homeDirectoryPath))
                termInfoDirectories = termInfoDirectories.Append(Path.Combine(homeDirectoryPath, ".terminfo"));
            var termInfoDirs = Environment.GetEnvironmentVariable("TERMINFO_DIRS");
            if (termInfoDirs is not null)
                termInfoDirectories = termInfoDirectories.Concat(termInfoDirs.Split(':'));
            if (!OperatingSystem.IsWindows())
            {
                termInfoDirectories =
                    termInfoDirectories
                    .Concat(new[]
                    {
                        "/usr/share/misc/terminfo",
                        "/usr/share/terminfo",
                        "/usr/share/lib/terminfo",
                        "/usr/lib/terminfo",
                        "/usr/local/share/terminfo",
                        "/usr/local/share/lib/terminfo",
                        "/usr/local/lib/terminfo",
                        "/usr/local/ncurses/lib/terminfo",
                        "/lib/terminfo",
                        "/etc/terminfo",
                    });
            }

            return
                termInfoDirectories
                .Distinct()
                .Where(Directory.Exists);
        }

        private static IEnumerable<string> GetTerminalInfoRelativePathNames(string terminalName)
        {
            var relativePathNames = new[] { terminalName }.AsEnumerable();
            if (terminalName.Length >= 1)
            {
                relativePathNames =
                    relativePathNames
                    .Concat(new[]
                    {
                        Path.Combine(terminalName[..1], terminalName),
                        Path.Combine(((int)terminalName[0]).ToString(), terminalName),
                    });
            }

            if (terminalName.Length >= 2)
                relativePathNames = relativePathNames.Append(Path.Combine(terminalName[..2], terminalName));
            return relativePathNames;
        }

        private static TerminalInfoDatabase? TryReadTerminalDatabale(FileInfo termInfoFile, bool includeUniqueCapabilities)
        {
            try
            {
                return ReadTerminalDatabale(termInfoFile, includeUniqueCapabilities);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static TerminalInfoDatabase? GetTerminalDatabaleForWindows(bool includeUniqueCapabilities)
        {
            if (!OperatingSystem.IsWindows())
                return null;

            var termInfoFilePath = "";
            var terminalNames = new[] { _terminalNameWindowsLocalConsole };
            var booleanCapabilities = new Dictionary<TermInfoBooleanCapabilities, bool>
            {
                { TermInfoBooleanCapabilities.AutoLeftMargin, false },
                { TermInfoBooleanCapabilities.AutoRightMargin, true },
                { TermInfoBooleanCapabilities.BackspacesWithBs, true },
                { TermInfoBooleanCapabilities.BackColorErase, true },
                { TermInfoBooleanCapabilities.CanChange, true },
                { TermInfoBooleanCapabilities.CeolStandoutGlitch, false },
                { TermInfoBooleanCapabilities.ColAddrGlitch, false },
                { TermInfoBooleanCapabilities.CpiChangesRes, false },
                { TermInfoBooleanCapabilities.CrCancelsMicroMode, false },
                { TermInfoBooleanCapabilities.DestTabsMagicSmso, false },
                { TermInfoBooleanCapabilities.EatNewlineGlitch, true },
                { TermInfoBooleanCapabilities.EraseOverstrike, false },
                { TermInfoBooleanCapabilities.GenericType, false },
                { TermInfoBooleanCapabilities.HardCopy, false },
                { TermInfoBooleanCapabilities.HardCursor, false },
                { TermInfoBooleanCapabilities.HasMetaKey, true },
                { TermInfoBooleanCapabilities.HasPrintWheel, false },
                { TermInfoBooleanCapabilities.HasStatusLine, false },
                { TermInfoBooleanCapabilities.HueLightnessSaturation, false },
                { TermInfoBooleanCapabilities.InsertNullGlitch, false },
                { TermInfoBooleanCapabilities.LpiChangesRes, false },
                { TermInfoBooleanCapabilities.MemoryAbove, false },
                { TermInfoBooleanCapabilities.MemoryBelow, false },
                { TermInfoBooleanCapabilities.MoveInsertMode, true },
                { TermInfoBooleanCapabilities.MoveStandoutMode, true },
                { TermInfoBooleanCapabilities.NeedsXonXoff, false },
                { TermInfoBooleanCapabilities.NonDestScrollRegion, false },
                { TermInfoBooleanCapabilities.NonRevRmcup, false },
                { TermInfoBooleanCapabilities.NoEscCtlc, false },
                { TermInfoBooleanCapabilities.NoPadChar, true },
                { TermInfoBooleanCapabilities.OverStrike, false },
                { TermInfoBooleanCapabilities.PrtrSilent, true },
                { TermInfoBooleanCapabilities.RowAddrGlitch, false },
                { TermInfoBooleanCapabilities.SemiAutoRightMargin, false },
                { TermInfoBooleanCapabilities.StatusLineEscOk, false },
                { TermInfoBooleanCapabilities.TildeGlitch, false },
                { TermInfoBooleanCapabilities.TransparentUnderline, false },
                { TermInfoBooleanCapabilities.XonXoff, false },
            };
            var numberCapabilities = new Dictionary<TermInfoNumberCapabilities, int>
            {
                { TermInfoNumberCapabilities.Columns, 80 },
                { TermInfoNumberCapabilities.InitTabs, 8 },
                { TermInfoNumberCapabilities.Lines, 24 },
                { TermInfoNumberCapabilities.MaxColors, 256 },
                { TermInfoNumberCapabilities.MaxPairs, 65536 },
            };
            var stringCapabilities = new Dictionary<TermInfoStringCapabilities, string>
            {
                { TermInfoStringCapabilities.AcsChars, "``aaffggiijjkkllmmnnooppqqrrssttuuvvwwxxyyzz{{||}}~~" },
                { TermInfoStringCapabilities.BackTab, "\u001b[Z" },
                { TermInfoStringCapabilities.Bell, "\a" },
                { TermInfoStringCapabilities.CarriageReturn, "\r" },
                { TermInfoStringCapabilities.ChangeScrollRegion, "\u001b[%i%p1%d;%p2%dr" },
                { TermInfoStringCapabilities.ClearAllTabs, "\u001b[3g" },
                { TermInfoStringCapabilities.ClearScreen, "\u001b[H\u001b[2J" },
                { TermInfoStringCapabilities.ClrBol, "\u001b[1K" },
                { TermInfoStringCapabilities.ClrEol, "\u001b[K" },
                { TermInfoStringCapabilities.ClrEos, "\u001b[J" },
                { TermInfoStringCapabilities.ColumnAddress, "\u001b[%i%p1%dG" },
                { TermInfoStringCapabilities.CursorAddress, "\u001b[%i%p1%d;%p2%dH" },
                { TermInfoStringCapabilities.CursorDown, "\n" },
                { TermInfoStringCapabilities.CursorHome, "\u001b[H" },
                { TermInfoStringCapabilities.CursorInvisible, "\u001b[?25l" },
                { TermInfoStringCapabilities.CursorLeft, "\b" },
                { TermInfoStringCapabilities.CursorNormal, "\u001b[?12l\u001b[?25h" },
                { TermInfoStringCapabilities.CursorRight, "\u001b[C" },
                { TermInfoStringCapabilities.CursorUp, "\u001b[A" },
                { TermInfoStringCapabilities.CursorVisible, "\u001b[?12;25h" },
                { TermInfoStringCapabilities.DeleteCharacter, "\u001b[P" },
                { TermInfoStringCapabilities.DeleteLine, "\u001b[M" },
                { TermInfoStringCapabilities.EnterAltCharsetMode, "\u001b(0" },
                { TermInfoStringCapabilities.EnterAmMode, "\u001b[?7h" },
                { TermInfoStringCapabilities.EnterBlinkMode, "\u001b[5m" },
                { TermInfoStringCapabilities.EnterBoldMode, "\u001b[1m" },
                { TermInfoStringCapabilities.EnterCaMode, "\u001b[?1049h\u001b[22;0;0t" },
                { TermInfoStringCapabilities.EnterDimMode, "\u001b[2m" },
                { TermInfoStringCapabilities.EnterInsertMode, "\u001b[4h" },
                { TermInfoStringCapabilities.EnterItalicsMode, "\u001b[3m" },
                { TermInfoStringCapabilities.EnterReverseMode, "\u001b[7m" },
                { TermInfoStringCapabilities.EnterSecureMode, "\u001b[8m" },
                { TermInfoStringCapabilities.EnterStandoutMode, "\u001b[7m" },
                { TermInfoStringCapabilities.EnterUnderlineMode, "\u001b[4m" },
                { TermInfoStringCapabilities.EraseChars, "\u001b[%p1%dX" },
                { TermInfoStringCapabilities.ExitAltCharsetMode, "\u001b(B" },
                { TermInfoStringCapabilities.ExitAmMode, "\u001b[?7l" },
                { TermInfoStringCapabilities.ExitAttributeMode, "\u001b(B\u001b[m" },
                { TermInfoStringCapabilities.ExitCaMode, "\u001b[?1049l\u001b[23;0;0t" },
                { TermInfoStringCapabilities.ExitInsertMode, "\u001b[4l" },
                { TermInfoStringCapabilities.ExitItalicsMode, "\u001b[23m" },
                { TermInfoStringCapabilities.ExitStandoutMode, "\u001b[27m" },
                { TermInfoStringCapabilities.ExitUnderlineMode, "\u001b[24m" },
                { TermInfoStringCapabilities.FlashScreen, "\u001b[?5h$<100/>\u001b[?5l" },
                { TermInfoStringCapabilities.InitializeColor, "\u001b]4;%p1%d;rgb:%p2%{255}%*%{1000}%/%2.2X/%p3%{255}%*%{1000}%/%2.2X/%p4%{255}%*%{1000}%/%2.2X\u001b\\" },
                { TermInfoStringCapabilities.Init_2string, "\u001b[!p\u001b[?3;4l\u001b[4l\u001b>" },
                { TermInfoStringCapabilities.InsertLine, "\u001b[L" },
                { TermInfoStringCapabilities.KeyB2, "\u001bOE" },
                { TermInfoStringCapabilities.KeyBackspace, "\u007f" },
                { TermInfoStringCapabilities.KeyBtab, "\u001b[Z" },
                { TermInfoStringCapabilities.KeyDc, "\u001b[3~" },
                { TermInfoStringCapabilities.KeyDown, "\u001bOB" },
                { TermInfoStringCapabilities.KeyEnd, "\u001bOF" },
                { TermInfoStringCapabilities.KeyEnter, "\u001bOM" },
                { TermInfoStringCapabilities.KeyF1, "\u001bOP" },
                { TermInfoStringCapabilities.KeyF2, "\u001bOQ" },
                { TermInfoStringCapabilities.KeyF3, "\u001bOR" },
                { TermInfoStringCapabilities.KeyF4, "\u001bOS" },
                { TermInfoStringCapabilities.KeyF5, "\u001b[15~" },
                { TermInfoStringCapabilities.KeyF6, "\u001b[17~" },
                { TermInfoStringCapabilities.KeyF7, "\u001b[18~" },
                { TermInfoStringCapabilities.KeyF8, "\u001b[19~" },
                { TermInfoStringCapabilities.KeyF9, "\u001b[20~" },
                { TermInfoStringCapabilities.KeyF10, "\u001b[21~" },
                { TermInfoStringCapabilities.KeyF11, "\u001b[23~" },
                { TermInfoStringCapabilities.KeyF12, "\u001b[24~" },
                { TermInfoStringCapabilities.KeyF13, "\u001b[1;2P" },
                { TermInfoStringCapabilities.KeyF14, "\u001b[1;2Q" },
                { TermInfoStringCapabilities.KeyF15, "\u001b[1;2R" },
                { TermInfoStringCapabilities.KeyF16, "\u001b[1;2S" },
                { TermInfoStringCapabilities.KeyF17, "\u001b[15;2~" },
                { TermInfoStringCapabilities.KeyF18, "\u001b[17;2~" },
                { TermInfoStringCapabilities.KeyF19, "\u001b[18;2~" },
                { TermInfoStringCapabilities.KeyF20, "\u001b[19;2~" },
                { TermInfoStringCapabilities.KeyF21, "\u001b[20;2~" },
                { TermInfoStringCapabilities.KeyF22, "\u001b[21;2~" },
                { TermInfoStringCapabilities.KeyF23, "\u001b[23;2~" },
                { TermInfoStringCapabilities.KeyF24, "\u001b[24;2~" },
                { TermInfoStringCapabilities.KeyF25, "\u001b[1;5P" },
                { TermInfoStringCapabilities.KeyF26, "\u001b[1;5Q" },
                { TermInfoStringCapabilities.KeyF27, "\u001b[1;5R" },
                { TermInfoStringCapabilities.KeyF28, "\u001b[1;5S" },
                { TermInfoStringCapabilities.KeyF29, "\u001b[15;5~" },
                { TermInfoStringCapabilities.KeyF30, "\u001b[17;5~" },
                { TermInfoStringCapabilities.KeyF31, "\u001b[18;5~" },
                { TermInfoStringCapabilities.KeyF32, "\u001b[19;5~" },
                { TermInfoStringCapabilities.KeyF33, "\u001b[20;5~" },
                { TermInfoStringCapabilities.KeyF34, "\u001b[21;5~" },
                { TermInfoStringCapabilities.KeyF35, "\u001b[23;5~" },
                { TermInfoStringCapabilities.KeyF36, "\u001b[24;5~" },
                { TermInfoStringCapabilities.KeyF37, "\u001b[1;6P" },
                { TermInfoStringCapabilities.KeyF38, "\u001b[1;6Q" },
                { TermInfoStringCapabilities.KeyF39, "\u001b[1;6R" },
                { TermInfoStringCapabilities.KeyF40, "\u001b[1;6S" },
                { TermInfoStringCapabilities.KeyF41, "\u001b[15;6~" },
                { TermInfoStringCapabilities.KeyF42, "\u001b[17;6~" },
                { TermInfoStringCapabilities.KeyF43, "\u001b[18;6~" },
                { TermInfoStringCapabilities.KeyF44, "\u001b[19;6~" },
                { TermInfoStringCapabilities.KeyF45, "\u001b[20;6~" },
                { TermInfoStringCapabilities.KeyF46, "\u001b[21;6~" },
                { TermInfoStringCapabilities.KeyF47, "\u001b[23;6~" },
                { TermInfoStringCapabilities.KeyF48, "\u001b[24;6~" },
                { TermInfoStringCapabilities.KeyF49, "\u001b[1;3P" },
                { TermInfoStringCapabilities.KeyF50, "\u001b[1;3Q" },
                { TermInfoStringCapabilities.KeyF51, "\u001b[1;3R" },
                { TermInfoStringCapabilities.KeyF52, "\u001b[1;3S" },
                { TermInfoStringCapabilities.KeyF53, "\u001b[15;3~" },
                { TermInfoStringCapabilities.KeyF54, "\u001b[17;3~" },
                { TermInfoStringCapabilities.KeyF55, "\u001b[18;3~" },
                { TermInfoStringCapabilities.KeyF56, "\u001b[19;3~" },
                { TermInfoStringCapabilities.KeyF57, "\u001b[20;3~" },
                { TermInfoStringCapabilities.KeyF58, "\u001b[21;3~" },
                { TermInfoStringCapabilities.KeyF59, "\u001b[23;3~" },
                { TermInfoStringCapabilities.KeyF60, "\u001b[24;3~" },
                { TermInfoStringCapabilities.KeyF61, "\u001b[1;4P" },
                { TermInfoStringCapabilities.KeyF62, "\u001b[1;4Q" },
                { TermInfoStringCapabilities.KeyF63, "\u001b[1;4R" },
                { TermInfoStringCapabilities.KeyHome, "\u001bOH" },
                { TermInfoStringCapabilities.KeyIc, "\u001b[2~" },
                { TermInfoStringCapabilities.KeyLeft, "\u001bOD" },
                { TermInfoStringCapabilities.KeyMouse, "\u001b[M" },
                { TermInfoStringCapabilities.KeyNpage, "\u001b[6~" },
                { TermInfoStringCapabilities.KeypadLocal, "\u001b[?1l\u001b>" },
                { TermInfoStringCapabilities.KeypadXmit, "\u001b[?1h\u001b=" },
                { TermInfoStringCapabilities.KeyPpage, "\u001b[5~" },
                { TermInfoStringCapabilities.KeyRight, "\u001bOC" },
                { TermInfoStringCapabilities.KeySdc, "\u001b[3;2~" },
                { TermInfoStringCapabilities.KeySend, "\u001b[1;2F" },
                { TermInfoStringCapabilities.KeySf, "\u001b[1;2B" },
                { TermInfoStringCapabilities.KeyShome, "\u001b[1;2H" },
                { TermInfoStringCapabilities.KeySic, "\u001b[2;2~" },
                { TermInfoStringCapabilities.KeySleft, "\u001b[1;2D" },
                { TermInfoStringCapabilities.KeySnext, "\u001b[6;2~" },
                { TermInfoStringCapabilities.KeySprevious, "\u001b[5;2~" },
                { TermInfoStringCapabilities.KeySr, "\u001b[1;2A" },
                { TermInfoStringCapabilities.KeySright, "\u001b[1;2C" },
                { TermInfoStringCapabilities.KeyUp, "\u001bOA" },
                { TermInfoStringCapabilities.MemoryLock, "\u001bl" },
                { TermInfoStringCapabilities.MemoryUnlock, "\u001bm" },
                { TermInfoStringCapabilities.MetaOff, "\u001b[?1034l" },
                { TermInfoStringCapabilities.MetaOn, "\u001b[?1034h" },
                { TermInfoStringCapabilities.OrigColors, "\u001b]104\u0007" },
                { TermInfoStringCapabilities.OrigPair, "\u001b[39;49m" },
                { TermInfoStringCapabilities.ParmDch, "\u001b[%p1%dP" },
                { TermInfoStringCapabilities.ParmDeleteLine, "\u001b[%p1%dM" },
                { TermInfoStringCapabilities.ParmDownCursor, "\u001b[%p1%dB" },
                { TermInfoStringCapabilities.ParmIch, "\u001b[%p1%d@" },
                { TermInfoStringCapabilities.ParmIndex, "\u001b[%p1%dS" },
                { TermInfoStringCapabilities.ParmInsertLine, "\u001b[%p1%dL" },
                { TermInfoStringCapabilities.ParmLeftCursor, "\u001b[%p1%dD" },
                { TermInfoStringCapabilities.ParmRightCursor, "\u001b[%p1%dC" },
                { TermInfoStringCapabilities.ParmRindex, "\u001b[%p1%dT" },
                { TermInfoStringCapabilities.ParmUpCursor, "\u001b[%p1%dA" },
                { TermInfoStringCapabilities.PrintScreen, "\u001b[i" },
                { TermInfoStringCapabilities.PrtrOff, "\u001b[4i" },
                { TermInfoStringCapabilities.PrtrOn, "\u001b[5i" },
                { TermInfoStringCapabilities.Reset_1string, "\u001bc\u001b]104\u0007" },
                { TermInfoStringCapabilities.Reset_2string, "\u001b[!p\u001b[?3;4l\u001b[4l\u001b>" },
                { TermInfoStringCapabilities.RestoreCursor, "\u001b8" },
                { TermInfoStringCapabilities.RowAddress, "\u001b[%i%p1%dd" },
                { TermInfoStringCapabilities.SaveCursor, "\u001b7" },
                { TermInfoStringCapabilities.ScrollForward, "\n" },
                { TermInfoStringCapabilities.ScrollReverse, "\u001bM" },
                { TermInfoStringCapabilities.SetABackground, "\u001b[%?%p1%{8}%<%t4%p1%d%e%p1%{16}%<%t10%p1%{8}%-%d%e48;5;%p1%d%;m" },
                { TermInfoStringCapabilities.SetAForeground, "\u001b[%?%p1%{8}%<%t3%p1%d%e%p1%{16}%<%t9%p1%{8}%-%d%e38;5;%p1%d%;m" },
                { TermInfoStringCapabilities.SetAttributes, "%?%p9%t\u001b(0%e\u001b(B%;\u001b[0%?%p6%t;1%;%?%p5%t;2%;%?%p2%t;4%;%?%p1%p3%|%t;7%;%?%p4%t;5%;%?%p7%t;8%;m" },
                { TermInfoStringCapabilities.SetTab, "\u001bH" },
                { TermInfoStringCapabilities.Tab, "\t" },
                { TermInfoStringCapabilities.User6, "\u001b[%i%d;%dR" },
                { TermInfoStringCapabilities.User7, "\u001b[6n" },
                { TermInfoStringCapabilities.User8, "\u001b[?%[;0123456789]c" },
                { TermInfoStringCapabilities.User9, "\u001b[c" },
            };
            var extendedBooleanCapabilities = new Dictionary<string, bool>
            {
                { "AX", true },
                { "XT", true },
            };
            var extendedNumberCapabilities = new Dictionary<string, int>();
            var extendedStringCapabilities = new Dictionary<string, string>
            {
                { "Cr", "\u001b]112\u0007" },
                { "Cs", "\u001b]12;%p1%s\u0007" },
                { "E3", "\u001b[3J" },
                { "kDC3", "\u001b[3;3~" },
                { "kDC4", "\u001b[3;4~" },
                { "kDC5", "\u001b[3;5~" },
                { "kDC6", "\u001b[3;6~" },
                { "kDC7", "\u001b[3;7~" },
                { "kDN", "\u001b[1;2B" },
                { "kDN3", "\u001b[1;3B" },
                { "kDN4", "\u001b[1;4B" },
                { "kDN5", "\u001b[1;5B" },
                { "kDN6", "\u001b[1;6B" },
                { "kDN7", "\u001b[1;7B" },
                { "kEND3", "\u001b[1;3F" },
                { "kEND4", "\u001b[1;4F" },
                { "kEND5", "\u001b[1;5F" },
                { "kEND6", "\u001b[1;6F" },
                { "kEND7", "\u001b[1;7F" },
                { "kHOM3", "\u001b[1;3H" },
                { "kHOM4", "\u001b[1;4H" },
                { "kHOM5", "\u001b[1;5H" },
                { "kHOM6", "\u001b[1;6H" },
                { "kHOM7", "\u001b[1;7H" },
                { "kIC3", "\u001b[2;3~" },
                { "kIC4", "\u001b[2;4~" },
                { "kIC5", "\u001b[2;5~" },
                { "kIC6", "\u001b[2;6~" },
                { "kIC7", "\u001b[2;7~" },
                { "kLFT3", "\u001b[1;3D" },
                { "kLFT4", "\u001b[1;4D" },
                { "kLFT5", "\u001b[1;5D" },
                { "kLFT6", "\u001b[1;6D" },
                { "kLFT7", "\u001b[1;7D" },
                { "kNXT3", "\u001b[6;3~" },
                { "kNXT4", "\u001b[6;4~" },
                { "kNXT5", "\u001b[6;5~" },
                { "kNXT6", "\u001b[6;6~" },
                { "kNXT7", "\u001b[6;7~" },
                { "kPRV3", "\u001b[5;3~" },
                { "kPRV4", "\u001b[5;4~" },
                { "kPRV5", "\u001b[5;5~" },
                { "kPRV6", "\u001b[5;6~" },
                { "kPRV7", "\u001b[5;7~" },
                { "kRIT3", "\u001b[1;3C" },
                { "kRIT4", "\u001b[1;4C" },
                { "kRIT5", "\u001b[1;5C" },
                { "kRIT6", "\u001b[1;6C" },
                { "kRIT7", "\u001b[1;7C" },
                { "kUP", "\u001b[1;2A" },
                { "kUP3", "\u001b[1;3A" },
                { "kUP4", "\u001b[1;4A" },
                { "kUP5", "\u001b[1;5A" },
                { "kUP6", "\u001b[1;6A" },
                { "kUP7", "\u001b[1;7A" },
                { "Ms", "\u001b]52;%p1%s;%p2%s\u0007" },
                { "rmxx", "\u001b[29m" },
                { "Se", "\u001b[2 q" },
                { "smxx", "\u001b[9m" },
                { "Ss", "\u001b[%p1%d q" },
                { "TS", "\u001b]2;" },
            };
            return
                new TerminalInfoDatabase(
                    termInfoFilePath,
                    terminalNames,
                    booleanCapabilities,
                    numberCapabilities,
                    stringCapabilities,
                    extendedBooleanCapabilities,
                    extendedNumberCapabilities,
                    extendedStringCapabilities,
                    includeUniqueCapabilities);
        }

        private static IEnumerable<string> ReadTerminalNames(FileStream inStream, short nameSectionBytes)
        {
            var terminalNamesBuffer = new byte[nameSectionBytes].AsSpan();
            inStream.ReadBytes(terminalNamesBuffer);
            var terminalNames = NullTerminatedByteArrayToAsciiString(terminalNamesBuffer, out _);
            return terminalNames.Split('|');
        }

        private static IEnumerable<(TermInfoBooleanCapabilities index, bool value)> ReadBooleanCapabilityValues(FileStream inStream, short boolSectionCount)
        {
            for (var index = 0; index < boolSectionCount; ++index)
            {
                var value = inStream.ReadByte();
                yield return ((TermInfoBooleanCapabilities)index, value == 0 ? false : value == 1 ? true : throw new Exception("Bad file format."));
            }
        }

        private static IEnumerable<(TermInfoNumberCapabilities index, int value)> ReadNumberCapabilityValues(FileStream inStream, short numberSectioncount, bool is32bitInteger)
        {
            for (var index = 0; index < numberSectioncount; index++)
            {
                var value = is32bitInteger ? inStream.ReadInt32Le() : inStream.ReadInt16Le();
                if (value >= 0)
                    yield return ((TermInfoNumberCapabilities)index, value);
                if (value < -1)
                    throw new Exception("Bad file format.");
            }
        }

        private static IEnumerable<short> ReadStringSectionOffsets(FileStream inStream, short stringSectionOffsetCount)
        {
            for (var count = 0; count < stringSectionOffsetCount; count++)
            {
                var offset = inStream.ReadInt16Le();
                yield return
                    offset >= -1
                    ? offset
                    : throw new Exception("Bad file format.");
            }
        }

        private static IEnumerable<(TermInfoStringCapabilities index, string value)> ReadStringCapabilityValues(FileStream inStream, short stringSectionTableBytes, ReadOnlyMemory<short> stringSectionOffsets)
        {
            var stringTableBuffer = new byte[stringSectionTableBytes];
            inStream.ReadBytes(stringTableBuffer);
            for (var index = 0; index < stringSectionOffsets.Length; ++index)
            {
                var offset = stringSectionOffsets.Span[index];
                if (offset >= stringSectionTableBytes)
                    throw new Exception("Bad file format.");
                if (offset >= 0)
                    yield return ((TermInfoStringCapabilities)index, NullTerminatedByteArrayToAsciiString(stringTableBuffer.AsSpan(offset), out _));
            }
        }

        private static IEnumerable<bool> ReadExtendedBooleanCapabilityValues(FileStream inStream, short extendedBoolValueCount)
        {
            for (var index = 0; index < extendedBoolValueCount; ++index)
            {
                var value = inStream.ReadByte();
                yield return
                    value == 0
                    ? false
                    : value == 1
                    ? true
                    : throw new Exception("Bad file format.");
            }
        }

        private static IEnumerable<int> ReadExtendedNumberCapabilityValues(FileStream inStream, short extendedNumberValueCount, bool is32bitInteger)
        {
            for (var index = 0; index < extendedNumberValueCount; index++)
            {
                var value = is32bitInteger ? inStream.ReadInt32Le() : inStream.ReadInt16Le();
                yield return
                    value >= 0
                    ? value
                    : throw new Exception("Bad file format.");
            }
        }

        private static IEnumerable<short> ReadExtendedStringSectionOffsets(FileStream inStream, short stringSectionOffsetCount)
        {
            for (var count = 0; count < stringSectionOffsetCount; count++)
            {
                var offset = inStream.ReadInt16Le();
                yield return
                    offset >= 0
                    ? offset
                    : throw new Exception("Bad file format.");
            }
        }

        private static IEnumerable<(string value, int length)> ReadExtendedStringValues(ReadOnlyMemory<byte> extendedStringTableBuffer, ReadOnlyMemory<short> extendedStringOffsets)
        {
            for (var index = 0; index < extendedStringOffsets.Length; ++index)
            {
                var offset = extendedStringOffsets.Span[index];
                if (offset >= extendedStringTableBuffer.Length)
                    throw new Exception("Bad file format.");
                if (offset >= 0)
                {
                    var value = NullTerminatedByteArrayToAsciiString(extendedStringTableBuffer.Span[offset..], out var length);
                    yield return (value, length);
                }
            }
        }

        private static string NullTerminatedByteArrayToAsciiString(ReadOnlySpan<byte> buffer, out int length)
        {
            var nullIndex = buffer.IndexOf((byte)0);
            try
            {
                if (nullIndex >= 0)
                {
                    length = nullIndex + 1;
                    return Encoding.ASCII.GetString(buffer[..nullIndex]);
                }
                else
                {
                    throw new Exception("Not found end of string.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Bad file format.", ex);
            }
        }
    }
}
