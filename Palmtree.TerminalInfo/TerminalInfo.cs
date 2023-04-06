using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Palmtree.IO;

namespace Palmtree.Terminal
{
    /// <summary>
    /// A database class of escape codes for working with terminals.
    /// </summary>
    /// <remarks>
    /// For the meaning of properties and methods, refer to "terminfo(5)" in the UNIX-based OS manual.
    /// </remarks>
    public class TerminalInfo
    {
        private readonly TerminalInfoDatabase _database;

        private TerminalInfo(TerminalInfoDatabase database) => _database = database;

        #region Capability accesser

        // Capability name: acs_chars
        // Terminals supporting this capability: 308 terminals
        // Values of this capability:
        //   "``aaffggjjkkllmmnnooppqqrrssttuuvvwwxxyyzz{{||}}~~" (101 terminals)
        //   "``aaffggiijjkkllmmnnooppqqrrssttuuvvwwxxyyzz{{||}}~~" (76 terminals)
        //   "``aaffggjjkkllmmnnooqqssttuuvvwwxx~~" (29 terminals)
        //   "++,,--..00``aaffgghhiijjkkllmmnnooppqqrrssttuuvvwwxxyyzz{{||}}~~" (21 terminals)
        //   "+\u0010,\u0011-\u0018.\u00190?`\u0004a?f?g?h?i?j?k?l?m?n?o~p?q?r?s_t?u?v?w?x?y?z?{?|?}?~?" (11 terminals)
        //   "+/,.0[a2fxgqh1ihjYk?lZm@nEqDtCu4vAwBx3yszr{c~~" (10 terminals)
        //   "++,,--..00__``aaffgghhiijjkkllmmnnooppqqrrssttuuvvwwxxyyzz{{||}c~~" (7 terminals)
        //   "00``aaffgghhjjkkllmmnnooppqqrrssttuuvvwwxxyyzz{{||}}~~" (7 terminals)
        //   "a;j5k3l2m1n8q:t4u9v=w0x6" (7 terminals)
        //   "+C,D-A.B0E``aaffgghFiGjjkkllmmnnooppqqrrssttuuvvwwxxyyzz{{||}}~~" (6 terminals)
        //   "jHkGlFmEnIqKtMuLvOwNxJ" (6 terminals)
        //   "``aaffggjjkkllmmnnooppqqrrssttuuvvwwxxyyzz{{||}}~~-A.B+C,D0EhFiG" (4 terminals)
        //   "+?,?-?.?0#`?a:f?g?h#i?jjkkllmmnno?p?q?rrssttuuvvwwxxy?z?{?|?}?~?" (3 terminals)
        //   "+\u0010,\u0011-\u0018.\u00190?`\u0004a?f?g?h?j?k?l?m?n?o~p?q?r?s_t?u?v?w?x?y?z?{?|?}?~?" (3 terminals)
        //   "jLkDl@mHnhq`tXuTv\\wPxd" (3 terminals)
        //   "++,,--..00ii``aaffgghhjjkkllmmnnooppqqrrssttuuvvwwxxyyzz{{||}}~~" (2 terminals)
        //   "0_`RjHkGlFmEnIoPqKsQtMuLvOwNxJ" (2 terminals)
        //   "+\u0010,\u0011-\u0018.\u00190?`\u0004a?f?g?h?i?j?k?l?m?n?o~p?q?r?s_t?u?v?w?x?y?z?{?|?~?" (1 terminal)
        //   "+\u0010,\u0011-\u0018.\u00190?`\u0004a\u000bf?g?h?i?j?k?l?m?n?o~p?q?r?s_t?u?v?w?x?y?z?{?|?}?~?" (1 terminal)
        //   "++,,--..00``aabbccddeeffgghhiijjkkllmmnnooppqqrrssttuuvvwwxxyyzz{{||}}~~" (1 terminal)
        //   "++,,--..00``aaffgghhiijjkkllmmnnooppqqrrssttuuvvwwxxyyzz||}}~~" (1 terminal)
        //   "++,,--..00``aaffgghhiijjkkllmmnnooppqqrrssttuuvvwwxxyyzz~~" (1 terminal)
        //   "0?+?,?-^`\u0004a?f?g?h?j?k?l?m?n?o~p?q?r?s_t?u?v?w?x?y?z?{?|?}?~?" (1 terminal)
        //   "0?+?,?-^`\u0004a?f?g?h?j?k?l?m?n?o~p?q?r?s_t?u?v?w?x?y?z?{?|?~?" (1 terminal)
        //   "a?f?g?h?j?k?l?m?n?q?t?u?v?w?x?y?z?{?|?~?" (1 terminal)
        //   "ffggjjkkllmmnnooqqssttuuvvwwxxyyzz||}}" (1 terminal)
        //   "j+k+l+m+n+o~q`s_t+u+v+w+x|" (1 terminal)
        /// <summary>Get the value of capability "acs_chars".</summary>
        /// <returns>If not null it is the value of the capability "acs_chars". If null, this terminal information does not support the capability "acs_chars".</returns>
        public string? AcsChars => _database.GetStringCapabilityValue(TermInfoStringCapabilities.AcsChars);

        // Capability name: auto_left_margin
        // Terminals supporting this capability: 320 terminals
        // Values of this capability:
        //   false (270 terminals)
        //   true (50 terminals)
        /// <summary>Get the value of capability "auto_left_margin".</summary>
        /// <returns>If not null it is the value of the capability "auto_left_margin". If null, this terminal information does not support the capability "auto_left_margin".</returns>
        public bool? AutoLeftMargin => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.AutoLeftMargin);

        // Capability name: auto_right_margin
        // Terminals supporting this capability: 320 terminals
        // Values of this capability:
        //   true (309 terminals)
        //   false (11 terminals)
        /// <summary>Get the value of capability "auto_right_margin".</summary>
        /// <returns>If not null it is the value of the capability "auto_right_margin". If null, this terminal information does not support the capability "auto_right_margin".</returns>
        public bool? AutoRightMargin => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.AutoRightMargin);

        // Capability name: AX
        // Terminals supporting this capability: 117 terminals
        // Values of this capability:
        //   true (117 terminals)
        /// <summary>Get the value of extended capability "AX".</summary>
        /// <returns>If not null it is the value of the extended capability "AX". If null, this terminal information does not support the extended capability "AX".</returns>
        /// <remarks>
        /// This capability means whether the terminal understands the default ANSI escape codes for foreground/background colors ("\u001b[39m"/"\u001b[49m").
        /// (See "man screen(1)")
        /// </remarks>
        public bool? AX => _database.GetBooleanCapabilityValue("AX");

        // Capability name: back_color_erase
        // Terminals supporting this capability: 180 terminals
        // Values of this capability:
        //   true (146 terminals)
        //   false (34 terminals)
        /// <summary>Get the value of capability "back_color_erase".</summary>
        /// <returns>If not null it is the value of the capability "back_color_erase". If null, this terminal information does not support the capability "back_color_erase".</returns>
        public bool? BackColorErase => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.BackColorErase);

        // Capability name: back_tab
        // Terminals supporting this capability: 166 terminals
        // Values of this capability:
        //   "\u001b[Z" (140 terminals)
        //   "\u001bI" (25 terminals)
        //   "?Z" (1 terminal)
        /// <summary>Get the value of capability "back_tab".</summary>
        /// <returns>If not null it is the value of the capability "back_tab". If null, this terminal information does not support the capability "back_tab".</returns>
        public string? BackTab => _database.GetStringCapabilityValue(TermInfoStringCapabilities.BackTab);

        // Capability name: backspaces_with_bs
        // Terminals supporting this capability: 133 terminals
        // Values of this capability:
        //   true (133 terminals)
        /// <summary>Get the value of capability "backspaces_with_bs".</summary>
        /// <returns>If not null it is the value of the capability "backspaces_with_bs". If null, this terminal information does not support the capability "backspaces_with_bs".</returns>
        public bool? BackspacesWithBs => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.BackspacesWithBs);

        // Capability name: bell
        // Terminals supporting this capability: 306 terminals
        // Values of this capability:
        //   "\u0007" (305 terminals)
        //   "\u0007\u001b^ \u001b\\" (1 terminal)
        /// <summary>Get the value of capability "bell".</summary>
        /// <returns>If not null it is the value of the capability "bell". If null, this terminal information does not support the capability "bell".</returns>
        public string? Bell => _database.GetStringCapabilityValue(TermInfoStringCapabilities.Bell);

        // Capability name: blink2
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b[6m" (2 terminals)
        /// <summary>Get the value of extended capability "blink2".</summary>
        /// <returns>If not null it is the value of the extended capability "blink2". If null, this terminal information does not support the extended capability "blink2".</returns>
        public string? Blink2 => _database.GetStringCapabilityValue("blink2");

        // Capability name: buttons
        // Terminals supporting this capability: 7 terminals
        // Values of this capability:
        //   5 (7 terminals)
        /// <summary>Get the value of capability "buttons".</summary>
        /// <returns>If not null it is the value of the capability "buttons". If null, this terminal information does not support the capability "buttons".</returns>
        public int? Buttons => _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.Buttons);

        // Capability name: C0
        // Terminals supporting this capability: 3 terminals
        // Values of this capability:
        //   "`>a9f!j%k4l<m-n=p#q,rpt=u5v-w<x5yvzy|l~$" (1 terminal)
        //   "ffggj+k+l+m+n+ovq-swt+u+v+w+xx}},m+k.l-j0\u007f" (1 terminal)
        //   "ffggjjkkllmmnnooqqssttuuvvwwxxyyzz||}}" (1 terminal)
        /// <summary>Get the value of extended capability "C0".</summary>
        /// <returns>If not null it is the value of the extended capability "C0". If null, this terminal information does not support the extended capability "C0".</returns>
        /// <remarks>
        /// This capability is a string value used as a conversion table for font 0. See "acs_chars" for details.
        /// (See "man screen(1)")
        /// </remarks>
        public string? C0 => _database.GetStringCapabilityValue("C0");

        // Capability name: can_change
        // Terminals supporting this capability: 185 terminals
        // Values of this capability:
        //   false (146 terminals)
        //   true (39 terminals)
        /// <summary>Get the value of capability "can_change".</summary>
        /// <returns>If not null it is the value of the capability "can_change". If null, this terminal information does not support the capability "can_change".</returns>
        public bool? CanChange => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.CanChange);

        // Capability name: carriage_return
        // Terminals supporting this capability: 317 terminals
        // Values of this capability:
        //   "\r" (288 terminals)
        //   "\r$<1>" (29 terminals)
        /// <summary>Get the value of capability "carriage_return".</summary>
        /// <returns>If not null it is the value of the capability "carriage_return". If null, this terminal information does not support the capability "carriage_return".</returns>
        public string? CarriageReturn => _database.GetStringCapabilityValue(TermInfoStringCapabilities.CarriageReturn);

        // Capability name: ceol_standout_glitch
        // Terminals supporting this capability: 320 terminals
        // Values of this capability:
        //   false (320 terminals)
        /// <summary>Get the value of capability "ceol_standout_glitch".</summary>
        /// <returns>If not null it is the value of the capability "ceol_standout_glitch". If null, this terminal information does not support the capability "ceol_standout_glitch".</returns>
        public bool? CeolStandoutGlitch => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.CeolStandoutGlitch);

        // Capability name: change_scroll_region
        // Terminals supporting this capability: 280 terminals
        // Values of this capability:
        //   "\u001b[%i%p1%d;%p2%dr" (222 terminals)
        //   "\u001b[%i%p1%d;%p2%dr$<20>" (28 terminals)
        //   "\u001b[%i%p1%d;%p2%dr$<5>" (21 terminals)
        //   "\u001b[%i%p1%d;%p2%dr$<2>" (8 terminals)
        //   "?%i%p1%d;%p2%dr" (1 terminal)
        /// <summary>Get the value of capability "change_scroll_region".</summary>
        /// <param name="lineTop"><see cref="int"/> value that is the top line of the region</param>
        /// <param name="lineBottom"><see cref="int"/> value that is the bottom line of the region</param>
        /// <returns>If not null it is the value of the capability "change_scroll_region". If null, this terminal information does not support the capability "change_scroll_region".</returns>
        public string? ChangeScrollRegion(int lineTop, int lineBottom)
            => lineTop < 0
                ? throw new ArgumentOutOfRangeException(nameof(lineTop))
                : lineBottom < 0
                ? throw new ArgumentOutOfRangeException(nameof(lineBottom))
                : lineTop > lineBottom
                ? throw new ArgumentException($"Must be {nameof(lineTop)} <= {lineBottom}")
                : _database.GetStringCapabilityValue(TermInfoStringCapabilities.ChangeScrollRegion, lineTop, lineBottom);

        // Capability name: char_set_names
        // Terminals supporting this capability: 1 terminal
        // Values of this capability:
        //   "%?%p1%{1}%=%tNorth American%e%p1%{2}%=%tBritish%e%p1%{3}%=%tFlemish%e%p1%{4}%=%tFrench Canadian%e%p1%{5}%=%tDanish%e%p1%{6}%=%tFinnish%e%p1%{7}%=%tGerman%e%p1%{8}%=%tDutch%e%p1%{9}%=%tItalian%e%p1%{10}%=%tSwiss (French)%e%p1%{11}%=%tSwiss (German)%e%p1%{12}%=%tSwedish%e%p1%{13}%=%tNorwegian%e%p1%{14}%=%tFrench/Belgian%e%p1%{15}%=%tSpanish%;" (1 terminal)
        /// <summary>Get the value of capability "char_set_names".</summary>
        /// <param name="charSetNumber"><see cref="int"/> value that is the character set name number</param>
        /// <returns>If not null it is the value of the capability "char_set_names". If null, this terminal information does not support the capability "char_set_names".</returns>
        public string? CharSetNames(int charSetNumber)
            => charSetNumber >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.CharSetNames, charSetNumber)
                : throw new ArgumentOutOfRangeException(nameof(charSetNumber));

        // Capability name: clear_all_tabs
        // Terminals supporting this capability: 300 terminals
        // Values of this capability:
        //   "\u001b[3g" (274 terminals)
        //   "\u001b0" (17 terminals)
        //   "\u001b3" (8 terminals)
        //   "?3g" (1 terminal)
        /// <summary>Get the value of capability "clear_all_tabs".</summary>
        /// <returns>If not null it is the value of the capability "clear_all_tabs". If null, this terminal information does not support the capability "clear_all_tabs".</returns>
        public string? ClearAllTabs => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ClearAllTabs);

        // Capability name: clear_margins
        // Terminals supporting this capability: 11 terminals
        // Values of this capability:
        //   "\u001b[?69l" (11 terminals)
        /// <summary>Get the value of capability "clear_margins".</summary>
        /// <returns>If not null it is the value of the capability "clear_margins". If null, this terminal information does not support the capability "clear_margins".</returns>
        public string? ClearMargins => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ClearMargins);

        // Capability name: clear_screen
        // Terminals supporting this capability: 317 terminals
        // Values of this capability:
        //   "\u001b[H\u001b[2J" (126 terminals)
        //   "\u001b[H\u001b[J" (78 terminals)
        //   "\u001b[2J\u001b[1;1H$<20>" (29 terminals)
        //   "\u001b[H\u001b[J$<40>" (29 terminals)
        //   "\u001b[H\u001b[J$<30>" (8 terminals)
        //   "\u001b[H\u001b[J$<50>" (8 terminals)
        //   "\u001b*" (7 terminals)
        //   "\u001b+$<20>" (7 terminals)
        //   "\f" (4 terminals)
        //   "\u001b[H\u001b[J$<110>" (4 terminals)
        //   "\u001b+$<100>" (4 terminals)
        //   "\u001b+$<260>" (4 terminals)
        //   "\u001b[H\u001b[0J" (2 terminals)
        //   "\u001b+$<160>" (2 terminals)
        //   "\u001bc" (2 terminals)
        //   "?H?2J" (1 terminal)
        //   "\u001b[H\u001b[2J$<50>" (1 terminal)
        //   "\u001b*$<10>" (1 terminal)
        /// <summary>Get the value of capability "clear_screen".</summary>
        /// <returns>If not null it is the value of the capability "clear_screen". If null, this terminal information does not support the capability "clear_screen".</returns>
        public string? ClearScreen => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ClearScreen);

        // Capability name: clr_bol
        // Terminals supporting this capability: 262 terminals
        // Values of this capability:
        //   "\u001b[1K" (215 terminals)
        //   "\u001b[1K$<3>" (22 terminals)
        //   "\u001b[1K$<5>" (21 terminals)
        //   "\u001b[2K" (2 terminals)
        //   "?1K" (1 terminal)
        //   "\u001b[1K$<12>" (1 terminal)
        /// <summary>Get the value of capability "clr_bol".</summary>
        /// <returns>If not null it is the value of the capability "clr_bol". If null, this terminal information does not support the capability "clr_bol".</returns>
        public string? ClrBol => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ClrBol);

        // Capability name: clr_eol
        // Terminals supporting this capability: 315 terminals
        // Values of this capability:
        //   "\u001b[K" (236 terminals)
        //   "\u001b[0K$<5>" (21 terminals)
        //   "\u001b[K$<3>" (15 terminals)
        //   "\u001bT" (15 terminals)
        //   "\u001b[0K$<3>" (8 terminals)
        //   "\u001bt" (8 terminals)
        //   "\u001b[K$<1>" (4 terminals)
        //   "\u001bK" (3 terminals)
        //   "\u001bt$<5>" (2 terminals)
        //   "?K" (1 terminal)
        //   "\u0018" (1 terminal)
        //   "\u001b[K$<10>" (1 terminal)
        /// <summary>Get the value of capability "clr_eol".</summary>
        /// <returns>If not null it is the value of the capability "clr_eol". If null, this terminal information does not support the capability "clr_eol".</returns>
        public string? ClrEol => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ClrEol);

        // Capability name: clr_eos
        // Terminals supporting this capability: 314 terminals
        // Values of this capability:
        //   "\u001b[J" (206 terminals)
        //   "\u001b[J$<40>" (29 terminals)
        //   "\u001b[0J" (23 terminals)
        //   "\u001b[0J$<5>" (8 terminals)
        //   "\u001b[J$<30>" (8 terminals)
        //   "\u001by" (8 terminals)
        //   "\u001b[J$<50>" (7 terminals)
        //   "\u001bY$<20>" (7 terminals)
        //   "\u001b[J$<110>" (4 terminals)
        //   "\u001bY$<100>" (4 terminals)
        //   "\u001by$<260>" (4 terminals)
        //   "\u001bk" (3 terminals)
        //   "\u001by$<160>" (2 terminals)
        //   "?J" (1 terminal)
        /// <summary>Get the value of capability "clr_eos".</summary>
        /// <returns>If not null it is the value of the capability "clr_eos". If null, this terminal information does not support the capability "clr_eos".</returns>
        public string? ClrEos => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ClrEos);

        // Capability name: col_addr_glitch
        // Terminals supporting this capability: 133 terminals
        // Values of this capability:
        //   false (133 terminals)
        /// <summary>Get the value of capability "col_addr_glitch".</summary>
        /// <returns>If not null it is the value of the capability "col_addr_glitch". If null, this terminal information does not support the capability "col_addr_glitch".</returns>
        public bool? ColAddrGlitch => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.ColAddrGlitch);

        // Capability name: column_address
        // Terminals supporting this capability: 239 terminals
        // Values of this capability:
        //   "\u001b[%i%p1%dG" (178 terminals)
        //   "\u001b[%i%p1%d`" (30 terminals)
        //   "\u001b[%p1%dG$<40>" (29 terminals)
        //   "?%i%p1%dG" (1 terminal)
        //   "\u001b[%p1%{1}%+%dG" (1 terminal)
        /// <summary>Get the value of capability "column_address".</summary>
        /// <param name="column"><see cref="int"/> value that is the absolute horizontal position</param>
        /// <returns>If not null it is the value of the capability "column_address". If null, this terminal information does not support the capability "column_address".</returns>
        public string? ColumnAddress(int column)
            => column >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.ColumnAddress, column)
                : throw new ArgumentOutOfRangeException(nameof(column));

        // Capability name: columns
        // Terminals supporting this capability: 290 terminals
        // Values of this capability:
        //   80 (234 terminals)
        //   132 (55 terminals)
        //   40 (1 terminal)
        /// <summary>Get the value of capability "columns".</summary>
        /// <returns>If not null it is the value of the capability "columns". If null, this terminal information does not support the capability "columns".</returns>
        public int? Columns => _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.Columns);

        // Capability name: cpi_changes_res
        // Terminals supporting this capability: 133 terminals
        // Values of this capability:
        //   false (133 terminals)
        /// <summary>Get the value of capability "cpi_changes_res".</summary>
        /// <returns>If not null it is the value of the capability "cpi_changes_res". If null, this terminal information does not support the capability "cpi_changes_res".</returns>
        public bool? CpiChangesRes => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.CpiChangesRes);

        // Capability name: cr_cancels_micro_mode
        // Terminals supporting this capability: 133 terminals
        // Values of this capability:
        //   false (133 terminals)
        /// <summary>Get the value of capability "cr_cancels_micro_mode".</summary>
        /// <returns>If not null it is the value of the capability "cr_cancels_micro_mode". If null, this terminal information does not support the capability "cr_cancels_micro_mode".</returns>
        public bool? CrCancelsMicroMode => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.CrCancelsMicroMode);

        // Capability name: Cr
        // Terminals supporting this capability: 32 terminals
        // Values of this capability:
        //   "\u001b]112\u0007" (32 terminals)
        /// <summary>Get the value of extended capability "Cr".</summary>
        /// <returns>If not null it is the value of the extended capability "Cr". If null, this terminal information does not support the extended capability "Cr".</returns>
        /// <remarks>
        /// This capability is <see cref="string"/> value that is an escape code that sets the cursor color to the default color.
        /// </remarks>
        public string? Cr => _database.GetStringCapabilityValue("Cr");

        // Capability name: crt_no_scrolling
        // Terminals supporting this capability: 21 terminals
        // Values of this capability:
        //   false (21 terminals)
        /// <summary>Get the value of capability "crt_no_scrolling".</summary>
        /// <returns>If not null it is the value of the capability "crt_no_scrolling". If null, this terminal information does not support the capability "crt_no_scrolling".</returns>
        public bool? CrtNoScrolling => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.CrtNoScrolling);

        // Capability name: Cs
        // Terminals supporting this capability: 32 terminals
        // Values of this capability:
        //   "\u001b]12;%p1%s\u0007" (32 terminals)
        /// <summary>Get the value of extended capability "Cs".</summary>
        /// <param name="cursorColorName"><see cref="string"/> value that is the name of the cursor color (eg "white")</param>
        /// <returns>If not null it is the value of the extended capability "Cs". If null, this terminal information does not support the extended capability "Cs".</returns>
        /// <remarks>
        /// This capability is <see cref="string"/> value that is an escape code to set the cursor color.
        /// </remarks>
        public string? Cs(string cursorColorName)
            => _database.GetStringCapabilityValue("Cs", cursorColorName);

        // Capability name: cursor_address
        // Terminals supporting this capability: 317 terminals
        // Values of this capability:
        //   "\u001b[%i%p1%d;%p2%dH" (246 terminals)
        //   "\u001b[%i%p1%d;%p2%dH$<10>" (15 terminals)
        //   "\u001b[%i%p1%d;%p2%dH$<30>" (15 terminals)
        //   "\u001b=%p1%' '%+%c%p2%' '%+%c" (13 terminals)
        //   "\u001b[%i%p1%d;%p2%dH$<5>" (8 terminals)
        //   "\u001b[%i%p1%d;%p2%dH$<1>" (5 terminals)
        //   "\u001ba%i%p1%dR%p2%dC" (4 terminals)
        //   "\u001ba%i%p1%dR%p2%dC$<2>" (4 terminals)
        //   "\u001bY%p1%' '%+%c%p2%' '%+%c" (3 terminals)
        //   "\u001b=%p1%' '%+%c%p2%' '%+%c$<2>" (2 terminals)
        //   "?%i%p1%d;%p2%dH" (1 terminal)
        //   "\u001f%p1%'A'%+%c%p2%'A'%+%c" (1 terminal)
        /// <summary>Get the value of capability "cursor_address".</summary>
        /// <param name="line"><see cref="int"/> value that is the line to move the cursor to</param>
        /// <param name="column"><see cref="int"/> value that is the column to move the cursor to</param>
        /// <returns>If not null it is the value of the capability "cursor_address". If null, this terminal information does not support the capability "cursor_address".</returns>
        public string? CursorAddress(int line, int column)
            => line < 0
                ? throw new ArgumentOutOfRangeException(nameof(line))
                : column < 0
                ? throw new ArgumentOutOfRangeException(nameof(column))
                : _database.GetStringCapabilityValue(TermInfoStringCapabilities.CursorAddress, line, column);

        // Capability name: cursor_down
        // Terminals supporting this capability: 317 terminals
        // Values of this capability:
        //   "\n" (268 terminals)
        //   "\u001b[B$<5>" (29 terminals)
        //   "\u001b[B" (11 terminals)
        //   "\u001bD" (7 terminals)
        //   "\u0016" (2 terminals)
        /// <summary>Get the value of capability "cursor_down".</summary>
        /// <returns>If not null it is the value of the capability "cursor_down". If null, this terminal information does not support the capability "cursor_down".</returns>
        public string? CursorDown => _database.GetStringCapabilityValue(TermInfoStringCapabilities.CursorDown);

        // Capability name: cursor_home
        // Terminals supporting this capability: 317 terminals
        // Values of this capability:
        //   "\u001b[H" (279 terminals)
        //   "\u001e" (16 terminals)
        //   "\u001b[H$<1>" (8 terminals)
        //   "\u001b{" (8 terminals)
        //   "\u001bY  " (3 terminals)
        //   "\u001e$<2>" (2 terminals)
        //   "?H" (1 terminal)
        /// <summary>Get the value of capability "cursor_home".</summary>
        /// <returns>If not null it is the value of the capability "cursor_home". If null, this terminal information does not support the capability "cursor_home".</returns>
        public string? CursorHome => _database.GetStringCapabilityValue(TermInfoStringCapabilities.CursorHome);

        // Capability name: cursor_invisible
        // Terminals supporting this capability: 278 terminals
        // Values of this capability:
        //   "\u001b[?25l" (244 terminals)
        //   "\u001b`0" (17 terminals)
        //   "\u001b[?25l\u001b[?1c" (10 terminals)
        //   "\u001b.0" (2 terminals)
        //   "\u001b[<1h" (2 terminals)
        //   "??25l" (1 terminal)
        //   "\u0014" (1 terminal)
        //   "\u001b[1v" (1 terminal)
        /// <summary>Get the value of capability "cursor_invisible".</summary>
        /// <returns>If not null it is the value of the capability "cursor_invisible". If null, this terminal information does not support the capability "cursor_invisible".</returns>
        public string? CursorInvisible => _database.GetStringCapabilityValue(TermInfoStringCapabilities.CursorInvisible);

        // Capability name: cursor_left
        // Terminals supporting this capability: 317 terminals
        // Values of this capability:
        //   "\b" (282 terminals)
        //   "\u001b[D$<5>" (29 terminals)
        //   "\u0015" (3 terminals)
        //   "\u001b[D" (3 terminals)
        /// <summary>Get the value of capability "cursor_left".</summary>
        /// <returns>If not null it is the value of the capability "cursor_left". If null, this terminal information does not support the capability "cursor_left".</returns>
        public string? CursorLeft => _database.GetStringCapabilityValue(TermInfoStringCapabilities.CursorLeft);

        // Capability name: cursor_normal
        // Terminals supporting this capability: 278 terminals
        // Values of this capability:
        //   "\u001b[?25h" (141 terminals)
        //   "\u001b[?12l\u001b[?25h" (53 terminals)
        //   "\u001b[34h\u001b[?25h" (50 terminals)
        //   "\u001b`1" (17 terminals)
        //   "\u001b[?25h\u001b[?0c" (10 terminals)
        //   "\u001b.2" (2 terminals)
        //   "\u001b[<1l" (2 terminals)
        //   "??25l??25h" (1 terminal)
        //   "\u0011" (1 terminal)
        //   "\u001b[v" (1 terminal)
        /// <summary>Get the value of capability "cursor_normal".</summary>
        /// <returns>If not null it is the value of the capability "cursor_normal". If null, this terminal information does not support the capability "cursor_normal".</returns>
        public string? CursorNormal => _database.GetStringCapabilityValue(TermInfoStringCapabilities.CursorNormal);

        // Capability name: cursor_right
        // Terminals supporting this capability: 317 terminals
        // Values of this capability:
        //   "\u001b[C" (250 terminals)
        //   "\u001b[C$<5>" (29 terminals)
        //   "\f" (25 terminals)
        //   "\u001b[C$<2>" (8 terminals)
        //   "\u0006" (3 terminals)
        //   "?C" (1 terminal)
        //   "\t" (1 terminal)
        /// <summary>Get the value of capability "cursor_right".</summary>
        /// <returns>If not null it is the value of the capability "cursor_right". If null, this terminal information does not support the capability "cursor_right".</returns>
        public string? CursorRight => _database.GetStringCapabilityValue(TermInfoStringCapabilities.CursorRight);

        // Capability name: cursor_to_ll
        // Terminals supporting this capability: 20 terminals
        // Values of this capability:
        //   "\u001b{\u000b" (10 terminals)
        //   "\u001e\u000b" (7 terminals)
        //   "\u0001" (3 terminals)
        /// <summary>Get the value of capability "cursor_to_ll".</summary>
        /// <returns>If not null it is the value of the capability "cursor_to_ll". If null, this terminal information does not support the capability "cursor_to_ll".</returns>
        public string? CursorToLl => _database.GetStringCapabilityValue(TermInfoStringCapabilities.CursorToLl);

        // Capability name: cursor_up
        // Terminals supporting this capability: 317 terminals
        // Values of this capability:
        //   "\u001b[A" (222 terminals)
        //   "\u001b[A$<5>" (29 terminals)
        //   "\u001bM" (28 terminals)
        //   "\u000b" (26 terminals)
        //   "\u001b[A$<2>" (8 terminals)
        //   "\u001a" (3 terminals)
        //   "?A" (1 terminal)
        /// <summary>Get the value of capability "cursor_up".</summary>
        /// <returns>If not null it is the value of the capability "cursor_up". If null, this terminal information does not support the capability "cursor_up".</returns>
        public string? CursorUp => _database.GetStringCapabilityValue(TermInfoStringCapabilities.CursorUp);

        // Capability name: cursor_visible
        // Terminals supporting this capability: 114 terminals
        // Values of this capability:
        //   "\u001b[?12;25h" (42 terminals)
        //   "\u001b[?25h\u001b[34l" (29 terminals)
        //   "\u001b[34l" (23 terminals)
        //   "\u001b[?25h\u001b[?8c" (10 terminals)
        //   "\u001b[?25h" (6 terminals)
        //   "\u001b.1" (2 terminals)
        //   "??12;25h" (1 terminal)
        //   "\u001b[2v" (1 terminal)
        /// <summary>Get the value of capability "cursor_visible".</summary>
        /// <returns>If not null it is the value of the capability "cursor_visible". If null, this terminal information does not support the capability "cursor_visible".</returns>
        public string? CursorVisible => _database.GetStringCapabilityValue(TermInfoStringCapabilities.CursorVisible);

        // Capability name: delete_character
        // Terminals supporting this capability: 277 terminals
        // Values of this capability:
        //   "\u001b[P" (178 terminals)
        //   "\u001b[1P$<5>" (29 terminals)
        //   "\u001b[P$<30>" (16 terminals)
        //   "\u001b[P$<3>" (14 terminals)
        //   "\u001b[P$<7>" (10 terminals)
        //   "\u001bW" (8 terminals)
        //   "\u001bW$<1>" (5 terminals)
        //   "\u001bW$<16>" (4 terminals)
        //   "\u001bE" (3 terminals)
        //   "\u001bW$<11>" (2 terminals)
        //   "\u001bW$<19>" (2 terminals)
        //   "\u001bW$<2>" (2 terminals)
        //   "\u001bW$<9>" (2 terminals)
        //   "?P" (1 terminal)
        //   "\u001b[P$<1>" (1 terminal)
        /// <summary>Get the value of capability "delete_character".</summary>
        /// <returns>If not null it is the value of the capability "delete_character". If null, this terminal information does not support the capability "delete_character".</returns>
        public string? DeleteCharacter => _database.GetStringCapabilityValue(TermInfoStringCapabilities.DeleteCharacter);

        // Capability name: delete_line
        // Terminals supporting this capability: 308 terminals
        // Values of this capability:
        //   "\u001b[M" (217 terminals)
        //   "\u001b[M$<2>" (29 terminals)
        //   "\u001b[M$<5>" (29 terminals)
        //   "\u001bR" (15 terminals)
        //   "\u001b[M$<3>" (4 terminals)
        //   "\u001bR$<11>" (4 terminals)
        //   "\u001bR$<5>" (4 terminals)
        //   "\u001bl$<2*>" (3 terminals)
        //   "\u001bR$<4>" (2 terminals)
        //   "?M" (1 terminal)
        /// <summary>Get the value of capability "delete_line".</summary>
        /// <returns>If not null it is the value of the capability "delete_line". If null, this terminal information does not support the capability "delete_line".</returns>
        public string? DeleteLine => _database.GetStringCapabilityValue(TermInfoStringCapabilities.DeleteLine);

        // Capability name: dest_tabs_magic_smso
        // Terminals supporting this capability: 297 terminals
        // Values of this capability:
        //   false (297 terminals)
        /// <summary>Get the value of capability "dest_tabs_magic_smso".</summary>
        /// <returns>If not null it is the value of the capability "dest_tabs_magic_smso". If null, this terminal information does not support the capability "dest_tabs_magic_smso".</returns>
        public bool? DestTabsMagicSmso => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.DestTabsMagicSmso);

        // Capability name: dis_status_line
        // Terminals supporting this capability: 171 terminals
        // Values of this capability:
        //   "\u001b]2;\u0007" (41 terminals)
        //   "\u001b[0$~\u001b[1$~" (29 terminals)
        //   "\u001b]0;\u0007" (23 terminals)
        //   "\u001bF\r" (17 terminals)
        //   "\u001b[0$~" (16 terminals)
        //   "\u001b[1$~" (11 terminals)
        //   "\u001b[>,\u0001\u0001\u001b[>-\u0001\u0001" (8 terminals)
        //   "\u001bg\u001bf\r" (8 terminals)
        //   "\u001b_\u001b\\" (6 terminals)
        //   "\u001b[40l" (5 terminals)
        //   "\u001b7\u001b[99;0H\u001b[K\u001b8" (4 terminals)
        //   "\u001f@A\u0018\n" (3 terminals)
        /// <summary>Get the value of capability "dis_status_line".</summary>
        /// <returns>If not null it is the value of the capability "dis_status_line". If null, this terminal information does not support the capability "dis_status_line".</returns>
        public string? DisStatusLine => _database.GetStringCapabilityValue(TermInfoStringCapabilities.DisStatusLine);

        // Capability name: display_clock
        // Terminals supporting this capability: 11 terminals
        // Values of this capability:
        //   "\u001b`b" (10 terminals)
        //   "\u001b[31h" (1 terminal)
        /// <summary>Get the value of capability "display_clock".</summary>
        /// <returns>If not null it is the value of the capability "display_clock". If null, this terminal information does not support the capability "display_clock".</returns>
        public string? DisplayClock => _database.GetStringCapabilityValue(TermInfoStringCapabilities.DisplayClock);

        // Capability name: display_pc_char
        // Terminals supporting this capability: 7 terminals
        // Values of this capability:
        //   "%?%p1%{8}%=%t\u001b%%G???\u001b%%@%e%p1%{10}%=%t\u001b%%G???\u001b%%@%e%p1%{12}%=%t\u001b%%G???\u001b%%@%e%p1%{13}%=%t\u001b%%G???\u001b%%@%e%p1%{14}%=%t\u001b%%G???\u001b%%@%e%p1%{15}%=%t\u001b%%G???\u001b%%@%e%p1%{27}%=%t\u001b%%G???\u001b%%@%e%p1%{155}%=%t\u001b%%G???\u001b%%@%e%p1%c%;" (7 terminals)
        /// <summary>Get the value of capability "display_pc_char".</summary>
        /// <param name="c"><see cref="char"/> value that is the character to display</param>
        /// <returns>If not null it is the value of the capability "display_pc_char". If null, this terminal information does not support the capability "display_pc_char".</returns>
        public string? DisplayPcChar(char c)
            => c.IsInClosedInterval('\0', '\u007f')
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.DisplayPcChar, c)
                : throw new ArgumentOutOfRangeException(nameof(c));

        // Capability name: E0
        // Terminals supporting this capability: 24 terminals
        // Values of this capability:
        //   "\u001b(B" (21 terminals)
        //   "\u000f" (3 terminals)
        /// <summary>Get the value of extended capability "E0".</summary>
        /// <returns>If not null it is the value of the extended capability "E0". If null, this terminal information does not support the extended capability "E0".</returns>
        /// <remarks>
        /// This capability is a string value that reverts to the standard font according to the ISO-2022 font selection sequence.
        /// (See "man screen(1)")
        /// </remarks>
        public string? E0 => _database.GetStringCapabilityValue("E0");

        // Capability name: E3
        // Terminals supporting this capability: 55 terminals
        // Values of this capability:
        //   "\u001b[3J" (53 terminals)
        //   "\u001b[99H\u001b[2J\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" (2 terminals)
        /// <summary>Get the value of extended capability "E3".</summary>
        /// <returns>If not null it is the value of the extended capability "E3". If null, this terminal information does not support the extended capability "E3".</returns>
        public string? E3 => _database.GetStringCapabilityValue("E3");

        // Capability name: eat_newline_glitch
        // Terminals supporting this capability: 320 terminals
        // Values of this capability:
        //   true (288 terminals)
        //   false (32 terminals)
        /// <summary>Get the value of capability "eat_newline_glitch".</summary>
        /// <returns>If not null it is the value of the capability "eat_newline_glitch". If null, this terminal information does not support the capability "eat_newline_glitch".</returns>
        public bool? EatNewlineGlitch => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.EatNewlineGlitch);

        // Capability name: ena_acs
        // Terminals supporting this capability: 191 terminals
        // Values of this capability:
        //   "\u001b)0" (95 terminals)
        //   "\u001b(B\u001b)0" (84 terminals)
        //   "" (11 terminals)
        //   "\u001b)U" (1 terminal)
        /// <summary>Get the value of capability "ena_acs".</summary>
        /// <returns>If not null it is the value of the capability "ena_acs". If null, this terminal information does not support the capability "ena_acs".</returns>
        public string? EnaAcs => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnaAcs);

        // Capability name: enter_alt_charset_mode
        // Terminals supporting this capability: 307 terminals
        // Values of this capability:
        //   "\u000e" (174 terminals)
        //   "\u001b(0" (74 terminals)
        //   "\u000e$<20>" (21 terminals)
        //   "\u001bcE" (10 terminals)
        //   "\u001b[11m" (9 terminals)
        //   "\u001b$" (8 terminals)
        //   "\u001bH\u0002" (7 terminals)
        //   "\u001b1" (3 terminals)
        //   "\u001b(0$<2>" (1 terminal)
        /// <summary>Get the value of capability "enter_alt_charset_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_alt_charset_mode". If null, this terminal information does not support the capability "enter_alt_charset_mode".</returns>
        public string? EnterAltCharsetMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterAltCharsetMode);

        // Capability name: enter_am_mode
        // Terminals supporting this capability: 237 terminals
        // Values of this capability:
        //   "\u001b[?7h" (224 terminals)
        //   "\u001bd/" (10 terminals)
        //   "\u001b[=7h" (2 terminals)
        //   "??7h" (1 terminal)
        /// <summary>Get the value of capability "enter_am_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_am_mode". If null, this terminal information does not support the capability "enter_am_mode".</returns>
        public string? EnterAmMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterAmMode);

        // Capability name: enter_blink_mode
        // Terminals supporting this capability: 258 terminals
        // Values of this capability:
        //   "\u001b[5m" (237 terminals)
        //   "\u001bG2" (12 terminals)
        //   "\u001b[5m$<2>" (7 terminals)
        //   "?5m" (1 terminal)
        //   "\u001bH" (1 terminal)
        /// <summary>Get the value of capability "enter_blink_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_blink_mode". If null, this terminal information does not support the capability "enter_blink_mode".</returns>
        public string? EnterBlinkMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterBlinkMode);

        // Capability name: enter_bold_mode
        // Terminals supporting this capability: 279 terminals
        // Values of this capability:
        //   "\u001b[1m" (268 terminals)
        //   "\u001b[1m$<2>" (9 terminals)
        //   "?1m" (1 terminal)
        //   "\u001b[1;43m" (1 terminal)
        /// <summary>Get the value of capability "enter_bold_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_bold_mode". If null, this terminal information does not support the capability "enter_bold_mode".</returns>
        public string? EnterBoldMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterBoldMode);

        // Capability name: enter_ca_mode
        // Terminals supporting this capability: 200 terminals
        // Values of this capability:
        //   "\u001b7\u001b[?47h" (63 terminals)
        //   "\u001b[?1049h" (45 terminals)
        //   "\u001b[?1049h\u001b[22;0;0t" (35 terminals)
        //   "\u001b[ Q\u001b[?67;8h" (24 terminals)
        //   "\u001bw0" (8 terminals)
        //   "\u001b[?47h" (7 terminals)
        //   "\u001b[ Q" (5 terminals)
        //   "\u001b\\1\u001b-07 " (4 terminals)
        //   "\u001b7" (4 terminals)
        //   "\u001bw1" (2 terminals)
        //   "??1049h" (1 terminal)
        //   "\u001b[?1048h\u001b[?1047h" (1 terminal)
        //   "\u001b[22;0;0t" (1 terminal)
        /// <summary>Get the value of capability "enter_ca_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_ca_mode". If null, this terminal information does not support the capability "enter_ca_mode".</returns>
        public string? EnterCaMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterCaMode);

        // Capability name: enter_dim_mode
        // Terminals supporting this capability: 191 terminals
        // Values of this capability:
        //   "\u001b[2m" (164 terminals)
        //   "\u001bGp" (10 terminals)
        //   "\u001b[0t\u001b[2m" (8 terminals)
        //   "\u001b`7\u001b)" (7 terminals)
        //   "\u001b[=5h" (2 terminals)
        /// <summary>Get the value of capability "enter_dim_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_dim_mode". If null, this terminal information does not support the capability "enter_dim_mode".</returns>
        public string? EnterDimMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterDimMode);

        // Capability name: enter_insert_mode
        // Terminals supporting this capability: 285 terminals
        // Values of this capability:
        //   "\u001b[4h" (256 terminals)
        //   "\u001bq" (25 terminals)
        //   "\u001bF" (3 terminals)
        //   "?4h" (1 terminal)
        /// <summary>Get the value of capability "enter_insert_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_insert_mode". If null, this terminal information does not support the capability "enter_insert_mode".</returns>
        public string? EnterInsertMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterInsertMode);

        // Capability name: enter_italics_mode
        // Terminals supporting this capability: 87 terminals
        // Values of this capability:
        //   "\u001b[3m" (87 terminals)
        /// <summary>Get the value of capability "enter_italics_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_italics_mode". If null, this terminal information does not support the capability "enter_italics_mode".</returns>
        public string? EnterItalicsMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterItalicsMode);

        // Capability name: enter_pc_charset_mode
        // Terminals supporting this capability: 25 terminals
        // Values of this capability:
        //   "\u001b[11m" (25 terminals)
        /// <summary>Get the value of capability "enter_pc_charset_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_pc_charset_mode". If null, this terminal information does not support the capability "enter_pc_charset_mode".</returns>
        public string? EnterPcCharsetMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterPcCharsetMode);

        // Capability name: enter_protected_mode
        // Terminals supporting this capability: 17 terminals
        // Values of this capability:
        //   "\u001b)" (10 terminals)
        //   "\u001b`7\u001b)" (7 terminals)
        /// <summary>Get the value of capability "enter_protected_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_protected_mode". If null, this terminal information does not support the capability "enter_protected_mode".</returns>
        public string? EnterProtectedMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterProtectedMode);

        // Capability name: enter_reverse_mode
        // Terminals supporting this capability: 314 terminals
        // Values of this capability:
        //   "\u001b[7m" (269 terminals)
        //   "\u001bG4" (18 terminals)
        //   "\u001b[7m$<2>" (9 terminals)
        //   "\u001b[1t\u001b[7m" (8 terminals)
        //   "\u001b`6\u001b)" (7 terminals)
        //   "?7m" (1 terminal)
        //   "\u001b[7;34m" (1 terminal)
        //   "\u001b]" (1 terminal)
        /// <summary>Get the value of capability "enter_reverse_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_reverse_mode". If null, this terminal information does not support the capability "enter_reverse_mode".</returns>
        public string? EnterReverseMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterReverseMode);

        // Capability name: enter_secure_mode
        // Terminals supporting this capability: 178 terminals
        // Values of this capability:
        //   "\u001b[8m" (165 terminals)
        //   "\u001bG1" (12 terminals)
        //   "?8m" (1 terminal)
        /// <summary>Get the value of capability "enter_secure_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_secure_mode". If null, this terminal information does not support the capability "enter_secure_mode".</returns>
        public string? EnterSecureMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterSecureMode);

        // Capability name: enter_shadow_mode
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b[1:2m" (2 terminals)
        /// <summary>Get the value of capability "enter_shadow_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_shadow_mode". If null, this terminal information does not support the capability "enter_shadow_mode".</returns>
        public string? EnterShadowMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterShadowMode);

        // Capability name: enter_standout_mode
        // Terminals supporting this capability: 317 terminals
        // Values of this capability:
        //   "\u001b[7m" (246 terminals)
        //   "\u001b[3m" (16 terminals)
        //   "\u001bGt" (10 terminals)
        //   "\u001b[1;7m" (8 terminals)
        //   "\u001b[1t\u001b[7m" (8 terminals)
        //   "\u001b[7m$<2>" (8 terminals)
        //   "\u001bG4" (8 terminals)
        //   "\u001b`6\u001b)" (7 terminals)
        //   "\u001bR\u001b0P\u001bV" (3 terminals)
        //   "?7m" (1 terminal)
        //   "\u001b[7;31m" (1 terminal)
        //   "\u001b]" (1 terminal)
        /// <summary>Get the value of capability "enter_standout_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_standout_mode". If null, this terminal information does not support the capability "enter_standout_mode".</returns>
        public string? EnterStandoutMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterStandoutMode);

        // Capability name: enter_subscript_mode
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b[74m" (2 terminals)
        /// <summary>Get the value of capability "enter_subscript_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_subscript_mode". If null, this terminal information does not support the capability "enter_subscript_mode".</returns>
        public string? EnterSubscriptMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterSubscriptMode);

        // Capability name: enter_superscript_mode
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b[73m" (2 terminals)
        /// <summary>Get the value of capability "enter_superscript_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_superscript_mode". If null, this terminal information does not support the capability "enter_superscript_mode".</returns>
        public string? EnterSuperscriptMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterSuperscriptMode);

        // Capability name: enter_underline_mode
        // Terminals supporting this capability: 306 terminals
        // Values of this capability:
        //   "\u001b[4m" (267 terminals)
        //   "\u001bG8" (18 terminals)
        //   "\u001b[2t\u001b[4m" (8 terminals)
        //   "\u001b[4m$<2>" (8 terminals)
        //   "\u001b0`" (3 terminals)
        //   "?4m" (1 terminal)
        //   "\u001b[4;42m" (1 terminal)
        /// <summary>Get the value of capability "enter_underline_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_underline_mode". If null, this terminal information does not support the capability "enter_underline_mode".</returns>
        public string? EnterUnderlineMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterUnderlineMode);

        // Capability name: enter_xon_mode
        // Terminals supporting this capability: 12 terminals
        // Values of this capability:
        //   "\u001bc21" (10 terminals)
        //   "\u000f" (2 terminals)
        /// <summary>Get the value of capability "enter_xon_mode".</summary>
        /// <returns>If not null it is the value of the capability "enter_xon_mode". If null, this terminal information does not support the capability "enter_xon_mode".</returns>
        public string? EnterXonMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.EnterXonMode);

        // Capability name: erase_chars
        // Terminals supporting this capability: 203 terminals
        // Values of this capability:
        //   "\u001b[%p1%dX" (180 terminals)
        //   "\u001b[%p1%dX$<5>" (21 terminals)
        //   "?%p1%dX" (1 terminal)
        //   "\u001b[%p1%dX$<.1*>" (1 terminal)
        /// <summary>Get the value of capability "erase_chars".</summary>
        /// <param name="n"><see cref="int"/> value that is the number of characters to erase</param>
        /// <returns>If not null it is the value of the capability "erase_chars". If null, this terminal information does not support the capability "erase_chars".</returns>
        public string? EraseChars(int n)
            => n >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.EraseChars, n)
                : throw new ArgumentOutOfRangeException(nameof(n));

        // Capability name: erase_overstrike
        // Terminals supporting this capability: 318 terminals
        // Values of this capability:
        //   false (279 terminals)
        //   true (39 terminals)
        /// <summary>Get the value of capability "erase_overstrike".</summary>
        /// <returns>If not null it is the value of the capability "erase_overstrike". If null, this terminal information does not support the capability "erase_overstrike".</returns>
        public bool? EraseOverstrike => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.EraseOverstrike);

        // Capability name: exit_alt_charset_mode
        // Terminals supporting this capability: 307 terminals
        // Values of this capability:
        //   "\u000f" (174 terminals)
        //   "\u001b(B" (74 terminals)
        //   "\u000f$<20>" (21 terminals)
        //   "\u001bcD" (10 terminals)
        //   "\u001b[10m" (9 terminals)
        //   "\u001bH\u0003" (7 terminals)
        //   "\u001b%%" (6 terminals)
        //   "\u001b2" (3 terminals)
        //   "\u001b%" (2 terminals)
        //   "\u001b(B$<4>" (1 terminal)
        /// <summary>Get the value of capability "exit_alt_charset_mode".</summary>
        /// <returns>If not null it is the value of the capability "exit_alt_charset_mode". If null, this terminal information does not support the capability "exit_alt_charset_mode".</returns>
        public string? ExitAltCharsetMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ExitAltCharsetMode);

        // Capability name: exit_am_mode
        // Terminals supporting this capability: 237 terminals
        // Values of this capability:
        //   "\u001b[?7l" (224 terminals)
        //   "\u001bd." (10 terminals)
        //   "\u001b[=7l" (2 terminals)
        //   "??7l" (1 terminal)
        /// <summary>Get the value of capability "exit_am_mode".</summary>
        /// <returns>If not null it is the value of the capability "exit_am_mode". If null, this terminal information does not support the capability "exit_am_mode".</returns>
        public string? ExitAmMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ExitAmMode);

        // Capability name: exit_attribute_mode
        // Terminals supporting this capability: 317 terminals
        // Values of this capability:
        //   "\u001b[m\u000f" (121 terminals)
        //   "\u001b(B\u001b[m" (40 terminals)
        //   "\u001b[0m\u000f" (35 terminals)
        //   "\u001b[0m\u000f$<20>" (29 terminals)
        //   "\u001b[m\u001b(B" (26 terminals)
        //   "\u001b[m" (13 terminals)
        //   "\u001b(\u001bH\u0003\u001bG0\u001bcD" (10 terminals)
        //   "\u001b[0m" (10 terminals)
        //   "\u001b(\u001bH\u0003" (7 terminals)
        //   "\u001b[0;10m" (6 terminals)
        //   "\u001b[m\u000f$<2>" (6 terminals)
        //   "\u001bG0" (6 terminals)
        //   "\u001b0@" (3 terminals)
        //   "\u001bG0\u001b[=5l" (2 terminals)
        //   "?0m\u001b(B" (1 terminal)
        //   "\u001b[m\u001b(B$<2>" (1 terminal)
        //   "\u001bI\u001b\\\u001bG" (1 terminal)
        /// <summary>Get the value of capability "exit_attribute_mode".</summary>
        /// <returns>If not null it is the value of the capability "exit_attribute_mode". If null, this terminal information does not support the capability "exit_attribute_mode".</returns>
        public string? ExitAttributeMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ExitAttributeMode);

        // Capability name: exit_ca_mode
        // Terminals supporting this capability: 200 terminals
        // Values of this capability:
        //   "\u001b[2J\u001b[?47l\u001b8" (63 terminals)
        //   "\u001b[?1049l" (41 terminals)
        //   "\u001b[?1049l\u001b[23;0;0t" (35 terminals)
        //   "\u001b[ R" (29 terminals)
        //   "\u001bw1" (8 terminals)
        //   "\u001b[2J\u001b[?47l" (7 terminals)
        //   "\u001b[2J\u001b8" (4 terminals)
        //   "\u001b[r\u001b[?1049l" (4 terminals)
        //   "\u001b\\2\u001b-07 " (2 terminals)
        //   "\u001b\\3\u001b-07 " (2 terminals)
        //   "\u001bw0" (2 terminals)
        //   "??1049l" (1 terminal)
        //   "\u001b[?1047l\u001b[?1048l" (1 terminal)
        //   "\u001b[23;0;0t" (1 terminal)
        /// <summary>Get the value of capability "exit_ca_mode".</summary>
        /// <returns>If not null it is the value of the capability "exit_ca_mode". If null, this terminal information does not support the capability "exit_ca_mode".</returns>
        public string? ExitCaMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ExitCaMode);

        // Capability name: exit_insert_mode
        // Terminals supporting this capability: 285 terminals
        // Values of this capability:
        //   "\u001b[4l" (256 terminals)
        //   "\u001br" (25 terminals)
        //   "\u001bF" (3 terminals)
        //   "?4l" (1 terminal)
        /// <summary>Get the value of capability "exit_insert_mode".</summary>
        /// <returns>If not null it is the value of the capability "exit_insert_mode". If null, this terminal information does not support the capability "exit_insert_mode".</returns>
        public string? ExitInsertMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ExitInsertMode);

        // Capability name: exit_italics_mode
        // Terminals supporting this capability: 87 terminals
        // Values of this capability:
        //   "\u001b[23m" (87 terminals)
        /// <summary>Get the value of capability "exit_italics_mode".</summary>
        /// <returns>If not null it is the value of the capability "exit_italics_mode". If null, this terminal information does not support the capability "exit_italics_mode".</returns>
        public string? ExitItalicsMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ExitItalicsMode);

        // Capability name: exit_pc_charset_mode
        // Terminals supporting this capability: 25 terminals
        // Values of this capability:
        //   "\u001b[10m" (25 terminals)
        /// <summary>Get the value of capability "exit_pc_charset_mode".</summary>
        /// <returns>If not null it is the value of the capability "exit_pc_charset_mode". If null, this terminal information does not support the capability "exit_pc_charset_mode".</returns>
        public string? ExitPcCharsetMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ExitPcCharsetMode);

        // Capability name: exit_scancode_mode
        // Terminals supporting this capability: 1 terminal
        // Values of this capability:
        //   "\u001b[?0;0r\u001b>\u001b[?3l\u001b[?4l\u001b[?5l\u001b[?7h\u001b[?8h" (1 terminal)
        /// <summary>Get the value of capability "exit_scancode_mode".</summary>
        /// <returns>If not null it is the value of the capability "exit_scancode_mode". If null, this terminal information does not support the capability "exit_scancode_mode".</returns>
        public string? ExitScancodeMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ExitScancodeMode);

        // Capability name: exit_shadow_mode
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b[22m" (2 terminals)
        /// <summary>Get the value of capability "exit_shadow_mode".</summary>
        /// <returns>If not null it is the value of the capability "exit_shadow_mode". If null, this terminal information does not support the capability "exit_shadow_mode".</returns>
        public string? ExitShadowMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ExitShadowMode);

        // Capability name: exit_standout_mode
        // Terminals supporting this capability: 317 terminals
        // Values of this capability:
        //   "\u001b[27m" (172 terminals)
        //   "\u001b[m" (81 terminals)
        //   "\u001bG0" (18 terminals)
        //   "\u001b[23m" (16 terminals)
        //   "\u001b[0m" (10 terminals)
        //   "\u001b[m$<2>" (8 terminals)
        //   "\u001b(" (7 terminals)
        //   "\u001bR\u001b0@\u001bV" (3 terminals)
        //   "?27m" (1 terminal)
        //   "\u001b\\" (1 terminal)
        /// <summary>Get the value of capability "exit_standout_mode".</summary>
        /// <returns>If not null it is the value of the capability "exit_standout_mode". If null, this terminal information does not support the capability "exit_standout_mode".</returns>
        public string? ExitStandoutMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ExitStandoutMode);

        // Capability name: exit_subscript_mode
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b[75m" (2 terminals)
        /// <summary>Get the value of capability "exit_subscript_mode".</summary>
        /// <returns>If not null it is the value of the capability "exit_subscript_mode". If null, this terminal information does not support the capability "exit_subscript_mode".</returns>
        public string? ExitSubscriptMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ExitSubscriptMode);

        // Capability name: exit_superscript_mode
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b[75m" (2 terminals)
        /// <summary>Get the value of capability "exit_superscript_mode".</summary>
        /// <returns>If not null it is the value of the capability "exit_superscript_mode". If null, this terminal information does not support the capability "exit_superscript_mode".</returns>
        public string? ExitSuperscriptMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ExitSuperscriptMode);

        // Capability name: exit_underline_mode
        // Terminals supporting this capability: 305 terminals
        // Values of this capability:
        //   "\u001b[24m" (209 terminals)
        //   "\u001b[m" (56 terminals)
        //   "\u001bG0" (18 terminals)
        //   "\u001b[0m" (10 terminals)
        //   "\u001b[m$<2>" (8 terminals)
        //   "\u001b0@" (3 terminals)
        //   "?24m" (1 terminal)
        /// <summary>Get the value of capability "exit_underline_mode".</summary>
        /// <returns>If not null it is the value of the capability "exit_underline_mode". If null, this terminal information does not support the capability "exit_underline_mode".</returns>
        public string? ExitUnderlineMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ExitUnderlineMode);

        // Capability name: exit_xon_mode
        // Terminals supporting this capability: 12 terminals
        // Values of this capability:
        //   "\u001bc20" (10 terminals)
        //   "\u000e" (2 terminals)
        /// <summary>Get the value of capability "exit_xon_mode".</summary>
        /// <returns>If not null it is the value of the capability "exit_xon_mode". If null, this terminal information does not support the capability "exit_xon_mode".</returns>
        public string? ExitXonMode => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ExitXonMode);

        // Capability name: flash_screen
        // Terminals supporting this capability: 215 terminals
        // Values of this capability:
        //   "\u001b[?5h$<100/>\u001b[?5l" (92 terminals)
        //   "\u001b[?5h$<200/>\u001b[?5l" (30 terminals)
        //   "\u001bg" (22 terminals)
        //   "\u001b`8$<100/>\u001b`9" (17 terminals)
        //   "\u001b[30h\u001b,$<100/>\u001b[30l" (12 terminals)
        //   "\u001b[?5h\u001b[?5l" (11 terminals)
        //   "\u001b[30h\u001b,$<250/>\u001b[30l" (8 terminals)
        //   "\u001b[30h\u001b,$<300/>\u001b[30l" (5 terminals)
        //   "\u001bb$<200/>\u001bd" (5 terminals)
        //   "\u001b[?5h$<20/>\u001b[?5l" (4 terminals)
        //   "\u001bd$<200/>\u001bb" (3 terminals)
        //   "\u001f@A\u001bW \u007f\u0012\u007f\u0012P\r\u0018\n" (3 terminals)
        //   "??5h$<100/>??5l" (1 terminal)
        //   "\u0007" (1 terminal)
        //   "\u001bg\u001b^ \u001b\\" (1 terminal)
        /// <summary>Get the value of capability "flash_screen".</summary>
        /// <returns>If not null it is the value of the capability "flash_screen". If null, this terminal information does not support the capability "flash_screen".</returns>
        public string? FlashScreen => _database.GetStringCapabilityValue(TermInfoStringCapabilities.FlashScreen);

        // Capability name: from_status_line
        // Terminals supporting this capability: 171 terminals
        // Values of this capability:
        //   "\u0007" (64 terminals)
        //   "\u001b[0$}" (56 terminals)
        //   "\r" (25 terminals)
        //   "\u001b[1;24r\u001b8" (9 terminals)
        //   "\u0001" (8 terminals)
        //   "\u001b\\" (6 terminals)
        //   "\n" (3 terminals)
        /// <summary>Get the value of capability "from_status_line".</summary>
        /// <returns>If not null it is the value of the capability "from_status_line". If null, this terminal information does not support the capability "from_status_line".</returns>
        public string? FromStatusLine => _database.GetStringCapabilityValue(TermInfoStringCapabilities.FromStatusLine);

        // Capability name: G0
        // Terminals supporting this capability: 24 terminals
        // Values of this capability:
        //   true (24 terminals)
        /// <summary>Get the value of extended capability "G0".</summary>
        /// <returns>If not null it is the value of the extended capability "G0". If null, this terminal information does not support the extended capability "G0".</returns>
        /// <remarks>
        /// This capability means whether the terminal can handle ISO-2022 font selection sequences.
        /// (See "man screen(1)")
        /// </remarks>
        public bool? G0 => _database.GetBooleanCapabilityValue("G0");

        // Capability name: generic_type
        // Terminals supporting this capability: 318 terminals
        // Values of this capability:
        //   false (318 terminals)
        /// <summary>Get the value of capability "generic_type".</summary>
        /// <returns>If not null it is the value of the capability "generic_type". If null, this terminal information does not support the capability "generic_type".</returns>
        public bool? GenericType => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.GenericType);

        // Capability name: gnu_has_meta_key
        // Terminals supporting this capability: 21 terminals
        // Values of this capability:
        //   false (21 terminals)
        /// <summary>Get the value of capability "gnu_has_meta_key".</summary>
        /// <returns>If not null it is the value of the capability "gnu_has_meta_key". If null, this terminal information does not support the capability "gnu_has_meta_key".</returns>
        public bool? GnuHasMetaKey => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.GnuHasMetaKey);

        // Capability name: grbom
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b[>1l" (2 terminals)
        /// <summary>Get the value of extended capability "grbom".</summary>
        /// <returns>If not null it is the value of the extended capability "grbom". If null, this terminal information does not support the extended capability "grbom".</returns>
        public string? Grbom => _database.GetStringCapabilityValue("grbom");

        // Capability name: gsbom
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b[>1h" (2 terminals)
        /// <summary>Get the value of extended capability "gsbom".</summary>
        /// <returns>If not null it is the value of the extended capability "gsbom". If null, this terminal information does not support the extended capability "gsbom".</returns>
        public string? Gsbom => _database.GetStringCapabilityValue("gsbom");

        // Capability name: hard_copy
        // Terminals supporting this capability: 318 terminals
        // Values of this capability:
        //   false (318 terminals)
        /// <summary>Get the value of capability "hard_copy".</summary>
        /// <returns>If not null it is the value of the capability "hard_copy". If null, this terminal information does not support the capability "hard_copy".</returns>
        public bool? HardCopy => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.HardCopy);

        // Capability name: hard_cursor
        // Terminals supporting this capability: 194 terminals
        // Values of this capability:
        //   false (194 terminals)
        /// <summary>Get the value of capability "hard_cursor".</summary>
        /// <returns>If not null it is the value of the capability "hard_cursor". If null, this terminal information does not support the capability "hard_cursor".</returns>
        public bool? HardCursor => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.HardCursor);

        // Capability name: has_hardware_tabs
        // Terminals supporting this capability: 21 terminals
        // Values of this capability:
        //   true (21 terminals)
        /// <summary>Get the value of capability "has_hardware_tabs".</summary>
        /// <returns>If not null it is the value of the capability "has_hardware_tabs". If null, this terminal information does not support the capability "has_hardware_tabs".</returns>
        public bool? HasHardwareTabs => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.HasHardwareTabs);

        // Capability name: has_meta_key
        // Terminals supporting this capability: 318 terminals
        // Values of this capability:
        //   false (188 terminals)
        //   true (130 terminals)
        /// <summary>Get the value of capability "has_meta_key".</summary>
        /// <returns>If not null it is the value of the capability "has_meta_key". If null, this terminal information does not support the capability "has_meta_key".</returns>
        public bool? HasMetaKey => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.HasMetaKey);

        // Capability name: has_print_wheel
        // Terminals supporting this capability: 133 terminals
        // Values of this capability:
        //   false (133 terminals)
        /// <summary>Get the value of capability "has_print_wheel".</summary>
        /// <returns>If not null it is the value of the capability "has_print_wheel". If null, this terminal information does not support the capability "has_print_wheel".</returns>
        public bool? HasPrintWheel => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.HasPrintWheel);

        // Capability name: has_status_line
        // Terminals supporting this capability: 318 terminals
        // Values of this capability:
        //   true (165 terminals)
        //   false (153 terminals)
        /// <summary>Get the value of capability "has_status_line".</summary>
        /// <returns>If not null it is the value of the capability "has_status_line". If null, this terminal information does not support the capability "has_status_line".</returns>
        public bool? HasStatusLine => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.HasStatusLine);

        // Capability name: hue_lightness_saturation
        // Terminals supporting this capability: 133 terminals
        // Values of this capability:
        //   false (133 terminals)
        /// <summary>Get the value of capability "hue_lightness_saturation".</summary>
        /// <returns>If not null it is the value of the capability "hue_lightness_saturation". If null, this terminal information does not support the capability "hue_lightness_saturation".</returns>
        public bool? HueLightnessSaturation => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.HueLightnessSaturation);

        // Capability name: init_1string
        // Terminals supporting this capability: 83 terminals
        // Values of this capability:
        //   "\u001b[?5W" (28 terminals)
        //   "\u001b[?47l\u001b=\u001b[?1l" (17 terminals)
        //   "\u001b[2;4;20;30l\u001b[?1;10l\u001b[12h\u001b[?7;8;25h" (8 terminals)
        //   "\u001bcB0\u001bcC1" (6 terminals)
        //   "\u001b`:\u001b`9$<30>" (5 terminals)
        //   "\u001b[!p" (4 terminals)
        //   "\u001b[62;1\"p\u001b[?5W" (4 terminals)
        //   "\u001bcB2\u001bcC3" (4 terminals)
        //   "\u001b[?47l\u001b>\u001b[?1l" (3 terminals)
        //   "\u001b`;\u001b`9$<30>" (2 terminals)
        //   "\u001b[90;1\"p\u001b[?5W$<6>" (1 terminal)
        //   "\u001b>\u001b[?3l\u001b[?4l\u001b[?5l\u001b[?7h\u001b[?8h" (1 terminal)
        /// <summary>Get the value of capability "init_1string".</summary>
        /// <returns>If not null it is the value of the capability "init_1string". If null, this terminal information does not support the capability "init_1string".</returns>
        public string? Init_1string => _database.GetStringCapabilityValue(TermInfoStringCapabilities.Init_1string);

        // Capability name: init_2string
        // Terminals supporting this capability: 257 terminals
        // Values of this capability:
        //   "\u001b[!p\u001b[?3;4l\u001b[4l\u001b>" (39 terminals)
        //   "\u001b[m\u001b[?7h\u001b[4l\u001b>\u001b7\u001b[r\u001b[?1;3;4;6l\u001b8" (30 terminals)
        //   "\u001b[2;4;20;30l\u001b[?1;4;10;16l\u001b[12h\u001b[?7;8;25;67h" (24 terminals)
        //   "\u001b)0" (21 terminals)
        //   "\u001b[r\u001b[m\u001b[2J\u001b[H\u001b[?7h\u001b[?1;3;4;6l\u001b[4l" (20 terminals)
        //   "\u001bd$\u001bcD\u001b'\u001br\u001bH\u0003\u001bd/\u001bO\u001be1\u001bd*\u001b`@\u001b`9\u001b`1\u000e\u0014\u001bl" (10 terminals)
        //   "\u001b7\u001b[r\u001b[m\u001b[?7h\u001b[?1;3;4;6l\u001b[4l\u001b8\u001b>" (9 terminals)
        //   "\u001b[!p\u001b[?7;19;67h\u001b[?1;3;4l\u001b(B\u001b)0\u000f\u001b[2J\u001b[1;1H\u001b>$<200>" (8 terminals)
        //   "\u000e\u0014\u001b'\u001b(" (7 terminals)
        //   "\u001b7\u001b[r\u001b[m\u001b[?7h\u001b[?1;4;6l\u001b[4l\u001b8\u001b>\u001b]R" (7 terminals)
        //   "\u001b[!p\u001b[?3;7;19;67h\u001b[?1;4l\u001b(B\u001b)0\u000f\u001b[2J\u001b[1;1H\u001b>$<200>" (6 terminals)
        //   "\u001b[!p\u001b[?7;19;67h\u001b[?1;3;4l\u001b[1;0%w\u001b(B\u001b)0\u000f\u001b[2J\u001b[1;1H\u001b>$<200>" (6 terminals)
        //   "\u001b[4l\u001b>\u001b[?1034l" (6 terminals)
        //   "\u001b[!p\u001b[?3;7;19;67h\u001b[?1;4l\u001b[1;0%w\u001b(B\u001b)0\u000f\u001b[2J\u001b[1;1H\u001b>$<200>" (4 terminals)
        //   "\u001b[1;24r\u001b[?10;3l\u001b[?1;25h\u001b[4l\u001b[m\u001b(B\u001b=" (4 terminals)
        //   "\u001b[2;4;20;30l\u001b[?1;4;10;16l\u001b[12h\u001b[?7;8;25h" (4 terminals)
        //   "\u001b[2;4;20;30l\u001b[?1;4;10;16l\u001b[12h\u001b[?7;8;25h$<16>" (4 terminals)
        //   "\u001b[r\u001b[m\u001b[2J\u001b[?7;25h\u001b[?1;3;4;5;6;9;66;1000;1001;1049l\u001b[4l" (4 terminals)
        //   "\u001b>\u001b(B\u001b)0\u000f" (4 terminals)
        //   "\u001b7\u001b[r\u001b[m\u001b[?7h\u001b[?1;4;6l\u001b[4l\u001b8\u001b>" (4 terminals)
        //   "\u001b[!p\u001b[?3;7;19;67h\u001b[?4l\u001b[1;0%w\u001b(B\u001b)0\u000f\u001b[2J\u001b[1;1H$<200>" (3 terminals)
        //   "\u001bV\u001bB" (3 terminals)
        //   "\u001b[!p\u001b[?3;7;19;67h\u001b[?4l\u001b(B\u001b)0\u000f\u001b[2J\u001b[1;1H$<200>" (2 terminals)
        //   "\u001b[=3h\u001bF1\u001bd\u001bG0\u001b[=5l\u001b%\u001bl" (2 terminals)
        //   "\u001b[0m" (2 terminals)
        //   "\u001b[12h" (2 terminals)
        //   "\u001b>\u001b[?3h\u001b[?4l\u001b[?5l\u001b[?7h\u001b[?8h\u001b[1;24r\u001b[24;1H" (2 terminals)
        //   "\u001b>\u001b[?3h\u001b[?4l\u001b[?5l\u001b[?7l\u001b[?8h\u001b[1;24r\u001b[24;1H" (2 terminals)
        //   "\u001b>\u001b[?3l\u001b[?4l\u001b[?5l\u001b[?7h\u001b[?8h\u001b[1;24r\u001b[24;1H" (2 terminals)
        //   "\u001b>\u001b[?3l\u001b[?4l\u001b[?5l\u001b[?7l\u001b[?8h\u001b[1;24r\u001b[24;1H" (2 terminals)
        //   "\u001b;`ZQ\u001b:iC\u001b:iE\u0011" (1 terminal)
        //   "\u001b[1;24r\u001b[24;1H" (1 terminal)
        //   "\u001b[1;25r\u001b[25;1H" (1 terminal)
        //   "\u001b[2;4;20;30;40l\u001b[?1;10;16l\u001b[12h\u001b[?7;8;25h" (1 terminal)
        //   "\u001b[62\"p\u001b G?m??7h\u001b>\u001b7??1;3;4;6l?4l?r\u001b8" (1 terminal)
        //   "\u001b]R\u001b]P3FFFF80\u001b[?8c" (1 terminal)
        //   "\u001b>\u001b[?3h\u001b[?4l\u001b[?5l\u001b[?7h\u001b[?8h\u001b[1;42r\u001b[42;1H" (1 terminal)
        //   "\u001b>\u001b[?3h\u001b[?4l\u001b[?5l\u001b[?7l\u001b[?8h\u001b[1;42r\u001b[42;1H" (1 terminal)
        //   "\u001bDF\u001bC\u001bb\u001bG0\u001bg\u001br\u001bO\u001b'\u001b(\u001b%\u001bw\u001bX\u001be \u000f\t\u001bk\u001b016\u001b004\u001bx0??\u001bx1??\u001bx2??\t\u001bx3??\u001bx4\r?\u001b\\2\u001b-07 " (1 terminal)
        //   "\u001bDF\u001bC\u001bb\u001bG0\u001bg\u001br\u001bO\u001b'\u001b(\u001b%\u001bw\u001bX\u001be \u000f\t\u001bl\u001b016\u001b004\u001bx0??\u001bx1??\u001bx2??\t\u001bx3??\u001bx4\r?" (1 terminal)
        //   "\u001bDF\u001bC\u001bb\u001bG0\u001br\u001bO\u001b'\u001b(\u001b%\u001bw\u001bX\u001be \u000f\t\u001bk\u001b016\u001b004\u001bx0??\u001bx1??\u001bx2??\t\u001bx3??\u001bx4\r?\u001b\\3\u001b-07 " (1 terminal)
        //   "\u001bDF\u001bC\u001bd\u001bG0\u001bg\u001br\u001bO\u001b'\u001b(\u001b%\u001bw\u001bX\u001be \u000f\t\u001bk\u001b016\u001b004\u001bx0??\u001bx1??\u001bx2??\t\u001bx3??\u001bx4\r?\u001b\\2\u001b-07 \t" (1 terminal)
        //   "\u001bDF\u001bC\u001bd\u001bG0\u001bg\u001br\u001bO\u001b'\u001b(\u001b%\u001bw\u001bX\u001be \u000f\t\u001bk\u001b016\u001b004\u001bx0??\u001bx1??\u001bx2??\t\u001bx3??\u001bx4\r?\u001b\\3\u001b-07 \t" (1 terminal)
        //   "\u001bDF\u001bC\u001bd\u001bG0\u001bg\u001br\u001bO\u001b'\u001b(\u001b%\u001bw\u001bX\u001be \u000f\t\u001bl\u001b016\u001b004\u001bx0??\u001bx1??\u001bx2??\t\u001bx3??\u001bx4\r?\u001bf\r" (1 terminal)
        /// <summary>Get the value of capability "init_2string".</summary>
        /// <returns>If not null it is the value of the capability "init_2string". If null, this terminal information does not support the capability "init_2string".</returns>
        public string? Init_2string => _database.GetStringCapabilityValue(TermInfoStringCapabilities.Init_2string);

        // Capability name: init_3string
        // Terminals supporting this capability: 52 terminals
        // Values of this capability:
        //   "\u001b>\u001b(B\u001b)0\u000f\u001b[m" (32 terminals)
        //   "\u001b[m" (8 terminals)
        //   "\u001bwJ\u001bw1$<150>" (8 terminals)
        //   "\u001bw0$<20>" (2 terminals)
        //   "\u001b[?67h\u001b[64;1\"p" (1 terminal)
        //   "\u001b>\u000f\u001b)0\u001b(B\u001b[63;0w\u001b[m" (1 terminal)
        /// <summary>Get the value of capability "init_3string".</summary>
        /// <returns>If not null it is the value of the capability "init_3string". If null, this terminal information does not support the capability "init_3string".</returns>
        public string? Init_3string => _database.GetStringCapabilityValue(TermInfoStringCapabilities.Init_3string);

        // Capability name: init_file
        // Terminals supporting this capability: 5 terminals
        // Values of this capability:
        //   "/usr/share/tabset/vt100" (4 terminals)
        //   "/usr/share/tabset/vt300" (1 terminal)
        /// <summary>Get the value of capability "init_file".</summary>
        /// <returns>If not null it is the value of the capability "init_file". If null, this terminal information does not support the capability "init_file".</returns>
        public string? InitFile => _database.GetStringCapabilityValue(TermInfoStringCapabilities.InitFile);

        // Capability name: init_tabs
        // Terminals supporting this capability: 245 terminals
        // Values of this capability:
        //   8 (245 terminals)
        /// <summary>Get the value of capability "init_tabs".</summary>
        /// <returns>If not null it is the value of the capability "init_tabs". If null, this terminal information does not support the capability "init_tabs".</returns>
        public int? InitTabs => _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.InitTabs);

        // Capability name: initialize_color
        // Terminals supporting this capability: 39 terminals
        // Values of this capability:
        //   "\u001b]4;%p1%d;rgb:%p2%{255}%*%{1000}%/%2.2X/%p3%{255}%*%{1000}%/%2.2X/%p4%{255}%*%{1000}%/%2.2X\u001b\\" (16 terminals)
        //   "\u001b]P%p1%x%p2%{255}%*%{1000}%/%02x%p3%{255}%*%{1000}%/%02x%p4%{255}%*%{1000}%/%02x" (16 terminals)
        //   "\u001b]4;%p1%d;rgb:%p2%{65535}%*%{1000}%/%4.4X/%p3%{65535}%*%{1000}%/%4.4X/%p4%{65535}%*%{1000}%/%4.4X\u001b\\" (4 terminals)
        //   "\u001b[66;%p1%d;%?%p2%{250}%<%t%{0}%e%p2%{500}%<%t%{16}%e%p2%{750}%<%t%' '%e%'0'%;%?%p3%{250}%<%t%{0}%e%p3%{500}%<%t%{4}%e%p3%{750}%<%t%{8}%e%{12}%;%?%p4%{250}%<%t%{0}%e%p4%{500}%<%t%{1}%e%p4%{750}%<%t%{2}%e%{3}%;%{1}%+%+%+%dw" (1 terminal)
        //   "\u001b]P%?%p1%{9}%>%t%p1%{10}%-%'a'%+%c%e%p1%d%;%p2%{255}%*%{1000}%/%Pr%gr%{16}%/%Px%?%gx%{9}%>%t%gx%{10}%-%'a'%+%c%e%gx%d%;%gr%{15}%&%Px%?%gx%{9}%>%t%gx%{10}%-%'a'%+%c%e%gx%d%;%p3%{255}%*%{1000}%/%Pr%gr%{16}%/%Px%?%gx%{9}%>%t%gx%{10}%-%'a'%+%c%e%gx%d%;%gr%{15}%&%Px%?%gx%{9}%>%t%gx%{10}%-%'a'%+%c%e%gx%d%;%p4%{255}%*%{1000}%/%Pr%gr%{16}%/%Px%?%gx%{9}%>%t%gx%{10}%-%'a'%+%c%e%gx%d%;%gr%{15}%&%Px%?%gx%{9}%>%t%gx%{10}%-%'a'%+%c%e%gx%d%;" (1 terminal)
        //   "\u001b]P%p1%{15}%&%X%p2%{255}%&%02X%p3%{255}%&%02X%p4%{255}%&%02X" (1 terminal)
        /// <summary>Get the value of capability "initialize_color".</summary>
        /// <param name="colorNumber"><see cref="int"/> value that is the number of the color to change (0&lt;= <paramref name="colorNumber"/> &lt; <see cref="MaxColors"/>)</param>
        /// <param name="r"><see cref="int"/> value that is the red component of the color (0&lt;= <paramref name="r"/> &lt;= 1000)</param>
        /// <param name="g"><see cref="int"/> value that is the green component of the color (0&lt;= <paramref name="g"/> &lt;= 1000)</param>
        /// <param name="b"><see cref="int"/> value that is the blue component of the color (0&lt;= <paramref name="b"/> &lt;= 1000)</param>
        /// <returns>If not null it is the value of the capability "initialize_color". If null, this terminal information does not support the capability "initialize_color".</returns>
        public string? InitializeColor(int colorNumber, int r, int g, int b)
        {
            var maxColors = _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.MaxColors);
            return
                maxColors is null
                ? null
                : colorNumber.IsOutOfHalfClosedInterval(0, maxColors.Value)
                ? throw new ArgumentOutOfRangeException(nameof(colorNumber))
                : r.IsOutOfHalfClosedInterval(0, 1000)
                ? throw new ArgumentOutOfRangeException(nameof(r))
                : g.IsOutOfHalfClosedInterval(0, 1000)
                ? throw new ArgumentOutOfRangeException(nameof(g))
                : b.IsOutOfHalfClosedInterval(0, 1000)
                ? throw new ArgumentOutOfRangeException(nameof(b))
                : _database.GetStringCapabilityValue(TermInfoStringCapabilities.InitializeColor, colorNumber, r, g, b);
        }

        // Capability name: insert_character
        // Terminals supporting this capability: 55 terminals
        // Values of this capability:
        //   "\u001b[@" (47 terminals)
        //   "\u001bQ" (8 terminals)
        /// <summary>Get the value of capability "insert_character".</summary>
        /// <returns>If not null it is the value of the capability "insert_character". If null, this terminal information does not support the capability "insert_character".</returns>
        public string? InsertCharacter => _database.GetStringCapabilityValue(TermInfoStringCapabilities.InsertCharacter);

        // Capability name: insert_line
        // Terminals supporting this capability: 308 terminals
        // Values of this capability:
        //   "\u001b[L" (209 terminals)
        //   "\u001b[L$<5>" (33 terminals)
        //   "\u001b[L$<3>" (28 terminals)
        //   "\u001bE" (15 terminals)
        //   "\u001b[L$<2>" (9 terminals)
        //   "\u001bE$<4>" (6 terminals)
        //   "\u001bE$<11>" (4 terminals)
        //   "\u001bM$<2*>" (3 terminals)
        //   "?L" (1 terminal)
        /// <summary>Get the value of capability "insert_line".</summary>
        /// <returns>If not null it is the value of the capability "insert_line". If null, this terminal information does not support the capability "insert_line".</returns>
        public string? InsertLine => _database.GetStringCapabilityValue(TermInfoStringCapabilities.InsertLine);

        // Capability name: insert_null_glitch
        // Terminals supporting this capability: 315 terminals
        // Values of this capability:
        //   false (315 terminals)
        /// <summary>Get the value of capability "insert_null_glitch".</summary>
        /// <returns>If not null it is the value of the capability "insert_null_glitch". If null, this terminal information does not support the capability "insert_null_glitch".</returns>
        public bool? InsertNullGlitch => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.InsertNullGlitch);

        // Capability name: insert_padding
        // Terminals supporting this capability: 58 terminals
        // Values of this capability:
        //   "$<4>" (20 terminals)
        //   "$<1>" (16 terminals)
        //   "$<7>" (10 terminals)
        //   "$<3>" (6 terminals)
        //   "$<5>" (4 terminals)
        //   "$<6>" (2 terminals)
        /// <summary>Get the value of capability "insert_padding".</summary>
        /// <returns>If not null it is the value of the capability "insert_padding". If null, this terminal information does not support the capability "insert_padding".</returns>
        public string? InsertPadding => _database.GetStringCapabilityValue(TermInfoStringCapabilities.InsertPadding);

        // Capability name: ka2
        // Terminals supporting this capability: 113 terminals
        // Values of this capability:
        //   "\u001bOx" (113 terminals)
        /// <summary>Get the value of extended capability "ka2".</summary>
        /// <returns>If not null it is the value of the extended capability "ka2". If null, this terminal information does not support the extended capability "ka2".</returns>
        public string? Ka2 => _database.GetStringCapabilityValue("ka2");

        // Capability name: kb1
        // Terminals supporting this capability: 113 terminals
        // Values of this capability:
        //   "\u001bOt" (113 terminals)
        /// <summary>Get the value of extended capability "kb1".</summary>
        /// <returns>If not null it is the value of the extended capability "kb1". If null, this terminal information does not support the extended capability "kb1".</returns>
        public string? Kb1 => _database.GetStringCapabilityValue("kb1");

        // Capability name: kb3
        // Terminals supporting this capability: 113 terminals
        // Values of this capability:
        //   "\u001bOv" (113 terminals)
        /// <summary>Get the value of extended capability "kb3".</summary>
        /// <returns>If not null it is the value of the extended capability "kb3". If null, this terminal information does not support the extended capability "kb3".</returns>
        public string? Kb3 => _database.GetStringCapabilityValue("kb3");

        // Capability name: kc2
        // Terminals supporting this capability: 113 terminals
        // Values of this capability:
        //   "\u001bOr" (113 terminals)
        /// <summary>Get the value of extended capability "kc2".</summary>
        /// <returns>If not null it is the value of the extended capability "kc2". If null, this terminal information does not support the extended capability "kc2".</returns>
        public string? Kc2 => _database.GetStringCapabilityValue("kc2");

        // Capability name: kDC3
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[3;3~" (80 terminals)
        //   "\u001b[>3;3~" (1 terminal)
        /// <summary>Get the value of extended capability "kDC3".</summary>
        /// <returns>If not null it is the value of the extended capability "kDC3". If null, this terminal information does not support the extended capability "kDC3".</returns>
        public string? KDC3 => _database.GetStringCapabilityValue("kDC3");

        // Capability name: kDC4
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[3;4~" (80 terminals)
        //   "\u001b[>3;4~" (1 terminal)
        /// <summary>Get the value of extended capability "kDC4".</summary>
        /// <returns>If not null it is the value of the extended capability "kDC4". If null, this terminal information does not support the extended capability "kDC4".</returns>
        public string? KDC4 => _database.GetStringCapabilityValue("kDC4");

        // Capability name: kDC5
        // Terminals supporting this capability: 97 terminals
        // Values of this capability:
        //   "\u001b[3;5~" (88 terminals)
        //   "\u001b[3^" (8 terminals)
        //   "\u001b[>3;5~" (1 terminal)
        /// <summary>Get the value of extended capability "kDC5".</summary>
        /// <returns>If not null it is the value of the extended capability "kDC5". If null, this terminal information does not support the extended capability "kDC5".</returns>
        public string? KDC5 => _database.GetStringCapabilityValue("kDC5");

        // Capability name: kDC6
        // Terminals supporting this capability: 89 terminals
        // Values of this capability:
        //   "\u001b[3;6~" (80 terminals)
        //   "\u001b[3@" (8 terminals)
        //   "\u001b[>3;6~" (1 terminal)
        /// <summary>Get the value of extended capability "kDC6".</summary>
        /// <returns>If not null it is the value of the extended capability "kDC6". If null, this terminal information does not support the extended capability "kDC6".</returns>
        public string? KDC6 => _database.GetStringCapabilityValue("kDC6");

        // Capability name: kDC7
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[3;7~" (80 terminals)
        //   "\u001b[>3;7~" (1 terminal)
        /// <summary>Get the value of extended capability "kDC7".</summary>
        /// <returns>If not null it is the value of the extended capability "kDC7". If null, this terminal information does not support the extended capability "kDC7".</returns>
        public string? KDC7 => _database.GetStringCapabilityValue("kDC7");

        // Capability name: kDN
        // Terminals supporting this capability: 103 terminals
        // Values of this capability:
        //   "\u001b[1;2B" (77 terminals)
        //   "\u001b[b" (20 terminals)
        //   "\u001bO1;2B" (2 terminals)
        //   "\u001bO2B" (2 terminals)
        //   "\u001b[>1;2B" (1 terminal)
        //   "\u001b[2B" (1 terminal)
        /// <summary>Get the value of extended capability "kDN".</summary>
        /// <returns>If not null it is the value of the extended capability "kDN". If null, this terminal information does not support the extended capability "kDN".</returns>
        public string? KDN => _database.GetStringCapabilityValue("kDN");

        // Capability name: kDN3
        // Terminals supporting this capability: 89 terminals
        // Values of this capability:
        //   "\u001b[1;3B" (82 terminals)
        //   "\u001bO1;3B" (2 terminals)
        //   "\u001bO3B" (2 terminals)
        //   "\u001b[>1;3B" (1 terminal)
        //   "\u001b[3B" (1 terminal)
        //   "\u001b\u001b[B" (1 terminal)
        /// <summary>Get the value of extended capability "kDN3".</summary>
        /// <returns>If not null it is the value of the extended capability "kDN3". If null, this terminal information does not support the extended capability "kDN3".</returns>
        public string? KDN3 => _database.GetStringCapabilityValue("kDN3");

        // Capability name: kDN4
        // Terminals supporting this capability: 84 terminals
        // Values of this capability:
        //   "\u001b[1;4B" (77 terminals)
        //   "\u001bO1;4B" (2 terminals)
        //   "\u001bO4B" (2 terminals)
        //   "\u001b[>1;4B" (1 terminal)
        //   "\u001b[1;10B" (1 terminal)
        //   "\u001b[4B" (1 terminal)
        /// <summary>Get the value of extended capability "kDN4".</summary>
        /// <returns>If not null it is the value of the extended capability "kDN4". If null, this terminal information does not support the extended capability "kDN4".</returns>
        public string? KDN4 => _database.GetStringCapabilityValue("kDN4");

        // Capability name: kDN5
        // Terminals supporting this capability: 109 terminals
        // Values of this capability:
        //   "\u001b[1;5B" (83 terminals)
        //   "\u001bOb" (20 terminals)
        //   "\u001bO1;5B" (2 terminals)
        //   "\u001bO5B" (2 terminals)
        //   "\u001b[>1;5B" (1 terminal)
        //   "\u001b[5B" (1 terminal)
        /// <summary>Get the value of extended capability "kDN5".</summary>
        /// <returns>If not null it is the value of the extended capability "kDN5". If null, this terminal information does not support the extended capability "kDN5".</returns>
        public string? KDN5 => _database.GetStringCapabilityValue("kDN5");

        // Capability name: kDN6
        // Terminals supporting this capability: 96 terminals
        // Values of this capability:
        //   "\u001b[1;6B" (78 terminals)
        //   "\u001bOB" (12 terminals)
        //   "\u001bO1;6B" (2 terminals)
        //   "\u001bO6B" (2 terminals)
        //   "\u001b[>1;6B" (1 terminal)
        //   "\u001b[6B" (1 terminal)
        /// <summary>Get the value of extended capability "kDN6".</summary>
        /// <returns>If not null it is the value of the extended capability "kDN6". If null, this terminal information does not support the extended capability "kDN6".</returns>
        public string? KDN6 => _database.GetStringCapabilityValue("kDN6");

        // Capability name: kDN7
        // Terminals supporting this capability: 83 terminals
        // Values of this capability:
        //   "\u001b[1;7B" (77 terminals)
        //   "\u001bO1;7B" (2 terminals)
        //   "\u001bO7B" (2 terminals)
        //   "\u001b[>1;7B" (1 terminal)
        //   "\u001b[7B" (1 terminal)
        /// <summary>Get the value of extended capability "kDN7".</summary>
        /// <returns>If not null it is the value of the extended capability "kDN7". If null, this terminal information does not support the extended capability "kDN7".</returns>
        public string? KDN7 => _database.GetStringCapabilityValue("kDN7");

        // Capability name: kEND3
        // Terminals supporting this capability: 82 terminals
        // Values of this capability:
        //   "\u001b[1;3F" (75 terminals)
        //   "\u001b[8;3~" (3 terminals)
        //   "\u001b[>1;3F" (1 terminal)
        //   "\u001b[1;9F" (1 terminal)
        //   "\u001b[3F" (1 terminal)
        //   "\u001bO3F" (1 terminal)
        /// <summary>Get the value of extended capability "kEND3".</summary>
        /// <returns>If not null it is the value of the extended capability "kEND3". If null, this terminal information does not support the extended capability "kEND3".</returns>
        public string? KEND3 => _database.GetStringCapabilityValue("kEND3");

        // Capability name: kEND4
        // Terminals supporting this capability: 82 terminals
        // Values of this capability:
        //   "\u001b[1;4F" (75 terminals)
        //   "\u001b[8;4~" (3 terminals)
        //   "\u001b[>1;4F" (1 terminal)
        //   "\u001b[1;10F" (1 terminal)
        //   "\u001b[4F" (1 terminal)
        //   "\u001bO4F" (1 terminal)
        /// <summary>Get the value of extended capability "kEND4".</summary>
        /// <returns>If not null it is the value of the extended capability "kEND4". If null, this terminal information does not support the extended capability "kEND4".</returns>
        public string? KEND4 => _database.GetStringCapabilityValue("kEND4");

        // Capability name: kEND5
        // Terminals supporting this capability: 91 terminals
        // Values of this capability:
        //   "\u001b[1;5F" (77 terminals)
        //   "\u001b[8^" (8 terminals)
        //   "\u001b[8;5~" (3 terminals)
        //   "\u001b[>1;5F" (1 terminal)
        //   "\u001b[5F" (1 terminal)
        //   "\u001bO5F" (1 terminal)
        /// <summary>Get the value of extended capability "kEND5".</summary>
        /// <returns>If not null it is the value of the extended capability "kEND5". If null, this terminal information does not support the extended capability "kEND5".</returns>
        public string? KEND5 => _database.GetStringCapabilityValue("kEND5");

        // Capability name: kEND6
        // Terminals supporting this capability: 90 terminals
        // Values of this capability:
        //   "\u001b[1;6F" (76 terminals)
        //   "\u001b[8@" (8 terminals)
        //   "\u001b[8;6~" (3 terminals)
        //   "\u001b[>1;6F" (1 terminal)
        //   "\u001b[6F" (1 terminal)
        //   "\u001bO6F" (1 terminal)
        /// <summary>Get the value of extended capability "kEND6".</summary>
        /// <returns>If not null it is the value of the extended capability "kEND6". If null, this terminal information does not support the extended capability "kEND6".</returns>
        public string? KEND6 => _database.GetStringCapabilityValue("kEND6");

        // Capability name: kEND7
        // Terminals supporting this capability: 82 terminals
        // Values of this capability:
        //   "\u001b[1;7F" (75 terminals)
        //   "\u001b[8;7~" (3 terminals)
        //   "\u001b[>1;7F" (1 terminal)
        //   "\u001b[1;13F" (1 terminal)
        //   "\u001b[7F" (1 terminal)
        //   "\u001bO7F" (1 terminal)
        /// <summary>Get the value of extended capability "kEND7".</summary>
        /// <returns>If not null it is the value of the extended capability "kEND7". If null, this terminal information does not support the extended capability "kEND7".</returns>
        public string? KEND7 => _database.GetStringCapabilityValue("kEND7");

        // Capability name: kEND8
        // Terminals supporting this capability: 1 terminal
        // Values of this capability:
        //   "\u001b[1;14F" (1 terminal)
        /// <summary>Get the value of extended capability "kEND8".</summary>
        /// <returns>If not null it is the value of the extended capability "kEND8". If null, this terminal information does not support the extended capability "kEND8".</returns>
        public string? KEND8 => _database.GetStringCapabilityValue("kEND8");

        // Capability name: key_a1
        // Terminals supporting this capability: 158 terminals
        // Values of this capability:
        //   "\u001bOw" (101 terminals)
        //   "\u001bOq" (32 terminals)
        //   "\u001b[H" (15 terminals)
        //   "\u001b[1~" (6 terminals)
        //   "\u001b[7~" (3 terminals)
        //   "?w" (1 terminal)
        /// <summary>Get the value of capability "key_a1".</summary>
        /// <returns>If not null it is the value of the capability "key_a1". If null, this terminal information does not support the capability "key_a1".</returns>
        public string? KeyA1 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyA1);

        // Capability name: key_a3
        // Terminals supporting this capability: 158 terminals
        // Values of this capability:
        //   "\u001bOy" (101 terminals)
        //   "\u001bOs" (32 terminals)
        //   "\u001bOu" (15 terminals)
        //   "\u001b[5~" (9 terminals)
        //   "?u" (1 terminal)
        /// <summary>Get the value of capability "key_a3".</summary>
        /// <returns>If not null it is the value of the capability "key_a3". If null, this terminal information does not support the capability "key_a3".</returns>
        public string? KeyA3 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyA3);

        // Capability name: key_b2
        // Terminals supporting this capability: 219 terminals
        // Values of this capability:
        //   "\u001bOu" (109 terminals)
        //   "\u001bOr" (32 terminals)
        //   "\u001b[G" (23 terminals)
        //   "\u001bOE" (19 terminals)
        //   "\u001b[E" (18 terminals)
        //   "\u001b[V" (15 terminals)
        //   "\u001b[218z" (2 terminals)
        //   "?y" (1 terminal)
        /// <summary>Get the value of capability "key_b2".</summary>
        /// <returns>If not null it is the value of the capability "key_b2". If null, this terminal information does not support the capability "key_b2".</returns>
        public string? KeyB2 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyB2);

        // Capability name: key_backspace
        // Terminals supporting this capability: 317 terminals
        // Values of this capability:
        //   "\u007f" (190 terminals)
        //   "\b" (124 terminals)
        //   "\u001bOl" (2 terminals)
        //   "\u0013G" (1 terminal)
        /// <summary>Get the value of capability "key_backspace".</summary>
        /// <returns>If not null it is the value of the capability "key_backspace". If null, this terminal information does not support the capability "key_backspace".</returns>
        public string? KeyBackspace => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyBackspace);

        // Capability name: key_beg
        // Terminals supporting this capability: 9 terminals
        // Values of this capability:
        //   "\u001bOE" (3 terminals)
        //   "\u001bOu" (3 terminals)
        //   "\u001b[E" (2 terminals)
        //   "?E" (1 terminal)
        /// <summary>Get the value of capability "key_beg".</summary>
        /// <returns>If not null it is the value of the capability "key_beg". If null, this terminal information does not support the capability "key_beg".</returns>
        public string? KeyBeg => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyBeg);

        // Capability name: key_btab
        // Terminals supporting this capability: 203 terminals
        // Values of this capability:
        //   "\u001b[Z" (173 terminals)
        //   "\u001bI" (25 terminals)
        //   "\u001bO" (3 terminals)
        //   "?Z" (1 terminal)
        //   "\u001b\t" (1 terminal)
        /// <summary>Get the value of capability "key_btab".</summary>
        /// <returns>If not null it is the value of the capability "key_btab". If null, this terminal information does not support the capability "key_btab".</returns>
        public string? KeyBtab => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyBtab);

        // Capability name: key_c1
        // Terminals supporting this capability: 158 terminals
        // Values of this capability:
        //   "\u001bOq" (116 terminals)
        //   "\u001bOp" (32 terminals)
        //   "\u001b[4~" (6 terminals)
        //   "\u001b[8~" (3 terminals)
        //   "?q" (1 terminal)
        /// <summary>Get the value of capability "key_c1".</summary>
        /// <returns>If not null it is the value of the capability "key_c1". If null, this terminal information does not support the capability "key_c1".</returns>
        public string? KeyC1 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyC1);

        // Capability name: key_c3
        // Terminals supporting this capability: 158 terminals
        // Values of this capability:
        //   "\u001bOs" (101 terminals)
        //   "\u001bOn" (32 terminals)
        //   "\u001b[U" (15 terminals)
        //   "\u001b[6~" (9 terminals)
        //   "?s" (1 terminal)
        /// <summary>Get the value of capability "key_c3".</summary>
        /// <returns>If not null it is the value of the capability "key_c3". If null, this terminal information does not support the capability "key_c3".</returns>
        public string? KeyC3 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyC3);

        // Capability name: key_cancel
        // Terminals supporting this capability: 3 terminals
        // Values of this capability:
        //   "\u001bOQ" (2 terminals)
        //   "\u0013E" (1 terminal)
        /// <summary>Get the value of capability "key_cancel".</summary>
        /// <returns>If not null it is the value of the capability "key_cancel". If null, this terminal information does not support the capability "key_cancel".</returns>
        public string? KeyCancel => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyCancel);

        // Capability name: key_catab
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b3" (2 terminals)
        /// <summary>Get the value of capability "key_catab".</summary>
        /// <returns>If not null it is the value of the capability "key_catab". If null, this terminal information does not support the capability "key_catab".</returns>
        public string? KeyCatab => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyCatab);

        // Capability name: key_clear
        // Terminals supporting this capability: 20 terminals
        // Values of this capability:
        //   "\u001b*" (8 terminals)
        //   "\u001b[3;5~" (6 terminals)
        //   "\u001b[2J" (3 terminals)
        //   "\u001b\r" (2 terminals)
        //   "\u001bJ" (1 terminal)
        /// <summary>Get the value of capability "key_clear".</summary>
        /// <returns>If not null it is the value of the capability "key_clear". If null, this terminal information does not support the capability "key_clear".</returns>
        public string? KeyClear => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyClear);

        // Capability name: key_copy
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b[197z" (2 terminals)
        /// <summary>Get the value of capability "key_copy".</summary>
        /// <returns>If not null it is the value of the capability "key_copy". If null, this terminal information does not support the capability "key_copy".</returns>
        public string? KeyCopy => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyCopy);

        // Capability name: key_ctab
        // Terminals supporting this capability: 4 terminals
        // Values of this capability:
        //   "\t" (2 terminals)
        //   "\u001b2" (2 terminals)
        /// <summary>Get the value of capability "key_ctab".</summary>
        /// <returns>If not null it is the value of the capability "key_ctab". If null, this terminal information does not support the capability "key_ctab".</returns>
        public string? KeyCtab => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyCtab);

        // Capability name: key_dc
        // Terminals supporting this capability: 295 terminals
        // Values of this capability:
        //   "\u001b[3~" (220 terminals)
        //   "\u001bW" (25 terminals)
        //   "\u001b[4~" (19 terminals)
        //   "\u007f" (18 terminals)
        //   "\u001b-" (4 terminals)
        //   "\u001bE" (3 terminals)
        //   "\u001b[3z" (2 terminals)
        //   "\u001b[P" (2 terminals)
        //   "?3~" (1 terminal)
        //   "\u001bP" (1 terminal)
        /// <summary>Get the value of capability "key_dc".</summary>
        /// <returns>If not null it is the value of the capability "key_dc". If null, this terminal information does not support the capability "key_dc".</returns>
        public string? KeyDc => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyDc);

        // Capability name: key_dl
        // Terminals supporting this capability: 44 terminals
        // Values of this capability:
        //   "\u001bR" (25 terminals)
        //   "\u001b[M" (11 terminals)
        //   "\u001b[3;2~" (6 terminals)
        //   "\u001b\u001b[A" (2 terminals)
        /// <summary>Get the value of capability "key_dl".</summary>
        /// <returns>If not null it is the value of the capability "key_dl". If null, this terminal information does not support the capability "key_dl".</returns>
        public string? KeyDl => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyDl);

        // Capability name: key_down
        // Terminals supporting this capability: 319 terminals
        // Values of this capability:
        //   "\u001bOB" (183 terminals)
        //   "\u001b[B" (106 terminals)
        //   "\n" (20 terminals)
        //   "\u0016" (8 terminals)
        //   "?B" (1 terminal)
        //   "\u001bB" (1 terminal)
        /// <summary>Get the value of capability "key_down".</summary>
        /// <returns>If not null it is the value of the capability "key_down". If null, this terminal information does not support the capability "key_down".</returns>
        public string? KeyDown => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyDown);

        // Capability name: key_eic
        // Terminals supporting this capability: 11 terminals
        // Values of this capability:
        //   "\u001b[2;2~" (6 terminals)
        //   "\u001bF" (3 terminals)
        //   "\u001bQ" (2 terminals)
        /// <summary>Get the value of capability "key_eic".</summary>
        /// <returns>If not null it is the value of the capability "key_eic". If null, this terminal information does not support the capability "key_eic".</returns>
        public string? KeyEic => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyEic);

        // Capability name: key_end
        // Terminals supporting this capability: 216 terminals
        // Values of this capability:
        //   "\u001b[4~" (79 terminals)
        //   "\u001bOF" (76 terminals)
        //   "\u001b[8~" (22 terminals)
        //   "\u001b[1~" (11 terminals)
        //   "\u001b[5~" (8 terminals)
        //   "\u001b[F" (7 terminals)
        //   "\u001bk" (4 terminals)
        //   "\u001b)4\r" (2 terminals)
        //   "\u001b[220z" (2 terminals)
        //   "\u001b[OF" (2 terminals)
        //   "?4~" (1 terminal)
        //   "\u0013I" (1 terminal)
        //   "\u001bF" (1 terminal)
        /// <summary>Get the value of capability "key_end".</summary>
        /// <returns>If not null it is the value of the capability "key_end". If null, this terminal information does not support the capability "key_end".</returns>
        public string? KeyEnd => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyEnd);

        // Capability name: key_enter
        // Terminals supporting this capability: 210 terminals
        // Values of this capability:
        //   "\u001bOM" (191 terminals)
        //   "\u001b7" (17 terminals)
        //   "?M" (1 terminal)
        //   "\u0013A" (1 terminal)
        /// <summary>Get the value of capability "key_enter".</summary>
        /// <returns>If not null it is the value of the capability "key_enter". If null, this terminal information does not support the capability "key_enter".</returns>
        public string? KeyEnter => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyEnter);

        // Capability name: key_eol
        // Terminals supporting this capability: 97 terminals
        // Values of this capability:
        //   "\u001b[4~" (34 terminals)
        //   "\u001b[8^" (24 terminals)
        //   "\u001bT" (17 terminals)
        //   "\u001b[K" (8 terminals)
        //   "\u001bt" (8 terminals)
        //   "\u001b[1;2F" (6 terminals)
        /// <summary>Get the value of capability "key_eol".</summary>
        /// <returns>If not null it is the value of the capability "key_eol". If null, this terminal information does not support the capability "key_eol".</returns>
        public string? KeyEol => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyEol);

        // Capability name: key_eos
        // Terminals supporting this capability: 55 terminals
        // Values of this capability:
        //   "\u001b[1~" (24 terminals)
        //   "\u001bY" (17 terminals)
        //   "\u001by" (8 terminals)
        //   "\u001b[1;5F" (6 terminals)
        /// <summary>Get the value of capability "key_eos".</summary>
        /// <returns>If not null it is the value of the capability "key_eos". If null, this terminal information does not support the capability "key_eos".</returns>
        public string? KeyEos => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyEos);

        // Capability name: key_f0
        // Terminals supporting this capability: 46 terminals
        // Values of this capability:
        //   "\u001bOy" (26 terminals)
        //   "\u001b[21~" (18 terminals)
        //   "\u001b[Y" (2 terminals)
        /// <summary>Get the value of capability "key_f0".</summary>
        /// <returns>If not null it is the value of the capability "key_f0". If null, this terminal information does not support the capability "key_f0".</returns>
        public string? KeyF0 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF0);

        // Capability name: key_f1
        // Terminals supporting this capability: 318 terminals
        // Values of this capability:
        //   "\u001bOP" (201 terminals)
        //   "\u001b[11~" (49 terminals)
        //   "\u0001@\r" (25 terminals)
        //   "\u001b[[A" (15 terminals)
        //   "\u001b[?5i" (8 terminals)
        //   "\u001b1" (4 terminals)
        //   "\u00021\r" (3 terminals)
        //   "\u001b[17~" (3 terminals)
        //   "\u001b[224z" (2 terminals)
        //   "\u001b[M" (2 terminals)
        //   "\u001b[V" (2 terminals)
        //   "\u001bOq" (2 terminals)
        //   "?11~" (1 terminal)
        //   "\u001bp" (1 terminal)
        /// <summary>Get the value of capability "key_f1".</summary>
        /// <returns>If not null it is the value of the capability "key_f1". If null, this terminal information does not support the capability "key_f1".</returns>
        public string? KeyF1 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF1);

        // Capability name: key_f10
        // Terminals supporting this capability: 287 terminals
        // Values of this capability:
        //   "\u001b[21~" (246 terminals)
        //   "\u0001I\r" (25 terminals)
        //   "\u001b0" (4 terminals)
        //   "\u001b[28~" (3 terminals)
        //   "\u001b[233z" (2 terminals)
        //   "\u001b[V" (2 terminals)
        //   "\u001bOp" (2 terminals)
        //   "?21~" (1 terminal)
        //   "\u001b[29~" (1 terminal)
        //   "\u001bOY" (1 terminal)
        /// <summary>Get the value of capability "key_f10".</summary>
        /// <returns>If not null it is the value of the capability "key_f10". If null, this terminal information does not support the capability "key_f10".</returns>
        public string? KeyF10 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF10);

        // Capability name: key_f11
        // Terminals supporting this capability: 286 terminals
        // Values of this capability:
        //   "\u001b[23~" (246 terminals)
        //   "\u0001J\r" (25 terminals)
        //   "\u001b!" (4 terminals)
        //   "\u001b[29~" (3 terminals)
        //   "\u001b[192z" (2 terminals)
        //   "\u001b[W" (2 terminals)
        //   "\u001bOP1" (2 terminals)
        //   "?23~" (1 terminal)
        //   "\u001bOZ" (1 terminal)
        /// <summary>Get the value of capability "key_f11".</summary>
        /// <returns>If not null it is the value of the capability "key_f11". If null, this terminal information does not support the capability "key_f11".</returns>
        public string? KeyF11 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF11);

        // Capability name: key_f12
        // Terminals supporting this capability: 278 terminals
        // Values of this capability:
        //   "\u001b[24~" (246 terminals)
        //   "\u0001K\r" (17 terminals)
        //   "\u001b@" (4 terminals)
        //   "\u001b[31~" (3 terminals)
        //   "\u001b[193z" (2 terminals)
        //   "\u001b[X" (2 terminals)
        //   "\u001bOP2" (2 terminals)
        //   "?24~" (1 terminal)
        //   "\u001bO[" (1 terminal)
        /// <summary>Get the value of capability "key_f12".</summary>
        /// <returns>If not null it is the value of the capability "key_f12". If null, this terminal information does not support the capability "key_f12".</returns>
        public string? KeyF12 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF12);

        // Capability name: key_f13
        // Terminals supporting this capability: 259 terminals
        // Values of this capability:
        //   "\u001b[25~" (139 terminals)
        //   "\u001b[1;2P" (54 terminals)
        //   "\u001bO2P" (21 terminals)
        //   "\u0001L\r" (17 terminals)
        //   "\u001bO1;2P" (9 terminals)
        //   "\u001b[11;2~" (4 terminals)
        //   "\u001b\u0013\u001b1" (4 terminals)
        //   "\u001b[32~" (3 terminals)
        //   "\u001b[194z" (2 terminals)
        //   "\u001b[Y" (2 terminals)
        //   "\u001bOP3" (2 terminals)
        //   "?25~" (1 terminal)
        //   "\u0019{1" (1 terminal)
        /// <summary>Get the value of capability "key_f13".</summary>
        /// <returns>If not null it is the value of the capability "key_f13". If null, this terminal information does not support the capability "key_f13".</returns>
        public string? KeyF13 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF13);

        // Capability name: key_f14
        // Terminals supporting this capability: 259 terminals
        // Values of this capability:
        //   "\u001b[26~" (139 terminals)
        //   "\u001b[1;2Q" (54 terminals)
        //   "\u001bO2Q" (21 terminals)
        //   "\u0001M\r" (17 terminals)
        //   "\u001bO1;2Q" (9 terminals)
        //   "\u001b[12;2~" (4 terminals)
        //   "\u001b\u0013\u001b2" (4 terminals)
        //   "\u001b[33~" (3 terminals)
        //   "\u001b[195z" (2 terminals)
        //   "\u001b[Z" (2 terminals)
        //   "\u001bOP4" (2 terminals)
        //   "?26~" (1 terminal)
        //   "\u0019{2" (1 terminal)
        /// <summary>Get the value of capability "key_f14".</summary>
        /// <returns>If not null it is the value of the capability "key_f14". If null, this terminal information does not support the capability "key_f14".</returns>
        public string? KeyF14 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF14);

        // Capability name: key_f15
        // Terminals supporting this capability: 259 terminals
        // Values of this capability:
        //   "\u001b[28~" (139 terminals)
        //   "\u001b[1;2R" (54 terminals)
        //   "\u001bO2R" (21 terminals)
        //   "\u0001N\r" (17 terminals)
        //   "\u001bO1;2R" (9 terminals)
        //   "\u001b[13;2~" (4 terminals)
        //   "\u001b\u0013\u001b3" (4 terminals)
        //   "\u001b[34~" (3 terminals)
        //   "\u001b[196z" (2 terminals)
        //   "\u001b[a" (2 terminals)
        //   "\u001bOP5" (2 terminals)
        //   "?28~" (1 terminal)
        //   "\u0019{3" (1 terminal)
        /// <summary>Get the value of capability "key_f15".</summary>
        /// <returns>If not null it is the value of the capability "key_f15". If null, this terminal information does not support the capability "key_f15".</returns>
        public string? KeyF15 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF15);

        // Capability name: key_f16
        // Terminals supporting this capability: 254 terminals
        // Values of this capability:
        //   "\u001b[29~" (139 terminals)
        //   "\u001b[1;2S" (54 terminals)
        //   "\u001bO2S" (21 terminals)
        //   "\u0001O\r" (17 terminals)
        //   "\u001bO1;2S" (9 terminals)
        //   "\u001b[14;2~" (4 terminals)
        //   "\u001b\u0013\u001b4" (4 terminals)
        //   "\u001b[b" (2 terminals)
        //   "\u001bOP6" (2 terminals)
        //   "?29~" (1 terminal)
        //   "\u0019{4" (1 terminal)
        /// <summary>Get the value of capability "key_f16".</summary>
        /// <returns>If not null it is the value of the capability "key_f16". If null, this terminal information does not support the capability "key_f16".</returns>
        public string? KeyF16 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF16);

        // Capability name: key_f17
        // Terminals supporting this capability: 238 terminals
        // Values of this capability:
        //   "\u001b[31~" (138 terminals)
        //   "\u001b[15;2~" (88 terminals)
        //   "\u001b\u0013\u001b5" (4 terminals)
        //   "\u001b[198z" (2 terminals)
        //   "\u001b[c" (2 terminals)
        //   "\u001bOP7" (2 terminals)
        //   "?31~" (1 terminal)
        //   "\u0019{5" (1 terminal)
        /// <summary>Get the value of capability "key_f17".</summary>
        /// <returns>If not null it is the value of the capability "key_f17". If null, this terminal information does not support the capability "key_f17".</returns>
        public string? KeyF17 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF17);

        // Capability name: key_f18
        // Terminals supporting this capability: 238 terminals
        // Values of this capability:
        //   "\u001b[32~" (135 terminals)
        //   "\u001b[17;2~" (88 terminals)
        //   "\u001b\u0013\u001b6" (4 terminals)
        //   "\u001b[22~" (3 terminals)
        //   "\u001b[199z" (2 terminals)
        //   "\u001b[d" (2 terminals)
        //   "\u001bOP8" (2 terminals)
        //   "?32~" (1 terminal)
        //   "\u0019{6" (1 terminal)
        /// <summary>Get the value of capability "key_f18".</summary>
        /// <returns>If not null it is the value of the capability "key_f18". If null, this terminal information does not support the capability "key_f18".</returns>
        public string? KeyF18 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF18);

        // Capability name: key_f19
        // Terminals supporting this capability: 238 terminals
        // Values of this capability:
        //   "\u001b[33~" (138 terminals)
        //   "\u001b[18;2~" (88 terminals)
        //   "\u001b\u0013\u001b7" (4 terminals)
        //   "\u001b[200z" (2 terminals)
        //   "\u001b[e" (2 terminals)
        //   "\u001bOP9" (2 terminals)
        //   "?33~" (1 terminal)
        //   "\u0019{7" (1 terminal)
        /// <summary>Get the value of capability "key_f19".</summary>
        /// <returns>If not null it is the value of the capability "key_f19". If null, this terminal information does not support the capability "key_f19".</returns>
        public string? KeyF19 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF19);

        // Capability name: key_f2
        // Terminals supporting this capability: 318 terminals
        // Values of this capability:
        //   "\u001bOQ" (201 terminals)
        //   "\u001b[12~" (49 terminals)
        //   "\u0001A\r" (25 terminals)
        //   "\u001b[[B" (15 terminals)
        //   "\u001b[?3i" (8 terminals)
        //   "\u001b2" (4 terminals)
        //   "\u00022\r" (3 terminals)
        //   "\u001b[18" (3 terminals)
        //   "\u001b[225z" (2 terminals)
        //   "\u001b[N" (2 terminals)
        //   "\u001b[U" (2 terminals)
        //   "\u001bOr" (2 terminals)
        //   "?12~" (1 terminal)
        //   "\u001bq" (1 terminal)
        /// <summary>Get the value of capability "key_f2".</summary>
        /// <returns>If not null it is the value of the capability "key_f2". If null, this terminal information does not support the capability "key_f2".</returns>
        public string? KeyF2 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF2);

        // Capability name: key_f20
        // Terminals supporting this capability: 238 terminals
        // Values of this capability:
        //   "\u001b[34~" (138 terminals)
        //   "\u001b[19;2~" (88 terminals)
        //   "\u001b\u0013\u001b8" (4 terminals)
        //   "\u001b[201z" (2 terminals)
        //   "\u001b[f" (2 terminals)
        //   "\u001bOP0" (2 terminals)
        //   "?34~" (1 terminal)
        //   "\u0019{8" (1 terminal)
        /// <summary>Get the value of capability "key_f20".</summary>
        /// <returns>If not null it is the value of the capability "key_f20". If null, this terminal information does not support the capability "key_f20".</returns>
        public string? KeyF20 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF20);

        // Capability name: key_f21
        // Terminals supporting this capability: 143 terminals
        // Values of this capability:
        //   "\u001b[20;2~" (88 terminals)
        //   "\u001b[31~" (21 terminals)
        //   "\u001b[23$" (17 terminals)
        //   "\u001b[35~" (8 terminals)
        //   "\u001b\u0013\u001b9" (4 terminals)
        //   "\u001b[g" (2 terminals)
        //   "\u001bOP*" (2 terminals)
        //   "\u0019{9" (1 terminal)
        /// <summary>Get the value of capability "key_f21".</summary>
        /// <returns>If not null it is the value of the capability "key_f21". If null, this terminal information does not support the capability "key_f21".</returns>
        public string? KeyF21 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF21);

        // Capability name: key_f22
        // Terminals supporting this capability: 135 terminals
        // Values of this capability:
        //   "\u001b[21;2~" (88 terminals)
        //   "\u001b[32~" (21 terminals)
        //   "\u001b[24$" (17 terminals)
        //   "\u001b\u0013\u001b0" (4 terminals)
        //   "\u001b[h" (2 terminals)
        //   "\u001bOP#" (2 terminals)
        //   "\u0019{0" (1 terminal)
        /// <summary>Get the value of capability "key_f22".</summary>
        /// <returns>If not null it is the value of the capability "key_f22". If null, this terminal information does not support the capability "key_f22".</returns>
        public string? KeyF22 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF22);

        // Capability name: key_f23
        // Terminals supporting this capability: 132 terminals
        // Values of this capability:
        //   "\u001b[23;2~" (88 terminals)
        //   "\u001b[33~" (21 terminals)
        //   "\u001b[11^" (16 terminals)
        //   "\u001b\u0013\u001b!" (4 terminals)
        //   "\u001b[i" (2 terminals)
        //   "\u0019{*" (1 terminal)
        /// <summary>Get the value of capability "key_f23".</summary>
        /// <returns>If not null it is the value of the capability "key_f23". If null, this terminal information does not support the capability "key_f23".</returns>
        public string? KeyF23 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF23);

        // Capability name: key_f24
        // Terminals supporting this capability: 132 terminals
        // Values of this capability:
        //   "\u001b[24;2~" (88 terminals)
        //   "\u001b[34~" (21 terminals)
        //   "\u001b[12^" (16 terminals)
        //   "\u001b\u0013\u001b@" (4 terminals)
        //   "\u001b[j" (2 terminals)
        //   "\u0019{#" (1 terminal)
        /// <summary>Get the value of capability "key_f24".</summary>
        /// <returns>If not null it is the value of the capability "key_f24". If null, this terminal information does not support the capability "key_f24".</returns>
        public string? KeyF24 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF24);

        // Capability name: key_f25
        // Terminals supporting this capability: 130 terminals
        // Values of this capability:
        //   "\u001b[1;5P" (53 terminals)
        //   "\u001b[35~" (21 terminals)
        //   "\u001bO5P" (21 terminals)
        //   "\u001b[13^" (16 terminals)
        //   "\u001bO1;5P" (9 terminals)
        //   "\u001b[11;5~" (4 terminals)
        //   "\u001b\u0003\u001b1" (4 terminals)
        //   "\u001b[k" (2 terminals)
        /// <summary>Get the value of capability "key_f25".</summary>
        /// <returns>If not null it is the value of the capability "key_f25". If null, this terminal information does not support the capability "key_f25".</returns>
        public string? KeyF25 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF25);

        // Capability name: key_f26
        // Terminals supporting this capability: 130 terminals
        // Values of this capability:
        //   "\u001b[1;5Q" (53 terminals)
        //   "\u001b[1~" (21 terminals)
        //   "\u001bO5Q" (21 terminals)
        //   "\u001b[14^" (16 terminals)
        //   "\u001bO1;5Q" (9 terminals)
        //   "\u001b[12;5~" (4 terminals)
        //   "\u001b\u0003\u001b2" (4 terminals)
        //   "\u001b[l" (2 terminals)
        /// <summary>Get the value of capability "key_f26".</summary>
        /// <returns>If not null it is the value of the capability "key_f26". If null, this terminal information does not support the capability "key_f26".</returns>
        public string? KeyF26 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF26);

        // Capability name: key_f27
        // Terminals supporting this capability: 130 terminals
        // Values of this capability:
        //   "\u001b[1;5R" (53 terminals)
        //   "\u001b[2~" (21 terminals)
        //   "\u001bO5R" (21 terminals)
        //   "\u001b[15^" (16 terminals)
        //   "\u001bO1;5R" (9 terminals)
        //   "\u001b[13;5~" (4 terminals)
        //   "\u001b\u0003\u001b3" (4 terminals)
        //   "\u001b[m" (2 terminals)
        /// <summary>Get the value of capability "key_f27".</summary>
        /// <returns>If not null it is the value of the capability "key_f27". If null, this terminal information does not support the capability "key_f27".</returns>
        public string? KeyF27 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF27);

        // Capability name: key_f28
        // Terminals supporting this capability: 130 terminals
        // Values of this capability:
        //   "\u001b[1;5S" (53 terminals)
        //   "\u001b[3~" (21 terminals)
        //   "\u001bO5S" (21 terminals)
        //   "\u001b[17^" (16 terminals)
        //   "\u001bO1;5S" (9 terminals)
        //   "\u001b[14;5~" (4 terminals)
        //   "\u001b\u0003\u001b4" (4 terminals)
        //   "\u001b[n" (2 terminals)
        /// <summary>Get the value of capability "key_f28".</summary>
        /// <returns>If not null it is the value of the capability "key_f28". If null, this terminal information does not support the capability "key_f28".</returns>
        public string? KeyF28 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF28);

        // Capability name: key_f29
        // Terminals supporting this capability: 130 terminals
        // Values of this capability:
        //   "\u001b[15;5~" (87 terminals)
        //   "\u001b[4~" (21 terminals)
        //   "\u001b[18^" (16 terminals)
        //   "\u001b\u0003\u001b5" (4 terminals)
        //   "\u001b[o" (2 terminals)
        /// <summary>Get the value of capability "key_f29".</summary>
        /// <returns>If not null it is the value of the capability "key_f29". If null, this terminal information does not support the capability "key_f29".</returns>
        public string? KeyF29 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF29);

        // Capability name: key_f3
        // Terminals supporting this capability: 318 terminals
        // Values of this capability:
        //   "\u001bOR" (201 terminals)
        //   "\u001b[13~" (49 terminals)
        //   "\u0001B\r" (25 terminals)
        //   "\u001b[[C" (15 terminals)
        //   "\u001b[2i" (8 terminals)
        //   "\u001b3" (4 terminals)
        //   "\u00023\r" (3 terminals)
        //   "\u001b[19~" (3 terminals)
        //   "\u001b[226z" (2 terminals)
        //   "\u001b[O" (2 terminals)
        //   "\u001b[T" (2 terminals)
        //   "\u001bOs" (2 terminals)
        //   "?13~" (1 terminal)
        //   "\u001br" (1 terminal)
        /// <summary>Get the value of capability "key_f3".</summary>
        /// <returns>If not null it is the value of the capability "key_f3". If null, this terminal information does not support the capability "key_f3".</returns>
        public string? KeyF3 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF3);

        // Capability name: key_f30
        // Terminals supporting this capability: 130 terminals
        // Values of this capability:
        //   "\u001b[17;5~" (87 terminals)
        //   "\u001b[5~" (21 terminals)
        //   "\u001b[19^" (16 terminals)
        //   "\u001b\u0003\u001b6" (4 terminals)
        //   "\u001b[p" (2 terminals)
        /// <summary>Get the value of capability "key_f30".</summary>
        /// <returns>If not null it is the value of the capability "key_f30". If null, this terminal information does not support the capability "key_f30".</returns>
        public string? KeyF30 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF30);

        // Capability name: key_f31
        // Terminals supporting this capability: 132 terminals
        // Values of this capability:
        //   "\u001b[18;5~" (87 terminals)
        //   "\u001b[6~" (21 terminals)
        //   "\u001b[20^" (16 terminals)
        //   "\u001b\u0003\u001b7" (4 terminals)
        //   "\u001b[208z" (2 terminals)
        //   "\u001b[q" (2 terminals)
        /// <summary>Get the value of capability "key_f31".</summary>
        /// <returns>If not null it is the value of the capability "key_f31". If null, this terminal information does not support the capability "key_f31".</returns>
        public string? KeyF31 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF31);

        // Capability name: key_f32
        // Terminals supporting this capability: 132 terminals
        // Values of this capability:
        //   "\u001b[19;5~" (87 terminals)
        //   "\u001b[7~" (21 terminals)
        //   "\u001b[21^" (16 terminals)
        //   "\u001b\u0003\u001b8" (4 terminals)
        //   "\u001b[209z" (2 terminals)
        //   "\u001b[r" (2 terminals)
        /// <summary>Get the value of capability "key_f32".</summary>
        /// <returns>If not null it is the value of the capability "key_f32". If null, this terminal information does not support the capability "key_f32".</returns>
        public string? KeyF32 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF32);

        // Capability name: key_f33
        // Terminals supporting this capability: 132 terminals
        // Values of this capability:
        //   "\u001b[20;5~" (87 terminals)
        //   "\u001b[8~" (21 terminals)
        //   "\u001b[23^" (16 terminals)
        //   "\u001b\u0003\u001b9" (4 terminals)
        //   "\u001b[210z" (2 terminals)
        //   "\u001b[s" (2 terminals)
        /// <summary>Get the value of capability "key_f33".</summary>
        /// <returns>If not null it is the value of the capability "key_f33". If null, this terminal information does not support the capability "key_f33".</returns>
        public string? KeyF33 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF33);

        // Capability name: key_f34
        // Terminals supporting this capability: 132 terminals
        // Values of this capability:
        //   "\u001b[21;5~" (87 terminals)
        //   "\u001b[9~" (21 terminals)
        //   "\u001b[24^" (16 terminals)
        //   "\u001b\u0003\u001b0" (4 terminals)
        //   "\u001b[211z" (2 terminals)
        //   "\u001b[t" (2 terminals)
        /// <summary>Get the value of capability "key_f34".</summary>
        /// <returns>If not null it is the value of the capability "key_f34". If null, this terminal information does not support the capability "key_f34".</returns>
        public string? KeyF34 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF34);

        // Capability name: key_f35
        // Terminals supporting this capability: 132 terminals
        // Values of this capability:
        //   "\u001b[23;5~" (87 terminals)
        //   "\u001b[10~" (21 terminals)
        //   "\u001b[25^" (16 terminals)
        //   "\u001b\u0003\u001b!" (4 terminals)
        //   "\u001b[212z" (2 terminals)
        //   "\u001b[u" (2 terminals)
        /// <summary>Get the value of capability "key_f35".</summary>
        /// <returns>If not null it is the value of the capability "key_f35". If null, this terminal information does not support the capability "key_f35".</returns>
        public string? KeyF35 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF35);

        // Capability name: key_f36
        // Terminals supporting this capability: 111 terminals
        // Values of this capability:
        //   "\u001b[24;5~" (87 terminals)
        //   "\u001b[26^" (16 terminals)
        //   "\u001b\u0003\u001b@" (4 terminals)
        //   "\u001b[213z" (2 terminals)
        //   "\u001b[v" (2 terminals)
        /// <summary>Get the value of capability "key_f36".</summary>
        /// <returns>If not null it is the value of the capability "key_f36". If null, this terminal information does not support the capability "key_f36".</returns>
        public string? KeyF36 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF36);

        // Capability name: key_f37
        // Terminals supporting this capability: 106 terminals
        // Values of this capability:
        //   "\u001b[1;6P" (50 terminals)
        //   "\u001bO6P" (21 terminals)
        //   "\u001b[28^" (16 terminals)
        //   "\u001bO1;6P" (9 terminals)
        //   "\u001b[11;6~" (4 terminals)
        //   "\u001b\u0001\u001b1" (4 terminals)
        //   "\u001b[w" (2 terminals)
        /// <summary>Get the value of capability "key_f37".</summary>
        /// <returns>If not null it is the value of the capability "key_f37". If null, this terminal information does not support the capability "key_f37".</returns>
        public string? KeyF37 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF37);

        // Capability name: key_f38
        // Terminals supporting this capability: 108 terminals
        // Values of this capability:
        //   "\u001b[1;6Q" (50 terminals)
        //   "\u001bO6Q" (21 terminals)
        //   "\u001b[29^" (16 terminals)
        //   "\u001bO1;6Q" (9 terminals)
        //   "\u001b[12;6~" (4 terminals)
        //   "\u001b\u0001\u001b2" (4 terminals)
        //   "\u001b[215z" (2 terminals)
        //   "\u001b[x" (2 terminals)
        /// <summary>Get the value of capability "key_f38".</summary>
        /// <returns>If not null it is the value of the capability "key_f38". If null, this terminal information does not support the capability "key_f38".</returns>
        public string? KeyF38 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF38);

        // Capability name: key_f39
        // Terminals supporting this capability: 106 terminals
        // Values of this capability:
        //   "\u001b[1;6R" (50 terminals)
        //   "\u001bO6R" (21 terminals)
        //   "\u001b[31^" (16 terminals)
        //   "\u001bO1;6R" (9 terminals)
        //   "\u001b[13;6~" (4 terminals)
        //   "\u001b\u0001\u001b3" (4 terminals)
        //   "\u001b[y" (2 terminals)
        /// <summary>Get the value of capability "key_f39".</summary>
        /// <returns>If not null it is the value of the capability "key_f39". If null, this terminal information does not support the capability "key_f39".</returns>
        public string? KeyF39 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF39);

        // Capability name: key_f4
        // Terminals supporting this capability: 318 terminals
        // Values of this capability:
        //   "\u001bOS" (201 terminals)
        //   "\u001b[14~" (49 terminals)
        //   "\u0001C\r" (25 terminals)
        //   "\u001b[[D" (15 terminals)
        //   "\u001b[@" (8 terminals)
        //   "\u001b4" (4 terminals)
        //   "\u00024\r" (3 terminals)
        //   "\u001b[20~" (3 terminals)
        //   "\u001b[227z" (2 terminals)
        //   "\u001b[P" (2 terminals)
        //   "\u001b[S" (2 terminals)
        //   "\u001bOt" (2 terminals)
        //   "?14~" (1 terminal)
        //   "\u001bs" (1 terminal)
        /// <summary>Get the value of capability "key_f4".</summary>
        /// <returns>If not null it is the value of the capability "key_f4". If null, this terminal information does not support the capability "key_f4".</returns>
        public string? KeyF4 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF4);

        // Capability name: key_f40
        // Terminals supporting this capability: 108 terminals
        // Values of this capability:
        //   "\u001b[1;6S" (50 terminals)
        //   "\u001bO6S" (21 terminals)
        //   "\u001b[32^" (16 terminals)
        //   "\u001bO1;6S" (9 terminals)
        //   "\u001b[14;6~" (4 terminals)
        //   "\u001b\u0001\u001b4" (4 terminals)
        //   "\u001b[217z" (2 terminals)
        //   "\u001b[z" (2 terminals)
        /// <summary>Get the value of capability "key_f40".</summary>
        /// <returns>If not null it is the value of the capability "key_f40". If null, this terminal information does not support the capability "key_f40".</returns>
        public string? KeyF40 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF40);

        // Capability name: key_f41
        // Terminals supporting this capability: 106 terminals
        // Values of this capability:
        //   "\u001b[15;6~" (84 terminals)
        //   "\u001b[33^" (16 terminals)
        //   "\u001b\u0001\u001b5" (4 terminals)
        //   "\u001b[@" (2 terminals)
        /// <summary>Get the value of capability "key_f41".</summary>
        /// <returns>If not null it is the value of the capability "key_f41". If null, this terminal information does not support the capability "key_f41".</returns>
        public string? KeyF41 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF41);

        // Capability name: key_f42
        // Terminals supporting this capability: 108 terminals
        // Values of this capability:
        //   "\u001b[17;6~" (84 terminals)
        //   "\u001b[34^" (16 terminals)
        //   "\u001b\u0001\u001b6" (4 terminals)
        //   "\u001b[[" (2 terminals)
        //   "\u001b[219z" (2 terminals)
        /// <summary>Get the value of capability "key_f42".</summary>
        /// <returns>If not null it is the value of the capability "key_f42". If null, this terminal information does not support the capability "key_f42".</returns>
        public string? KeyF42 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF42);

        // Capability name: key_f43
        // Terminals supporting this capability: 106 terminals
        // Values of this capability:
        //   "\u001b[18;6~" (84 terminals)
        //   "\u001b[23@" (16 terminals)
        //   "\u001b\u0001\u001b7" (4 terminals)
        //   "\u001b[\\" (2 terminals)
        /// <summary>Get the value of capability "key_f43".</summary>
        /// <returns>If not null it is the value of the capability "key_f43". If null, this terminal information does not support the capability "key_f43".</returns>
        public string? KeyF43 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF43);

        // Capability name: key_f44
        // Terminals supporting this capability: 108 terminals
        // Values of this capability:
        //   "\u001b[19;6~" (84 terminals)
        //   "\u001b[24@" (16 terminals)
        //   "\u001b\u0001\u001b8" (4 terminals)
        //   "\u001b[]" (2 terminals)
        //   "\u001b[221z" (2 terminals)
        /// <summary>Get the value of capability "key_f44".</summary>
        /// <returns>If not null it is the value of the capability "key_f44". If null, this terminal information does not support the capability "key_f44".</returns>
        public string? KeyF44 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF44);

        // Capability name: key_f45
        // Terminals supporting this capability: 92 terminals
        // Values of this capability:
        //   "\u001b[20;6~" (84 terminals)
        //   "\u001b\u0001\u001b9" (4 terminals)
        //   "\u001b[^" (2 terminals)
        //   "\u001b[222z" (2 terminals)
        /// <summary>Get the value of capability "key_f45".</summary>
        /// <returns>If not null it is the value of the capability "key_f45". If null, this terminal information does not support the capability "key_f45".</returns>
        public string? KeyF45 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF45);

        // Capability name: key_f46
        // Terminals supporting this capability: 92 terminals
        // Values of this capability:
        //   "\u001b[21;6~" (84 terminals)
        //   "\u001b\u0001\u001b0" (4 terminals)
        //   "\u001b[_" (2 terminals)
        //   "\u001b[234z" (2 terminals)
        /// <summary>Get the value of capability "key_f46".</summary>
        /// <returns>If not null it is the value of the capability "key_f46". If null, this terminal information does not support the capability "key_f46".</returns>
        public string? KeyF46 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF46);

        // Capability name: key_f47
        // Terminals supporting this capability: 92 terminals
        // Values of this capability:
        //   "\u001b[23;6~" (84 terminals)
        //   "\u001b\u0001\u001b!" (4 terminals)
        //   "\u001b[`" (2 terminals)
        //   "\u001b[235z" (2 terminals)
        /// <summary>Get the value of capability "key_f47".</summary>
        /// <returns>If not null it is the value of the capability "key_f47". If null, this terminal information does not support the capability "key_f47".</returns>
        public string? KeyF47 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF47);

        // Capability name: key_f48
        // Terminals supporting this capability: 90 terminals
        // Values of this capability:
        //   "\u001b[24;6~" (84 terminals)
        //   "\u001b\u0001\u001b@" (4 terminals)
        //   "\u001b[{" (2 terminals)
        /// <summary>Get the value of capability "key_f48".</summary>
        /// <returns>If not null it is the value of the capability "key_f48". If null, this terminal information does not support the capability "key_f48".</returns>
        public string? KeyF48 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF48);

        // Capability name: key_f49
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[1;3P" (50 terminals)
        //   "\u001bO3P" (18 terminals)
        //   "\u001bO1;3P" (9 terminals)
        //   "\u001b[11;3~" (4 terminals)
        /// <summary>Get the value of capability "key_f49".</summary>
        /// <returns>If not null it is the value of the capability "key_f49". If null, this terminal information does not support the capability "key_f49".</returns>
        public string? KeyF49 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF49);

        // Capability name: key_f5
        // Terminals supporting this capability: 262 terminals
        // Values of this capability:
        //   "\u001b[15~" (170 terminals)
        //   "\u001b[M" (29 terminals)
        //   "\u0001D\r" (25 terminals)
        //   "\u001b[[E" (15 terminals)
        //   "\u001b[21~" (4 terminals)
        //   "\u001b5" (4 terminals)
        //   "\u00025\r" (3 terminals)
        //   "\u001b[228z" (2 terminals)
        //   "\u001b[G" (2 terminals)
        //   "\u001b[Q" (2 terminals)
        //   "\u001bOu" (2 terminals)
        //   "?15~" (1 terminal)
        //   "\u001b[17~" (1 terminal)
        //   "\u001bOT" (1 terminal)
        //   "\u001bt" (1 terminal)
        /// <summary>Get the value of capability "key_f5".</summary>
        /// <returns>If not null it is the value of the capability "key_f5". If null, this terminal information does not support the capability "key_f5".</returns>
        public string? KeyF5 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF5);

        // Capability name: key_f50
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[1;3Q" (50 terminals)
        //   "\u001bO3Q" (18 terminals)
        //   "\u001bO1;3Q" (9 terminals)
        //   "\u001b[12;3~" (4 terminals)
        /// <summary>Get the value of capability "key_f50".</summary>
        /// <returns>If not null it is the value of the capability "key_f50". If null, this terminal information does not support the capability "key_f50".</returns>
        public string? KeyF50 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF50);

        // Capability name: key_f51
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[1;3R" (50 terminals)
        //   "\u001bO3R" (18 terminals)
        //   "\u001bO1;3R" (9 terminals)
        //   "\u001b[13;3~" (4 terminals)
        /// <summary>Get the value of capability "key_f51".</summary>
        /// <returns>If not null it is the value of the capability "key_f51". If null, this terminal information does not support the capability "key_f51".</returns>
        public string? KeyF51 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF51);

        // Capability name: key_f52
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[1;3S" (50 terminals)
        //   "\u001bO3S" (18 terminals)
        //   "\u001bO1;3S" (9 terminals)
        //   "\u001b[14;3~" (4 terminals)
        /// <summary>Get the value of capability "key_f52".</summary>
        /// <returns>If not null it is the value of the capability "key_f52". If null, this terminal information does not support the capability "key_f52".</returns>
        public string? KeyF52 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF52);

        // Capability name: key_f53
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[15;3~" (81 terminals)
        /// <summary>Get the value of capability "key_f53".</summary>
        /// <returns>If not null it is the value of the capability "key_f53". If null, this terminal information does not support the capability "key_f53".</returns>
        public string? KeyF53 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF53);

        // Capability name: key_f54
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[17;3~" (81 terminals)
        /// <summary>Get the value of capability "key_f54".</summary>
        /// <returns>If not null it is the value of the capability "key_f54". If null, this terminal information does not support the capability "key_f54".</returns>
        public string? KeyF54 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF54);

        // Capability name: key_f55
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[18;3~" (81 terminals)
        /// <summary>Get the value of capability "key_f55".</summary>
        /// <returns>If not null it is the value of the capability "key_f55". If null, this terminal information does not support the capability "key_f55".</returns>
        public string? KeyF55 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF55);

        // Capability name: key_f56
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[19;3~" (81 terminals)
        /// <summary>Get the value of capability "key_f56".</summary>
        /// <returns>If not null it is the value of the capability "key_f56". If null, this terminal information does not support the capability "key_f56".</returns>
        public string? KeyF56 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF56);

        // Capability name: key_f57
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[20;3~" (81 terminals)
        /// <summary>Get the value of capability "key_f57".</summary>
        /// <returns>If not null it is the value of the capability "key_f57". If null, this terminal information does not support the capability "key_f57".</returns>
        public string? KeyF57 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF57);

        // Capability name: key_f58
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[21;3~" (81 terminals)
        /// <summary>Get the value of capability "key_f58".</summary>
        /// <returns>If not null it is the value of the capability "key_f58". If null, this terminal information does not support the capability "key_f58".</returns>
        public string? KeyF58 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF58);

        // Capability name: key_f59
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[23;3~" (81 terminals)
        /// <summary>Get the value of capability "key_f59".</summary>
        /// <returns>If not null it is the value of the capability "key_f59". If null, this terminal information does not support the capability "key_f59".</returns>
        public string? KeyF59 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF59);

        // Capability name: key_f6
        // Terminals supporting this capability: 291 terminals
        // Values of this capability:
        //   "\u001b[17~" (246 terminals)
        //   "\u0001E\r" (25 terminals)
        //   "\u001b6" (4 terminals)
        //   "\u00026\r" (3 terminals)
        //   "\u001b[23~" (3 terminals)
        //   "\u001b[229z" (2 terminals)
        //   "\u001b[R" (2 terminals)
        //   "\u001bOv" (2 terminals)
        //   "?17~" (1 terminal)
        //   "\u001b[18~" (1 terminal)
        //   "\u001bOU" (1 terminal)
        //   "\u001bu" (1 terminal)
        /// <summary>Get the value of capability "key_f6".</summary>
        /// <returns>If not null it is the value of the capability "key_f6". If null, this terminal information does not support the capability "key_f6".</returns>
        public string? KeyF6 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF6);

        // Capability name: key_f60
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[24;3~" (81 terminals)
        /// <summary>Get the value of capability "key_f60".</summary>
        /// <returns>If not null it is the value of the capability "key_f60". If null, this terminal information does not support the capability "key_f60".</returns>
        public string? KeyF60 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF60);

        // Capability name: key_f61
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[1;4P" (50 terminals)
        //   "\u001bO4P" (18 terminals)
        //   "\u001bO1;4P" (9 terminals)
        //   "\u001b[11;4~" (4 terminals)
        /// <summary>Get the value of capability "key_f61".</summary>
        /// <returns>If not null it is the value of the capability "key_f61". If null, this terminal information does not support the capability "key_f61".</returns>
        public string? KeyF61 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF61);

        // Capability name: key_f62
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[1;4Q" (50 terminals)
        //   "\u001bO4Q" (18 terminals)
        //   "\u001bO1;4Q" (9 terminals)
        //   "\u001b[12;4~" (4 terminals)
        /// <summary>Get the value of capability "key_f62".</summary>
        /// <returns>If not null it is the value of the capability "key_f62". If null, this terminal information does not support the capability "key_f62".</returns>
        public string? KeyF62 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF62);

        // Capability name: key_f63
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[1;4R" (50 terminals)
        //   "\u001bO4R" (18 terminals)
        //   "\u001bO1;4R" (9 terminals)
        //   "\u001b[13;4~" (4 terminals)
        /// <summary>Get the value of capability "key_f63".</summary>
        /// <returns>If not null it is the value of the capability "key_f63". If null, this terminal information does not support the capability "key_f63".</returns>
        public string? KeyF63 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF63);

        // Capability name: key_f7
        // Terminals supporting this capability: 291 terminals
        // Values of this capability:
        //   "\u001b[18~" (246 terminals)
        //   "\u0001F\r" (25 terminals)
        //   "\u001b7" (4 terminals)
        //   "\u00027\r" (3 terminals)
        //   "\u001b[24~" (3 terminals)
        //   "\u001b[230z" (2 terminals)
        //   "\u001b[S" (2 terminals)
        //   "\u001bOw" (2 terminals)
        //   "?18~" (1 terminal)
        //   "\u001b[19~" (1 terminal)
        //   "\u001bOV" (1 terminal)
        //   "\u001bv" (1 terminal)
        /// <summary>Get the value of capability "key_f7".</summary>
        /// <returns>If not null it is the value of the capability "key_f7". If null, this terminal information does not support the capability "key_f7".</returns>
        public string? KeyF7 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF7);

        // Capability name: key_f8
        // Terminals supporting this capability: 291 terminals
        // Values of this capability:
        //   "\u001b[19~" (246 terminals)
        //   "\u0001G\r" (25 terminals)
        //   "\u001b8" (4 terminals)
        //   "\u00028\r" (3 terminals)
        //   "\u001b[25~" (3 terminals)
        //   "\u001b[231z" (2 terminals)
        //   "\u001b[T" (2 terminals)
        //   "\u001bOx" (2 terminals)
        //   "?19~" (1 terminal)
        //   "\u001b[20~" (1 terminal)
        //   "\u001bOW" (1 terminal)
        //   "\u001bw" (1 terminal)
        /// <summary>Get the value of capability "key_f8".</summary>
        /// <returns>If not null it is the value of the capability "key_f8". If null, this terminal information does not support the capability "key_f8".</returns>
        public string? KeyF8 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF8);

        // Capability name: key_f9
        // Terminals supporting this capability: 287 terminals
        // Values of this capability:
        //   "\u001b[20~" (246 terminals)
        //   "\u0001H\r" (25 terminals)
        //   "\u001b9" (4 terminals)
        //   "\u001b[26~" (3 terminals)
        //   "\u001b[232z" (2 terminals)
        //   "\u001b[U" (2 terminals)
        //   "\u001bOy" (2 terminals)
        //   "?20~" (1 terminal)
        //   "\u001b[21~" (1 terminal)
        //   "\u001bOX" (1 terminal)
        /// <summary>Get the value of capability "key_f9".</summary>
        /// <returns>If not null it is the value of the capability "key_f9". If null, this terminal information does not support the capability "key_f9".</returns>
        public string? KeyF9 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyF9);

        // Capability name: key_find
        // Terminals supporting this capability: 121 terminals
        // Values of this capability:
        //   "\u001b[1~" (119 terminals)
        //   "\u001b[200z" (2 terminals)
        /// <summary>Get the value of capability "key_find".</summary>
        /// <returns>If not null it is the value of the capability "key_find". If null, this terminal information does not support the capability "key_find".</returns>
        public string? KeyFind => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyFind);

        // Capability name: key_help
        // Terminals supporting this capability: 82 terminals
        // Values of this capability:
        //   "\u001b[28~" (73 terminals)
        //   "\u001b[1~" (4 terminals)
        //   "\u001b[196z" (2 terminals)
        //   "\u001bOm" (2 terminals)
        //   "\u0013D" (1 terminal)
        /// <summary>Get the value of capability "key_help".</summary>
        /// <returns>If not null it is the value of the capability "key_help". If null, this terminal information does not support the capability "key_help".</returns>
        public string? KeyHelp => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyHelp);

        // Capability name: key_home
        // Terminals supporting this capability: 289 terminals
        // Values of this capability:
        //   "\u001bOH" (80 terminals)
        //   "\u001b[1~" (79 terminals)
        //   "\u001b[H" (41 terminals)
        //   "\u001e" (25 terminals)
        //   "\u001b[7~" (22 terminals)
        //   "\u001b[26~" (21 terminals)
        //   "\u001b[2~" (8 terminals)
        //   "\u001bh" (5 terminals)
        //   "\u0001" (3 terminals)
        //   "\u001b[214z" (2 terminals)
        //   "\u001b[OH" (2 terminals)
        //   "?1~" (1 terminal)
        /// <summary>Get the value of capability "key_home".</summary>
        /// <returns>If not null it is the value of the capability "key_home". If null, this terminal information does not support the capability "key_home".</returns>
        public string? KeyHome => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyHome);

        // Capability name: key_ic
        // Terminals supporting this capability: 288 terminals
        // Values of this capability:
        //   "\u001b[2~" (234 terminals)
        //   "\u001bQ" (26 terminals)
        //   "\u001b[@" (8 terminals)
        //   "\u001b[1~" (4 terminals)
        //   "\u001b[L" (4 terminals)
        //   "\u001b+" (4 terminals)
        //   "\u001bF" (3 terminals)
        //   "\u001b[2z" (2 terminals)
        //   "\u001b[4h" (2 terminals)
        //   "?2~" (1 terminal)
        /// <summary>Get the value of capability "key_ic".</summary>
        /// <returns>If not null it is the value of the capability "key_ic". If null, this terminal information does not support the capability "key_ic".</returns>
        public string? KeyIc => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyIc);

        // Capability name: key_il
        // Terminals supporting this capability: 44 terminals
        // Values of this capability:
        //   "\u001bE" (25 terminals)
        //   "\u001b[L" (11 terminals)
        //   "\u001b[2;5~" (6 terminals)
        //   "\u001b\u001b[B" (2 terminals)
        /// <summary>Get the value of capability "key_il".</summary>
        /// <returns>If not null it is the value of the capability "key_il". If null, this terminal information does not support the capability "key_il".</returns>
        public string? KeyIl => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyIl);

        // Capability name: key_left
        // Terminals supporting this capability: 319 terminals
        // Values of this capability:
        //   "\u001bOD" (183 terminals)
        //   "\u001b[D" (106 terminals)
        //   "\b" (25 terminals)
        //   "\u0015" (3 terminals)
        //   "?D" (1 terminal)
        //   "\u001bD" (1 terminal)
        /// <summary>Get the value of capability "key_left".</summary>
        /// <returns>If not null it is the value of the capability "key_left". If null, this terminal information does not support the capability "key_left".</returns>
        public string? KeyLeft => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyLeft);

        // Capability name: key_ll
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b[4~" (2 terminals)
        /// <summary>Get the value of capability "key_ll".</summary>
        /// <returns>If not null it is the value of the capability "key_ll". If null, this terminal information does not support the capability "key_ll".</returns>
        public string? KeyLl => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyLl);

        // Capability name: key_mouse
        // Terminals supporting this capability: 174 terminals
        // Values of this capability:
        //   "\u001b[M" (134 terminals)
        //   "\u001b[<" (38 terminals)
        //   "?M" (1 terminal)
        //   "\u001b[>M" (1 terminal)
        /// <summary>Get the value of capability "key_mouse".</summary>
        /// <returns>If not null it is the value of the capability "key_mouse". If null, this terminal information does not support the capability "key_mouse".</returns>
        public string? KeyMouse => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyMouse);

        // Capability name: key_next
        // Terminals supporting this capability: 10 terminals
        // Values of this capability:
        //   "\t" (10 terminals)
        /// <summary>Get the value of capability "key_next".</summary>
        /// <returns>If not null it is the value of the capability "key_next". If null, this terminal information does not support the capability "key_next".</returns>
        public string? KeyNext => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyNext);

        // Capability name: key_npage
        // Terminals supporting this capability: 295 terminals
        // Values of this capability:
        //   "\u001b[6~" (263 terminals)
        //   "\u001bK" (19 terminals)
        //   "\u001b/" (4 terminals)
        //   "\u001b[222z" (2 terminals)
        //   "\u001b[G" (2 terminals)
        //   "\u001bOn" (2 terminals)
        //   "?6~" (1 terminal)
        //   "\u0013H" (1 terminal)
        //   "\u001bS" (1 terminal)
        /// <summary>Get the value of capability "key_npage".</summary>
        /// <returns>If not null it is the value of the capability "key_npage". If null, this terminal information does not support the capability "key_npage".</returns>
        public string? KeyNpage => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyNpage);

        // Capability name: key_ppage
        // Terminals supporting this capability: 295 terminals
        // Values of this capability:
        //   "\u001b[5~" (255 terminals)
        //   "\u001bJ" (19 terminals)
        //   "\u001b[3~" (8 terminals)
        //   "\u001b?" (4 terminals)
        //   "\u001b[216z" (2 terminals)
        //   "\u001b[I" (2 terminals)
        //   "\u001bOR" (2 terminals)
        //   "?5~" (1 terminal)
        //   "\u0013B" (1 terminal)
        //   "\u001bT" (1 terminal)
        /// <summary>Get the value of capability "key_ppage".</summary>
        /// <returns>If not null it is the value of the capability "key_ppage". If null, this terminal information does not support the capability "key_ppage".</returns>
        public string? KeyPpage => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyPpage);

        // Capability name: key_previous
        // Terminals supporting this capability: 10 terminals
        // Values of this capability:
        //   "\u001b[Z" (10 terminals)
        /// <summary>Get the value of capability "key_previous".</summary>
        /// <returns>If not null it is the value of the capability "key_previous". If null, this terminal information does not support the capability "key_previous".</returns>
        public string? KeyPrevious => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyPrevious);

        // Capability name: key_print
        // Terminals supporting this capability: 25 terminals
        // Values of this capability:
        //   "\u001bP" (17 terminals)
        //   "\u001b[?5i" (8 terminals)
        /// <summary>Get the value of capability "key_print".</summary>
        /// <returns>If not null it is the value of the capability "key_print". If null, this terminal information does not support the capability "key_print".</returns>
        public string? KeyPrint => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyPrint);

        // Capability name: key_redo
        // Terminals supporting this capability: 29 terminals
        // Values of this capability:
        //   "\u001b[29~" (29 terminals)
        /// <summary>Get the value of capability "key_redo".</summary>
        /// <returns>If not null it is the value of the capability "key_redo". If null, this terminal information does not support the capability "key_redo".</returns>
        public string? KeyRedo => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyRedo);

        // Capability name: key_refresh
        // Terminals supporting this capability: 3 terminals
        // Values of this capability:
        //   "\u001bOS" (2 terminals)
        //   "\u0013C" (1 terminal)
        /// <summary>Get the value of capability "key_refresh".</summary>
        /// <returns>If not null it is the value of the capability "key_refresh". If null, this terminal information does not support the capability "key_refresh".</returns>
        public string? KeyRefresh => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyRefresh);

        // Capability name: key_replace
        // Terminals supporting this capability: 17 terminals
        // Values of this capability:
        //   "\u001br" (17 terminals)
        /// <summary>Get the value of capability "key_replace".</summary>
        /// <returns>If not null it is the value of the capability "key_replace". If null, this terminal information does not support the capability "key_replace".</returns>
        public string? KeyReplace => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyReplace);

        // Capability name: key_right
        // Terminals supporting this capability: 319 terminals
        // Values of this capability:
        //   "\u001bOC" (183 terminals)
        //   "\u001b[C" (106 terminals)
        //   "\f" (25 terminals)
        //   "\u0006" (3 terminals)
        //   "?C" (1 terminal)
        //   "\u001bC" (1 terminal)
        /// <summary>Get the value of capability "key_right".</summary>
        /// <returns>If not null it is the value of the capability "key_right". If null, this terminal information does not support the capability "key_right".</returns>
        public string? KeyRight => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyRight);

        // Capability name: key_sdc
        // Terminals supporting this capability: 119 terminals
        // Values of this capability:
        //   "\u001b[3;2~" (97 terminals)
        //   "\u001b[3$" (21 terminals)
        //   "\u001b[>3;2~" (1 terminal)
        /// <summary>Get the value of capability "key_sdc".</summary>
        /// <returns>If not null it is the value of the capability "key_sdc". If null, this terminal information does not support the capability "key_sdc".</returns>
        public string? KeySdc => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeySdc);

        // Capability name: key_select
        // Terminals supporting this capability: 129 terminals
        // Values of this capability:
        //   "\u001b[4~" (129 terminals)
        /// <summary>Get the value of capability "key_select".</summary>
        /// <returns>If not null it is the value of the capability "key_select". If null, this terminal information does not support the capability "key_select".</returns>
        public string? KeySelect => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeySelect);

        // Capability name: key_send
        // Terminals supporting this capability: 112 terminals
        // Values of this capability:
        //   "\u001b[1;2F" (85 terminals)
        //   "\u001b[8$" (21 terminals)
        //   "\u001b[8;2~" (3 terminals)
        //   "\u001b[>1;2F" (1 terminal)
        //   "\u001b[2F" (1 terminal)
        //   "\u001bO2F" (1 terminal)
        /// <summary>Get the value of capability "key_send".</summary>
        /// <returns>If not null it is the value of the capability "key_send". If null, this terminal information does not support the capability "key_send".</returns>
        public string? KeySend => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeySend);

        // Capability name: key_sf
        // Terminals supporting this capability: 101 terminals
        // Values of this capability:
        //   "\u001b[1;2B" (84 terminals)
        //   "\u001b[B" (6 terminals)
        //   "\u001b[a" (5 terminals)
        //   "\u001bO2B" (2 terminals)
        //   "\u001b[>1;2B" (1 terminal)
        //   "\u001b[2B" (1 terminal)
        //   "\u001bO1;2B" (1 terminal)
        //   "\u001bOB" (1 terminal)
        /// <summary>Get the value of capability "key_sf".</summary>
        /// <returns>If not null it is the value of the capability "key_sf". If null, this terminal information does not support the capability "key_sf".</returns>
        public string? KeySf => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeySf);

        // Capability name: key_sfind
        // Terminals supporting this capability: 4 terminals
        // Values of this capability:
        //   "\u001b[1$" (4 terminals)
        /// <summary>Get the value of capability "key_sfind".</summary>
        /// <returns>If not null it is the value of the capability "key_sfind". If null, this terminal information does not support the capability "key_sfind".</returns>
        public string? KeySfind => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeySfind);

        // Capability name: key_shome
        // Terminals supporting this capability: 129 terminals
        // Values of this capability:
        //   "\u001b[1;2H" (85 terminals)
        //   "\u001b[7$" (21 terminals)
        //   "\u001b{" (17 terminals)
        //   "\u001b[7;2~" (3 terminals)
        //   "\u001b[>1;2H" (1 terminal)
        //   "\u001b[2H" (1 terminal)
        //   "\u001bO2H" (1 terminal)
        /// <summary>Get the value of capability "key_shome".</summary>
        /// <returns>If not null it is the value of the capability "key_shome". If null, this terminal information does not support the capability "key_shome".</returns>
        public string? KeyShome => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyShome);

        // Capability name: key_sic
        // Terminals supporting this capability: 99 terminals
        // Values of this capability:
        //   "\u001b[2;2~" (89 terminals)
        //   "\u001b[2$" (9 terminals)
        //   "\u001b[>2;2~" (1 terminal)
        /// <summary>Get the value of capability "key_sic".</summary>
        /// <returns>If not null it is the value of the capability "key_sic". If null, this terminal information does not support the capability "key_sic".</returns>
        public string? KeySic => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeySic);

        // Capability name: key_sleft
        // Terminals supporting this capability: 130 terminals
        // Values of this capability:
        //   "\u001b[1;2D" (95 terminals)
        //   "\u001b[d" (21 terminals)
        //   "\u001b[D" (6 terminals)
        //   "\u001bO2D" (3 terminals)
        //   "\u001bO1;2D" (2 terminals)
        //   "\u001b[>1;2D" (1 terminal)
        //   "\u001b[2D" (1 terminal)
        //   "\u001bOD" (1 terminal)
        /// <summary>Get the value of capability "key_sleft".</summary>
        /// <returns>If not null it is the value of the capability "key_sleft". If null, this terminal information does not support the capability "key_sleft".</returns>
        public string? KeySleft => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeySleft);

        // Capability name: key_snext
        // Terminals supporting this capability: 110 terminals
        // Values of this capability:
        //   "\u001b[6;2~" (91 terminals)
        //   "\u001b[6$" (18 terminals)
        //   "\u001b[>6;2~" (1 terminal)
        /// <summary>Get the value of capability "key_snext".</summary>
        /// <returns>If not null it is the value of the capability "key_snext". If null, this terminal information does not support the capability "key_snext".</returns>
        public string? KeySnext => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeySnext);

        // Capability name: key_sprevious
        // Terminals supporting this capability: 110 terminals
        // Values of this capability:
        //   "\u001b[5;2~" (91 terminals)
        //   "\u001b[5$" (18 terminals)
        //   "\u001b[>5;2~" (1 terminal)
        /// <summary>Get the value of capability "key_sprevious".</summary>
        /// <returns>If not null it is the value of the capability "key_sprevious". If null, this terminal information does not support the capability "key_sprevious".</returns>
        public string? KeySprevious => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeySprevious);

        // Capability name: key_sr
        // Terminals supporting this capability: 101 terminals
        // Values of this capability:
        //   "\u001b[1;2A" (84 terminals)
        //   "\u001b[A" (6 terminals)
        //   "\u001b[b" (5 terminals)
        //   "\u001bO2A" (2 terminals)
        //   "\u001b[>1;2A" (1 terminal)
        //   "\u001b[2A" (1 terminal)
        //   "\u001bO1;2A" (1 terminal)
        //   "\u001bOA" (1 terminal)
        /// <summary>Get the value of capability "key_sr".</summary>
        /// <returns>If not null it is the value of the capability "key_sr". If null, this terminal information does not support the capability "key_sr".</returns>
        public string? KeySr => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeySr);

        // Capability name: key_sright
        // Terminals supporting this capability: 130 terminals
        // Values of this capability:
        //   "\u001b[1;2C" (95 terminals)
        //   "\u001b[c" (21 terminals)
        //   "\u001b[C" (6 terminals)
        //   "\u001bO2C" (3 terminals)
        //   "\u001bO1;2C" (2 terminals)
        //   "\u001b[>1;2C" (1 terminal)
        //   "\u001b[2C" (1 terminal)
        //   "\u001bOC" (1 terminal)
        /// <summary>Get the value of capability "key_sright".</summary>
        /// <returns>If not null it is the value of the capability "key_sright". If null, this terminal information does not support the capability "key_sright".</returns>
        public string? KeySright => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeySright);

        // Capability name: key_stab
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b1" (2 terminals)
        /// <summary>Get the value of capability "key_stab".</summary>
        /// <returns>If not null it is the value of the capability "key_stab". If null, this terminal information does not support the capability "key_stab".</returns>
        public string? KeyStab => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyStab);

        // Capability name: key_suspend
        // Terminals supporting this capability: 22 terminals
        // Values of this capability:
        //   "\u001a" (22 terminals)
        /// <summary>Get the value of capability "key_suspend".</summary>
        /// <returns>If not null it is the value of the capability "key_suspend". If null, this terminal information does not support the capability "key_suspend".</returns>
        public string? KeySuspend => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeySuspend);

        // Capability name: key_undo
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b[195z" (2 terminals)
        /// <summary>Get the value of capability "key_undo".</summary>
        /// <returns>If not null it is the value of the capability "key_undo". If null, this terminal information does not support the capability "key_undo".</returns>
        public string? KeyUndo => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyUndo);

        // Capability name: key_up
        // Terminals supporting this capability: 319 terminals
        // Values of this capability:
        //   "\u001bOA" (183 terminals)
        //   "\u001b[A" (106 terminals)
        //   "\u000b" (25 terminals)
        //   "\u001a" (3 terminals)
        //   "?A" (1 terminal)
        //   "\u001bA" (1 terminal)
        /// <summary>Get the value of capability "key_up".</summary>
        /// <returns>If not null it is the value of the capability "key_up". If null, this terminal information does not support the capability "key_up".</returns>
        public string? KeyUp => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeyUp);

        // Capability name: keypad_local
        // Terminals supporting this capability: 239 terminals
        // Values of this capability:
        //   "\u001b[?1l\u001b>" (172 terminals)
        //   "\u001b>" (49 terminals)
        //   "\u001b[?1l" (6 terminals)
        //   "\u001b[?1l\u001b>$<10/>" (4 terminals)
        //   "\u001bk" (4 terminals)
        //   "" (3 terminals)
        //   "??1l\u001b>" (1 terminal)
        /// <summary>Get the value of capability "keypad_local".</summary>
        /// <returns>If not null it is the value of the capability "keypad_local". If null, this terminal information does not support the capability "keypad_local".</returns>
        public string? KeypadLocal => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeypadLocal);

        // Capability name: keypad_xmit
        // Terminals supporting this capability: 239 terminals
        // Values of this capability:
        //   "\u001b[?1h\u001b=" (172 terminals)
        //   "\u001b=" (36 terminals)
        //   "\u001b[?1l\u001b=" (9 terminals)
        //   "\u001b[?1h" (6 terminals)
        //   "\u001b[?1h\u001b=$<10/>" (4 terminals)
        //   "\u001b[?1l\u001b[?7h\u001b=" (4 terminals)
        //   "\u001bl" (4 terminals)
        //   "" (3 terminals)
        //   "??1h\u001b=" (1 terminal)
        /// <summary>Get the value of capability "keypad_xmit".</summary>
        /// <returns>If not null it is the value of the capability "keypad_xmit". If null, this terminal information does not support the capability "keypad_xmit".</returns>
        public string? KeypadXmit => _database.GetStringCapabilityValue(TermInfoStringCapabilities.KeypadXmit);

        // Capability name: kF1
        // Terminals supporting this capability: 28 terminals
        // Values of this capability:
        //   "\u0001`\r" (25 terminals)
        //   "\u0002!\r" (3 terminals)
        /// <summary>Get the value of extended capability "kF1".</summary>
        /// <returns>If not null it is the value of the extended capability "kF1". If null, this terminal information does not support the extended capability "kF1".</returns>
        public string? KF1 => _database.GetStringCapabilityValue("kF1");

        // Capability name: kF10
        // Terminals supporting this capability: 25 terminals
        // Values of this capability:
        //   "\u0001i\r" (25 terminals)
        /// <summary>Get the value of extended capability "kF10".</summary>
        /// <returns>If not null it is the value of the extended capability "kF10". If null, this terminal information does not support the extended capability "kF10".</returns>
        public string? KF10 => _database.GetStringCapabilityValue("kF10");

        // Capability name: kF11
        // Terminals supporting this capability: 25 terminals
        // Values of this capability:
        //   "\u0001j\r" (25 terminals)
        /// <summary>Get the value of extended capability "kF11".</summary>
        /// <returns>If not null it is the value of the extended capability "kF11". If null, this terminal information does not support the extended capability "kF11".</returns>
        public string? KF11 => _database.GetStringCapabilityValue("kF11");

        // Capability name: kF12
        // Terminals supporting this capability: 17 terminals
        // Values of this capability:
        //   "\u0001k\r" (17 terminals)
        /// <summary>Get the value of extended capability "kF12".</summary>
        /// <returns>If not null it is the value of the extended capability "kF12". If null, this terminal information does not support the extended capability "kF12".</returns>
        public string? KF12 => _database.GetStringCapabilityValue("kF12");

        // Capability name: kF13
        // Terminals supporting this capability: 17 terminals
        // Values of this capability:
        //   "\u0001l\r" (17 terminals)
        /// <summary>Get the value of extended capability "kF13".</summary>
        /// <returns>If not null it is the value of the extended capability "kF13". If null, this terminal information does not support the extended capability "kF13".</returns>
        public string? KF13 => _database.GetStringCapabilityValue("kF13");

        // Capability name: kF14
        // Terminals supporting this capability: 17 terminals
        // Values of this capability:
        //   "\u0001m\r" (17 terminals)
        /// <summary>Get the value of extended capability "kF14".</summary>
        /// <returns>If not null it is the value of the extended capability "kF14". If null, this terminal information does not support the extended capability "kF14".</returns>
        public string? KF14 => _database.GetStringCapabilityValue("kF14");

        // Capability name: kF15
        // Terminals supporting this capability: 17 terminals
        // Values of this capability:
        //   "\u0001n\r" (17 terminals)
        /// <summary>Get the value of extended capability "kF15".</summary>
        /// <returns>If not null it is the value of the extended capability "kF15". If null, this terminal information does not support the extended capability "kF15".</returns>
        public string? KF15 => _database.GetStringCapabilityValue("kF15");

        // Capability name: kF16
        // Terminals supporting this capability: 17 terminals
        // Values of this capability:
        //   "\u0001o\r" (17 terminals)
        /// <summary>Get the value of extended capability "kF16".</summary>
        /// <returns>If not null it is the value of the extended capability "kF16". If null, this terminal information does not support the extended capability "kF16".</returns>
        public string? KF16 => _database.GetStringCapabilityValue("kF16");

        // Capability name: kF2
        // Terminals supporting this capability: 28 terminals
        // Values of this capability:
        //   "\u0001a\r" (25 terminals)
        //   "\u0002\"\r" (3 terminals)
        /// <summary>Get the value of extended capability "kF2".</summary>
        /// <returns>If not null it is the value of the extended capability "kF2". If null, this terminal information does not support the extended capability "kF2".</returns>
        public string? KF2 => _database.GetStringCapabilityValue("kF2");

        // Capability name: kF3
        // Terminals supporting this capability: 28 terminals
        // Values of this capability:
        //   "\u0001b\r" (25 terminals)
        //   "\u0002#\r" (3 terminals)
        /// <summary>Get the value of extended capability "kF3".</summary>
        /// <returns>If not null it is the value of the extended capability "kF3". If null, this terminal information does not support the extended capability "kF3".</returns>
        public string? KF3 => _database.GetStringCapabilityValue("kF3");

        // Capability name: kF4
        // Terminals supporting this capability: 28 terminals
        // Values of this capability:
        //   "\u0001c\r" (25 terminals)
        //   "\u0002$\r" (3 terminals)
        /// <summary>Get the value of extended capability "kF4".</summary>
        /// <returns>If not null it is the value of the extended capability "kF4". If null, this terminal information does not support the extended capability "kF4".</returns>
        public string? KF4 => _database.GetStringCapabilityValue("kF4");

        // Capability name: kF5
        // Terminals supporting this capability: 28 terminals
        // Values of this capability:
        //   "\u0001d\r" (25 terminals)
        //   "\u0002%\r" (3 terminals)
        /// <summary>Get the value of extended capability "kF5".</summary>
        /// <returns>If not null it is the value of the extended capability "kF5". If null, this terminal information does not support the extended capability "kF5".</returns>
        public string? KF5 => _database.GetStringCapabilityValue("kF5");

        // Capability name: kF6
        // Terminals supporting this capability: 28 terminals
        // Values of this capability:
        //   "\u0001e\r" (25 terminals)
        //   "\u0002&\r" (3 terminals)
        /// <summary>Get the value of extended capability "kF6".</summary>
        /// <returns>If not null it is the value of the extended capability "kF6". If null, this terminal information does not support the extended capability "kF6".</returns>
        public string? KF6 => _database.GetStringCapabilityValue("kF6");

        // Capability name: kF7
        // Terminals supporting this capability: 28 terminals
        // Values of this capability:
        //   "\u0001f\r" (25 terminals)
        //   "\u0002'\r" (3 terminals)
        /// <summary>Get the value of extended capability "kF7".</summary>
        /// <returns>If not null it is the value of the extended capability "kF7". If null, this terminal information does not support the extended capability "kF7".</returns>
        public string? KF7 => _database.GetStringCapabilityValue("kF7");

        // Capability name: kF8
        // Terminals supporting this capability: 28 terminals
        // Values of this capability:
        //   "\u0001g\r" (25 terminals)
        //   "\u0002(\r" (3 terminals)
        /// <summary>Get the value of extended capability "kF8".</summary>
        /// <returns>If not null it is the value of the extended capability "kF8". If null, this terminal information does not support the extended capability "kF8".</returns>
        public string? KF8 => _database.GetStringCapabilityValue("kF8");

        // Capability name: kF9
        // Terminals supporting this capability: 25 terminals
        // Values of this capability:
        //   "\u0001h\r" (25 terminals)
        /// <summary>Get the value of extended capability "kF9".</summary>
        /// <returns>If not null it is the value of the extended capability "kF9". If null, this terminal information does not support the extended capability "kF9".</returns>
        public string? KF9 => _database.GetStringCapabilityValue("kF9");

        // Capability name: kFND5
        // Terminals supporting this capability: 4 terminals
        // Values of this capability:
        //   "\u001b[1^" (4 terminals)
        /// <summary>Get the value of extended capability "kFND5".</summary>
        /// <returns>If not null it is the value of the extended capability "kFND5". If null, this terminal information does not support the extended capability "kFND5".</returns>
        public string? KFND5 => _database.GetStringCapabilityValue("kFND5");

        // Capability name: kFND6
        // Terminals supporting this capability: 4 terminals
        // Values of this capability:
        //   "\u001b[1@" (4 terminals)
        /// <summary>Get the value of extended capability "kFND6".</summary>
        /// <returns>If not null it is the value of the extended capability "kFND6". If null, this terminal information does not support the extended capability "kFND6".</returns>
        public string? KFND6 => _database.GetStringCapabilityValue("kFND6");

        // Capability name: kHOM3
        // Terminals supporting this capability: 82 terminals
        // Values of this capability:
        //   "\u001b[1;3H" (75 terminals)
        //   "\u001b[7;3~" (3 terminals)
        //   "\u001b[>1;3H" (1 terminal)
        //   "\u001b[1;9H" (1 terminal)
        //   "\u001b[3H" (1 terminal)
        //   "\u001bO3H" (1 terminal)
        /// <summary>Get the value of extended capability "kHOM3".</summary>
        /// <returns>If not null it is the value of the extended capability "kHOM3". If null, this terminal information does not support the extended capability "kHOM3".</returns>
        public string? KHOM3 => _database.GetStringCapabilityValue("kHOM3");

        // Capability name: kHOM4
        // Terminals supporting this capability: 82 terminals
        // Values of this capability:
        //   "\u001b[1;4H" (75 terminals)
        //   "\u001b[7;4~" (3 terminals)
        //   "\u001b[>1;4H" (1 terminal)
        //   "\u001b[1;10H" (1 terminal)
        //   "\u001b[4H" (1 terminal)
        //   "\u001bO4H" (1 terminal)
        /// <summary>Get the value of extended capability "kHOM4".</summary>
        /// <returns>If not null it is the value of the extended capability "kHOM4". If null, this terminal information does not support the extended capability "kHOM4".</returns>
        public string? KHOM4 => _database.GetStringCapabilityValue("kHOM4");

        // Capability name: kHOM5
        // Terminals supporting this capability: 91 terminals
        // Values of this capability:
        //   "\u001b[1;5H" (77 terminals)
        //   "\u001b[7^" (8 terminals)
        //   "\u001b[7;5~" (3 terminals)
        //   "\u001b[>1;5H" (1 terminal)
        //   "\u001b[5H" (1 terminal)
        //   "\u001bO5H" (1 terminal)
        /// <summary>Get the value of extended capability "kHOM5".</summary>
        /// <returns>If not null it is the value of the extended capability "kHOM5". If null, this terminal information does not support the extended capability "kHOM5".</returns>
        public string? KHOM5 => _database.GetStringCapabilityValue("kHOM5");

        // Capability name: kHOM6
        // Terminals supporting this capability: 90 terminals
        // Values of this capability:
        //   "\u001b[1;6H" (76 terminals)
        //   "\u001b[7@" (8 terminals)
        //   "\u001b[7;6~" (3 terminals)
        //   "\u001b[>1;6H" (1 terminal)
        //   "\u001b[6H" (1 terminal)
        //   "\u001bO6H" (1 terminal)
        /// <summary>Get the value of extended capability "kHOM6".</summary>
        /// <returns>If not null it is the value of the extended capability "kHOM6". If null, this terminal information does not support the extended capability "kHOM6".</returns>
        public string? KHOM6 => _database.GetStringCapabilityValue("kHOM6");

        // Capability name: kHOM7
        // Terminals supporting this capability: 82 terminals
        // Values of this capability:
        //   "\u001b[1;7H" (75 terminals)
        //   "\u001b[7;7~" (3 terminals)
        //   "\u001b[>1;7H" (1 terminal)
        //   "\u001b[1;13H" (1 terminal)
        //   "\u001b[7H" (1 terminal)
        //   "\u001bO7H" (1 terminal)
        /// <summary>Get the value of extended capability "kHOM7".</summary>
        /// <returns>If not null it is the value of the extended capability "kHOM7". If null, this terminal information does not support the extended capability "kHOM7".</returns>
        public string? KHOM7 => _database.GetStringCapabilityValue("kHOM7");

        // Capability name: kHOM8
        // Terminals supporting this capability: 1 terminal
        // Values of this capability:
        //   "\u001b[1;14H" (1 terminal)
        /// <summary>Get the value of extended capability "kHOM8".</summary>
        /// <returns>If not null it is the value of the extended capability "kHOM8". If null, this terminal information does not support the extended capability "kHOM8".</returns>
        public string? KHOM8 => _database.GetStringCapabilityValue("kHOM8");

        // Capability name: kIC3
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[2;3~" (80 terminals)
        //   "\u001b[>2;3~" (1 terminal)
        /// <summary>Get the value of extended capability "kIC3".</summary>
        /// <returns>If not null it is the value of the extended capability "kIC3". If null, this terminal information does not support the extended capability "kIC3".</returns>
        public string? KIC3 => _database.GetStringCapabilityValue("kIC3");

        // Capability name: kIC4
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[2;4~" (80 terminals)
        //   "\u001b[>2;4~" (1 terminal)
        /// <summary>Get the value of extended capability "kIC4".</summary>
        /// <returns>If not null it is the value of the extended capability "kIC4". If null, this terminal information does not support the extended capability "kIC4".</returns>
        public string? KIC4 => _database.GetStringCapabilityValue("kIC4");

        // Capability name: kIC5
        // Terminals supporting this capability: 91 terminals
        // Values of this capability:
        //   "\u001b[2;5~" (82 terminals)
        //   "\u001b[2^" (8 terminals)
        //   "\u001b[>2;5~" (1 terminal)
        /// <summary>Get the value of extended capability "kIC5".</summary>
        /// <returns>If not null it is the value of the extended capability "kIC5". If null, this terminal information does not support the extended capability "kIC5".</returns>
        public string? KIC5 => _database.GetStringCapabilityValue("kIC5");

        // Capability name: kIC6
        // Terminals supporting this capability: 91 terminals
        // Values of this capability:
        //   "\u001b[2;6~" (82 terminals)
        //   "\u001b[2@" (8 terminals)
        //   "\u001b[>2;6~" (1 terminal)
        /// <summary>Get the value of extended capability "kIC6".</summary>
        /// <returns>If not null it is the value of the extended capability "kIC6". If null, this terminal information does not support the extended capability "kIC6".</returns>
        public string? KIC6 => _database.GetStringCapabilityValue("kIC6");

        // Capability name: kIC7
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[2;7~" (80 terminals)
        //   "\u001b[>2;7~" (1 terminal)
        /// <summary>Get the value of extended capability "kIC7".</summary>
        /// <returns>If not null it is the value of the extended capability "kIC7". If null, this terminal information does not support the extended capability "kIC7".</returns>
        public string? KIC7 => _database.GetStringCapabilityValue("kIC7");

        // Capability name: kLFT3
        // Terminals supporting this capability: 97 terminals
        // Values of this capability:
        //   "\u001b[1;3D" (82 terminals)
        //   "\u001bb" (8 terminals)
        //   "\u001bO1;3D" (2 terminals)
        //   "\u001bO3D" (2 terminals)
        //   "\u001b[>1;3D" (1 terminal)
        //   "\u001b[3D" (1 terminal)
        //   "\u001b\u001b[D" (1 terminal)
        /// <summary>Get the value of extended capability "kLFT3".</summary>
        /// <returns>If not null it is the value of the extended capability "kLFT3". If null, this terminal information does not support the extended capability "kLFT3".</returns>
        public string? KLFT3 => _database.GetStringCapabilityValue("kLFT3");

        // Capability name: kLFT4
        // Terminals supporting this capability: 84 terminals
        // Values of this capability:
        //   "\u001b[1;4D" (77 terminals)
        //   "\u001bO1;4D" (2 terminals)
        //   "\u001bO4D" (2 terminals)
        //   "\u001b[>1;4D" (1 terminal)
        //   "\u001b[1;10D" (1 terminal)
        //   "\u001b[4D" (1 terminal)
        /// <summary>Get the value of extended capability "kLFT4".</summary>
        /// <returns>If not null it is the value of the extended capability "kLFT4". If null, this terminal information does not support the extended capability "kLFT4".</returns>
        public string? KLFT4 => _database.GetStringCapabilityValue("kLFT4");

        // Capability name: kLFT5
        // Terminals supporting this capability: 120 terminals
        // Values of this capability:
        //   "\u001b[1;5D" (91 terminals)
        //   "\u001bOd" (20 terminals)
        //   "\u001b[5D" (4 terminals)
        //   "\u001bO1;5D" (2 terminals)
        //   "\u001bO5D" (2 terminals)
        //   "\u001b[>1;5D" (1 terminal)
        /// <summary>Get the value of extended capability "kLFT5".</summary>
        /// <returns>If not null it is the value of the extended capability "kLFT5". If null, this terminal information does not support the extended capability "kLFT5".</returns>
        public string? KLFT5 => _database.GetStringCapabilityValue("kLFT5");

        // Capability name: kLFT6
        // Terminals supporting this capability: 96 terminals
        // Values of this capability:
        //   "\u001b[1;6D" (78 terminals)
        //   "\u001bOD" (12 terminals)
        //   "\u001bO1;6D" (2 terminals)
        //   "\u001bO6D" (2 terminals)
        //   "\u001b[>1;6D" (1 terminal)
        //   "\u001b[6D" (1 terminal)
        /// <summary>Get the value of extended capability "kLFT6".</summary>
        /// <returns>If not null it is the value of the extended capability "kLFT6". If null, this terminal information does not support the extended capability "kLFT6".</returns>
        public string? KLFT6 => _database.GetStringCapabilityValue("kLFT6");

        // Capability name: kLFT7
        // Terminals supporting this capability: 83 terminals
        // Values of this capability:
        //   "\u001b[1;7D" (77 terminals)
        //   "\u001bO1;7D" (2 terminals)
        //   "\u001bO7D" (2 terminals)
        //   "\u001b[>1;7D" (1 terminal)
        //   "\u001b[7D" (1 terminal)
        /// <summary>Get the value of extended capability "kLFT7".</summary>
        /// <returns>If not null it is the value of the extended capability "kLFT7". If null, this terminal information does not support the extended capability "kLFT7".</returns>
        public string? KLFT7 => _database.GetStringCapabilityValue("kLFT7");

        // Capability name: kNXT3
        // Terminals supporting this capability: 87 terminals
        // Values of this capability:
        //   "\u001b[6;3~" (85 terminals)
        //   "\u001b[>6;3~" (1 terminal)
        //   "\u001b\u001b[6~" (1 terminal)
        /// <summary>Get the value of extended capability "kNXT3".</summary>
        /// <returns>If not null it is the value of the extended capability "kNXT3". If null, this terminal information does not support the extended capability "kNXT3".</returns>
        public string? KNXT3 => _database.GetStringCapabilityValue("kNXT3");

        // Capability name: kNXT4
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[6;4~" (80 terminals)
        //   "\u001b[>6;4~" (1 terminal)
        /// <summary>Get the value of extended capability "kNXT4".</summary>
        /// <returns>If not null it is the value of the extended capability "kNXT4". If null, this terminal information does not support the extended capability "kNXT4".</returns>
        public string? KNXT4 => _database.GetStringCapabilityValue("kNXT4");

        // Capability name: kNXT5
        // Terminals supporting this capability: 96 terminals
        // Values of this capability:
        //   "\u001b[6;5~" (87 terminals)
        //   "\u001b[6^" (8 terminals)
        //   "\u001b[>6;5~" (1 terminal)
        /// <summary>Get the value of extended capability "kNXT5".</summary>
        /// <returns>If not null it is the value of the extended capability "kNXT5". If null, this terminal information does not support the extended capability "kNXT5".</returns>
        public string? KNXT5 => _database.GetStringCapabilityValue("kNXT5");

        // Capability name: kNXT6
        // Terminals supporting this capability: 91 terminals
        // Values of this capability:
        //   "\u001b[6;6~" (82 terminals)
        //   "\u001b[6@" (8 terminals)
        //   "\u001b[>6;6~" (1 terminal)
        /// <summary>Get the value of extended capability "kNXT6".</summary>
        /// <returns>If not null it is the value of the extended capability "kNXT6". If null, this terminal information does not support the extended capability "kNXT6".</returns>
        public string? KNXT6 => _database.GetStringCapabilityValue("kNXT6");

        // Capability name: kNXT7
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[6;7~" (80 terminals)
        //   "\u001b[>6;7~" (1 terminal)
        /// <summary>Get the value of extended capability "kNXT7".</summary>
        /// <returns>If not null it is the value of the extended capability "kNXT7". If null, this terminal information does not support the extended capability "kNXT7".</returns>
        public string? KNXT7 => _database.GetStringCapabilityValue("kNXT7");

        // Capability name: kp5
        // Terminals supporting this capability: 10 terminals
        // Values of this capability:
        //   "\u001bOE" (10 terminals)
        /// <summary>Get the value of extended capability "kp5".</summary>
        /// <returns>If not null it is the value of the extended capability "kp5". If null, this terminal information does not support the extended capability "kp5".</returns>
        public string? Kp5 => _database.GetStringCapabilityValue("kp5");

        // Capability name: kpADD
        // Terminals supporting this capability: 10 terminals
        // Values of this capability:
        //   "\u001bOk" (10 terminals)
        /// <summary>Get the value of extended capability "kpADD".</summary>
        /// <returns>If not null it is the value of the extended capability "kpADD". If null, this terminal information does not support the extended capability "kpADD".</returns>
        public string? KpADD => _database.GetStringCapabilityValue("kpADD");

        // Capability name: kpCMA
        // Terminals supporting this capability: 10 terminals
        // Values of this capability:
        //   "\u001bOl" (10 terminals)
        /// <summary>Get the value of extended capability "kpCMA".</summary>
        /// <returns>If not null it is the value of the extended capability "kpCMA". If null, this terminal information does not support the extended capability "kpCMA".</returns>
        public string? KpCMA => _database.GetStringCapabilityValue("kpCMA");

        // Capability name: kpDIV
        // Terminals supporting this capability: 10 terminals
        // Values of this capability:
        //   "\u001bOo" (10 terminals)
        /// <summary>Get the value of extended capability "kpDIV".</summary>
        /// <returns>If not null it is the value of the extended capability "kpDIV". If null, this terminal information does not support the extended capability "kpDIV".</returns>
        public string? KpDIV => _database.GetStringCapabilityValue("kpDIV");

        // Capability name: kpDOT
        // Terminals supporting this capability: 10 terminals
        // Values of this capability:
        //   "\u001bOn" (10 terminals)
        /// <summary>Get the value of extended capability "kpDOT".</summary>
        /// <returns>If not null it is the value of the extended capability "kpDOT". If null, this terminal information does not support the extended capability "kpDOT".</returns>
        public string? KpDOT => _database.GetStringCapabilityValue("kpDOT");

        // Capability name: kpMUL
        // Terminals supporting this capability: 10 terminals
        // Values of this capability:
        //   "\u001bOj" (10 terminals)
        /// <summary>Get the value of extended capability "kpMUL".</summary>
        /// <returns>If not null it is the value of the extended capability "kpMUL". If null, this terminal information does not support the extended capability "kpMUL".</returns>
        public string? KpMUL => _database.GetStringCapabilityValue("kpMUL");

        // Capability name: kPRV3
        // Terminals supporting this capability: 87 terminals
        // Values of this capability:
        //   "\u001b[5;3~" (85 terminals)
        //   "\u001b[>5;3~" (1 terminal)
        //   "\u001b\u001b[5~" (1 terminal)
        /// <summary>Get the value of extended capability "kPRV3".</summary>
        /// <returns>If not null it is the value of the extended capability "kPRV3". If null, this terminal information does not support the extended capability "kPRV3".</returns>
        public string? KPRV3 => _database.GetStringCapabilityValue("kPRV3");

        // Capability name: kPRV4
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[5;4~" (80 terminals)
        //   "\u001b[>5;4~" (1 terminal)
        /// <summary>Get the value of extended capability "kPRV4".</summary>
        /// <returns>If not null it is the value of the extended capability "kPRV4". If null, this terminal information does not support the extended capability "kPRV4".</returns>
        public string? KPRV4 => _database.GetStringCapabilityValue("kPRV4");

        // Capability name: kPRV5
        // Terminals supporting this capability: 96 terminals
        // Values of this capability:
        //   "\u001b[5;5~" (87 terminals)
        //   "\u001b[5^" (8 terminals)
        //   "\u001b[>5;5~" (1 terminal)
        /// <summary>Get the value of extended capability "kPRV5".</summary>
        /// <returns>If not null it is the value of the extended capability "kPRV5". If null, this terminal information does not support the extended capability "kPRV5".</returns>
        public string? KPRV5 => _database.GetStringCapabilityValue("kPRV5");

        // Capability name: kPRV6
        // Terminals supporting this capability: 91 terminals
        // Values of this capability:
        //   "\u001b[5;6~" (82 terminals)
        //   "\u001b[5@" (8 terminals)
        //   "\u001b[>5;6~" (1 terminal)
        /// <summary>Get the value of extended capability "kPRV6".</summary>
        /// <returns>If not null it is the value of the extended capability "kPRV6". If null, this terminal information does not support the extended capability "kPRV6".</returns>
        public string? KPRV6 => _database.GetStringCapabilityValue("kPRV6");

        // Capability name: kPRV7
        // Terminals supporting this capability: 81 terminals
        // Values of this capability:
        //   "\u001b[5;7~" (80 terminals)
        //   "\u001b[>5;7~" (1 terminal)
        /// <summary>Get the value of extended capability "kPRV7".</summary>
        /// <returns>If not null it is the value of the extended capability "kPRV7". If null, this terminal information does not support the extended capability "kPRV7".</returns>
        public string? KPRV7 => _database.GetStringCapabilityValue("kPRV7");

        // Capability name: kpSUB
        // Terminals supporting this capability: 10 terminals
        // Values of this capability:
        //   "\u001bOm" (10 terminals)
        /// <summary>Get the value of extended capability "kpSUB".</summary>
        /// <returns>If not null it is the value of the extended capability "kpSUB". If null, this terminal information does not support the extended capability "kpSUB".</returns>
        public string? KpSUB => _database.GetStringCapabilityValue("kpSUB");

        // Capability name: kpZRO
        // Terminals supporting this capability: 10 terminals
        // Values of this capability:
        //   "\u001bOp" (10 terminals)
        /// <summary>Get the value of extended capability "kpZRO".</summary>
        /// <returns>If not null it is the value of the extended capability "kpZRO". If null, this terminal information does not support the extended capability "kpZRO".</returns>
        public string? KpZRO => _database.GetStringCapabilityValue("kpZRO");

        // Capability name: kRIT3
        // Terminals supporting this capability: 97 terminals
        // Values of this capability:
        //   "\u001b[1;3C" (82 terminals)
        //   "\u001bf" (8 terminals)
        //   "\u001bO1;3C" (2 terminals)
        //   "\u001bO3C" (2 terminals)
        //   "\u001b[>1;3C" (1 terminal)
        //   "\u001b[3C" (1 terminal)
        //   "\u001b\u001b[C" (1 terminal)
        /// <summary>Get the value of extended capability "kRIT3".</summary>
        /// <returns>If not null it is the value of the extended capability "kRIT3". If null, this terminal information does not support the extended capability "kRIT3".</returns>
        public string? KRIT3 => _database.GetStringCapabilityValue("kRIT3");

        // Capability name: kRIT4
        // Terminals supporting this capability: 84 terminals
        // Values of this capability:
        //   "\u001b[1;4C" (77 terminals)
        //   "\u001bO1;4C" (2 terminals)
        //   "\u001bO4C" (2 terminals)
        //   "\u001b[>1;4C" (1 terminal)
        //   "\u001b[1;10C" (1 terminal)
        //   "\u001b[4C" (1 terminal)
        /// <summary>Get the value of extended capability "kRIT4".</summary>
        /// <returns>If not null it is the value of the extended capability "kRIT4". If null, this terminal information does not support the extended capability "kRIT4".</returns>
        public string? KRIT4 => _database.GetStringCapabilityValue("kRIT4");

        // Capability name: kRIT5
        // Terminals supporting this capability: 120 terminals
        // Values of this capability:
        //   "\u001b[1;5C" (91 terminals)
        //   "\u001bOc" (20 terminals)
        //   "\u001b[5C" (4 terminals)
        //   "\u001bO1;5C" (2 terminals)
        //   "\u001bO5C" (2 terminals)
        //   "\u001b[>1;5C" (1 terminal)
        /// <summary>Get the value of extended capability "kRIT5".</summary>
        /// <returns>If not null it is the value of the extended capability "kRIT5". If null, this terminal information does not support the extended capability "kRIT5".</returns>
        public string? KRIT5 => _database.GetStringCapabilityValue("kRIT5");

        // Capability name: kRIT6
        // Terminals supporting this capability: 96 terminals
        // Values of this capability:
        //   "\u001b[1;6C" (78 terminals)
        //   "\u001bOC" (12 terminals)
        //   "\u001bO1;6C" (2 terminals)
        //   "\u001bO6C" (2 terminals)
        //   "\u001b[>1;6C" (1 terminal)
        //   "\u001b[6C" (1 terminal)
        /// <summary>Get the value of extended capability "kRIT6".</summary>
        /// <returns>If not null it is the value of the extended capability "kRIT6". If null, this terminal information does not support the extended capability "kRIT6".</returns>
        public string? KRIT6 => _database.GetStringCapabilityValue("kRIT6");

        // Capability name: kRIT7
        // Terminals supporting this capability: 83 terminals
        // Values of this capability:
        //   "\u001b[1;7C" (77 terminals)
        //   "\u001bO1;7C" (2 terminals)
        //   "\u001bO7C" (2 terminals)
        //   "\u001b[>1;7C" (1 terminal)
        //   "\u001b[7C" (1 terminal)
        /// <summary>Get the value of extended capability "kRIT7".</summary>
        /// <returns>If not null it is the value of the extended capability "kRIT7". If null, this terminal information does not support the extended capability "kRIT7".</returns>
        public string? KRIT7 => _database.GetStringCapabilityValue("kRIT7");

        // Capability name: kUP
        // Terminals supporting this capability: 103 terminals
        // Values of this capability:
        //   "\u001b[1;2A" (77 terminals)
        //   "\u001b[a" (20 terminals)
        //   "\u001bO1;2A" (2 terminals)
        //   "\u001bO2A" (2 terminals)
        //   "\u001b[>1;2A" (1 terminal)
        //   "\u001b[2A" (1 terminal)
        /// <summary>Get the value of extended capability "kUP".</summary>
        /// <returns>If not null it is the value of the extended capability "kUP". If null, this terminal information does not support the extended capability "kUP".</returns>
        public string? KUP => _database.GetStringCapabilityValue("kUP");

        // Capability name: kUP3
        // Terminals supporting this capability: 89 terminals
        // Values of this capability:
        //   "\u001b[1;3A" (82 terminals)
        //   "\u001bO1;3A" (2 terminals)
        //   "\u001bO3A" (2 terminals)
        //   "\u001b[>1;3A" (1 terminal)
        //   "\u001b[3A" (1 terminal)
        //   "\u001b\u001b[A" (1 terminal)
        /// <summary>Get the value of extended capability "kUP3".</summary>
        /// <returns>If not null it is the value of the extended capability "kUP3". If null, this terminal information does not support the extended capability "kUP3".</returns>
        public string? KUP3 => _database.GetStringCapabilityValue("kUP3");

        // Capability name: kUP4
        // Terminals supporting this capability: 84 terminals
        // Values of this capability:
        //   "\u001b[1;4A" (77 terminals)
        //   "\u001bO1;4A" (2 terminals)
        //   "\u001bO4A" (2 terminals)
        //   "\u001b[>1;4A" (1 terminal)
        //   "\u001b[1;10A" (1 terminal)
        //   "\u001b[4A" (1 terminal)
        /// <summary>Get the value of extended capability "kUP4".</summary>
        /// <returns>If not null it is the value of the extended capability "kUP4". If null, this terminal information does not support the extended capability "kUP4".</returns>
        public string? KUP4 => _database.GetStringCapabilityValue("kUP4");

        // Capability name: kUP5
        // Terminals supporting this capability: 109 terminals
        // Values of this capability:
        //   "\u001b[1;5A" (83 terminals)
        //   "\u001bOa" (20 terminals)
        //   "\u001bO1;5A" (2 terminals)
        //   "\u001bO5A" (2 terminals)
        //   "\u001b[>1;5A" (1 terminal)
        //   "\u001b[5A" (1 terminal)
        /// <summary>Get the value of extended capability "kUP5".</summary>
        /// <returns>If not null it is the value of the extended capability "kUP5". If null, this terminal information does not support the extended capability "kUP5".</returns>
        public string? KUP5 => _database.GetStringCapabilityValue("kUP5");

        // Capability name: kUP6
        // Terminals supporting this capability: 96 terminals
        // Values of this capability:
        //   "\u001b[1;6A" (78 terminals)
        //   "\u001bOA" (12 terminals)
        //   "\u001bO1;6A" (2 terminals)
        //   "\u001bO6A" (2 terminals)
        //   "\u001b[>1;6A" (1 terminal)
        //   "\u001b[6A" (1 terminal)
        /// <summary>Get the value of extended capability "kUP6".</summary>
        /// <returns>If not null it is the value of the extended capability "kUP6". If null, this terminal information does not support the extended capability "kUP6".</returns>
        public string? KUP6 => _database.GetStringCapabilityValue("kUP6");

        // Capability name: kUP7
        // Terminals supporting this capability: 83 terminals
        // Values of this capability:
        //   "\u001b[1;7A" (77 terminals)
        //   "\u001bO1;7A" (2 terminals)
        //   "\u001bO7A" (2 terminals)
        //   "\u001b[>1;7A" (1 terminal)
        //   "\u001b[7A" (1 terminal)
        /// <summary>Get the value of extended capability "kUP7".</summary>
        /// <returns>If not null it is the value of the extended capability "kUP7". If null, this terminal information does not support the extended capability "kUP7".</returns>
        public string? KUP7 => _database.GetStringCapabilityValue("kUP7");

        // Capability name: lab_f0
        // Terminals supporting this capability: 5 terminals
        // Values of this capability:
        //   "F1" (3 terminals)
        //   "End" (2 terminals)
        /// <summary>Get the value of capability "lab_f0".</summary>
        /// <returns>If not null it is the value of the capability "lab_f0". If null, this terminal information does not support the capability "lab_f0".</returns>
        public string? LabF0 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.LabF0);

        // Capability name: lab_f1
        // Terminals supporting this capability: 65 terminals
        // Values of this capability:
        //   "PF1" (33 terminals)
        //   "pf1" (26 terminals)
        //   "F2" (3 terminals)
        //   "PgUp" (2 terminals)
        //   "\u001bOP" (1 terminal)
        /// <summary>Get the value of capability "lab_f1".</summary>
        /// <returns>If not null it is the value of the capability "lab_f1". If null, this terminal information does not support the capability "lab_f1".</returns>
        public string? LabF1 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.LabF1);

        // Capability name: lab_f2
        // Terminals supporting this capability: 65 terminals
        // Values of this capability:
        //   "PF2" (33 terminals)
        //   "pf2" (26 terminals)
        //   "F3" (3 terminals)
        //   "PgDn" (2 terminals)
        //   "\u001bOQ" (1 terminal)
        /// <summary>Get the value of capability "lab_f2".</summary>
        /// <returns>If not null it is the value of the capability "lab_f2". If null, this terminal information does not support the capability "lab_f2".</returns>
        public string? LabF2 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.LabF2);

        // Capability name: lab_f3
        // Terminals supporting this capability: 65 terminals
        // Values of this capability:
        //   "PF3" (33 terminals)
        //   "pf3" (26 terminals)
        //   "F4" (3 terminals)
        //   "Num +" (2 terminals)
        //   "\u001bOR" (1 terminal)
        /// <summary>Get the value of capability "lab_f3".</summary>
        /// <returns>If not null it is the value of the capability "lab_f3". If null, this terminal information does not support the capability "lab_f3".</returns>
        public string? LabF3 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.LabF3);

        // Capability name: lab_f4
        // Terminals supporting this capability: 65 terminals
        // Values of this capability:
        //   "PF4" (33 terminals)
        //   "pf4" (26 terminals)
        //   "F5" (3 terminals)
        //   "Num -" (2 terminals)
        //   "\u001bOS" (1 terminal)
        /// <summary>Get the value of capability "lab_f4".</summary>
        /// <returns>If not null it is the value of the capability "lab_f4". If null, this terminal information does not support the capability "lab_f4".</returns>
        public string? LabF4 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.LabF4);

        // Capability name: lab_f5
        // Terminals supporting this capability: 5 terminals
        // Values of this capability:
        //   "F6" (3 terminals)
        //   "Num 5" (2 terminals)
        /// <summary>Get the value of capability "lab_f5".</summary>
        /// <returns>If not null it is the value of the capability "lab_f5". If null, this terminal information does not support the capability "lab_f5".</returns>
        public string? LabF5 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.LabF5);

        // Capability name: lab_f6
        // Terminals supporting this capability: 3 terminals
        // Values of this capability:
        //   "F7" (3 terminals)
        /// <summary>Get the value of capability "lab_f6".</summary>
        /// <returns>If not null it is the value of the capability "lab_f6". If null, this terminal information does not support the capability "lab_f6".</returns>
        public string? LabF6 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.LabF6);

        // Capability name: lab_f7
        // Terminals supporting this capability: 3 terminals
        // Values of this capability:
        //   "F8" (3 terminals)
        /// <summary>Get the value of capability "lab_f7".</summary>
        /// <returns>If not null it is the value of the capability "lab_f7". If null, this terminal information does not support the capability "lab_f7".</returns>
        public string? LabF7 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.LabF7);

        // Capability name: label_height
        // Terminals supporting this capability: 17 terminals
        // Values of this capability:
        //   1 (17 terminals)
        /// <summary>Get the value of capability "label_height".</summary>
        /// <returns>If not null it is the value of the capability "label_height". If null, this terminal information does not support the capability "label_height".</returns>
        public int? LabelHeight => _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.LabelHeight);

        // Capability name: label_off
        // Terminals supporting this capability: 17 terminals
        // Values of this capability:
        //   "\u001bA11" (17 terminals)
        /// <summary>Get the value of capability "label_off".</summary>
        /// <returns>If not null it is the value of the capability "label_off". If null, this terminal information does not support the capability "label_off".</returns>
        public string? LabelOff => _database.GetStringCapabilityValue(TermInfoStringCapabilities.LabelOff);

        // Capability name: label_on
        // Terminals supporting this capability: 17 terminals
        // Values of this capability:
        //   "\u001bA10" (17 terminals)
        /// <summary>Get the value of capability "label_on".</summary>
        /// <returns>If not null it is the value of the capability "label_on". If null, this terminal information does not support the capability "label_on".</returns>
        public string? LabelOn => _database.GetStringCapabilityValue(TermInfoStringCapabilities.LabelOn);

        // Capability name: label_width
        // Terminals supporting this capability: 17 terminals
        // Values of this capability:
        //   8 (9 terminals)
        //   7 (8 terminals)
        /// <summary>Get the value of capability "label_width".</summary>
        /// <returns>If not null it is the value of the capability "label_width". If null, this terminal information does not support the capability "label_width".</returns>
        public int? LabelWidth => _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.LabelWidth);

        // Capability name: linefeed_is_newline
        // Terminals supporting this capability: 21 terminals
        // Values of this capability:
        //   false (21 terminals)
        /// <summary>Get the value of capability "linefeed_is_newline".</summary>
        /// <returns>If not null it is the value of the capability "linefeed_is_newline". If null, this terminal information does not support the capability "linefeed_is_newline".</returns>
        public bool? LinefeedIsNewline => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.LinefeedIsNewline);

        // Capability name: lines_of_memory
        // Terminals supporting this capability: 8 terminals
        // Values of this capability:
        //   0 (8 terminals)
        /// <summary>Get the value of capability "lines_of_memory".</summary>
        /// <returns>If not null it is the value of the capability "lines_of_memory". If null, this terminal information does not support the capability "lines_of_memory".</returns>
        public int? LinesOfMemory => _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.LinesOfMemory);

        // Capability name: lines
        // Terminals supporting this capability: 290 terminals
        // Values of this capability:
        //   24 (266 terminals)
        //   25 (10 terminals)
        //   42 (6 terminals)
        //   36 (4 terminals)
        //   48 (4 terminals)
        /// <summary>Get the value of capability "lines".</summary>
        /// <returns>If not null it is the value of the capability "lines". If null, this terminal information does not support the capability "lines".</returns>
        public int? Lines => _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.Lines);

        // Capability name: lpi_changes_res
        // Terminals supporting this capability: 133 terminals
        // Values of this capability:
        //   false (133 terminals)
        /// <summary>Get the value of capability "lpi_changes_res".</summary>
        /// <returns>If not null it is the value of the capability "lpi_changes_res". If null, this terminal information does not support the capability "lpi_changes_res".</returns>
        public bool? LpiChangesRes => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.LpiChangesRes);

        // Capability name: magic_cookie_glitch
        // Terminals supporting this capability: 10 terminals
        // Values of this capability:
        //   1 (10 terminals)
        /// <summary>Get the value of capability "magic_cookie_glitch".</summary>
        /// <returns>If not null it is the value of the capability "magic_cookie_glitch". If null, this terminal information does not support the capability "magic_cookie_glitch".</returns>
        public int? MagicCookieGlitch => _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.MagicCookieGlitch);

        // Capability name: max_attributes
        // Terminals supporting this capability: 15 terminals
        // Values of this capability:
        //   1 (15 terminals)
        /// <summary>Get the value of capability "max_attributes".</summary>
        /// <returns>If not null it is the value of the capability "max_attributes". If null, this terminal information does not support the capability "max_attributes".</returns>
        public int? MaxAttributes => _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.MaxAttributes);

        // Capability name: max_colors
        // Terminals supporting this capability: 197 terminals
        // Values of this capability:
        //   8 (120 terminals)
        //   256 (42 terminals)
        //   16 (17 terminals)
        //   16777216 (12 terminals)
        //   88 (5 terminals)
        //   64 (1 terminal)
        /// <summary>Get the value of capability "max_colors".</summary>
        /// <returns>If not null it is the value of the capability "max_colors". If null, this terminal information does not support the capability "max_colors".</returns>
        public int? MaxColors => _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.MaxColors);

        // Capability name: max_pairs
        // Terminals supporting this capability: 197 terminals
        // Values of this capability:
        //   64 (120 terminals)
        //   65536 (52 terminals)
        //   256 (17 terminals)
        //   7744 (5 terminals)
        //   32767 (2 terminals)
        //   8 (1 terminal)
        /// <summary>Get the value of capability "max_pairs".</summary>
        /// <returns>If not null it is the value of the capability "max_pairs". If null, this terminal information does not support the capability "max_pairs".</returns>
        public int? MaxPairs => _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.MaxPairs);

        // Capability name: memory_above
        // Terminals supporting this capability: 315 terminals
        // Values of this capability:
        //   false (315 terminals)
        /// <summary>Get the value of capability "memory_above".</summary>
        /// <returns>If not null it is the value of the capability "memory_above". If null, this terminal information does not support the capability "memory_above".</returns>
        public bool? MemoryAbove => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.MemoryAbove);

        // Capability name: memory_below
        // Terminals supporting this capability: 315 terminals
        // Values of this capability:
        //   false (315 terminals)
        /// <summary>Get the value of capability "memory_below".</summary>
        /// <returns>If not null it is the value of the capability "memory_below". If null, this terminal information does not support the capability "memory_below".</returns>
        public bool? MemoryBelow => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.MemoryBelow);

        // Capability name: memory_lock
        // Terminals supporting this capability: 72 terminals
        // Values of this capability:
        //   "\u001bl" (72 terminals)
        /// <summary>Get the value of capability "memory_lock".</summary>
        /// <returns>If not null it is the value of the capability "memory_lock". If null, this terminal information does not support the capability "memory_lock".</returns>
        public string? MemoryLock => _database.GetStringCapabilityValue(TermInfoStringCapabilities.MemoryLock);

        // Capability name: memory_unlock
        // Terminals supporting this capability: 72 terminals
        // Values of this capability:
        //   "\u001bm" (72 terminals)
        /// <summary>Get the value of capability "memory_unlock".</summary>
        /// <returns>If not null it is the value of the capability "memory_unlock". If null, this terminal information does not support the capability "memory_unlock".</returns>
        public string? MemoryUnlock => _database.GetStringCapabilityValue(TermInfoStringCapabilities.MemoryUnlock);

        // Capability name: meta_off
        // Terminals supporting this capability: 33 terminals
        // Values of this capability:
        //   "\u001b[?1034l" (33 terminals)
        /// <summary>Get the value of capability "meta_off".</summary>
        /// <returns>If not null it is the value of the capability "meta_off". If null, this terminal information does not support the capability "meta_off".</returns>
        public string? MetaOff => _database.GetStringCapabilityValue(TermInfoStringCapabilities.MetaOff);

        // Capability name: meta_on
        // Terminals supporting this capability: 33 terminals
        // Values of this capability:
        //   "\u001b[?1034h" (33 terminals)
        /// <summary>Get the value of capability "meta_on".</summary>
        /// <returns>If not null it is the value of the capability "meta_on". If null, this terminal information does not support the capability "meta_on".</returns>
        public string? MetaOn => _database.GetStringCapabilityValue(TermInfoStringCapabilities.MetaOn);

        // Capability name: move_insert_mode
        // Terminals supporting this capability: 315 terminals
        // Values of this capability:
        //   true (284 terminals)
        //   false (31 terminals)
        /// <summary>Get the value of capability "move_insert_mode".</summary>
        /// <returns>If not null it is the value of the capability "move_insert_mode". If null, this terminal information does not support the capability "move_insert_mode".</returns>
        public bool? MoveInsertMode => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.MoveInsertMode);

        // Capability name: move_standout_mode
        // Terminals supporting this capability: 315 terminals
        // Values of this capability:
        //   true (282 terminals)
        //   false (33 terminals)
        /// <summary>Get the value of capability "move_standout_mode".</summary>
        /// <returns>If not null it is the value of the capability "move_standout_mode". If null, this terminal information does not support the capability "move_standout_mode".</returns>
        public bool? MoveStandoutMode => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.MoveStandoutMode);

        // Capability name: Ms
        // Terminals supporting this capability: 37 terminals
        // Values of this capability:
        //   "\u001b]52;%p1%s;%p2%s\u0007" (37 terminals)
        /// <summary>Get the value of extended capability "Ms".</summary>
        /// <param name="pc"><see cref="string"/> value to operate on. This is one of the characters 'c' (clipbord), 'p' (primary), 'q' (secondary), 's' (select), '0'-'7' (cut-buffers 0-7) contains 0 or more. In xterm, an empty <paramref name="pc"/> is equivalent to "s0".</param>
        /// <param name="pd"><see cref="string"/> value that is the data to operate on for "pc". If it is a base64 encoded string, the decoded string is set to the target indicated by <paramref name="pc"/>. In xterm, if it is "?", the target value indicated by <paramref name="pc"/> is posted to the host.</param>
        /// <returns>If not null it is the value of the extended capability "Ms". If null, this terminal information does not support the extended capability "Ms".</returns>
        /// <example>
        /// <list type="bullet">
        /// <item>Ms("c", "&lt;base64 encoded string&gt;") returns an OSC sequence that copies the string to the clipboard.</item>
        /// <item>Ms("c", "") returns an OSC sequence that clears the clipboard.</item>
        /// </list>
        /// </example>
        public string? Ms(string pc, string pd)
            => _database.GetStringCapabilityValue("Ms", pc, pd);

        // Capability name: needs_xon_xoff
        // Terminals supporting this capability: 252 terminals
        // Values of this capability:
        //   false (252 terminals)
        /// <summary>Get the value of capability "needs_xon_xoff".</summary>
        /// <returns>If not null it is the value of the capability "needs_xon_xoff". If null, this terminal information does not support the capability "needs_xon_xoff".</returns>
        public bool? NeedsXonXoff => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.NeedsXonXoff);

        // Capability name: newline
        // Terminals supporting this capability: 116 terminals
        // Values of this capability:
        //   "\u001bE" (65 terminals)
        //   "\r\n" (33 terminals)
        //   "\u001bE$<5>" (8 terminals)
        //   "\r\n$<3>" (4 terminals)
        //   "\r\n$<11>" (2 terminals)
        //   "\r\n$<6>" (2 terminals)
        //   "\r\u001b[S" (2 terminals)
        /// <summary>Get the value of capability "newline".</summary>
        /// <returns>If not null it is the value of the capability "newline". If null, this terminal information does not support the capability "newline".</returns>
        public string? Newline => _database.GetStringCapabilityValue(TermInfoStringCapabilities.Newline);

        // Capability name: no_color_video
        // Terminals supporting this capability: 56 terminals
        // Values of this capability:
        //   3 (18 terminals)
        //   18 (12 terminals)
        //   22 (7 terminals)
        //   16 (5 terminals)
        //   37 (5 terminals)
        //   0 (4 terminals)
        //   32 (2 terminals)
        //   127 (1 terminal)
        //   42 (1 terminal)
        //   48 (1 terminal)
        /// <summary>Get the value of capability "no_color_video".</summary>
        /// <returns>If not null it is the value of the capability "no_color_video". If null, this terminal information does not support the capability "no_color_video".</returns>
        public int? NoColorVideo => _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.NoColorVideo);

        // Capability name: no_correctly_working_cr
        // Terminals supporting this capability: 21 terminals
        // Values of this capability:
        //   false (21 terminals)
        /// <summary>Get the value of capability "no_correctly_working_cr".</summary>
        /// <returns>If not null it is the value of the capability "no_correctly_working_cr". If null, this terminal information does not support the capability "no_correctly_working_cr".</returns>
        public bool? NoCorrectlyWorkingCr => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.NoCorrectlyWorkingCr);

        // Capability name: no_esc_ctlc
        // Terminals supporting this capability: 320 terminals
        // Values of this capability:
        //   false (320 terminals)
        /// <summary>Get the value of capability "no_esc_ctlc".</summary>
        /// <returns>If not null it is the value of the capability "no_esc_ctlc". If null, this terminal information does not support the capability "no_esc_ctlc".</returns>
        public bool? NoEscCtlc => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.NoEscCtlc);

        // Capability name: no_pad_char
        // Terminals supporting this capability: 194 terminals
        // Values of this capability:
        //   false (117 terminals)
        //   true (77 terminals)
        /// <summary>Get the value of capability "no_pad_char".</summary>
        /// <returns>If not null it is the value of the capability "no_pad_char". If null, this terminal information does not support the capability "no_pad_char".</returns>
        public bool? NoPadChar => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.NoPadChar);

        // Capability name: non_dest_scroll_region
        // Terminals supporting this capability: 185 terminals
        // Values of this capability:
        //   false (185 terminals)
        /// <summary>Get the value of capability "non_dest_scroll_region".</summary>
        /// <returns>If not null it is the value of the capability "non_dest_scroll_region". If null, this terminal information does not support the capability "non_dest_scroll_region".</returns>
        public bool? NonDestScrollRegion => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.NonDestScrollRegion);

        // Capability name: non_rev_rmcup
        // Terminals supporting this capability: 194 terminals
        // Values of this capability:
        //   false (194 terminals)
        /// <summary>Get the value of capability "non_rev_rmcup".</summary>
        /// <returns>If not null it is the value of the capability "non_rev_rmcup". If null, this terminal information does not support the capability "non_rev_rmcup".</returns>
        public bool? NonRevRmcup => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.NonRevRmcup);

        // Capability name: norm
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b[22m" (2 terminals)
        /// <summary>Get the value of extended capability "norm".</summary>
        /// <returns>If not null it is the value of the extended capability "norm". If null, this terminal information does not support the extended capability "norm".</returns>
        public string? Norm => _database.GetStringCapabilityValue("norm");

        // Capability name: num_labels
        // Terminals supporting this capability: 46 terminals
        // Values of this capability:
        //   32 (29 terminals)
        //   8 (9 terminals)
        //   16 (8 terminals)
        /// <summary>Get the value of capability "num_labels".</summary>
        /// <returns>If not null it is the value of the capability "num_labels". If null, this terminal information does not support the capability "num_labels".</returns>
        public int? NumLabels => _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.NumLabels);

        // Capability name: opaq
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b[28m" (2 terminals)
        /// <summary>Get the value of extended capability "opaq".</summary>
        /// <returns>If not null it is the value of the extended capability "opaq". If null, this terminal information does not support the extended capability "opaq".</returns>
        public string? Opaq => _database.GetStringCapabilityValue("opaq");

        // Capability name: orig_colors
        // Terminals supporting this capability: 42 terminals
        // Values of this capability:
        //   "\u001b]104\u0007" (21 terminals)
        //   "\u001b]R" (19 terminals)
        //   "\u001b[60w\u001b[63;0w\u001b[66;1;4w\u001b[66;2;13w\u001b[66;3;16w\u001b[66;4;49w\u001b[66;5;51w\u001b[66;6;61w\u001b[66;7;64w" (1 terminal)
        //   "\u001b]R\u001b]P3FFFF80" (1 terminal)
        /// <summary>Get the value of capability "orig_colors".</summary>
        /// <returns>If not null it is the value of the capability "orig_colors". If null, this terminal information does not support the capability "orig_colors".</returns>
        public string? OrigColors => _database.GetStringCapabilityValue(TermInfoStringCapabilities.OrigColors);

        // Capability name: orig_pair
        // Terminals supporting this capability: 197 terminals
        // Values of this capability:
        //   "\u001b[39;49m" (175 terminals)
        //   "\u001b[0m" (17 terminals)
        //   "\u001b[37;40m" (2 terminals)
        //   "?39;49m" (1 terminal)
        //   "\u001b[m" (1 terminal)
        //   "\u001bG" (1 terminal)
        /// <summary>Get the value of capability "orig_pair".</summary>
        /// <returns>If not null it is the value of the capability "orig_pair". If null, this terminal information does not support the capability "orig_pair".</returns>
        public string? OrigPair => _database.GetStringCapabilityValue(TermInfoStringCapabilities.OrigPair);

        // Capability name: over_strike
        // Terminals supporting this capability: 307 terminals
        // Values of this capability:
        //   false (307 terminals)
        /// <summary>Get the value of capability "over_strike".</summary>
        /// <returns>If not null it is the value of the capability "over_strike". If null, this terminal information does not support the capability "over_strike".</returns>
        public bool? OverStrike => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.OverStrike);

        // Capability name: padding_baud_rate
        // Terminals supporting this capability: 8 terminals
        // Values of this capability:
        //   1201 (8 terminals)
        /// <summary>Get the value of capability "padding_baud_rate".</summary>
        /// <returns>If not null it is the value of the capability "padding_baud_rate". If null, this terminal information does not support the capability "padding_baud_rate".</returns>
        public int? PaddingBaudRate => _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.PaddingBaudRate);

        // Capability name: parm_dch
        // Terminals supporting this capability: 255 terminals
        // Values of this capability:
        //   "\u001b[%p1%dP" (184 terminals)
        //   "\u001b[%p1%dP$<5>" (29 terminals)
        //   "\u001b[%p1%dP$<3>" (18 terminals)
        //   "\u001b[%p1%dP$<3*>" (12 terminals)
        //   "\u001b[%p1%dP$<7>" (10 terminals)
        //   "?%p1%dP" (1 terminal)
        //   "\u001b[%p1%dP$<1*>" (1 terminal)
        /// <summary>Get the value of capability "parm_dch".</summary>
        /// <param name="n"><see cref="int"/> value that is the number of characters to delete</param>
        /// <returns>If not null it is the value of the capability "parm_dch". If null, this terminal information does not support the capability "parm_dch".</returns>
        public string? ParmDch(int n)
            => n >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.ParmDch, n)
                : throw new ArgumentOutOfRangeException(nameof(n));

        // Capability name: parm_delete_line
        // Terminals supporting this capability: 280 terminals
        // Values of this capability:
        //   "\u001b[%p1%dM" (209 terminals)
        //   "\u001b[%p1%dM$<2*>" (29 terminals)
        //   "\u001b[%p1%dM$<5>" (29 terminals)
        //   "\u001b[%p1%dM$<1*>" (8 terminals)
        //   "\u001b[%p1%dM$<3*>" (4 terminals)
        //   "?%p1%dM" (1 terminal)
        /// <summary>Get the value of capability "parm_delete_line".</summary>
        /// <param name="n"><see cref="int"/> value that is the number of lines to delete</param>
        /// <returns>If not null it is the value of the capability "parm_delete_line". If null, this terminal information does not support the capability "parm_delete_line".</returns>
        public string? ParmDeleteLine(int n)
            => n >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.ParmDeleteLine, n)
                : throw new ArgumentOutOfRangeException(nameof(n));

        // Capability name: parm_down_cursor
        // Terminals supporting this capability: 287 terminals
        // Values of this capability:
        //   "\u001b[%p1%dB" (257 terminals)
        //   "\u001b[%p1%dB$<5>" (29 terminals)
        //   "?%p1%dB" (1 terminal)
        /// <summary>Get the value of capability "parm_down_cursor".</summary>
        /// <param name="n"><see cref="int"/> value that is the number of lines to move the cursor down</param>
        /// <returns>If not null it is the value of the capability "parm_down_cursor". If null, this terminal information does not support the capability "parm_down_cursor".</returns>
        public string? ParmDownCursor(int n)
            => n >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.ParmDownCursor, n)
                : throw new ArgumentOutOfRangeException(nameof(n));

        // Capability name: parm_ich
        // Terminals supporting this capability: 234 terminals
        // Values of this capability:
        //   "\u001b[%p1%d@" (163 terminals)
        //   "\u001b[%p1%d@$<5>" (29 terminals)
        //   "\u001b[%p1%d@$<2>" (18 terminals)
        //   "\u001b[%p1%d@$<7>" (10 terminals)
        //   "\u001b[%p1%d@$<1*>" (9 terminals)
        //   "\u001b[%p1%d@$<4*>" (4 terminals)
        //   "?%p1%d@" (1 terminal)
        /// <summary>Get the value of capability "parm_ich".</summary>
        /// <param name="n"><see cref="int"/> value that is the number of characters to insert</param>
        /// <returns>If not null it is the value of the capability "parm_ich". If null, this terminal information does not support the capability "parm_ich".</returns>
        public string? ParmIch(int n)
            => n >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.ParmIch, n)
                : throw new ArgumentOutOfRangeException(nameof(n));

        // Capability name: parm_index
        // Terminals supporting this capability: 136 terminals
        // Values of this capability:
        //   "\u001b[%p1%dS" (107 terminals)
        //   "\u001b[%p1%dE$<5>" (29 terminals)
        /// <summary>Get the value of capability "parm_index".</summary>
        /// <param name="n"><see cref="int"/> value that is the number of lines to scroll forward</param>
        /// <returns>If not null it is the value of the capability "parm_index". If null, this terminal information does not support the capability "parm_index".</returns>
        public string? ParmIndex(int n)
            => n >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.ParmIndex, n)
                : throw new ArgumentOutOfRangeException(nameof(n));

        // Capability name: parm_insert_line
        // Terminals supporting this capability: 280 terminals
        // Values of this capability:
        //   "\u001b[%p1%dL" (209 terminals)
        //   "\u001b[%p1%dL$<5>" (29 terminals)
        //   "\u001b[%p1%dL$<3*>" (28 terminals)
        //   "\u001b[%p1%dL$<2*>" (9 terminals)
        //   "\u001b[%p1%dL$<5*>" (4 terminals)
        //   "?%p1%dL" (1 terminal)
        /// <summary>Get the value of capability "parm_insert_line".</summary>
        /// <param name="n"><see cref="int"/> value that is the number of lines to insert</param>
        /// <returns>If not null it is the value of the capability "parm_insert_line". If null, this terminal information does not support the capability "parm_insert_line".</returns>
        public string? ParmInsertLine(int n)
            => n >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.ParmInsertLine, n)
                : throw new ArgumentOutOfRangeException(nameof(n));

        // Capability name: parm_left_cursor
        // Terminals supporting this capability: 287 terminals
        // Values of this capability:
        //   "\u001b[%p1%dD" (257 terminals)
        //   "\u001b[%p1%dD$<5>" (29 terminals)
        //   "?%p1%dD" (1 terminal)
        /// <summary>Get the value of capability "parm_left_cursor".</summary>
        /// <param name="n"><see cref="int"/> value that is the number of columns to move the cursor left</param>
        /// <returns>If not null it is the value of the capability "parm_left_cursor". If null, this terminal information does not support the capability "parm_left_cursor".</returns>
        public string? ParmLeftCursor(int n)
            => n >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.ParmLeftCursor, n)
                : throw new ArgumentOutOfRangeException(nameof(n));

        // Capability name: parm_right_cursor
        // Terminals supporting this capability: 287 terminals
        // Values of this capability:
        //   "\u001b[%p1%dC" (257 terminals)
        //   "\u001b[%p1%dC$<5>" (29 terminals)
        //   "?%p1%dC" (1 terminal)
        /// <summary>Get the value of capability "parm_right_cursor".</summary>
        /// <param name="n"><see cref="int"/> value that is the number of columns to move the cursor to the right</param>
        /// <returns>If not null it is the value of the capability "parm_right_cursor". If null, this terminal information does not support the capability "parm_right_cursor".</returns>
        public string? ParmRightCursor(int n)
            => n >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.ParmRightCursor, n)
                : throw new ArgumentOutOfRangeException(nameof(n));

        // Capability name: parm_rindex
        // Terminals supporting this capability: 103 terminals
        // Values of this capability:
        //   "\u001b[%p1%dT" (103 terminals)
        /// <summary>Get the value of capability "parm_rindex".</summary>
        /// <param name="n"><see cref="int"/> value that is the number of lines to scroll backwards</param>
        /// <returns>If not null it is the value of the capability "parm_rindex". If null, this terminal information does not support the capability "parm_rindex".</returns>
        public string? ParmRindex(int n)
            => n >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.ParmRindex, n)
                : throw new ArgumentOutOfRangeException(nameof(n));

        // Capability name: parm_up_cursor
        // Terminals supporting this capability: 287 terminals
        // Values of this capability:
        //   "\u001b[%p1%dA" (257 terminals)
        //   "\u001b[%p1%dA$<5>" (29 terminals)
        //   "?%p1%dA" (1 terminal)
        /// <summary>Get the value of capability "parm_up_cursor".</summary>
        /// <param name="n"><see cref="int"/> value that is the number of lines to move the cursor up</param>
        /// <returns>If not null it is the value of the capability "parm_up_cursor". If null, this terminal information does not support the capability "parm_up_cursor".</returns>
        public string? ParmUpCursor(int n)
            => n >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.ParmUpCursor, n)
                : throw new ArgumentOutOfRangeException(nameof(n));

        // Capability name: pkey_local
        // Terminals supporting this capability: 10 terminals
        // Values of this capability:
        //   "\u001bZ2%p1%'?'%+%c%p2%s\u007f" (10 terminals)
        /// <summary>Get the value of capability "pkey_local".</summary>
        /// <param name="pKey"><see cref="int"/> value that is the program function key number</param>
        /// <param name="executeString"><see cref="string"/> value that is the execution string to set to the function key</param>
        /// <returns>If not null it is the value of the capability "pkey_local". If null, this terminal information does not support the capability "pkey_local".</returns>
        public string? PkeyLocal(int pKey, string executeString)
            => pKey >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.PkeyLocal, pKey, executeString)
                : throw new ArgumentOutOfRangeException(nameof(pKey));

        // Capability name: pkey_xmit
        // Terminals supporting this capability: 17 terminals
        // Values of this capability:
        //   "\u001bZ1%p1%'?'%+%c%p2%s\u007f" (10 terminals)
        //   "\u001bz%p1%'?'%+%c%p2%s\u007f" (7 terminals)
        /// <summary>Get the value of capability "pkey_xmit".</summary>
        /// <param name="pKey"><see cref="int"/> value that is the program function key number</param>
        /// <param name="transmitString"><see cref="string"/> value that is the transmit string to set to the function key</param>
        /// <returns>If not null it is the value of the capability "pkey_xmit". If null, this terminal information does not support the capability "pkey_xmit".</returns>
        public string? PkeyXmit(int pKey, int transmitString)
            => pKey >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.PkeyXmit, pKey, transmitString)
                : throw new ArgumentOutOfRangeException(nameof(pKey));

        // Capability name: plab_norm
        // Terminals supporting this capability: 17 terminals
        // Values of this capability:
        //   "\u001bz%p1%'/'%+%c%p2%s\r" (17 terminals)
        /// <summary>Get the value of capability "plab_norm".</summary>
        /// <param name="labelNumber"><see cref="int"/> value that is the number of the label</param>
        /// <param name="labelString"><see cref="string"/> value that is the string to set to the label</param>
        /// <returns>If not null it is the value of the capability "plab_norm". If null, this terminal information does not support the capability "plab_norm".</returns>
        public string? PlabNorm(int labelNumber, string labelString)
            => labelNumber >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.PlabNorm, labelNumber, labelString)
                : throw new ArgumentOutOfRangeException(nameof(labelNumber));

        // Capability name: print_screen
        // Terminals supporting this capability: 160 terminals
        // Values of this capability:
        //   "\u001b[i" (88 terminals)
        //   "\u001b[0i" (52 terminals)
        //   "\u001bP" (19 terminals)
        //   "?i" (1 terminal)
        /// <summary>Get the value of capability "print_screen".</summary>
        /// <returns>If not null it is the value of the capability "print_screen". If null, this terminal information does not support the capability "print_screen".</returns>
        public string? PrintScreen => _database.GetStringCapabilityValue(TermInfoStringCapabilities.PrintScreen);

        // Capability name: prtr_off
        // Terminals supporting this capability: 168 terminals
        // Values of this capability:
        //   "\u001b[4i" (132 terminals)
        //   "\u0014" (17 terminals)
        //   "\u001b[?4i" (10 terminals)
        //   "\u001ba" (8 terminals)
        //   "?4i" (1 terminal)
        /// <summary>Get the value of capability "prtr_off".</summary>
        /// <returns>If not null it is the value of the capability "prtr_off". If null, this terminal information does not support the capability "prtr_off".</returns>
        public string? PrtrOff => _database.GetStringCapabilityValue(TermInfoStringCapabilities.PrtrOff);

        // Capability name: prtr_on
        // Terminals supporting this capability: 168 terminals
        // Values of this capability:
        //   "\u001b[5i" (132 terminals)
        //   "\u001b[?5i" (10 terminals)
        //   "\u001bd#" (10 terminals)
        //   "\u001b`" (8 terminals)
        //   "\u0018" (7 terminals)
        //   "?5i" (1 terminal)
        /// <summary>Get the value of capability "prtr_on".</summary>
        /// <returns>If not null it is the value of the capability "prtr_on". If null, this terminal information does not support the capability "prtr_on".</returns>
        public string? PrtrOn => _database.GetStringCapabilityValue(TermInfoStringCapabilities.PrtrOn);

        // Capability name: prtr_silent
        // Terminals supporting this capability: 252 terminals
        // Values of this capability:
        //   true (129 terminals)
        //   false (123 terminals)
        /// <summary>Get the value of capability "prtr_silent".</summary>
        /// <returns>If not null it is the value of the capability "prtr_silent". If null, this terminal information does not support the capability "prtr_silent".</returns>
        public bool? PrtrSilent => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.PrtrSilent);

        // Capability name: remove_clock
        // Terminals supporting this capability: 11 terminals
        // Values of this capability:
        //   "\u001b`c" (10 terminals)
        //   "\u001b[31l" (1 terminal)
        /// <summary>Get the value of capability "remove_clock".</summary>
        /// <returns>If not null it is the value of the capability "remove_clock". If null, this terminal information does not support the capability "remove_clock".</returns>
        public string? RemoveClock => _database.GetStringCapabilityValue(TermInfoStringCapabilities.RemoveClock);

        // Capability name: repeat_char
        // Terminals supporting this capability: 18 terminals
        // Values of this capability:
        //   "%p1%c\u001b[%p2%{1}%-%db" (17 terminals)
        //   "%p1%c\u0012%p2%'?'%+%c" (1 terminal)
        /// <summary>Get the value of capability "repeat_char".</summary>
        /// <param name="c"><see cref="char"/> value that is the character to display repeatedly.</param>
        /// <param name="n"><see cref="int"/> value that is the number of repetitions.</param>
        /// <returns>If not null it is the value of the capability "repeat_char". If null, this terminal information does not support the capability "repeat_char".</returns>
        public string? RepeatChar(char c, int n)
            => c.IsOutOfClosedInterval('\0', '\u007f')
                ? throw new ArgumentOutOfRangeException(nameof(c))
                : n < 0
                ? throw new ArgumentOutOfRangeException(nameof(n))
                : _database.GetStringCapabilityValue(TermInfoStringCapabilities.RepeatChar, c, n);

        // Capability name: reset_1string
        // Terminals supporting this capability: 179 terminals
        // Values of this capability:
        //   "\u001bc" (82 terminals)
        //   "\u001b[13l\u001b[3l\u001b\\\u001b[63;1\"p\u001b[!p" (28 terminals)
        //   "\u001b>\u001b[1;3;4;5;6l\u001b[?7h\u001b[m\u001b[r\u001b[2J\u001b[H" (18 terminals)
        //   "\u001bc\u001b]R" (13 terminals)
        //   "\u001b[13l\u001b[3l\u001b!p" (12 terminals)
        //   "\u001b~!\u001b~4$<150>" (10 terminals)
        //   "\u001bc\u001b]104\u0007" (8 terminals)
        //   "\u000f" (2 terminals)
        //   "\u001bDF\u001bC\u001bg\u001br\u001bO\u001b'\u001b(\u001bw\u001bX\u001be \u000f\u001b0P\u001b6?\u001b0p\u001b4?\u001bf\r" (2 terminals)
        //   "\u001bM" (2 terminals)
        //   "\u001b[13l\u001b[3l\u001b!p\u001b[?4i" (1 terminal)
        //   "\u001bc\u001b>\u001b[?3l\u001b[?4l\u001b[?5l\u001b[?7h\u001b[?8h" (1 terminal)
        /// <summary>Get the value of capability "reset_1string".</summary>
        /// <returns>If not null it is the value of the capability "reset_1string". If null, this terminal information does not support the capability "reset_1string".</returns>
        public string? Reset_1string => _database.GetStringCapabilityValue(TermInfoStringCapabilities.Reset_1string);

        // Capability name: reset_2string
        // Terminals supporting this capability: 273 terminals
        // Values of this capability:
        //   "\u001b[!p\u001b[?3;4l\u001b[4l\u001b>" (40 terminals)
        //   "\u001b>\u001b[?3l\u001b[?4l\u001b[?5l\u001b[?7h\u001b[?8h" (27 terminals)
        //   "\u001bc\u001b[?1000l\u001b[?25h" (21 terminals)
        //   "\u001b7\u001b[r\u001b8\u001b[m\u001b[?7h\u001b[!p\u001b[?1;3;4;6l\u001b[4l\u001b>\u001b[?1000l\u001b[?25h" (17 terminals)
        //   "\u001b[r\u001b[m\u001b[2J\u001b[H\u001b[?7h\u001b[?1;3;4;6l\u001b[4l\u001b=\u001b[?1000l\u001b[?25h" (11 terminals)
        //   "\u001b<\u001b>\u001b[?3;4;5l\u001b[?7;8h\u001b[r" (11 terminals)
        //   "\u001b[35h\u001b[?3h" (10 terminals)
        //   "\u001b[35h\u001b[?3l" (10 terminals)
        //   "\u001b7\u001b[r\u001b8\u001b[m\u001b[?7h\u001b[?1;3;4;6l\u001b[4l\u001b>\u001b[?1000l\u001b[?25h" (10 terminals)
        //   "\u001b[!p\u001b[?7;19;67h\u001b[?1;3;4l\u001b(B\u001b)0\u000f\u001b[2J\u001b[1;1H\u001b>$<200>" (8 terminals)
        //   "\u001b[?3h" (8 terminals)
        //   "\u001b[r\u001b[m\u001b[2J\u001b[H\u001b[?7h\u001b[?1;3;4;6l\u001b[4l\u001b>\u001b[?1000l\u001b[?25h" (8 terminals)
        //   "\u001b<\u001b[\"p\u001b[50;6\"p\u001bc\u001b[?3l\u001b]R\u001b[?1000l" (7 terminals)
        //   "\u001b7\u001b[r\u001b8\u001b[m\u001b[?7h\u001b[?1;3;4;6l\u001b[4l\u001b>\u001b[?1000l" (7 terminals)
        //   "\u001b[!p\u001b[?3;7;19;67h\u001b[?1;4l\u001b(B\u001b)0\u000f\u001b[2J\u001b[1;1H\u001b>$<200>" (6 terminals)
        //   "\u001b[!p\u001b[?7;19;67h\u001b[?1;3;4l\u001b[1;0%w\u001b(B\u001b)0\u000f\u001b[2J\u001b[1;1H\u001b>$<200>" (6 terminals)
        //   "\u001b[35h\u001b[?3l$<80>" (6 terminals)
        //   "\u001b[4l\u001b>\u001b[?1034l" (6 terminals)
        //   "\u001b[!p\u001b[?3;7;19;67h\u001b[?1;4l\u001b[1;0%w\u001b(B\u001b)0\u000f\u001b[2J\u001b[1;1H\u001b>$<200>" (4 terminals)
        //   "\u001b[r\u001b[m\u001b[?7;25h\u001b[?1;3;4;5;6;9;66;1000;1001;1049l\u001b[4l" (4 terminals)
        //   "\u001b7\u001b[r\u001b8\u001b[m\u001b[?7h\u001b[?1;4;6l\u001b[4l\u001b>" (4 terminals)
        //   "\u001beF$<150>\u001b`;$<150>" (4 terminals)
        //   "\u001beG$<150>" (4 terminals)
        //   "\u001b[!p\u001b[?3;7;19;67h\u001b[?4l\u001b[1;0%w\u001b(B\u001b)0\u000f\u001b[2J\u001b[1;1H$<200>" (3 terminals)
        //   "\u001b[m\u001b[?7h\u001b[4l\u001b>\u001b7\u001b[r\u001b[?1;3;4;6l\u001b8" (3 terminals)
        //   "\u001b[!p\u001b[?3;7;19;67h\u001b[?4l\u001b(B\u001b)0\u000f\u001b[2J\u001b[1;1H$<200>" (2 terminals)
        //   "\u001b[35h\u001b[?3h$<80>" (2 terminals)
        //   "\u001b[35h\u001b[?3l$<70>" (2 terminals)
        //   "\u001b[35h$<70/>\u001b[?3h" (2 terminals)
        //   "\u001b`;$<150>" (2 terminals)
        //   "\u001b>\u001b[?3h\u001b[?4l\u001b[?5l\u001b[?7h\u001b[?8h\u001b[1;24r\u001b[24;1H" (2 terminals)
        //   "\u001b>\u001b[?3h\u001b[?4l\u001b[?5l\u001b[?7l\u001b[?8h\u001b[1;24r\u001b[24;1H" (2 terminals)
        //   "\u001b>\u001b[?3l\u001b[?4l\u001b[?5l\u001b[?7h\u001b[?8h\u001b[1;24r\u001b[24;1H" (2 terminals)
        //   "\u001b>\u001b[?3l\u001b[?4l\u001b[?5l\u001b[?7l\u001b[?8h\u001b[1;24r\u001b[24;1H" (2 terminals)
        //   "\u001b7\u001b[r\u001b8\u001b[m\u001b[?7h\u001b[?1;3;4;6l\u001b[4l\u001b>" (2 terminals)
        //   "\u001e\u001b[12H\u001b[2M\u001b[H\u001b[L\u001b[12H\u001b[2M\u001b[H\u001b[L\u001b[12H\u001b[2M\u001b[H\u001b[L\u001b[12H\u001b[2M\u001b[H\u001b[L\u001b[12H\u001b[2M\u001b[H\u001b[L\u001b[12H\u001b[2M\u001b[H\u001b[L\u001b[12H\u001b[2M\u001b[H\u001b[L\u001b[12H\u001b[2M\u001b[H\u001b[L\u001b[12H\u001b[2M\u001b[H\u001b[L\u001b[12H\u001b[2M\u001b[H\u001b[L\u001b[12H\u001b[2M\u001b[H\u001b[L\u001b[12H\u001b[2M\u001b[H\u001b[J\u001b[m" (2 terminals)
        //   "\u0014\u001fXA\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\n\u0018\f\u0011" (1 terminal)
        //   "\u001b[!p\u001b[?3;4l\u001b[4l\u001b>\u001b[?1000l" (1 terminal)
        //   "\u001b[35h\u001b[?3l$<8>" (1 terminal)
        //   "\u001b[62\"p\u001b G?m??7h\u001b>\u001b7??1;3;4;6l?4l?r\u001b8" (1 terminal)
        //   "\u001b>\u001b[?3h\u001b[?4l\u001b[?5l\u001b[?7h\u001b[?8h\u001b[1;42r\u001b[42;1H" (1 terminal)
        //   "\u001b>\u001b[?3h\u001b[?4l\u001b[?5l\u001b[?7l\u001b[?8h\u001b[1;42r\u001b[42;1H" (1 terminal)
        /// <summary>Get the value of capability "reset_2string".</summary>
        /// <returns>If not null it is the value of the capability "reset_2string". If null, this terminal information does not support the capability "reset_2string".</returns>
        public string? Reset_2string => _database.GetStringCapabilityValue(TermInfoStringCapabilities.Reset_2string);

        // Capability name: reset_3string
        // Terminals supporting this capability: 53 terminals
        // Values of this capability:
        //   "\u001b[?5l\u001b[47h\u001b[40l\u001b[r" (20 terminals)
        //   "\u001b[?5l" (13 terminals)
        //   "\u001bwG\u001be($<200>" (6 terminals)
        //   "\u001b[?5l\u001b[36*|\u001b[36t\u001b[40l\u001b[1;36r\u001b[132$|" (4 terminals)
        //   "\u001b[?5l\u001b[48*|\u001b[48t\u001b[40l\u001b[1;48r\u001b[132$|" (4 terminals)
        //   "\u001be*$<150>" (4 terminals)
        //   "\u001b[?67h\u001b[64;1\"p" (1 terminal)
        //   "\u001b[37;40m\u001b[8]" (1 terminal)
        /// <summary>Get the value of capability "reset_3string".</summary>
        /// <returns>If not null it is the value of the capability "reset_3string". If null, this terminal information does not support the capability "reset_3string".</returns>
        public string? Reset_3string => _database.GetStringCapabilityValue(TermInfoStringCapabilities.Reset_3string);

        // Capability name: reset_file
        // Terminals supporting this capability: 16 terminals
        // Values of this capability:
        //   "/usr/share/tabset/vt300" (11 terminals)
        //   "/usr/share/tabset/vt100" (5 terminals)
        /// <summary>Get the value of capability "reset_file".</summary>
        /// <returns>If not null it is the value of the capability "reset_file". If null, this terminal information does not support the capability "reset_file".</returns>
        public string? ResetFile => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ResetFile);

        // Capability name: restore_cursor
        // Terminals supporting this capability: 283 terminals
        // Values of this capability:
        //   "\u001b8" (283 terminals)
        /// <summary>Get the value of capability "restore_cursor".</summary>
        /// <returns>If not null it is the value of the capability "restore_cursor". If null, this terminal information does not support the capability "restore_cursor".</returns>
        public string? RestoreCursor => _database.GetStringCapabilityValue(TermInfoStringCapabilities.RestoreCursor);

        // Capability name: RGB
        // Terminals supporting this capability: 12 terminals
        // Values of this capability:
        //   true (12 terminals)
        /// <summary>Get the value of extended capability "RGB".</summary>
        /// <returns>If not null it is the value of the extended capability "RGB". If null, this terminal information does not support the extended capability "RGB".</returns>
        public bool? RGB => _database.GetBooleanCapabilityValue("RGB");

        // Capability name: Rmol
        // Terminals supporting this capability: 8 terminals
        // Values of this capability:
        //   "\u001b[55m" (8 terminals)
        /// <summary>Get the value of extended capability "Rmol".</summary>
        /// <returns>If not null it is the value of the extended capability "Rmol". If null, this terminal information does not support the extended capability "Rmol".</returns>
        public string? Rmol => _database.GetStringCapabilityValue("Rmol");

        // Capability name: rmxx
        // Terminals supporting this capability: 57 terminals
        // Values of this capability:
        //   "\u001b[29m" (57 terminals)
        /// <summary>Get the value of extended capability "rmxx".</summary>
        /// <returns>If not null it is the value of the extended capability "rmxx". If null, this terminal information does not support the extended capability "rmxx".</returns>
        public string? Rmxx => _database.GetStringCapabilityValue("rmxx");

        // Capability name: row_addr_glitch
        // Terminals supporting this capability: 133 terminals
        // Values of this capability:
        //   false (133 terminals)
        /// <summary>Get the value of capability "row_addr_glitch".</summary>
        /// <returns>If not null it is the value of the capability "row_addr_glitch". If null, this terminal information does not support the capability "row_addr_glitch".</returns>
        public bool? RowAddrGlitch => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.RowAddrGlitch);

        // Capability name: row_address
        // Terminals supporting this capability: 233 terminals
        // Values of this capability:
        //   "\u001b[%i%p1%dd" (202 terminals)
        //   "\u001b[%p1%dd$<40>" (29 terminals)
        //   "?%i%p1%dd" (1 terminal)
        //   "\u001b[%p1%{1}%+%dd" (1 terminal)
        /// <summary>Get the value of capability "row_address".</summary>
        /// <param name="row"><see cref="int"/> value that is the row number to move to</param>
        /// <returns>If not null it is the value of the capability "row_address". If null, this terminal information does not support the capability "row_address".</returns>
        public string? RowAddress(int row)
            => row >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.RowAddress, row)
                : throw new ArgumentOutOfRangeException(nameof(row));

        // Capability name: S0
        // Terminals supporting this capability: 24 terminals
        // Values of this capability:
        //   "\u001b(%p1%c" (21 terminals)
        //   "\u000e" (1 terminal)
        //   "\u001b)0\u000e" (1 terminal)
        //   "\u001b)3\u000e" (1 terminal)
        /// <summary>Get the value of extended capability "S0".</summary>
        /// <param name="charSet"><see cref="char"/> value representing the character set to switch to</param>
        /// <returns>If not null it is the value of the extended capability "S0". If null, this terminal information does not support the extended capability "S0".</returns>
        /// <remarks>
        /// This capability is a string value that switches fonts according to the ISO-2022 font selection sequence.
        /// (See "man screen(1)")
        /// </remarks>
        public string? S0(char charSet)
            => charSet.IsOutOfClosedInterval('\0', '\u007f')
                ? _database.GetStringCapabilityValue("S0", charSet)
                : throw new ArgumentOutOfRangeException(nameof(charSet));

        // Capability name: save_cursor
        // Terminals supporting this capability: 283 terminals
        // Values of this capability:
        //   "\u001b7" (283 terminals)
        /// <summary>Get the value of capability "save_cursor".</summary>
        /// <returns>If not null it is the value of the capability "save_cursor". If null, this terminal information does not support the capability "save_cursor".</returns>
        public string? SaveCursor => _database.GetStringCapabilityValue(TermInfoStringCapabilities.SaveCursor);

        // Capability name: scroll_forward
        // Terminals supporting this capability: 315 terminals
        // Values of this capability:
        //   "\n" (207 terminals)
        //   "\n$<2>" (44 terminals)
        //   "\u001bD$<5>" (29 terminals)
        //   "\u001bD" (14 terminals)
        //   "\n$<150*>" (4 terminals)
        //   "\n$<3>" (4 terminals)
        //   "\n$<5>" (4 terminals)
        //   "\n$<9>" (4 terminals)
        //   "\u001b[S" (3 terminals)
        //   "\n$<4>" (2 terminals)
        /// <summary>Get the value of capability "scroll_forward".</summary>
        /// <returns>If not null it is the value of the capability "scroll_forward". If null, this terminal information does not support the capability "scroll_forward".</returns>
        public string? ScrollForward => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ScrollForward);

        // Capability name: scroll_reverse
        // Terminals supporting this capability: 312 terminals
        // Values of this capability:
        //   "\u001bM" (204 terminals)
        //   "\u001bM$<2>" (37 terminals)
        //   "\u001bM$<5>" (37 terminals)
        //   "\u001bj" (15 terminals)
        //   "\u001bj$<10>" (4 terminals)
        //   "\u001bj$<7>" (4 terminals)
        //   "\u001bM$<3>" (4 terminals)
        //   "\u001b[T" (3 terminals)
        //   "\u001bj$<3>" (2 terminals)
        //   "?" (1 terminal)
        //   "\u000b" (1 terminal)
        /// <summary>Get the value of capability "scroll_reverse".</summary>
        /// <returns>If not null it is the value of the capability "scroll_reverse". If null, this terminal information does not support the capability "scroll_reverse".</returns>
        public string? ScrollReverse => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ScrollReverse);

        // Capability name: Se
        // Terminals supporting this capability: 38 terminals
        // Values of this capability:
        //   "\u001b[2 q" (35 terminals)
        //   "\u001b[0 q" (3 terminals)
        /// <summary>Get the value of extended capability "Se".</summary>
        /// <returns>If not null it is the value of the extended capability "Se". If null, this terminal information does not support the extended capability "Se".</returns>
        /// <remarks>
        /// This capability is a string value for code that initializes cursor styles.
        /// </remarks>
        public string? Se => _database.GetStringCapabilityValue("Se");

        // Capability name: select_char_set
        // Terminals supporting this capability: 1 terminal
        // Values of this capability:
        //   "%?%p1%{1}%=%t\u001b(B%e%p1%{2}%=%t\u001b(A%e%p1%{3}%=%t\u001b(R%e%p1%{4}%=%t\u001b(9%e%p1%{5}%=%t\u001b(E%e%p1%{6}%=%t\u001b(5%e%p1%{7}%=%t\u001b(K%e%p1%{8}%=%t\u001b(4%e%p1%{9}%=%t\u001b(Y%e%p1%{10}%=%t\u001b(=%e%p1%{11}%=%t\u001b(=%e%p1%{12}%=%t\u001b(7%e%p1%{13}%=%t\u001b(E%e%p1%{14}%=%t\u001b(R%e%p1%{15}%=%t\u001b(Z%;" (1 terminal)
        /// <summary>Get the value of capability "select_char_set".</summary>
        /// <param name="charSet"><see cref="int"/> value that is the number of the character set</param>
        /// <returns>If not null it is the value of the capability "select_char_set". If null, this terminal information does not support the capability "select_char_set".</returns>
        public string? SelectCharSet(int charSet)
            => charSet >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.SelectCharSet, charSet)
                : throw new ArgumentOutOfRangeException(nameof(charSet));

        // Capability name: semi_auto_right_margin
        // Terminals supporting this capability: 133 terminals
        // Values of this capability:
        //   false (133 terminals)
        /// <summary>Get the value of capability "semi_auto_right_margin".</summary>
        /// <returns>If not null it is the value of the capability "semi_auto_right_margin". If null, this terminal information does not support the capability "semi_auto_right_margin".</returns>
        public bool? SemiAutoRightMargin => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.SemiAutoRightMargin);

        // Capability name: set_a_background
        // Terminals supporting this capability: 196 terminals
        // Values of this capability:
        //   "\u001b[4%p1%dm" (118 terminals)
        //   "\u001b[%?%p1%{8}%<%t4%p1%d%e%p1%{16}%<%t10%p1%{8}%-%d%e48;5;%p1%d%;m" (43 terminals)
        //   "\u001b[%?%p1%{8}%<%t%p1%'('%+%e%p1%{92}%+%;%dm" (16 terminals)
        //   "\u001b[%?%p1%{8}%<%t4%p1%d%e48;2;%p1%{65536}%/%d;%p1%{256}%/%{255}%&%d;%p1%{255}%&%d%;m" (6 terminals)
        //   "\u001b[%?%p1%{8}%<%t4%p1%d%e48:2::%p1%{65536}%/%d:%p1%{256}%/%{255}%&%d:%p1%{255}%&%d%;m" (4 terminals)
        //   "\u001b[48;5;%p1%dm" (4 terminals)
        //   "\u001b[%?%p1%{8}%<%t4%p1%d%e48:2:%p1%{65536}%/%d:%p1%{256}%/%{255}%&%d:%p1%{255}%&%d%;m" (2 terminals)
        //   "?" (1 terminal)
        //   "?4%p1%dm" (1 terminal)
        //   "\u001b[4%p1%{8}%m%d%?%p1%{7}%>%t;5%e;25%;m" (1 terminal)
        /// <summary>Get the value of capability "set_a_background".</summary>
        /// <param name="color"><see cref="Color16"/> value that is 16 color code.</param>
        /// <returns>If not null it is the value of the capability "set_a_background". If null, this terminal information does not support the capability "set_a_background".</returns>
        public string? SetABackground(Color16 color)
        {
            var maxColors = _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.MaxColors);
            if (maxColors is not null && maxColors.Value.IsNoneOf(16, 88, 256))
                throw new InvalidOperationException("This terminal does not support 16 colors.");

            return _database.GetStringCapabilityValue(TermInfoStringCapabilities.SetABackground, (int)color);
        }

        /// <summary>Get the value of capability "set_a_background".</summary>
        /// <param name="color"><see cref="Color88"/> value that is an 88 color code.</param>
        /// <returns>If not null it is the value of the capability "set_a_background". If null, this terminal information does not support the capability "set_a_background".</returns>
        public string? SetABackground(Color88 color)
        {
            var maxColors = _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.MaxColors);
            if (maxColors is not null && maxColors.Value != 88)
                throw new InvalidOperationException("This terminal does not support 88 colors.");

            return _database.GetStringCapabilityValue(TermInfoStringCapabilities.SetABackground, (int)color);
        }

        /// <summary>Get the value of capability "set_a_background".</summary>
        /// <param name="color"><see cref="Color256"/> value that is an 256 color code.</param>
        /// <returns>If not null it is the value of the capability "set_a_background". If null, this terminal information does not support the capability "set_a_background".</returns>
        public string? SetABackground(Color256 color)
        {
            var maxColors = _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.MaxColors);
            if (maxColors is not null && maxColors.Value != 256)
                throw new InvalidOperationException("This terminal does not support 256 colors.");

            return _database.GetStringCapabilityValue(TermInfoStringCapabilities.SetABackground, (int)color);
        }

        /// <summary>Get the value of capability "set_a_background".</summary>
        /// <param name="color"><see cref="TrueColor"/> value that is an true color code.</param>
        /// <returns>If not null it is the value of the capability "set_a_background". If null, this terminal information does not support the capability "set_a_background".</returns>
        public string? SetABackground(TrueColor color)
        {
            var maxColors = _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.MaxColors);
            const int trueColors = 1 << 24;
            if (maxColors is not null && maxColors.Value != trueColors)
                throw new InvalidOperationException("This terminal does not support true color.");

            var colorCode = (int)color;
            if (colorCode < 8)
            {
                // Some terminals allow the capability "set_a_background" to specify true color.
                // However, in that case, #000000-#000007 means SGR color code (Black, Red, Green, Yellow, Blue, Magenta, Cyan, White) instead of RGB. (Oh my God!)
                // So if the specified color code is #000000-#000007, secretly change it to another similar color.
                var (r, g, b) = color.ToRgb();
                colorCode = (int)TrueColor.FromRgb((byte)(r + 1), (byte)(g + 1), (byte)(b + 1));
            }

            return _database.GetStringCapabilityValue(TermInfoStringCapabilities.SetABackground, colorCode);
        }

        // Capability name: set_a_foreground
        // Terminals supporting this capability: 196 terminals
        // Values of this capability:
        //   "\u001b[3%p1%dm" (118 terminals)
        //   "\u001b[%?%p1%{8}%<%t3%p1%d%e%p1%{16}%<%t9%p1%{8}%-%d%e38;5;%p1%d%;m" (43 terminals)
        //   "\u001b[%?%p1%{8}%<%t%p1%{30}%+%e%p1%'R'%+%;%dm" (16 terminals)
        //   "\u001b[%?%p1%{8}%<%t3%p1%d%e38;2;%p1%{65536}%/%d;%p1%{256}%/%{255}%&%d;%p1%{255}%&%d%;m" (6 terminals)
        //   "\u001b[%?%p1%{8}%<%t3%p1%d%e38:2::%p1%{65536}%/%d:%p1%{256}%/%{255}%&%d:%p1%{255}%&%d%;m" (4 terminals)
        //   "\u001b[38;5;%p1%dm" (4 terminals)
        //   "\u001b[%?%p1%{8}%<%t3%p1%d%e38:2:%p1%{65536}%/%d:%p1%{256}%/%{255}%&%d:%p1%{255}%&%d%;m" (2 terminals)
        //   "?3%p1%dm" (1 terminal)
        //   "\u001b[3%p1%{8}%m%d%?%p1%{7}%>%t;1%e;22%;m" (1 terminal)
        //   "\u001b%p1%'@'%+%c" (1 terminal)
        /// <summary>Get the value of capability "set_a_foreground".</summary>
        /// <param name="color"><see cref="Color16"/> value that is an ANSI 4-bit color code.</param>
        /// <returns>If not null it is the value of the capability "set_a_foreground". If null, this terminal information does not support the capability "set_a_foreground".</returns>
        public string? SetAForeground(Color16 color)
        {
            var maxColors = _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.MaxColors);
            if (maxColors is not null && maxColors.Value.IsNoneOf(16, 88, 256))
                throw new InvalidOperationException("This terminal does not support 16 colors.");

            return _database.GetStringCapabilityValue(TermInfoStringCapabilities.SetAForeground, (int)color);
        }

        /// <summary>Get the value of capability "set_a_background".</summary>
        /// <param name="color"><see cref="Color88"/> value that is an 88 color code.</param>
        /// <returns>If not null it is the value of the capability "set_a_background". If null, this terminal information does not support the capability "set_a_background".</returns>
        public string? SetAForeground(Color88 color)
        {
            var maxColors = _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.MaxColors);
            if (maxColors is not null && maxColors.Value != 88)
                throw new InvalidOperationException("This terminal does not support 88 colors.");

            return _database.GetStringCapabilityValue(TermInfoStringCapabilities.SetAForeground, (int)color);
        }

        /// <summary>Get the value of capability "set_a_background".</summary>
        /// <param name="color"><see cref="Color256"/> value that is an 256 color code.</param>
        /// <returns>If not null it is the value of the capability "set_a_background". If null, this terminal information does not support the capability "set_a_background".</returns>
        public string? SetAForeground(Color256 color)
        {
            var maxColors = _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.MaxColors);
            if (maxColors is not null && maxColors.Value != 256)
                throw new InvalidOperationException("This terminal does not support 256 colors.");

            return _database.GetStringCapabilityValue(TermInfoStringCapabilities.SetAForeground, (int)color);
        }

        /// <summary>Get the value of capability "set_a_background".</summary>
        /// <param name="color"><see cref="TrueColor"/> value that is an trur color code.</param>
        /// <returns>If not null it is the value of the capability "set_a_background". If null, this terminal information does not support the capability "set_a_background".</returns>
        public string? SetAForeground(TrueColor color)
        {
            var maxColors = _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.MaxColors);
            const int trueColors = 1 << 24;
            if (maxColors is not null && maxColors.Value != trueColors)
                throw new InvalidOperationException("This terminal does not support true color.");

            var colorCode = (int)color;
            if (colorCode < 8)
            {
                // Some terminals allow the capability "set_a_foreground" to specify true color.
                // However, in that case, #000000-#000007 means SGR color code (Black, Red, Green, Yellow, Blue, Magenta, Cyan, White) instead of RGB. (Oh my God!)
                // So if the specified color code is #000000-#000007, secretly change it to another similar color.
                var (r, g, b) = color.ToRgb();
                colorCode = (int)TrueColor.FromRgb((byte)(r + 1), (byte)(g + 1), (byte)(b + 1));
            }

            return _database.GetStringCapabilityValue(TermInfoStringCapabilities.SetAForeground, colorCode);
        }

        // Capability name: set_attributes
        // Terminals supporting this capability: 298 terminals
        // Values of this capability:
        //   "%?%p9%t\u001b(0%e\u001b(B%;\u001b[0%?%p6%t;1%;%?%p5%t;2%;%?%p2%t;4%;%?%p1%p3%|%t;7%;%?%p4%t;5%;%?%p7%t;8%;m" (35 terminals)
        //   "\u001b[0%?%p6%t;1%;%?%p2%t;4%;%?%p1%p3%|%t;7%;%?%p4%t;5%;%?%p5%t;2%;%?%p7%t;8%;m%?%p9%t\u000e%e\u000f%;" (34 terminals)
        //   "\u001b[0%?%p2%t;4%;%?%p3%p1%|%t;7%;%?%p4%t;5%;%?%p5%t;2%;%?%p6%t;1%;%?%p7%t;8%;m%?%p9%t\u000e%e\u000f%;" (33 terminals)
        //   "\u001b[0%?%p1%p6%|%t;1%;%?%p2%t;4%;%?%p1%p3%|%t;7%;%?%p4%t;5%;%?%p7%t;8%;m%?%p9%t\u000e%e\u000f%;$<20>" (29 terminals)
        //   "\u001b[0%?%p6%t;1%;%?%p2%t;4%;%?%p1%p3%|%t;7%;%?%p4%t;5%;m%?%p9%t\u000e%e\u000f%;" (17 terminals)
        //   "\u001b[0%?%p6%t;1%;%?%p1%t;3%;%?%p2%t;4%;%?%p3%t;7%;%?%p4%t;5%;%?%p5%t;2%;m%?%p9%t\u000e%e\u000f%;" (16 terminals)
        //   "\u001b[0%?%p1%p6%|%t;1%;%?%p2%t;4%;%?%p1%p3%|%t;7%;%?%p4%t;5%;m%?%p9%t\u000e%e\u000f%;" (12 terminals)
        //   "\u001b[0%?%p6%t;1%;%?%p2%t;4%;%?%p4%t;5%;%?%p1%p3%|%t;7%;m%?%p9%t\u001b(0%e\u001b(B%;$<2>" (11 terminals)
        //   "\u001b[0%?%p6%t;1%;%?%p2%t;4%;%?%p5%t;2%;%?%p7%t;8%;%?%p1%p3%|%t;7%;m%?%p9%t\u000e%e\u000f%;" (11 terminals)
        //   "%?%p8%t\u001b)%e\u001b(%;%?%p9%t\u001bcE%e\u001bcD%;\u001bG%'0'%?%p2%t%{8}%|%;%?%p1%p3%|%p6%|%t%{4}%|%;%?%p4%t%{2}%|%;%?%p1%p5%|%t%'@'%|%;%?%p7%t%{1}%|%;%c" (10 terminals)
        //   "\u001b[0;10%?%p1%t;7%;%?%p2%t;4%;%?%p3%t;7%;%?%p4%t;5%;%?%p5%t;2%;%?%p6%t;1%;m%?%p9%t\u000e%e\u000f%;" (9 terminals)
        //   "\u001b[0%?%p1%p6%|%t;1%;%?%p2%t;4%;%?%p1%p3%|%t;7%;%?%p4%t;5%;m%?%p9%t\u000e%e\u000f%;$<2>" (9 terminals)
        //   "%?%p5%t\u001b[0t%;%?%p3%p1%|%t\u001b[1t%;%?%p2%t\u001b[2t%;%?%p4%t\u001b[3t%;%?%p1%p2%p3%p4%p5%|%|%|%|%t\u001b[7m%e\u001b[m%;%?%p9%t\u000e%e\u000f%;" (8 terminals)
        //   "\u001b[0%?%p6%t;1%;%?%p2%t;4%;%?%p1%p3%|%t;7%;m%?%p9%t\u000e%e\u000f%;" (7 terminals)
        //   "\u001b[0%?%p6%t;1%;%?%p2%t;4%;%?%p1%p3%|%t;7%;m%?%p9%t\u001b(0%e\u001b(B%;" (7 terminals)
        //   "%?%p1%p3%|%t\u001b`6\u001b)%e%p5%p8%|%t\u001b`7\u001b)%e\u001b(%;%?%p9%t\u001bH\u0002%e\u001bH\u0003%;" (7 terminals)
        //   "\u001b[0%?%p6%t;1%;%?%p2%t;4%;%?%p1%p3%|%t;7%;%?%p4%t;5%;%?%p5%t;2%;%?%p7%t;8%;m" (6 terminals)
        //   "%?%p9%t\u001b(0%e\u001b(B%;\u001b[0%?%p6%t;1%;%?%p2%t;4%;%?%p1%p3%|%t;7%;%?%p4%t;5%;%?%p5%t;2%;%?%p7%t;8%;m" (5 terminals)
        //   "\u001b[%?%p6%t;1%;%?%p2%t;4%;%?%p1%p3%|%t;7%;%?%p4%t;5%;%?%p7%t;8%;m%?%p9%t\u001b(0%e\u001b(B%;" (4 terminals)
        //   "\u001b[0;10%?%p1%t;7%;%?%p2%t;4%;%?%p3%t;7%;%?%p4%t;5%;%?%p5%t;2%;%?%p6%t;1%;%?%p9%t;11%;m" (4 terminals)
        //   "\u001b[0%?%p6%t;1%;%?%p2%t;4%;%?%p4%t;5%;%?%p1%p3%|%t;7%;m%?%p9%t\u001b(0%e\u001b(B%;" (4 terminals)
        //   "\u001b[0%?%p6%t;1%;%?%p2%t;4%;%?%p1%p3%|%t;7%;%?%p4%t;5%;%?%p7%t;8%;m%?%p9%t\u000e%e\u000f%;" (3 terminals)
        //   "%?%p9%t\u001b(0%e\u001b(B%;\u001b[0%?%p6%t;1%;%?%p5%t;2%;%?%p2%t;4%;%?%p1%p3%|%t;7%;m" (3 terminals)
        //   "\u001b[0;10%?%p1%t;7%;%?%p2%t;4%;%?%p3%t;7%;%?%p4%t;5%;%?%p6%t;1%;%?%p7%t;8%;%?%p9%t;11%;m" (2 terminals)
        //   "\u001b[0%?%p1%t;7%;%?%p2%t;4%;%?%p3%t;7%;%?%p4%t;5%;%?%p5%t;2%;%?%p6%t;1%;%?%p7%t;8%;%?%p9%t;11%;m" (2 terminals)
        //   "\u001b[0%?%p6%t;1%;%?%p2%t;4%;%?%p1%p3%|%t;7%;%?%p4%t;5%;%?%p5%t;2%;m%?%p9%t\u000e%e\u000f%;" (2 terminals)
        //   "%?%p9%t\u001b(0%e\u001b(B%;\u001b[0%?%p6%t;1%;%?%p5%t;2%;%?%p2%t;4%;%?%p1%p3%|%t;7%;%?%p7%t;8%;m$<2>" (2 terminals)
        //   "?0%?%p6%t;1%;%?%p2%t;4%;%?%p1%p3%|%t;7%;%?%p4%t;5%;%?%p7%t;8%;m%?%p9%t\u001b(0%e\u001b(B%;" (1 terminal)
        //   "\u001b[0%?%p1%p6%|%t;1%;%?%p2%t;4%;%?%p1%p3%|%t;7%;%?%p4%t;5%;%?%p7%t;8%;m%?%p9%t\u000e%e\u000f%;" (1 terminal)
        //   "\u001b[0%?%p1%p6%|%t;1%;%?%p2%t;4%;%?%p5%t;2%;%?%p1%p3%|%t;7%;m%?%p9%t\u001b(0%e\u001b(B%;" (1 terminal)
        //   "%?%p1%t\u001b]%;%?%p3%t\u001b]%;%?%p4%t\u001bH%;" (1 terminal)
        //   "%?%p9%t\u000e%e\u000f%;\u001b[0%?%p6%t;1;43%;%?%p2%t;4;42%;%?%p1%t;7;31%;%?%p3%t;7;34%;m" (1 terminal)
        //   "%?%p9%t\u001b(0%e\u001b(B%;\u001b[0%?%p6%t;1%;%?%p2%t;4%;%?%p1%p3%|%t;7%;%?%p4%t;5%;%?%p7%t;8%;m" (1 terminal)
        /// <summary>Get the value of capability "set_attributes".</summary>
        /// <param name="standout"><see cref="bool"/> value that is whether the character's attribute is in "standout" mode</param>
        /// <param name="underline"><see cref="bool"/> value that is whether the character's attribute is in "underline" mode</param>
        /// <param name="reverse"><see cref="bool"/> value that is whether the character's attribute is in "reverse" mode</param>
        /// <param name="blink"><see cref="bool"/> value that is whether the character's attribute is in "blink" mode</param>
        /// <param name="dim"><see cref="bool"/> value that is whether the character's attribute is in "dim" mode</param>
        /// <param name="bold"><see cref="bool"/> value that is whether the character's attribute is in "bold" mode</param>
        /// <param name="invis"><see cref="bool"/> value that is whether the character's attribute is in "blank" mode</param>
        /// <param name="protect"><see cref="bool"/> value that is whether the character's attribute is in "protect" mode</param>
        /// <param name="altcharset"><see cref="bool"/> value that is whether the character's attribute is in "alternate character set" mode</param>
        /// <returns>If not null it is the value of the capability "set_attributes". If null, this terminal information does not support the capability "set_attributes".</returns>
        /// <remarks>
        /// Note that some parameters are not supported by all terminals.
        /// (e.g. terminal "xterm-256color" does not support "protect" mode, so <paramref name="protect"/> is ignored)
        /// </remarks>
        public string? SetAttributes(bool standout, bool underline, bool reverse, bool blink, bool dim, bool bold, bool invis, bool protect, bool altcharset)
            => _database.GetStringCapabilityValue(TermInfoStringCapabilities.SetAttributes, standout, underline, reverse, blink, dim, bold, invis, protect, altcharset);

        // Capability name: set_background
        // Terminals supporting this capability: 53 terminals
        // Values of this capability:
        //   "\u001b[4%?%p1%{1}%=%t4%e%p1%{3}%=%t6%e%p1%{4}%=%t1%e%p1%{6}%=%t3%e%p1%d%;m" (30 terminals)
        //   "%p1%{8}%/%{6}%*%{4}%+\u001b[%d%p1%{8}%m%Pa%?%ga%{1}%=%t4%e%ga%{3}%=%t6%e%ga%{4}%=%t1%e%ga%{6}%=%t3%e%ga%d%;m" (16 terminals)
        //   "%?%p1%{7}%>%t\u001b[48;5;%p1%dm%e\u001b[4%?%p1%{1}%=%t4%e%p1%{3}%=%t6%e%p1%{4}%=%t1%e%p1%{6}%=%t3%e%p1%d%;m%;" (4 terminals)
        //   "?" (1 terminal)
        //   "?4%?%p1%{1}%=%t4%e%p1%{3}%=%t6%e%p1%{4}%=%t1%e%p1%{6}%=%t3%e%p1%d%;m" (1 terminal)
        //   "\u001b[62;%p1%dw" (1 terminal)
        /// <summary>Get the value of capability "set_background".</summary>
        /// <param name="color"><see cref="Color8"/> value that is the background color of the character</param>
        /// <returns>If not null it is the value of the capability "set_background". If null, this terminal information does not support the capability "set_background".</returns>
        public string? SetBackground(Color8 color)
            => _database.GetStringCapabilityValue(TermInfoStringCapabilities.SetBackground, (int)color);

        // Capability name: set_foreground
        // Terminals supporting this capability: 53 terminals
        // Values of this capability:
        //   "\u001b[3%?%p1%{1}%=%t4%e%p1%{3}%=%t6%e%p1%{4}%=%t1%e%p1%{6}%=%t3%e%p1%d%;m" (30 terminals)
        //   "%p1%{8}%/%{6}%*%{3}%+\u001b[%d%p1%{8}%m%Pa%?%ga%{1}%=%t4%e%ga%{3}%=%t6%e%ga%{4}%=%t1%e%ga%{6}%=%t3%e%ga%d%;m" (16 terminals)
        //   "%?%p1%{7}%>%t\u001b[38;5;%p1%dm%e\u001b[3%?%p1%{1}%=%t4%e%p1%{3}%=%t6%e%p1%{4}%=%t1%e%p1%{6}%=%t3%e%p1%d%;m%;" (4 terminals)
        //   "?3%?%p1%{1}%=%t4%e%p1%{3}%=%t6%e%p1%{4}%=%t1%e%p1%{6}%=%t3%e%p1%d%;m" (1 terminal)
        //   "\u001b[61;%p1%dw" (1 terminal)
        //   "\u001b%?%p1%{1}%=%tD%e%p1%{3}%=%tF%e%p1%{4}%=%tA%e%p1%{6}%=%tC%e%p1%'@'%+%c%;" (1 terminal)
        /// <summary>Get the value of capability "set_foreground".</summary>
        /// <param name="color"><see cref="Color8"/> value that is the foreground color of the character</param>
        /// <returns>If not null it is the value of the capability "set_foreground". If null, this terminal information does not support the capability "set_foreground".</returns>
        public string? SetForeground(Color8 color)
            => _database.GetStringCapabilityValue(TermInfoStringCapabilities.SetForeground, (int)color);

        // Capability name: set_lr_margin
        // Terminals supporting this capability: 11 terminals
        // Values of this capability:
        //   "\u001b[?69h\u001b[%i%p1%d;%p2%ds" (11 terminals)
        /// <summary>Get the value of capability "set_lr_margin".</summary>
        /// <param name="leftMargin"><see cref="int"/> value that is the left margin</param>
        /// <param name="rightMargin"><see cref="int"/> value that is the right margin</param>
        /// <returns>If not null it is the value of the capability "set_lr_margin". If null, this terminal information does not support the capability "set_lr_margin".</returns>
        public string? SetLrMargin(int leftMargin, int rightMargin)
            => leftMargin < 0
                ? throw new ArgumentOutOfRangeException(nameof(leftMargin))
                : rightMargin < 0
                ? throw new ArgumentOutOfRangeException(nameof(rightMargin))
                : _database.GetStringCapabilityValue(TermInfoStringCapabilities.SetLrMargin, leftMargin, rightMargin);

        // Capability name: set_tab
        // Terminals supporting this capability: 306 terminals
        // Values of this capability:
        //   "\u001bH" (280 terminals)
        //   "\u001b1" (25 terminals)
        //   "?" (1 terminal)
        /// <summary>Get the value of capability "set_tab".</summary>
        /// <returns>If not null it is the value of the capability "set_tab". If null, this terminal information does not support the capability "set_tab".</returns>
        public string? SetTab => _database.GetStringCapabilityValue(TermInfoStringCapabilities.SetTab);

        // Capability name: set0_des_seq
        // Terminals supporting this capability: 30 terminals
        // Values of this capability:
        //   "\u001b(B" (23 terminals)
        //   "\u001b[10m" (7 terminals)
        /// <summary>Get the value of capability "set0_des_seq".</summary>
        /// <returns>If not null it is the value of the capability "set0_des_seq". If null, this terminal information does not support the capability "set0_des_seq".</returns>
        public string? Set0DesSeq => _database.GetStringCapabilityValue(TermInfoStringCapabilities.Set0DesSeq);

        // Capability name: set1_des_seq
        // Terminals supporting this capability: 30 terminals
        // Values of this capability:
        //   "\u001b(0" (21 terminals)
        //   "\u001b[11m" (7 terminals)
        //   "\u001b)B" (2 terminals)
        /// <summary>Get the value of capability "set1_des_seq".</summary>
        /// <returns>If not null it is the value of the capability "set1_des_seq". If null, this terminal information does not support the capability "set1_des_seq".</returns>
        public string? Set1DesSeq => _database.GetStringCapabilityValue(TermInfoStringCapabilities.Set1DesSeq);

        // Capability name: set2_des_seq
        // Terminals supporting this capability: 13 terminals
        // Values of this capability:
        //   "\u001b[12m" (7 terminals)
        //   "\u001b*B" (6 terminals)
        /// <summary>Get the value of capability "set2_des_seq".</summary>
        /// <returns>If not null it is the value of the capability "set2_des_seq". If null, this terminal information does not support the capability "set2_des_seq".</returns>
        public string? Set2DesSeq => _database.GetStringCapabilityValue(TermInfoStringCapabilities.Set2DesSeq);

        // Capability name: set3_des_seq
        // Terminals supporting this capability: 6 terminals
        // Values of this capability:
        //   "\u001b+B" (6 terminals)
        /// <summary>Get the value of capability "set3_des_seq".</summary>
        /// <returns>If not null it is the value of the capability "set3_des_seq". If null, this terminal information does not support the capability "set3_des_seq".</returns>
        public string? Set3DesSeq => _database.GetStringCapabilityValue(TermInfoStringCapabilities.Set3DesSeq);

        // Capability name: setal
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b[%?%p1%{8}%<%t5%p1%d%e58:2::%p1%{65536}%/%d:%p1%{256}%/%{255}%&%d:%p1%{255}%&%d%;m" (1 terminal)
        //   "\u001b[5%p1%dm" (1 terminal)
        /// <summary>Get the value of extended capability "setal".</summary>
        /// <param name="r">a byte value that is the red component of the cursor color( 0 &lt;= <paramref name="r"/> &lt;= 255)</param>
        /// <param name="g">a byte value that is the green component of the cursor color( 0 &lt;= <paramref name="g"/> &lt;= 255)</param>
        /// <param name="b">a byte value that is the blue component of the cursor color( 0 &lt;= <paramref name="b"/> &lt;= 255)</param>
        /// <returns>If not null it is the value of the extended capability "setal". If null, this terminal information does not support the extended capability "setal".</returns>
        public string? Setal(byte r, byte g, byte b)
            => _database.GetStringCapabilityValue("setal", (int)(((uint)r << 16) | ((uint)g << 8) | b));

        // Capability name: Smol
        // Terminals supporting this capability: 8 terminals
        // Values of this capability:
        //   "\u001b[53m" (8 terminals)
        /// <summary>Get the value of extended capability "Smol".</summary>
        /// <returns>If not null it is the value of the extended capability "Smol". If null, this terminal information does not support the extended capability "Smol".</returns>
        /// <remarks>
        /// This capability is <see cref="string"/> value that is an escape code to enable the overline attribute.
        /// </remarks>
        public string? Smol => _database.GetStringCapabilityValue("Smol");

        // Capability name: smul2
        // Terminals supporting this capability: 2 terminals
        // Values of this capability:
        //   "\u001b[21m" (2 terminals)
        /// <summary>Get the value of extended capability "smul2".</summary>
        /// <returns>If not null it is the value of the extended capability "smul2". If null, this terminal information does not support the extended capability "smul2".</returns>
        public string? Smul2 => _database.GetStringCapabilityValue("smul2");

        // Capability name: Smulx
        // Terminals supporting this capability: 12 terminals
        // Values of this capability:
        //   "\u001b[4:%p1%dm" (12 terminals)
        /// <summary>Get the value of extended capability "Smulx".</summary>
        /// <param name="underlineStyle"><see cref="UnderlineStyle"/> value that is the type of underline for characters</param>
        /// <returns>If not null it is the value of the extended capability "Smulx". If null, this terminal information does not support the extended capability "Smulx".</returns>
        /// <remarks>
        /// This capability is <see cref="string"/> value of escape codes that set the underline style.
        /// </remarks>
        public string? Smulx(UnderlineStyle underlineStyle)
            => underlineStyle.IsOutOfClosedInterval(UnderlineStyle.Minimum, UnderlineStyle.Maximum)
                ? _database.GetStringCapabilityValue("Smulx", (int)underlineStyle)
                : throw new ArgumentOutOfRangeException(nameof(underlineStyle));

        // Capability name: smxx
        // Terminals supporting this capability: 57 terminals
        // Values of this capability:
        //   "\u001b[9m" (57 terminals)
        /// <summary>Get the value of extended capability "smxx".</summary>
        /// <returns>If not null it is the value of the extended capability "smxx". If null, this terminal information does not support the extended capability "smxx".</returns>
        public string? Smxx => _database.GetStringCapabilityValue("smxx");

        // Capability name: Ss
        // Terminals supporting this capability: 38 terminals
        // Values of this capability:
        //   "\u001b[%p1%d q" (38 terminals)
        /// <summary>Get the value of extended capability "Ss".</summary>
        /// <param name="cursorStyle"><see cref="CursorStyle"/> value that is the cursor style</param>
        /// <returns>If not null it is the value of the extended capability "Ss". If null, this terminal information does not support the extended capability "Ss".</returns>
        /// <remarks>
        /// This capability is a string value for code that changes the cursor style.
        /// </remarks>
        public string? Ss(CursorStyle cursorStyle)
            => cursorStyle.IsOutOfClosedInterval(CursorStyle.Minimum, CursorStyle.Maximum)
                ? _database.GetStringCapabilityValue("Ss", (int)cursorStyle)
                : throw new ArgumentOutOfRangeException(nameof(cursorStyle));

        // Capability name: status_line_esc_ok
        // Terminals supporting this capability: 307 terminals
        // Values of this capability:
        //   false (288 terminals)
        //   true (19 terminals)
        /// <summary>Get the value of capability "status_line_esc_ok".</summary>
        /// <returns>If not null it is the value of the capability "status_line_esc_ok". If null, this terminal information does not support the capability "status_line_esc_ok".</returns>
        public bool? StatusLineEscOk => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.StatusLineEscOk);

        // Capability name: tab
        // Terminals supporting this capability: 313 terminals
        // Values of this capability:
        //   "\t" (296 terminals)
        //   "\t$<1>" (15 terminals)
        //   "\u001b[I" (2 terminals)
        /// <summary>Get the value of capability "tab".</summary>
        /// <returns>If not null it is the value of the capability "tab". If null, this terminal information does not support the capability "tab".</returns>
        public string? Tab => _database.GetStringCapabilityValue(TermInfoStringCapabilities.Tab);

        // Capability name: tilde_glitch
        // Terminals supporting this capability: 297 terminals
        // Values of this capability:
        //   false (296 terminals)
        //   true (1 terminal)
        /// <summary>Get the value of capability "tilde_glitch".</summary>
        /// <returns>If not null it is the value of the capability "tilde_glitch". If null, this terminal information does not support the capability "tilde_glitch".</returns>
        public bool? TildeGlitch => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.TildeGlitch);

        // Capability name: to_status_line
        // Terminals supporting this capability: 171 terminals
        // Values of this capability:
        //   "\u001b]2;" (40 terminals)
        //   "\u001b[2$~\u001b[1$}" (29 terminals)
        //   "\u001b[2$~\u001b[1$}\u001b[%i%p1%d`" (27 terminals)
        //   "\u001b]0;" (23 terminals)
        //   "\u001bF" (17 terminals)
        //   "\u001b[>,\u0001" (8 terminals)
        //   "\u001bg\u001bf" (8 terminals)
        //   "\u001b_" (6 terminals)
        //   "\u001b[40h\u001b7\u001b[25;%i%p1%dH" (4 terminals)
        //   "\u001b7\u001b[99;%i%p1%dH" (4 terminals)
        //   "\u001f@%?%p1%'?'%<%t%p1%'A'%+%c%e\u007f%p1%'>'%-%Pa%?%ga%{1}%&%t\t%;%?%ga%{2}%&%t\t\t%;%?%ga%{4}%&%t\t\t\t\t%;%?%ga%{07}%>%t\t\t\t\t\t\t\t\t%;%?%ga%{15}%>%t\t\t\t\t\t\t\t\t%;%;" (2 terminals)
        //   "\u001b[40l\u001b[40h\u001b7\u001b[99;%i%p1%dH" (1 terminal)
        //   "\u001b]2;%p1" (1 terminal)
        //   "\u001f@%p1%'A'%+%c" (1 terminal)
        /// <summary>Get the value of capability "to_status_line".</summary>
        /// <returns>If not null it is the value of the capability "to_status_line". If null, this terminal information does not support the capability "to_status_line".</returns>
        public string? ToStatusLine()
            => _database.GetStringCapabilityValue(TermInfoStringCapabilities.ToStatusLine);

        /// <summary>Get the value of capability "to_status_line".</summary>
        /// <param name="column"><see cref="int"/> value that is the column to move the status line to</param>
        /// <returns>If not null it is the value of the capability "to_status_line". If null, this terminal information does not support the capability "to_status_line".</returns>
        public string? ToStatusLine(int column)
            => column >= 0
                ? _database.GetStringCapabilityValue(TermInfoStringCapabilities.ToStatusLine, column)
                : throw new ArgumentOutOfRangeException(nameof(column));

        // Capability name: transparent_underline
        // Terminals supporting this capability: 296 terminals
        // Values of this capability:
        //   false (296 terminals)
        /// <summary>Get the value of capability "transparent_underline".</summary>
        /// <returns>If not null it is the value of the capability "transparent_underline". If null, this terminal information does not support the capability "transparent_underline".</returns>
        public bool? TransparentUnderline => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.TransparentUnderline);

        // Capability name: TS
        // Terminals supporting this capability: 60 terminals
        // Values of this capability:
        //   "\u001b]2;" (37 terminals)
        //   "\u001b]0;" (23 terminals)
        /// <summary>Get the value of extended capability "TS".</summary>
        /// <returns>If not null it is the value of the extended capability "TS". If null, this terminal information does not support the extended capability "TS".</returns>
        /// <remarks>
        /// This capability is a start code string value for changing the window title.
        /// To change the window title, send the value of TS followed by the window title and finally a terminator ("\u001b\\" or "\u0007").
        /// </remarks>
        public string? TS => _database.GetStringCapabilityValue("TS");

        // Capability name: U8
        // Terminals supporting this capability: 42 terminals
        // Values of this capability:
        //   1 (42 terminals)
        /// <summary>Get the value of extended capability "U8".</summary>
        /// <returns>If not null it is the value of the extended capability "U8". If null, this terminal information does not support the extended capability "U8".</returns>
        public int? U8 => _database.GetNumberCapabilityValue("U8");

        // Capability name: user0
        // Terminals supporting this capability: 3 terminals
        // Values of this capability:
        //   "\u001b~>\u001b8" (2 terminals)
        //   "\u001b[?38h\u001b8" (1 terminal)
        /// <summary>Get the value of capability "user0".</summary>
        /// <returns>If not null it is the value of the capability "user0". If null, this terminal information does not support the capability "user0".</returns>
        public string? User0 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.User0);

        // Capability name: user1
        // Terminals supporting this capability: 3 terminals
        // Values of this capability:
        //   "\u001b[42h" (2 terminals)
        //   "\u001b[?38l\u001b)0" (1 terminal)
        /// <summary>Get the value of capability "user1".</summary>
        /// <returns>If not null it is the value of the capability "user1". If null, this terminal information does not support the capability "user1".</returns>
        public string? User1 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.User1);

        // Capability name: user2
        // Terminals supporting this capability: 1 terminal
        // Values of this capability:
        //   "\u001b[92;52\"p" (1 terminal)
        /// <summary>Get the value of capability "user2".</summary>
        /// <returns>If not null it is the value of the capability "user2". If null, this terminal information does not support the capability "user2".</returns>
        public string? User2 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.User2);

        // Capability name: user3
        // Terminals supporting this capability: 1 terminal
        // Values of this capability:
        //   "\u001b~B" (1 terminal)
        /// <summary>Get the value of capability "user3".</summary>
        /// <returns>If not null it is the value of the capability "user3". If null, this terminal information does not support the capability "user3".</returns>
        public string? User3 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.User3);

        // Capability name: user4
        // Terminals supporting this capability: 1 terminal
        // Values of this capability:
        //   "\u001b[92;76\"p" (1 terminal)
        /// <summary>Get the value of capability "user4".</summary>
        /// <returns>If not null it is the value of the capability "user4". If null, this terminal information does not support the capability "user4".</returns>
        public string? User4 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.User4);

        // Capability name: user5
        // Terminals supporting this capability: 1 terminal
        // Values of this capability:
        //   "\u001b%!1\u001b[90;1\"p" (1 terminal)
        /// <summary>Get the value of capability "user5".</summary>
        /// <returns>If not null it is the value of the capability "user5". If null, this terminal information does not support the capability "user5".</returns>
        public string? User5 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.User5);

        // Capability name: user6
        // Terminals supporting this capability: 176 terminals
        // Values of this capability:
        //   "\u001b[%i%d;%dR" (173 terminals)
        //   "?[%i%d;%dR" (1 terminal)
        //   "\u001b[%i%d;%dH" (1 terminal)
        //   "\u001f%c%'A'%-%c%'A'%-" (1 terminal)
        /// <summary>Get the value of capability "user6".</summary>
        /// <returns>If not null it is the value of the capability "user6". If null, this terminal information does not support the capability "user6".</returns>
        public string? User6 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.User6);

        // Capability name: user7
        // Terminals supporting this capability: 176 terminals
        // Values of this capability:
        //   "\u001b[6n" (175 terminals)
        //   "\u001ba" (1 terminal)
        /// <summary>Get the value of capability "user7".</summary>
        /// <returns>If not null it is the value of the capability "user7". If null, this terminal information does not support the capability "user7".</returns>
        public string? User7 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.User7);

        // Capability name: user8
        // Terminals supporting this capability: 174 terminals
        // Values of this capability:
        //   "\u001b[?1;2c" (74 terminals)
        //   "\u001b[?%[;0123456789]c" (67 terminals)
        //   "\u001b[?6c" (27 terminals)
        //   "\u001b[?62;1;6c" (4 terminals)
        //   "?[?%[;0123456789]c" (1 terminal)
        //   "\u0001%[BCDEFGHIJKLbcresdfg0123456789]\u0004" (1 terminal)
        /// <summary>Get the value of capability "user8".</summary>
        /// <returns>If not null it is the value of the capability "user8". If null, this terminal information does not support the capability "user8".</returns>
        public string? User8 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.User8);

        // Capability name: user9
        // Terminals supporting this capability: 174 terminals
        // Values of this capability:
        //   "\u001b[c" (170 terminals)
        //   "\u001bZ" (3 terminals)
        //   "\u001b9{" (1 terminal)
        /// <summary>Get the value of capability "user9".</summary>
        /// <returns>If not null it is the value of the capability "user9". If null, this terminal information does not support the capability "user9".</returns>
        public string? User9 => _database.GetStringCapabilityValue(TermInfoStringCapabilities.User9);

        // Capability name: virtual_terminal
        // Terminals supporting this capability: 10 terminals
        // Values of this capability:
        //   3 (10 terminals)
        /// <summary>Get the value of capability "virtual_terminal".</summary>
        /// <returns>If not null it is the value of the capability "virtual_terminal". If null, this terminal information does not support the capability "virtual_terminal".</returns>
        public int? VirtualTerminal => _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.VirtualTerminal);

        // Capability name: width_status_line
        // Terminals supporting this capability: 98 terminals
        // Values of this capability:
        //   50 (29 terminals)
        //   132 (26 terminals)
        //   80 (17 terminals)
        //   45 (9 terminals)
        //   97 (8 terminals)
        //   78 (6 terminals)
        //   130 (2 terminals)
        //   40 (1 terminal)
        /// <summary>Get the value of capability "width_status_line".</summary>
        /// <returns>If not null it is the value of the capability "width_status_line". If null, this terminal information does not support the capability "width_status_line".</returns>
        public int? WidthStatusLine => _database.GetNumberCapabilityValue(TermInfoNumberCapabilities.WidthStatusLine);

        // Capability name: XC
        // Terminals supporting this capability: 3 terminals
        // Values of this capability:
        //   "B\u0019%,?!,?\",?#,?$,?%,?&,?',?(,?+,?P,?0,?1,?2,?3,?5,?7,?k,?;,?<,?=,?>,??,?AA,?BA,?CA,?DA,?HA,?JA,?a,?KC,?AE,?BE,?CE,?HE,?AI,?BI,?CI,?HI,?b,?DN,?AO,?BO,?CO,?DO,?HO,?4,?i,?AU,?BU,?CU,?HU,?BY,?l,?{,?Aa,?Ba,?Ca,?Da,?Ha,?Ja,?q,?Kc,?Ae,?Be,?Ce,?He,?Ai,?Bi,?Ci,?Hi,?r,?Dn,?Ao,?Bo,?Co,?Do,?Ho,?8,?y,?Au,?Bu,?Cu,?Hu,?By,?|,?Hy,?c,,0\u000f\u0019%\u000e,}#,f0,g1,\\,\\,,+.,./,0\u007f,--" (1 terminal)
        //   "B%\u001b(B,?\u001b(3},?\u001b(R[,?\u001b(3v,?\u001b(3f,?\u001b(3g,?\u001b(3~,?\u001b(3O,?\u001b(3P,?\u001b(3Q,?A,?A,?A,?A,?A,?A,?E,?C,?E,?E,?E,?E,?I,?I,?I,?I,?D,?N,?O,?O,?O,?O,?O,?x,?U,?U,?U,?U,?Y,?\u001b(3{,?\u001b(3A,?a,?\u001b(3B,?a,?\u001b(3C,?a,?e,?\u001b(R\\\\,?\u001b(3E,?\u001b(3D,?\u001b(3F,?\u001b(3G,?i,?i,?\u001b(3H,?\u001b(3I,?d,?n,?o,?o,?\u001b(3J,?o,?\u001b(3K,?\u001b(3h,?\u001b(3L,?u,?\u001b(3M,?\u001b(3N,?y,?y,,0\u001b)3%\u001b)0,\\,m,+k,.l,0\u007f,-j" (1 terminal)
        //   "B%\u001b(B,?\u001b(3},?\u001b(R[,?\u001b(3v,?\u001b(3f,?\u001b(3g,?\u001b(3Y,?\u001b(3~,?\u001b(3O,?\u001b(3P,?\u001b(3Q,?\u001b(3Z,?A,?A,?A,?A,?\u001b(3R,?A,?E,?C,?E,?\u001b(3S,?E,?E,?\u001b(3T,?I,?I,?I,?D,?\u001b(3W,?\u001b(3U,?O,?O,?O,?O,?x,?U,?U,?U,?\u001b(3V,?Y,?\u001b(3{,?\u001b(3A,?a,?\u001b(3B,?a,?\u001b(3C,?a,?e,?\u001b(R\\\\,?\u001b(3E,?\u001b(3D,?\u001b(3F,?\u001b(3G,?i,?i,?\u001b(3H,?\u001b(3I,?d,?\u001b(3X,?o,?o,?\u001b(3J,?o,?\u001b(3K,?\u001b(3h,?\u001b(3L,?u,?\u001b(3M,?\u001b(3N,?y,?y,,0\u001b)3%\u001b)0,\\,m,+k,.l,0\u007f,-j" (1 terminal)
        /// <summary>Get the value of extended capability "XC".</summary>
        /// <returns>If not null it is the value of the extended capability "XC". If null, this terminal information does not support the extended capability "XC".</returns>
        /// <remarks>
        /// This capability is a string value describing the character-to-string conversion depending on the current font.
        /// (See "man screen(1)")
        /// </remarks>
        public string? XC => _database.GetStringCapabilityValue("XC");

        // Capability name: xm
        // Terminals supporting this capability: 71 terminals
        // Values of this capability:
        //   "\u001b[<%i%p3%d;%p1%d;%p2%d;%?%p4%tM%em%;" (40 terminals)
        //   "\u001b[M%?%p4%t%p3%e%{3}%;%' '%+%c%p2%'!'%+%c%p1%'!'%+%c" (26 terminals)
        //   "\u001b[M%?%p4%t3%e%p3%' '%+%c%;%p2%'!'%+%u%p1%'!'%+%u" (2 terminals)
        //   "\u001b[M%p3%' '%+%c%p2%'!'%+%c%p1%'!'%+%c" (2 terminals)
        //   "\u001b[%p6%'!'%+%p5%'!'%+%c%p8%'!'%+%c%p7%'!'%+%c%p2%'!'%+%c%p1%'!'%+%cT" (1 terminal)
#if false // This capability is a string value for terminal-to-host events. Despite the existence of an entry in terminfo, the number and role of the parameters are unspecified.
        public string? Xm(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8) => _database.GetStringCapabilityValue("xm", p1, p2, p3, p4, p5, p6, p7, p8);
#endif

        // Capability name: XM
        // Terminals supporting this capability: 73 terminals
        // Values of this capability:
        //   "\u001b[?1006;1000%?%p1%{1}%=%th%el%;" (38 terminals)
        //   "\u001b[?1000%?%p1%{1}%=%th%el%;" (26 terminals)
        //   "\u001b[?1002%?%p1%{1}%=%th%el%;" (2 terminals)
        //   "\u001b[?1003%?%p1%{1}%=%th%el%;" (2 terminals)
        //   "\u001b[?1005;1000%?%p1%{1}%=%th%el%;" (2 terminals)
        //   "\u001b[?9%?%p1%{1}%=%th%el%;" (2 terminals)
        //   "\u001b[?1001%?%p1%{1}%=%th%el%;" (1 terminal)
        /// <summary>Get the value of extended capability "XM".</summary>
        /// <param name="enabledMouseEvent"><see cref="bool"/> value indicating whether mouse events are notified</param>
        /// <returns>If not null it is the value of the extended capability "XM". If null, this terminal information does not support the extended capability "XM".</returns>
        public string? XM(bool enabledMouseEvent) => _database.GetStringCapabilityValue("XM", enabledMouseEvent);

        // Capability name: xon_xoff
        // Terminals supporting this capability: 296 terminals
        // Values of this capability:
        //   true (170 terminals)
        //   false (126 terminals)
        /// <summary>Get the value of capability "xon_xoff".</summary>
        /// <returns>If not null it is the value of the capability "xon_xoff". If null, this terminal information does not support the capability "xon_xoff".</returns>
        public bool? XonXoff => _database.GetBooleanCapabilityValue(TermInfoBooleanCapabilities.XonXoff);

        // Capability name: XT
        // Terminals supporting this capability: 105 terminals
        // Values of this capability:
        //   true (105 terminals)
        /// <summary>Get the value of extended capability "XT".</summary>
        /// <returns>If not null it is the value of the extended capability "XT". If null, this terminal information does not support the extended capability "XT".</returns>
        /// <remarks>
        /// This capability is a boolean value indicating whether the terminal understands special xterm sequences (OSC, mouse tracking).
        /// (See "man screen(1)")
        /// </remarks>
        public bool? XT => _database.GetBooleanCapabilityValue("XT");

        #endregion

        #region Pseudo-capability accesser

        /// <summary>
        /// A string capability for setting the title of the console window.
        /// </summary>
        /// <param name="title"><see cref="string"/> value that is the title of the console window. This string must not contain control codes.</param>
        /// <returns>
        /// Escape code for setting the title of the console window.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// This capability is proprietary and incompatible with terminfo.
        /// </remarks>
        public string? SetTitle(string title)
            => title.None(c => c is < '\u0020' or '\u007f')
                ? _database.GetStringCapabilityValue("__set_title", title)
                : throw new ArgumentException($"{nameof(title)} contains characters that cannot be displayed.");

        /// <summary>
        /// A string capability for initializing the foreground and background colors of characters.
        /// </summary>
        /// <remarks>
        /// This capability is proprietary and incompatible with terminfo.
        /// </remarks>
        public string? ResetColor => _database.GetStringCapabilityValue("__reset_color");

        /// <summary>
        /// A string capability to clear the scroll buffer outside the bounds of the console window.
        /// </summary>
        /// <remarks>
        /// This capability is proprietary and incompatible with terminfo.
        /// </remarks>
        public string? EraseScrollBuffer => _database.GetStringCapabilityValue("__erase_scroll_buffer");

        /// <summary>
        /// A string capability to clear the console buffer.
        /// </summary>
        /// <remarks>
        /// This capability is proprietary and incompatible with terminfo.
        /// </remarks>
        public string? ClearBuffer => _database.GetStringCapabilityValue("__clear_buffer");

        /// <summary>
        /// CPR string capability.
        /// </summary>
        /// <remarks>
        /// This capability is proprietary and incompatible with terminfo.
        /// </remarks>
        public string? CursorPositionReport => _database.GetStringCapabilityValue("__cursor_position_report");

        /// <summary>
        /// A string capability to clear the upper left corner of the console window to before the cursor position.
        /// </summary>
        /// <remarks>
        /// This capability is proprietary and incompatible with terminfo.
        /// </remarks>
        public string? EraseInDisplay1 => _database.GetStringCapabilityValue("__erase_in_display_1");

        /// <summary>
        /// A string capability to clear the entire line under the cursor in the console window.
        /// </summary>
        /// <remarks>
        /// This capability is proprietary and incompatible with terminfo.
        /// </remarks>
        public string? EraseInLine2 => _database.GetStringCapabilityValue("__erase_in_line_2");

        #endregion

        #region JSON converter

        /// <summary>
        /// ターミナルの情報を JSON 形式で出力します。
        /// </summary>
        /// <param name="writer">
        /// 出力先の <see cref="TextWriter"/> オブジェクトです。
        /// </param>
        /// <param name="indent">
        /// 出力する JSON テキストに適用されるインデントの数です。
        /// </param>
        public void WriteTerminalInfo(TextWriter writer, int indent = 0)
            => _database.WriteTerminalnfo(writer, indent);

        /// <summary>
        /// ターミナルの情報を JSON 形式で取得します。
        /// </summary>
        /// <param name="indent">
        /// JSON テキストに適用されるインデントの数です。
        /// </param>
        /// <returns>
        /// JSON形式のターミナル情報である <see cref="string"/> オブジェクトです。
        /// </returns>
        public string GetTerminalnfo(int indent = 0)
        {
            var buffer = new StringBuilder();
            using var writer = new StringWriter(buffer);
            _database.WriteTerminalnfo(writer, indent);
            return buffer.ToString();
        }

        /// <summary>
        /// ターミナル情報を JSON 形式で提供するテキストリーダーを取得します。
        /// </summary>
        /// <param name="indent">
        /// JSON テキストに適用されるインデントの数です。
        /// </param>
        /// <returns>
        /// JSON 形式のターミナル情報を読み込むことが出来る <see cref="TextReader"/> オブジェクトです。
        /// </returns>
        public TextReader GetTerminalInfoReader(int indent = 0)
        {
            var fifoBuffer = new FifoTextStream(1024);
            _ = Task.Run(() =>
            {
                using var writer = fifoBuffer.OpenWriter();
                _database.WriteTerminalnfo(fifoBuffer.OpenWriter(), indent);
            });

            return fifoBuffer.OpenReader();
        }

        /// <summary>
        /// terminfo データベースのすべてのターミナルの情報を JSON 形式で出力します。
        /// </summary>
        /// <param name="writer">
        /// 出力先である <see cref="TextWriter"/> オブジェクトです。
        /// </param>
        /// <param name="indent">
        /// JSON テキストに適用されるインデントの数です。
        /// </param>
        /// <param name="includeUniqueCapabilities">
        /// true の場合、独自のキャパビリティが付加されたターミナル情報が返ります。
        /// false の場合、terminfo と互換性があるターミナル情報が返ります。
        /// </param>
        public static void WriteAllTerminalInfos(TextWriter writer, int indent = 0, bool includeUniqueCapabilities = false)
            => TerminalInfoDatabase.WriteAllTerminalInfos(writer, indent, includeUniqueCapabilities);

        /// <summary>
        /// terminfo データベースのすべてのターミナルの情報を JSON 形式で取得します。
        /// </summary>
        /// <param name="indent">
        /// JSON テキストに適用されるインデントの数です。
        /// </param>
        /// <param name="includeUniqueCapabilities">
        /// true の場合、独自のキャパビリティが付加されたターミナル情報が返ります。
        /// false の場合、terminfo と互換性があるターミナル情報が返ります。
        /// </param>
        /// <returns>
        /// JSON形式のターミナル情報である <see cref="string"/> オブジェクトです。
        /// </returns>
        public static string GetAllTerminalnfos(int indent = 0, bool includeUniqueCapabilities = false)
        {
            var buffer = new StringBuilder();
            using var writer = new StringWriter(buffer);
            TerminalInfoDatabase.WriteAllTerminalInfos(writer, indent, includeUniqueCapabilities);
            return buffer.ToString();
        }

        /// <summary>
        /// terminfo データベースのすべてのターミナルの情報を JSON 形式で提供するテキストリーダーを取得します。
        /// </summary>
        /// <param name="indent">
        /// JSON テキストに適用されるインデントの数です。
        /// </param>
        /// <param name="includeUniqueCapabilities">
        /// true の場合、独自のキャパビリティが付加されたターミナル情報が返ります。
        /// false の場合、terminfo と互換性があるターミナル情報が返ります。
        /// </param>
        /// <returns>
        /// JSON 形式のターミナル情報を読み込むことが出来る <see cref="TextReader"/> オブジェクトです。
        /// </returns>
        public static TextReader GetAllTerminalInfosReader(int indent = 0, bool includeUniqueCapabilities = false)
        {
            var fifoBuffer = new FifoTextStream(1024);
            _ = Task.Run(() =>
            {
                using var writer = fifoBuffer.OpenWriter();
                TerminalInfoDatabase.WriteAllTerminalInfos(fifoBuffer.OpenWriter(), indent, includeUniqueCapabilities);
            });

            return fifoBuffer.OpenReader();
        }

        #endregion

        #region TerminalInfo reader

        /// <summary>
        /// 現在使用中のターミナルの情報を terminfo データベースから取得します。
        /// </summary>
        /// <param name="includeUniqueCapabilities">
        /// true の場合、独自のキャパビリティが付加されたターミナル情報が返ります。
        /// false の場合、terminfo と互換性があるターミナル情報が返ります。
        /// </param>
        /// <returns>
        /// 現在使用中のターミナルの情報である <see cref="TerminalInfo"/> オブジェクトです。
        /// </returns>
        public static TerminalInfo? GetTerminalInfo(bool includeUniqueCapabilities = false)
        {
            var database = TerminalInfoDatabase.GetTerminalDatabase(includeUniqueCapabilities);
            return
                database is not null
                ? new TerminalInfo(database)
                : null;
        }

        /// <summary>
        /// 指定された名前のターミナルの情報を terminfo データベースから取得します。
        /// </summary>
        /// <param name="terminalName">
        /// ターミナルの名前です。
        /// </param>
        /// <param name="includeUniqueCapabilities">
        /// true の場合、独自のキャパビリティが付加されたターミナル情報が返ります。
        /// false の場合、terminfo と互換性があるターミナル情報が返ります。
        /// </param>
        /// <returns>
        /// <paramref name="terminalName"/> で指定された名前のターミナル情報である <see cref="TerminalInfo"/> オブジェクトです。
        /// </returns>
        public static TerminalInfo? FindTerminalInfo(string terminalName, bool includeUniqueCapabilities = false)
        {
            var database = TerminalInfoDatabase.FindTerminalDatabale(terminalName, includeUniqueCapabilities);
            return
                database is not null
                ? new TerminalInfo(database)
                : null;
        }

        /// <summary>
        /// 指定された terminfo ファイルを読み込みます。
        /// </summary>
        /// <param name="termInfoFile">
        /// terminfo ファイルである <see cref="FileInfo"/> オブジェクトです。
        /// </param>
        /// <param name="includeUniqueCapabilities">
        /// true の場合、独自のキャパビリティが付加されたターミナル情報が返ります。
        /// false の場合、terminfo と互換性があるターミナル情報が返ります。
        /// </param>
        /// <returns>
        /// <paramref name="termInfoFile"/> で指定された terminfo ファイルを読み込んで得られたターミナル情報である <see cref="TerminalInfo"/> オブジェクトです。
        /// </returns>
        public static TerminalInfo ReadTerminalInfo(FileInfo termInfoFile, bool includeUniqueCapabilities = false)
            => new(TerminalInfoDatabase.ReadTerminalDatabale(termInfoFile, includeUniqueCapabilities));

        /// <summary>
        /// terminfo データベースに格納されているすべてのターミナル情報を列挙します。
        /// </summary>
        /// <param name="includeUniqueCapabilities">
        /// true の場合、独自のキャパビリティが付加されたターミナル情報が返ります。
        /// false の場合、terminfo と互換性があるターミナル情報が返ります。
        /// </param>
        /// <returns>
        /// terminfo データベースに格納されているすべてのターミナル情報を列挙する列挙子です。
        /// </returns>
        public static IEnumerable<TerminalInfo> EnumerateTerminalInfo(bool includeUniqueCapabilities = false)
            => TerminalInfoDatabase.EnumerateTerminalInfoDatabase(includeUniqueCapabilities)
                .Select(terminalInfoDatabase => new TerminalInfo(terminalInfoDatabase));

        #endregion

        /// <summary>
        /// ターミナル名およびターミナルの別名の配列です。
        /// </summary>
        public IReadOnlyArray<string> TerminalNames => _database.TerminalNames;

        /// <summary>
        /// terminfo データベースのパス名です。
        /// </summary>
        public string TermInfoFilePath => _database.TermInfoFilePath;
    }
}
