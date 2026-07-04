using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.controlfromheremod.ui.styles;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui.badge;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui.list
{
    /// <summary>
    /// Reusable sub-elements shared by the row builders (<see cref="RowBuilder"/> for command modules and
    /// <see cref="OffListRowBuilder"/> for the off-list control point): the bordered vessel-type icon box,
    /// the shared "Piloting" badge and the left accent bar. Small labelled chips are built directly from
    /// the shared <see cref="BadgeBuilder"/>.
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

        /// <summary>Thin separator line overlaid at the bottom of a row (out of layout), so rows read as
        /// distinct entries. Returns the line GameObject so the caller can hide it on the last row.</summary>
        public static GameObject BuildRowSeparator(Transform parent, Color color)
        {
            var go = new GameObject("RowSeparator", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            var rect = go.GetComponent<RectTransform>();
            // Stretch horizontally, pinned to the bottom edge, one line tall.
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(0f, Palette.RowSeparatorThickness);
            rect.anchoredPosition = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.sprite = SpritesGlobal.FillSprite;
            img.type = Image.Type.Simple;
            img.color = color;
            img.raycastTarget = false;
            return go;
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

        /// <summary>The "Piloting" accent badge, shared by the command-module row and the off-list row.</summary>
        public static void BuildPilotingBadge(Transform parent)
        {
            new BadgeBuilder()
                .WithParent(parent)
                .WithObjectName("PilotingBadge")
                .WithText(ModLocalization.GetString("badgePiloting").ToUpperInvariant())
                .WithColors(DefaultPalette.AccentColor, DefaultPalette.AccentBgColor, DefaultPalette.AccentBorderColor)
                .WithBorderThickness(Palette.BadgeBorderThickness)
                .WithFontSize(Palette.BadgeFontSize)
                .WithPadding(Palette.BadgePaddingH, 1)
                .Build();
        }
    }
}
