using KSP.UI.Screens;
using UnityEngine;
using com.github.lhervier.ksp.controlfromheremod.ui.ugui;
using com.github.lhervier.ksp.shared;

namespace com.github.lhervier.ksp.controlfromheremod.ui
{
    /// <summary>
    /// Entry point in flight: the toolbar button and the uGUI window it toggles. Keeps the toolbar toggle
    /// and the window visibility in sync, including when KSP closes the window itself (Escape).
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ControlFromHereUI : MonoBehaviour
    {
        private static readonly ModLogger LOGGER = new ModLogger("UI");

        private ApplicationLauncherButton _toolbarButton;
        private ControlWindow _window;

        // Pulses/blinks the toolbar icon red when the active vessel is controlled off a command module.
        private ToolbarWarningAnimator _warningAnimator;

        // Single source of truth for the displayed state; guards against the toolbar/window resync loop.
        private bool _visible;

        private void Start()
        {
            ModLogger.SetLogLevel(LogLevel.Debug);

            _window = new ControlWindow();
            _window.OnClosed.Add(OnWindowClosed);

            GameEvents.onGUIApplicationLauncherReady.Add(OnLauncherReady);
            LOGGER.LogInfo("Started");
        }

        // Drive the toolbar warning animation once per frame from the active vessel's control point.
        private void Update()
        {
            if (_warningAnimator == null)
            {
                return;
            }
            ControlStatus status = CommandModulesService.GetControlStatus(FlightGlobals.ActiveVessel);
            _warningAnimator.Tick(status, Time.unscaledTime);
        }

        private void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnLauncherReady);

            if (_warningAnimator != null)
            {
                _warningAnimator.Destroy();
                _warningAnimator = null;
            }

            RemoveToolbarButton();

            if (_window != null)
            {
                _window.OnClosed.Remove(OnWindowClosed);
                _window.Destroy();
                _window = null;
            }
        }

        // ==========================================================================
        // Toolbar
        // ==========================================================================

        private void OnLauncherReady()
        {
            if (_toolbarButton != null)
            {
                return;
            }
            try
            {
                _toolbarButton = ApplicationLauncher.Instance.AddModApplication(
                    OnToggleOn,
                    OnToggleOff,
                    null, null, null, null,
                    ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                    GameDatabase.Instance.GetTexture(Constants.ModName + "/icon", false) ?? Texture2D.whiteTexture);

                _warningAnimator = new ToolbarWarningAnimator(_toolbarButton);
            }
            catch (System.Exception e)
            {
                LOGGER.LogError("Error creating toolbar button: " + e.Message);
            }
        }

        private void RemoveToolbarButton()
        {
            if (_toolbarButton == null)
            {
                return;
            }
            try
            {
                ApplicationLauncher.Instance.RemoveModApplication(_toolbarButton);
            }
            catch (System.Exception e)
            {
                LOGGER.LogError("Error removing toolbar button: " + e.Message);
            }
            _toolbarButton = null;
        }

        // ==========================================================================
        // Visibility
        // ==========================================================================

        private void OnToggleOn()
        {
            if (_visible) return;
            _visible = true;
            _window.Show();
        }

        private void OnToggleOff()
        {
            if (!_visible) return;
            _visible = false;
            _window.Hide();
        }

        // Window stopped being shown (our Hide, or a KSP/Escape close): resync the toolbar toggle.
        private void OnWindowClosed()
        {
            if (!_visible) return;
            _visible = false;
            if (_toolbarButton != null)
            {
                _toolbarButton.SetFalse();
            }
        }
    }
}
