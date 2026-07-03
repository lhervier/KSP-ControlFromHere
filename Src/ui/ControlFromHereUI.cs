using KSP.UI.Screens;
using UnityEngine;
using com.github.lhervier.ksp.controlfromheremod.breaker;
using com.github.lhervier.ksp.controlfromheremod.ui.ugui;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui.sprites;

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

        // Blinks the toolbar icon red while the thrust breaker is tripped.
        private ToolbarWarningAnimator _warningAnimator;

        // Single source of truth for the displayed state; guards against the toolbar/window resync loop.
        private bool _visible;

        // Previous breaker "tripped" reading, to reveal the window on the rising edge of a trip.
        private bool _wasTripped;

        // Registered once per game session (the sprite defs list is static and persists across scenes).
        private static bool _spritesRegistered;

        private void Start()
        {
            ModLogger.SetLogLevel(LogLevel.Debug);

            RegisterSprites();

            _window = new ControlWindow();
            _window.OnClosed.Add(OnWindowClosed);

            GameEvents.onGUIApplicationLauncherReady.Add(OnLauncherReady);
            LOGGER.LogInfo("Started");
        }

        // Make the mod's own inline sprites available in every TMP label (the "⚡" bolt used by the
        // breaker banner, which the game SDF font can't render). Registered once: the shared sprite-def
        // list is static, so re-registering on each flight scene would stack duplicates.
        private static void RegisterSprites()
        {
            if (_spritesRegistered)
            {
                return;
            }
            SpritesIcons.RegisterSprite("bolt", Constants.ModName + "/Textures/bolt", 0x26A1, 0.9f); // ⚡ U+26A1
            _spritesRegistered = true;
        }

        // Drive the toolbar blink from the breaker, and reveal the window when the breaker trips.
        private void Update()
        {
            ThrustBreaker breaker = ThrustBreaker.Instance;
            bool tripped = breaker != null && breaker.IsTripped;

            if (_warningAnimator != null)
            {
                _warningAnimator.Tick(tripped, Time.unscaledTime);
            }

            // On the rising edge of a trip, force the window open so the player sees why the throttle cut.
            if (tripped && !_wasTripped)
            {
                RevealWindow();
            }
            _wasTripped = tripped;
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

        // Reveal the window (e.g. the breaker just tripped). Going through the toolbar toggle keeps it in
        // sync; if the button isn't up yet SetTrue fires OnToggleOn which shows the window. When there is no
        // toolbar button, show directly.
        private void RevealWindow()
        {
            if (_toolbarButton != null)
            {
                _toolbarButton.SetTrue();
                return;
            }
            if (!_visible)
            {
                _visible = true;
                _window.Show();
            }
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
