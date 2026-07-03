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
    /// One command-module row: vessel-type icon, vessel name (+ priority tag + "Piloting" badge) over the
    /// localized part title, and the two action buttons (⚙ show PAW, ⌖ control from here). The control
    /// button is disabled on the module already in control. The controlling module's row is tinted.
    /// </summary>
    public class RowBuilder : IUGUIBuilder<RowController>
    {
        // No gear/crosshair glyph is guaranteed in the game SDF font; pick the first the font can render.
        private static string PawGlyph => DefaultPalette.PickGlyph("⚙", "☰", "≡", "P");          // ⚙ ☰ ≡
        private static string ControlGlyph => DefaultPalette.PickGlyph("⌖", "◎", "◉", "⊕", "C"); // ⌖ ◎ ◉ ⊕

        // ===========================================
        // Builder parameters
        // ===========================================

        private CommandModuleInfo _info;
        public RowBuilder WithInfo(CommandModuleInfo info)
        {
            this._info = info;
            return this;
        }

        // ===========================================
        // Build
        // ===========================================

        public RowController Build()
        {
            var rowGo = new GameObject("Row", typeof(RectTransform));

            // Background (tinted on the controlling module).
            var bg = rowGo.AddComponent<Image>();
            bg.sprite = SpritesGlobal.FillSprite;
            bg.type = Image.Type.Simple;
            bg.color = _info.IsActive ? Palette.RowActiveBgColor : Color.clear;
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

            // Left accent bar on the controlling module (overlaid, out of layout).
            if (_info.IsActive)
            {
                RowElements.BuildAccentBar(rowGo.transform, DefaultPalette.AccentColor);
            }

            // Stock vessel-type sprite from the shared module; the box stays empty when unavailable.
            RowElements.BuildIconBox(rowGo.transform, VesselTypeIcons.Get(_info.VesselType));
            BuildInfoColumn(rowGo.transform);
            BuildActions(rowGo.transform);

            // Hover feedback (PointerHandler, not EventTrigger, so the mouse wheel keeps scrolling the
            // list). The controlling module keeps its green tint on hover; a plain row gets the wash.
            var pointer = rowGo.AddComponent<PointerHandler>();
            Color hoverColor = _info.IsActive ? Palette.RowActiveBgColor : Palette.RowHoverColor;
            return rowGo.AddComponent<RowController>()
                .WithHover(bg, bg.color, hoverColor, pointer);
        }

        // ===========================================
        // Sub-elements
        // ===========================================

        private void BuildInfoColumn(Transform parent)
        {
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

            // Line 1: vessel name (greedy) + priority tag + "Piloting" badge.
            var line1 = NewHLine(colGo.transform, "Line1");
            line1.GetComponent<HorizontalLayoutGroup>().spacing = DefaultPalette.Spacing;

            var nameGo = new GameObject("Name", typeof(RectTransform));
            nameGo.transform.SetParent(line1.transform, false);
            var nameLe = nameGo.AddComponent<LayoutElement>();
            // Not greedy: the name takes its preferred width and shrinks (ellipsis) only when the
            // name + chips no longer fit. The trailing flexible spacer (below) eats the remaining
            // width instead, keeping the chips glued to the name on the left.
            nameLe.flexibleWidth = 0f;
            var name = UGUILabels.AddLabel(nameGo);
            name.text = _info.VesselName;
            name.fontSize = Palette.NameFontSize;
            name.color = Palette.NameColor;
            name.alignment = TextAlignmentOptions.Left;
            name.overflowMode = TextOverflowModes.Ellipsis;

            if (_info.NamingPriority > 0)
            {
                BuildPriorityTag(line1.transform);
            }
            // Orientation chip, only for parts exposing more than one control point (null otherwise).
            if (!string.IsNullOrEmpty(_info.ControlPointLabel))
            {
                BuildControlPointTag(line1.transform);
            }
            if (_info.IsActive)
            {
                BuildPilotingBadge(line1.transform);
            }
            // Flexible spacer: absorbs the leftover width so name + chips stay packed to the left.
            BuildFlexibleSpacer(line1.transform);

            // Line 2: localized part title.
            var titleGo = new GameObject("PartTitle", typeof(RectTransform));
            titleGo.transform.SetParent(colGo.transform, false);
            var title = UGUILabels.AddLabel(titleGo);
            title.text = _info.PartTitle;
            title.fontSize = Palette.PartTitleFontSize;
            title.color = Palette.PartTitleColor;
            title.alignment = TextAlignmentOptions.Left;
            title.overflowMode = TextOverflowModes.Ellipsis;
        }

        private void BuildPriorityTag(Transform parent)
        {
            GameObject chip = RowElements.BuildChip(
                parent,
                "PriorityTag",
                "P" + _info.NamingPriority,
                Palette.PrioTextColor,
                Palette.PrioBgColor,
                Palette.PrioBorderColor,
                Palette.PrioBorderThickness,
                Palette.PrioFontSize,
                Palette.PrioPaddingH);
            Tooltips.Attach(chip, ModLocalization.GetString("tooltipPriority"));
        }

        private void BuildControlPointTag(Transform parent)
        {
            GameObject chip = RowElements.BuildChip(
                parent,
                "ControlPointTag",
                _info.ControlPointLabel,
                Palette.ControlPointColor,
                Palette.ControlPointBgColor,
                Palette.ControlPointBorderColor,
                Palette.ControlPointBorderThickness,
                Palette.ControlPointFontSize,
                Palette.ControlPointPaddingH);
            Tooltips.Attach(chip, ModLocalization.GetString("tooltipControlPoint"));
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

            // "Show PAW" button.
            ButtonController paw = new ButtonBuilder()
                .WithObjectName("ShowPaw")
                .WithLabel(PawGlyph)
                .WithSize(Palette.RowButtonSize)
                .WithFontSize(Palette.RowButtonFontSize)
                .WithBackgroundColor(Palette.RowButtonBgColor)
                .WithHoverColor(Palette.RowButtonHoverColor)
                .Build();
            paw.transform.SetParent(groupGo.transform, false);
            var info = _info;
            paw.OnClick.Add(() => CommandModulesService.ShowPaw(info));
            Tooltips.Attach(paw.gameObject, ModLocalization.GetString("tooltipPaw"));

            // "Control from here" button (accent), disabled on the controlling module.
            ButtonController control = new ButtonBuilder()
                .WithObjectName("ControlFromHere")
                .WithLabel(ControlGlyph)
                .WithInteractableState(!_info.IsActive)
                .WithSize(Palette.RowButtonSize)
                .WithFontSize(Palette.RowButtonFontSize)
                .WithBackgroundColor(Palette.RowButtonBgColor)
                .WithHoverColor(Palette.RowButtonHoverColor)
                .WithTextColor(DefaultPalette.AccentColor)
                .Build();
            control.transform.SetParent(groupGo.transform, false);
            control.OnClick.Add(() => CommandModulesService.ControlFromHere(info));
            Tooltips.Attach(
                control.gameObject,
                ModLocalization.GetString(_info.IsActive ? "tooltipAlreadyControlling" : "tooltipControl"));
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
