using System;

namespace Palmtree.Terminal
{
    internal static class TermInfoNumberCapabilitiesExtensions
    {
        public static string ToJsonPropertyName(this TermInfoNumberCapabilities valueName)
            => valueName switch
            {
                TermInfoNumberCapabilities.Columns => "columns",
                TermInfoNumberCapabilities.InitTabs => "init_tabs",
                TermInfoNumberCapabilities.Lines => "lines",
                TermInfoNumberCapabilities.LinesOfMemory => "lines_of_memory",
                TermInfoNumberCapabilities.MagicCookieGlitch => "magic_cookie_glitch",
                TermInfoNumberCapabilities.PaddingBaudRate => "padding_baud_rate",
                TermInfoNumberCapabilities.VirtualTerminal => "virtual_terminal",
                TermInfoNumberCapabilities.WidthStatusLine => "width_status_line",
                TermInfoNumberCapabilities.NumLabels => "num_labels",
                TermInfoNumberCapabilities.LabelHeight => "label_height",
                TermInfoNumberCapabilities.LabelWidth => "label_width",
                TermInfoNumberCapabilities.MaxAttributes => "max_attributes",
                TermInfoNumberCapabilities.MaximumWindows => "maximum_windows",
                TermInfoNumberCapabilities.MaxColors => "max_colors",
                TermInfoNumberCapabilities.MaxPairs => "max_pairs",
                TermInfoNumberCapabilities.NoColorVideo => "no_color_video",
                TermInfoNumberCapabilities.BufferCapacity => "buffer_capacity",
                TermInfoNumberCapabilities.DotVertSpacing => "dot_vert_spacing",
                TermInfoNumberCapabilities.DotHorzSpacing => "dot_horz_spacing",
                TermInfoNumberCapabilities.MaxMicroAddress => "max_micro_address",
                TermInfoNumberCapabilities.MaxMicroJump => "max_micro_jump",
                TermInfoNumberCapabilities.MicroColSize => "micro_col_size",
                TermInfoNumberCapabilities.MicroLineSize => "micro_line_size",
                TermInfoNumberCapabilities.NumberOfPins => "number_of_pins",
                TermInfoNumberCapabilities.OutputResChar => "output_res_char",
                TermInfoNumberCapabilities.OutputResLine => "output_res_line",
                TermInfoNumberCapabilities.OutputResHorzInch => "output_res_horz_inch",
                TermInfoNumberCapabilities.OutputResVertInch => "output_res_vert_inch",
                TermInfoNumberCapabilities.PrintRate => "print_rate",
                TermInfoNumberCapabilities.WideCharSize => "wide_char_size",
                TermInfoNumberCapabilities.Buttons => "buttons",
                TermInfoNumberCapabilities.BitImageEntwining => "bit_image_entwining",
                TermInfoNumberCapabilities.BitImageType => "bit_image_type",
                TermInfoNumberCapabilities.MagicCookieGlitchUl => "magic_cookie_glitch_ul",
                TermInfoNumberCapabilities.CarriageReturnDelay => "carriage_return_delay",
                TermInfoNumberCapabilities.NewLineDelay => "new_line_delay",
                TermInfoNumberCapabilities.BackspaceDelay => "backspace_delay",
                TermInfoNumberCapabilities.HorizontalTabDelay => "horizontal_tab_delay",
                TermInfoNumberCapabilities.NumberOfFunctionKeys => "number_of_function_keys",
                _ => throw new ArgumentException($"Not supported value name: {valueName}", nameof(valueName)),
            };
    }
}
