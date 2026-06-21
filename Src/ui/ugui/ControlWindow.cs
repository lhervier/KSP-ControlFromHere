using com.github.lhervier.ksp.shared.ugui.popup;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui
{
    /// <summary>
    /// Lifecycle of the uGUI window: lazy spawn through ModPopupBuilder, show/hide, and OnClosed
    /// notification. The low-level mechanics (PopupDialog, Escape close, scene change) are delegated to the
    /// shared PopupController.
    /// </summary>
    public sealed class ControlWindow
    {
        private PopupController _popup;

        /// <summary>Fired whenever the window stops being shown (hidden by us, or closed by KSP/Escape).</summary>
        public EventVoid OnClosed = new EventVoid("ControlFromHere.Window.OnClosed");

        public void Show()
        {
            // == null is destruction-aware: after a KSP-driven close (Escape), the destroyed controller is
            // null here, which triggers a fresh spawn.
            if (_popup == null)
            {
                _popup = new ModPopupBuilder().Build();
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
    }
}
