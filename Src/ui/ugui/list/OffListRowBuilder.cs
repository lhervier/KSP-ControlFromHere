using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.controlfromheremod.ui.styles;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui.list
{
    /// <summary>
    /// The special row for the vessel's off-list control point — a part with no <see cref="ModuleCommand"/>
    /// (docking port, external seat, claw...) or nothing at all. Pinned at the top of the list and set
    /// apart with an amber accent (bar, tint, "Off-list" tag), mirroring the toolbar icon that blinks in
    /// the same situation. It carries the "Piloting" badge (it is what controls the vessel) but offers no
    /// "Control from here": it is not a command module. The ⚙ "Show PAW" button stays useful when there is
    /// a part, and is disabled when the vessel is uncontrolled (no part → no PAW). The icon is neutral.
    /// </summary>
    public class OffListRowBuilder : IUGUIBuilder<OffListRowController>
    {
        private static string PawGlyph => DefaultPalette.PickGlyph("⚙", "☰", "≡", "P");

        // ===========================================
        // Builder parameters
        // ===========================================

        private OffListControlInfo _info;
        public OffListRowBuilder WithInfo(OffListControlInfo info)
        {
            this._info = info;
            return this;
        }

        // ===========================================
        // Build
        // ===========================================

        public OffListRowController Build()
        {
            var rowGo = new GameObject("OffListRow", typeof(RectTransform));

            var bg = rowGo.AddComponent<Image>();
            bg.sprite = SpritesGlobal.FillSprite;
            bg.type = Image.Type.Simple;
            bg.color = Palette.OffListRowBgColor;
            bg.raycastTarget = true;

            var layout = rowGo.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt(Palette.RowPaddingH),
                Mathf.RoundToInt(Palette.RowPaddingH),
                Mathf.RoundToInt(Palette.RowPaddingV),
                Mathf.RoundToInt(Palette.RowPaddingV));
            layout.spacing = Palette.RowSpacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Discreet bottom separator line (amber-tinted, matching the off-list accent). The off-list
            // row is always followed by the modules (or the empty state), so it is never the last row.
            RowElements.BuildRowSeparator(rowGo.transform, Palette.OffListSeparatorColor);

            RowElements.BuildAccentBar(rowGo.transform, Palette.OffListAccentColor);

            // Stock vessel-type sprite of the control point (a docking port shows its own icon); the
            // info carries VesselType.Unknown when uncontrolled, giving a neutral icon. Box stays empty
            // when the sprite is unavailable.
            RowElements.BuildIconBox(rowGo.transform, VesselTypeIcons.Get(_info.VesselType));

            BuildInfoColumn(rowGo.transform);
            BuildActions(rowGo.transform);

            return rowGo.AddComponent<OffListRowController>();
        }

        // ===========================================
        // Sub-elements
        // ===========================================

        private void BuildInfoColumn(Transform parent)
        {
            bool uncontrolled = _info.Status == ControlStatus.Uncontrolled;

            var colGo = new GameObject("Info", typeof(RectTransform));
            colGo.transform.SetParent(parent, false);
            var colLe = colGo.AddComponent<LayoutElement>();
            colLe.flexibleWidth = 1f;

            var colLayout = colGo.AddComponent<VerticalLayoutGroup>();
            colLayout.padding = new RectOffset(0, 0, 0, 0);
            colLayout.spacing = 1f;
            colLayout.childAlignment = TextAnchor.MiddleLeft;
            colLayout.childControlWidth = true;
            colLayout.childControlHeight = true;
            colLayout.childForceExpandWidth = true;
            colLayout.childForceExpandHeight = false;

            // Line 1: name (greedy) + "Off-list" tag + "Piloting" badge (only when something controls).
            var line1 = NewHLine(colGo.transform, "Line1");
            line1.GetComponent<HorizontalLayoutGroup>().spacing = DefaultPalette.Spacing;

            var nameGo = new GameObject("Name", typeof(RectTransform));
            nameGo.transform.SetParent(line1.transform, false);
            var nameLe = nameGo.AddComponent<LayoutElement>();
            // Not greedy: the name shrinks (ellipsis) only when name + chips no longer fit; the trailing
            // flexible spacer eats the remaining width, keeping the chips glued to the name on the left.
            nameLe.flexibleWidth = 0f;
            var name = UGUILabels.AddLabel(nameGo);
            name.text = uncontrolled ? ModLocalization.GetString("labelUncontrolledName") : _info.VesselName;
            name.fontSize = Palette.NameFontSize;
            name.color = uncontrolled ? Palette.OffListMutedColor : Palette.NameColor;
            name.fontStyle = uncontrolled ? FontStyles.Italic : FontStyles.Normal;
            name.alignment = TextAlignmentOptions.Left;
            name.overflowMode = TextOverflowModes.Ellipsis;

            BuildOffListTag(line1.transform);
            if (!uncontrolled)
            {
                BuildPilotingBadge(line1.transform);
            }
            // Flexible spacer: absorbs the leftover width so name + chips stay packed to the left.
            BuildFlexibleSpacer(line1.transform);

            // Line 2: part title, or the "uncontrolled" explanation.
            var titleGo = new GameObject("PartTitle", typeof(RectTransform));
            titleGo.transform.SetParent(colGo.transform, false);
            var title = UGUILabels.AddLabel(titleGo);
            title.text = uncontrolled ? ModLocalization.GetString("labelUncontrolledDesc") : _info.PartTitle;
            title.fontSize = Palette.PartTitleFontSize;
            title.color = Palette.PartTitleColor;
            title.alignment = TextAlignmentOptions.Left;
            title.overflowMode = TextOverflowModes.Ellipsis;
        }

        private void BuildOffListTag(Transform parent)
        {
            GameObject chip = RowElements.BuildChip(
                parent,
                "OffListTag",
                ModLocalization.GetString("badgeOffList").ToUpperInvariant(),
                Palette.OffListAccentColor,
                Palette.OffListTagBgColor,
                Palette.OffListTagBorderColor,
                Palette.BadgeBorderThickness,
                Palette.BadgeFontSize,
                Palette.BadgePaddingH);
            Tooltips.Attach(chip, ModLocalization.GetString("tooltipOffList"));
        }

        private void BuildPilotingBadge(Transform parent)
        {
            RowElements.BuildChip(
                parent,
                "PilotingBadge",
                ModLocalization.GetString("badgePiloting").ToUpperInvariant(),
                DefaultPalette.AccentColor,
                DefaultPalette.AccentBgColor,
                DefaultPalette.AccentBorderColor,
                Palette.BadgeBorderThickness,
                Palette.BadgeFontSize,
                Palette.BadgePaddingH);
        }

        private void BuildActions(Transform parent)
        {
            var groupGo = new GameObject("Actions", typeof(RectTransform));
            groupGo.transform.SetParent(parent, false);

            var layout = groupGo.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            bool hasPaw = _info.Part != null;

            // "Show PAW" button: useful on a real part, disabled when the vessel is uncontrolled.
            ButtonController paw = new ButtonBuilder()
                .WithObjectName("ShowPaw")
                .WithLabel(PawGlyph)
                .WithInteractableState(hasPaw)
                .WithSize(Palette.RowButtonSize)
                .WithFontSize(Palette.RowButtonFontSize)
                .WithBackgroundColor(Palette.RowButtonBgColor)
                .WithHoverColor(Palette.RowButtonHoverColor)
                .Build();
            paw.transform.SetParent(groupGo.transform, false);
            if (hasPaw)
            {
                Part part = _info.Part;
                paw.OnClick.Add(() => CommandModulesService.ShowPaw(part));
            }
            Tooltips.Attach(
                paw.gameObject,
                ModLocalization.GetString(hasPaw ? "tooltipPaw" : "tooltipNoPaw"));
        }

        private static void BuildFlexibleSpacer(Transform parent)
        {
            var go = new GameObject("Spacer", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            le.minWidth = 0f;
        }

        private static GameObject NewHLine(Transform parent, string objectName)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 0f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return go;
        }
    }
}
