using UnityEngine;
using static com.github.lhervier.ksp.shared.ugui.styles.Utils;

namespace com.github.lhervier.ksp.controlfromheremod.ui.styles
{
    /// <summary>
    /// Colors and metrics of the window, matching the mockup (dark theme, green accent #8dbe45). No
    /// color/size is hard-coded in the builders: everything comes from here. Shared values (accent,
    /// label, title bar...) are reused from the shared DefaultPalette/PopupPalette.
    /// </summary>
    public static class Palette
    {
        // ==============================================================
        // Window
        // ==============================================================
        public const float WindowWidth = 340f;
        public const float WindowHeight = 420f;

        // ==============================================================
        // Title bar — vessel name (right column)
        // ==============================================================
        public const int VesselNameFontSize = 11;
        public static readonly Color VesselNameColor = Rgb(119, 119, 119);     // #777

        // ==============================================================
        // Scrollable body
        // ==============================================================
        public const float ScrollbarWidth = 8f;
        public static readonly Color ScrollbarBgColor = Rgb(13, 13, 13);       // #0d0d0d
        public static readonly Color ScrollbarHandleColor = Rgb(85, 85, 85);   // #555
        public static readonly Color ScrollbarHandleHoverColor = Rgb(136, 136, 136); // #888

        // ==============================================================
        // Row
        // ==============================================================
        public const float RowPaddingH = 8f;
        public const float RowPaddingV = 6f;
        public const float RowSpacing = 8f;
        public static readonly Color RowActiveBgColor = Rgba(141, 190, 69, 0.06f);
        public const float RowAccentBarThickness = 2f;

        // Vessel-type icon box
        public const float TypeIconSize = 20f;
        public const int TypeIconBorderThickness = 1;
        public static readonly Color TypeIconBgColor = Rgb(31, 31, 31);        // #1f1f1f
        public static readonly Color TypeIconBorderColor = Rgb(68, 68, 68);    // #444
        public const float TypeIconInnerPadding = 3f;

        // Vessel name (main text)
        public const int NameFontSize = 13;
        public static readonly Color NameColor = Rgb(232, 232, 232);           // #e8e8e8

        // Part title (sub text)
        public const int PartTitleFontSize = 10;
        public static readonly Color PartTitleColor = Rgb(102, 102, 102);      // #666

        // Priority tag "P50"
        public const int PrioFontSize = 9;
        public const float PrioPaddingH = 5f;
        public const int PrioBorderThickness = 1;
        public static readonly Color PrioTextColor = Rgb(124, 147, 176);       // #7c93b0
        public static readonly Color PrioBorderColor = Rgb(58, 74, 96);        // #3a4a60
        public static readonly Color PrioBgColor = Rgba(124, 147, 176, 0.10f);

        // "Piloting" badge (accent), shown on the controlling module
        public const int BadgeFontSize = 9;
        public const float BadgePaddingH = 5f;
        public const int BadgeBorderThickness = 1;

        // Row action buttons (PAW / Control)
        public const float RowButtonSize = 24f;
        public const int RowButtonFontSize = 13;
        public static readonly Color RowButtonBgColor = Rgb(56, 56, 56);       // #383838
        public static readonly Color RowButtonHoverColor = Rgb(72, 72, 72);    // #484848

        // ==============================================================
        // Empty state ("no command module")
        // ==============================================================
        public const int EmptyFontSize = 12;
        public const float EmptyPaddingH = 14f;
        public const float EmptyPaddingV = 16f;
        public static readonly Color EmptyTextColor = Rgb(119, 119, 119);      // #777
    }
}
