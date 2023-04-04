using System;

namespace Palmtree.Terminal
{
    internal static class TermInfoBooleanCapabilitiesExtensions
    {
        public static string ToJsonPropertyName(this TermInfoBooleanCapabilities valueName)
            => valueName switch
            {
                TermInfoBooleanCapabilities.AutoLeftMargin => "auto_left_margin",
                TermInfoBooleanCapabilities.AutoRightMargin => "auto_right_margin",
                TermInfoBooleanCapabilities.NoEscCtlc => "no_esc_ctlc",
                TermInfoBooleanCapabilities.CeolStandoutGlitch => "ceol_standout_glitch",
                TermInfoBooleanCapabilities.EatNewlineGlitch => "eat_newline_glitch",
                TermInfoBooleanCapabilities.EraseOverstrike => "erase_overstrike",
                TermInfoBooleanCapabilities.GenericType => "generic_type",
                TermInfoBooleanCapabilities.HardCopy => "hard_copy",
                TermInfoBooleanCapabilities.HasMetaKey => "has_meta_key",
                TermInfoBooleanCapabilities.HasStatusLine => "has_status_line",
                TermInfoBooleanCapabilities.InsertNullGlitch => "insert_null_glitch",
                TermInfoBooleanCapabilities.MemoryAbove => "memory_above",
                TermInfoBooleanCapabilities.MemoryBelow => "memory_below",
                TermInfoBooleanCapabilities.MoveInsertMode => "move_insert_mode",
                TermInfoBooleanCapabilities.MoveStandoutMode => "move_standout_mode",
                TermInfoBooleanCapabilities.OverStrike => "over_strike",
                TermInfoBooleanCapabilities.StatusLineEscOk => "status_line_esc_ok",
                TermInfoBooleanCapabilities.DestTabsMagicSmso => "dest_tabs_magic_smso",
                TermInfoBooleanCapabilities.TildeGlitch => "tilde_glitch",
                TermInfoBooleanCapabilities.TransparentUnderline => "transparent_underline",
                TermInfoBooleanCapabilities.XonXoff => "xon_xoff",
                TermInfoBooleanCapabilities.NeedsXonXoff => "needs_xon_xoff",
                TermInfoBooleanCapabilities.PrtrSilent => "prtr_silent",
                TermInfoBooleanCapabilities.HardCursor => "hard_cursor",
                TermInfoBooleanCapabilities.NonRevRmcup => "non_rev_rmcup",
                TermInfoBooleanCapabilities.NoPadChar => "no_pad_char",
                TermInfoBooleanCapabilities.NonDestScrollRegion => "non_dest_scroll_region",
                TermInfoBooleanCapabilities.CanChange => "can_change",
                TermInfoBooleanCapabilities.BackColorErase => "back_color_erase",
                TermInfoBooleanCapabilities.HueLightnessSaturation => "hue_lightness_saturation",
                TermInfoBooleanCapabilities.ColAddrGlitch => "col_addr_glitch",
                TermInfoBooleanCapabilities.CrCancelsMicroMode => "cr_cancels_micro_mode",
                TermInfoBooleanCapabilities.HasPrintWheel => "has_print_wheel",
                TermInfoBooleanCapabilities.RowAddrGlitch => "row_addr_glitch",
                TermInfoBooleanCapabilities.SemiAutoRightMargin => "semi_auto_right_margin",
                TermInfoBooleanCapabilities.CpiChangesRes => "cpi_changes_res",
                TermInfoBooleanCapabilities.LpiChangesRes => "lpi_changes_res",
                TermInfoBooleanCapabilities.BackspacesWithBs => "backspaces_with_bs",
                TermInfoBooleanCapabilities.CrtNoScrolling => "crt_no_scrolling",
                TermInfoBooleanCapabilities.NoCorrectlyWorkingCr => "no_correctly_working_cr",
                TermInfoBooleanCapabilities.GnuHasMetaKey => "gnu_has_meta_key",
                TermInfoBooleanCapabilities.LinefeedIsNewline => "linefeed_is_newline",
                TermInfoBooleanCapabilities.HasHardwareTabs => "has_hardware_tabs",
                TermInfoBooleanCapabilities.ReturnDoesClrEol => "return_does_clr_eol",
                _ => throw new ArgumentException($"Not supported value name: {valueName}", nameof(valueName)),
            };
    }
}
