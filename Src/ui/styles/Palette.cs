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
        // Faint white wash on hover for a plain (non-controlling) module row. The controlling module
        // keeps its green tint on hover; the off-list row keeps its amber tint — as in the mockup.
        public static readonly Color RowHoverColor = Rgba(255, 255, 255, 0.03f);
        public const float RowAccentBarThickness = 2f;
        // Discreet line separating each row, barely lighter than the window background (#1c1c1c).
        public const float RowSeparatorThickness = 1f;
        public static readonly Color RowSeparatorColor = Rgb(28, 28, 28);      // #1c1c1c

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

        // Control-point orientation chip (e.g. "Reversed"), shown only on parts with several control points
        public const int ControlPointFontSize = 9;
        public const float ControlPointPaddingH = 5f;
        public const int ControlPointBorderThickness = 1;
        public static readonly Color ControlPointColor = Rgb(176, 143, 202);   // #b08fca
        public static readonly Color ControlPointBorderColor = Rgb(74, 58, 94); // #4a3a5e
        public static readonly Color ControlPointBgColor = Rgba(176, 143, 202, 0.10f);

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
        // Off-list control-point row (docking port, seat... or nothing)
        // ==============================================================
        // Amber, orthogonal to the green "piloting" accent: green = in control, amber = off the list.
        public static readonly Color OffListAccentColor = Rgb(208, 162, 74);   // #d0a24a
        public static readonly Color OffListRowBgColor = Rgba(200, 150, 60, 0.05f);
        public static readonly Color OffListTagBgColor = Rgba(200, 150, 60, 0.10f);
        public static readonly Color OffListTagBorderColor = Rgb(110, 90, 32); // #6e5a20
        public static readonly Color OffListMutedColor = Rgb(138, 138, 138);   // #8a8a8a (uncontrolled name)
        // Amber-tinted variant of the row separator, for the off-list row (#2a2418).
        public static readonly Color OffListSeparatorColor = Rgb(42, 36, 24);  // #2a2418

        // Caption separating the pinned off-list row from the switchable modules
        public const int SeparatorFontSize = 9;
        public const float SeparatorPaddingV = 3f;
        public static readonly Color SeparatorTextColor = Rgb(106, 90, 48);    // #6a5a30

        // ==============================================================
        // Command-modules count sub-header (fixed, between banner and list)
        // ==============================================================
        public static readonly Color CountBgColor = Rgb(21, 21, 21);           // #151515
        public const int CountFontSize = 10;
        public const float CountPaddingH = 10f;
        public const float CountPaddingV = 4f;
        public static readonly Color CountTextColor = Rgb(136, 136, 136);      // #888
        public const float CountSeparatorThickness = 1f;
        public static readonly Color CountSeparatorColor = Rgb(34, 34, 34);    // #222

        // ==============================================================
        // Circuit-breaker banner (always visible, top of the window)
        // ==============================================================
        public static readonly Color BannerBgColor = Rgb(26, 26, 26);          // #1a1a1a
        public const float BannerPaddingH = 10f;
        public const float BannerPaddingV = 6f;
        public const float BannerSpacing = 8f;

        // "⚡ Disjoncteur" label (dimmed when disabled)
        public const int BannerLabelFontSize = 10;
        public static readonly Color BannerLabelColor = Rgb(136, 136, 136);    // #888
        public static readonly Color BannerLabelDisabledColor = Rgb(90, 90, 90); // #5a5a5a

        // Master enable switch (green when enabled, grey when disabled)
        public const int BannerSwitchFontSize = 11;
        public const float BannerSwitchPaddingH = 9f;
        public static readonly Color BannerSwitchOnText = Rgb(141, 190, 69);    // #8dbe45
        public static readonly Color BannerSwitchOnBg = Rgba(141, 190, 69, 0.12f);
        public static readonly Color BannerSwitchOnBorder = Rgb(74, 110, 32);   // #4a6e20
        public static readonly Color BannerSwitchOffText = Rgb(138, 138, 138);  // #8a8a8a
        public static readonly Color BannerSwitchOffBg = Rgb(36, 36, 36);       // #242424
        public static readonly Color BannerSwitchOffBorder = Rgb(68, 68, 68);   // #444

        // Arm-state pill / idle caption
        public const int BannerStateFontSize = 11;
        public static readonly Color BannerArmedColor = Rgb(141, 190, 69);      // #8dbe45
        public static readonly Color BannerTrippedColor = Rgb(224, 96, 58);     // #e0603a
        public static readonly Color BannerIdleColor = Rgb(90, 90, 90);         // #5a5a5a

        // Threshold stepper (Seuil − 5° +)
        public const int BannerThresholdFontSize = 10;
        public const int BannerThresholdValueFontSize = 12;
        public const float BannerStepButtonSize = 16f;
        public static readonly Color BannerThresholdValueColor = Rgb(203, 176, 106); // #cbb06a
        public static readonly Color BannerStepBgColor = Rgb(44, 44, 44);       // #2c2c2c
        public static readonly Color BannerStepHoverColor = Rgb(60, 60, 60);    // #3c3c3c
        public static readonly Color BannerStepTextColor = Rgb(170, 170, 170);  // #aaa

        // Trip alert row
        public const float BannerAlertPaddingH = 10f;
        public const float BannerAlertPaddingV = 8f;
        public static readonly Color BannerAlertBgColor = Rgba(224, 96, 58, 0.09f);
        public const int BannerAlertMsgFontSize = 11;
        public static readonly Color BannerAlertMsgColor = Rgb(236, 138, 106);  // #ec8a6a
        public const int BannerRecoLabelFontSize = 10;
        public static readonly Color BannerRecoLabelColor = Rgb(154, 138, 128); // #9a8a80

        // Inline "Rearm" mini button (green accent)
        public const int BannerMiniFontSize = 10;
        public const float BannerMiniPaddingH = 8f;
        public const float BannerMiniHeight = 18f;
        public static readonly Color BannerMiniBgColor = Rgb(51, 51, 51);       // #333
        public static readonly Color BannerMiniHoverColor = Rgb(69, 69, 69);    // #454545
        public static readonly Color BannerMiniTextColor = Rgb(141, 190, 69);   // #8dbe45

        // "✓ Aligné" chip on a suggested module row (while tripped)
        public const int AlignedFontSize = 9;
        public const float AlignedPaddingH = 5f;
        public const int AlignedBorderThickness = 1;
        public static readonly Color AlignedTextColor = Rgb(169, 217, 106);     // #a9d96a
        public static readonly Color AlignedBgColor = Rgba(141, 190, 69, 0.12f);
        public static readonly Color AlignedBorderColor = Rgb(74, 110, 32);     // #4a6e20

        // ==============================================================
        // Toolbar warning (thrust breaker tripped)
        // ==============================================================
        // Warm alert red (Radix "red 9"), not a raw #FF0000. The toolbar icon is tinted toward this
        // color and hard-blinks while the thrust circuit breaker is tripped.
        public static readonly Color WarningColor = Rgb(229, 72, 77);          // #e5484d

        // ==============================================================
        // Empty state ("no command module")
        // ==============================================================
        public const int EmptyFontSize = 12;
        public const float EmptyPaddingH = 14f;
        public const float EmptyPaddingV = 16f;
        public static readonly Color EmptyTextColor = Rgb(119, 119, 119);      // #777
    }
}
