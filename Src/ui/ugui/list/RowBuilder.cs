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
                BuildAccentBar(rowGo.transform);
            }

            BuildTypeIcon(rowGo.transform);
            BuildInfoColumn(rowGo.transform);
            BuildActions(rowGo.transform);

            return rowGo.AddComponent<RowController>();
        }

        // ===========================================
        // Sub-elements
        // ===========================================

        private void BuildAccentBar(Transform parent)
        {
            var go = new GameObject("AccentBar", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(Palette.RowAccentBarThickness, 0f);
            rect.anchoredPosition = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.sprite = SpritesGlobal.FillSprite;
            img.type = Image.Type.Simple;
            img.color = DefaultPalette.AccentColor;
            img.raycastTarget = false;
        }

        private void BuildTypeIcon(Transform parent)
        {
            var boxGo = new GameObject("TypeIcon", typeof(RectTransform));
            boxGo.transform.SetParent(parent, false);
            var le = boxGo.AddComponent<LayoutElement>();
            le.minWidth = le.preferredWidth = Palette.TypeIconSize;
            le.minHeight = le.preferredHeight = Palette.TypeIconSize;

            var box = boxGo.AddComponent<Image>();
            box.sprite = SpritesGlobal.Border(
                Palette.TypeIconBgColor,
                Palette.TypeIconBorderColor,
                Palette.TypeIconBorderThickness);
            box.type = Image.Type.Sliced;
            box.color = Color.white;
            box.raycastTarget = false;

            // Stock vessel-type sprite from the shared module; leave the box empty when unavailable.
            Sprite icon = VesselTypeIcons.Get(_info.VesselType);
            if (icon != null)
            {
                var iconGo = new GameObject("Glyph", typeof(RectTransform));
                iconGo.transform.SetParent(boxGo.transform, false);
                var iconRect = iconGo.GetComponent<RectTransform>();
                iconRect.anchorMin = Vector2.zero;
                iconRect.anchorMax = Vector2.one;
                iconRect.offsetMin = new Vector2(Palette.TypeIconInnerPadding, Palette.TypeIconInnerPadding);
                iconRect.offsetMax = new Vector2(-Palette.TypeIconInnerPadding, -Palette.TypeIconInnerPadding);
                var iconImg = iconGo.AddComponent<Image>();
                iconImg.sprite = icon;
                iconImg.type = Image.Type.Simple;
                iconImg.preserveAspect = true;
                iconImg.color = Color.white;
                iconImg.raycastTarget = false;
            }
        }

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
            nameLe.flexibleWidth = 1f;
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
            if (_info.IsActive)
            {
                BuildPilotingBadge(line1.transform);
            }

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
            GameObject chip = BuildChip(
                parent,
                "PriorityTag",
                "P" + _info.NamingPriority,
                Palette.PrioTextColor,
                Palette.PrioBgColor,
                Palette.PrioBorderColor,
                Palette.PrioBorderThickness,
                Palette.PrioFontSize,
                Palette.PrioPaddingH);
            Tooltips.Attach(chip, ModLocalization.GetString("CFHM_tooltipPriority"));
        }

        private void BuildPilotingBadge(Transform parent)
        {
            BuildChip(
                parent,
                "PilotingBadge",
                ModLocalization.GetString("CFHM_badgePiloting").ToUpperInvariant(),
                DefaultPalette.AccentColor,
                DefaultPalette.AccentBgColor,
                DefaultPalette.AccentBorderColor,
                Palette.BadgeBorderThickness,
                Palette.BadgeFontSize,
                Palette.BadgePaddingH);
        }

        // Small rounded chip: sliced border image + centered colored label. Width driven by its content.
        private static GameObject BuildChip(
            Transform parent,
            string objectName,
            string text,
            Color textColor,
            Color bgColor,
            Color borderColor,
            int borderThickness,
            int fontSize,
            float paddingH)
        {
            var chipGo = new GameObject(objectName, typeof(RectTransform));
            chipGo.transform.SetParent(parent, false);

            var image = chipGo.AddComponent<Image>();
            image.sprite = SpritesGlobal.Border(bgColor, borderColor, borderThickness);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = true;   // so the priority tooltip can be hovered

            var layout = chipGo.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(Mathf.RoundToInt(paddingH), Mathf.RoundToInt(paddingH), 1, 1);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(chipGo.transform, false);
            var label = UGUILabels.AddLabel(labelGo);
            label.text = text;
            label.fontSize = fontSize;
            label.color = textColor;
            label.alignment = TextAlignmentOptions.Center;

            return chipGo;
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
            Tooltips.Attach(paw.gameObject, ModLocalization.GetString("CFHM_tooltipPaw"));

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
                ModLocalization.GetString(_info.IsActive ? "CFHM_tooltipAlreadyControlling" : "CFHM_tooltipControl"));
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
