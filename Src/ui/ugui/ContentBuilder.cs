using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.controlfromheremod.ui.styles;
using com.github.lhervier.ksp.controlfromheremod.ui.ugui.breaker;
using com.github.lhervier.ksp.controlfromheremod.ui.ugui.list;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.scrollableview;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui
{
    /// <summary>
    /// Popup content (everything below the shared title bar): the always-visible circuit-breaker banner and
    /// the "command modules: X" count sub-header, stacked over the scrollable command-module list. The banner
    /// and sub-header keep their natural height at the top; the list fills the remaining space. Mounted by the
    /// shared PopupBuilder, which stretches it to fill the host.
    /// </summary>
    public class ContentBuilder : IUGUIBuilder<ContentController>
    {
        public ContentController Build()
        {
            var go = new GameObject("ControlFromHere.Content", typeof(RectTransform));

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 0f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Banner: natural (preferred) height, pinned at the top.
            BreakerBannerController banner = new BreakerBannerBuilder().Build();
            banner.transform.SetParent(go.transform, false);

            // Count sub-header: natural height, below the banner.
            CountHeaderController count = new CountHeaderBuilder().Build();
            count.transform.SetParent(go.transform, false);

            // List: takes all the leftover height and scrolls within it.
            ScrollableViewController scroll = new ScrollableViewBuilder<ListController>()
                .WithObjectName("ControlFromHere.Body")
                .WithContentBuilder(new ListBuilder())
                .WithScrollbarWidth(Palette.ScrollbarWidth)
                .WithScrollbarBackgroundColor(Palette.ScrollbarBgColor)
                .WithHandleColor(Palette.ScrollbarHandleColor)
                .WithHandleHoverColor(Palette.ScrollbarHandleHoverColor)
                .Build();
            scroll.transform.SetParent(go.transform, false);
            var scrollLe = scroll.gameObject.AddComponent<LayoutElement>();
            scrollLe.flexibleHeight = 1f;

            return go.AddComponent<ContentController>();
        }
    }
}
