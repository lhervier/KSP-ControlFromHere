using UnityEngine;
using com.github.lhervier.ksp.controlfromheremod.ui.styles;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui.popup;
using com.github.lhervier.ksp.controlfromheremod.ui.ugui;

namespace com.github.lhervier.ksp.controlfromheremod.ui
{
    /// <summary>
    /// Lifecycle of the uGUI window: lazy spawn, show/hide, and OnClosed notification. The low-level
    /// mechanics (PopupDialog, Escape close, scene change) are delegated to the shared PopupController.
    /// </summary>
    public sealed class ControlFromHereWindow
    {
        private const string DIALOG_ID = "ControlFromHereUGUI";

        private PopupController _popup;

        /// <summary>Fired whenever the window stops being shown (hidden by us, or closed by KSP/Escape).</summary>
        public EventVoid OnClosed = new EventVoid("ControlFromHere.Window.OnClosed");

        public void Show()
        {
            // == null is destruction-aware: after a KSP-driven close (Escape), the destroyed controller is
            // null here, which triggers a fresh spawn.
            if (_popup == null)
            {
                _popup = new PopupBuilder<TitleBarController, ContentController, MonoBehaviour>()
                    .WithPopupID(DIALOG_ID)
                    .WithTitle(ModLocalization.GetString("windowTitle"))
                    .WithIcon(LoadIcon())
                    .WithTitleBarBuilder(new TitleBarBuilder())
                    .WithContentBuilder(new ContentBuilder())
                    .WithSize(new Vector2(Palette.WindowWidth, Palette.WindowHeight))
                    .Build();
                if (_popup == null) return;   // KSP spawn failed
                _popup.OnClosed.Add(OnPopupClosed);
            }
            _popup.Show();
        }

        public void Hide()
        {
            if (_popup != null)
            {
                _popup.Hide();
            }
        }

        public void Destroy()
        {
            if (_popup != null)
            {
                _popup.OnClosed.Remove(OnPopupClosed);
                _popup.Dismiss();
                _popup = null;
            }
        }

        private void OnPopupClosed()
        {
            OnClosed.Fire();
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
