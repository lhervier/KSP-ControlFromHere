using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.controlfromheremod.ui.styles;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.sprites;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui.list
{
    /// <summary>
    /// Reusable sub-elements shared by the row builders (<see cref="RowBuilder"/> for command modules and
    /// <see cref="OffListRowBuilder"/> for the off-list control point): the bordered vessel-type icon box,
    /// the small rounded chips (priority tag, "Piloting" badge, "Off-list" tag) and the left accent bar.
    /// </summary>
    internal static class RowElements
    {
        /// <summary>
        /// Bordered icon box holding an optional sprite. The box is kept even when the sprite is null
        /// (e.g. the neutral off-list icon or an unavailable stock sprite), so rows stay aligned.
        /// </summary>
        public static void BuildIconBox(Transform parent, Sprite icon)
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

            if (icon == null)
            {
                return;
            }

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

        /// <summary>Left accent bar overlaid on the row (out of layout), used to mark the controlling
        /// module (accent color) and the off-list control point (off-list color).</summary>
        public static void BuildAccentBar(Transform parent, Color color)
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
            img.color = color;
            img.raycastTarget = false;
        }

        /// <summary>Small rounded chip: sliced border image + centered colored label. Width driven by
        /// its content. Returns the chip GameObject so callers can attach a tooltip.</summary>
        public static GameObject BuildChip(
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
            image.raycastTarget = true;   // so a tooltip can be hovered

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
    }
}
