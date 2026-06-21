using UnityEngine;
using TMPro;
using com.github.lhervier.ksp.controlfromheremod.ui.styles;
using com.github.lhervier.ksp.shared.ugui;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui
{
    /// <summary>
    /// Builds the popup title bar's right column: a single label showing the active vessel's name. The
    /// title bar frame, the title on the left and the ✕ close button are provided by the shared PopupBuilder.
    /// </summary>
    public class TitleBarBuilder : IUGUIBuilder<TitleBarController>
    {
        public TitleBarController Build()
        {
            var go = new GameObject("ControlFromHere.TitleBar.VesselName", typeof(RectTransform));

            var label = UGUILabels.AddLabel(go);
            label.fontSize = Palette.VesselNameFontSize;
            label.color = Palette.VesselNameColor;
            label.alignment = TextAlignmentOptions.Right;

            return go.AddComponent<TitleBarController>()
                .WithLabelComponent(label);
        }
    }
}
