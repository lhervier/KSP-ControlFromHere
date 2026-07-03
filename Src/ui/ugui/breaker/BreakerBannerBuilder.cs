using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.controlfromheremod.ui.styles;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.sprites;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui.breaker
{
    /// <summary>
    /// The always-visible circuit-breaker banner mounted at the top of the window. Builds the container
    /// (tinted background, vertical stack that grows to fit its rows) and hands it to the
    /// <see cref="BreakerBannerController"/>, which fills and refreshes it from the breaker's state.
    /// </summary>
    public class BreakerBannerBuilder : IUGUIBuilder<BreakerBannerController>
    {
        public BreakerBannerController Build()
        {
            var go = new GameObject("ControlFromHere.Breaker", typeof(RectTransform));

            var bg = go.AddComponent<Image>();
            bg.sprite = SpritesGlobal.FillSprite;
            bg.type = Image.Type.Simple;
            bg.color = Palette.BannerBgColor;
            bg.raycastTarget = true;

            // Stack the main row over the (optional) alert row. Height is driven by the rows' preferred
            // heights so the parent content layout can reserve exactly what the banner needs.
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 0f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            return go.AddComponent<BreakerBannerController>();
        }
    }
}
