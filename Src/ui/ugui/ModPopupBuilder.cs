using UnityEngine;
using com.github.lhervier.ksp.controlfromheremod.ui.styles;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.popup;
using com.github.lhervier.ksp.shared.ugui.scrollableview;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui
{
    /// <summary>
    /// Spawns the Control-From-Here window on top of the shared PopupBuilder: title + window icon, the
    /// vessel name in the title bar's right column, and the scrollable command-module list as content.
    /// Returns the shared PopupController the caller drives, or null if KSP failed to spawn the popup.
    /// </summary>
    public class ModPopupBuilder : IUGUIBuilder<PopupController>
    {
        private const string DIALOG_ID = "ControlFromHereUGUI";

        public PopupController Build()
        {
            return new PopupBuilder<TitleBarController, ContentController, MonoBehaviour>()
                .WithPopupID(DIALOG_ID)
                .WithTitle(ModLocalization.GetString("windowTitle"))
                .WithIcon(LoadIcon())
                .WithTitleBarBuilder(new TitleBarBuilder())
                .WithContentBuilder(new ContentBuilder())
                .WithSize(new Vector2(Palette.WindowWidth, Palette.WindowHeight))
                .Build();
        }

        // The mod's toolbar icon, reused as the window's title-bar icon. Null when the texture is missing.
        private static Sprite LoadIcon()
        {
            Texture2D tex = GameDatabase.Instance != null
                ? GameDatabase.Instance.GetTexture(Constants.ModName + "/icon", false)
                : null;
            if (tex == null)
            {
                return null;
            }
            return Sprite.Create(
                tex,
                new Rect(0f, 0f, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f);
        }
    }
}
