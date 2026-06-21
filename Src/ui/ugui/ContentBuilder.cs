using com.github.lhervier.ksp.controlfromheremod.ui.styles;
using com.github.lhervier.ksp.controlfromheremod.ui.ugui.list;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.scrollableview;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui
{
    /// <summary>
    /// Popup content (everything below the shared title bar): a scrollable view whose content is the
    /// command-module list. Mounted by the shared PopupBuilder, which stretches it to fill the content host.
    /// </summary>
    public class ContentBuilder : IUGUIBuilder<ScrollableViewController>
    {
        public ScrollableViewController Build()
        {
            return new ScrollableViewBuilder<ListController>()
                .WithObjectName("ControlFromHere.Body")
                .WithContentBuilder(new ListBuilder())
                .WithScrollbarWidth(Palette.ScrollbarWidth)
                .WithScrollbarBackgroundColor(Palette.ScrollbarBgColor)
                .WithHandleColor(Palette.ScrollbarHandleColor)
                .WithHandleHoverColor(Palette.ScrollbarHandleHoverColor)
                .Build();
        }
    }
}
