using KSP.UI.Screens;
using UnityEngine;
using com.github.lhervier.ksp.controlfromheremod.breaker;
using com.github.lhervier.ksp.controlfromheremod.ui.styles;
using com.github.lhervier.ksp.controlfromheremod.ui.ugui;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui.popup;
using com.github.lhervier.ksp.shared.ugui.sprites;

namespace com.github.lhervier.ksp.controlfromheremod.ui
{
    /// <summary>
    /// Entry point in flight: the toolbar button and the uGUI window it toggles. The window is driven by a
    /// (shared) PopupController that handles its own lazy spawn, position, and open state; we only open/close
    /// it and react to OnOpenChanged to keep the toolbar toggle in sync (including when KSP closes the window
    /// itself via Escape, or when it is restored open at scene load).
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ControlFromHereUI : MonoBehaviour
    {
        private static readonly ModLogger LOGGER = new ModLogger("UI");

        private const string DIALOG_ID = "ControlFromHereUGUI";

        private ApplicationLauncherButton _toolbarButton;
        private PopupController _popupController;

        // Blinks the toolbar icon red while the thrust breaker is tripped.
        private ToolbarWarningAnimator _warningAnimator;

        // Previous breaker "tripped" reading, to reveal the window on the rising edge of a trip.
        private bool _wasTripped;

        // Registered once per game session (the sprite defs list is static and persists across scenes).
        private static bool _spritesRegistered;

        private void Start()
        {
            ModLogger.SetLogLevel(LogLevel.Debug);

            RegisterSprites();

            // The popup controller is a component on THIS GameObject: it survives KSP destroying the window
            // (Escape) and persists its own open state, so we no longer track visibility ourselves.
            _popupController = new PopupBuilder<TitleBarController, ContentController, MonoBehaviour>()
                .WithHost(this.gameObject)
                .WithPopupID(DIALOG_ID)
                .WithTitle(ModLocalization.GetString("windowTitle"))
                .WithIcon(LoadIcon())
                .WithTitleBarBuilder(new TitleBarBuilder())
                .WithContentBuilder(new ContentBuilder())
                .WithSize(new Vector2(Palette.WindowWidth, Palette.WindowHeight))
                .Build();
            // The controller restores its own open state (in its Start, after this method returns), so we
            // only subscribe: a restored-open window then syncs the toolbar through this handler.
            if (_popupController != null)
            {
                _popupController.OnOpenChanged.Add(OnPopupOpenChanged);
            }

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

            // _popupController is a component on this GO: Unity destroys it with us, and it dismisses a
            // still-open window in its own OnDestroy. We only drop our reference and unsubscribe.
            if (_popupController != null)
            {
                _popupController.OnOpenChanged.Remove(OnPopupOpenChanged);
                _popupController = null;
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

            // The launcher may become ready after the window state was restored at scene load: press the
            // button now to reflect an already-open window (false: no callback).
            if (_toolbarButton != null && _popupController != null && _popupController.IsOpen)
            {
                _toolbarButton.SetTrue(false);
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
            if (_popupController != null) _popupController.Show();
        }

        private void OnToggleOff()
        {
            if (_popupController != null) _popupController.Hide();
        }

        // Reveal the window (e.g. the breaker just tripped). The open transition syncs the toolbar through
        // OnOpenChanged.
        private void RevealWindow()
        {
            if (_popupController != null) _popupController.Show();
        }

        // The window's open state changed (button, ×, Escape, or restore-at-load): keep the toolbar button
        // pressed state in sync. SetTrue/SetFalse(false): do not re-fire the toggle callbacks.
        private void OnPopupOpenChanged()
        {
            if (_toolbarButton == null)
            {
                return;
            }
            if (_popupController != null && _popupController.IsOpen)
            {
                _toolbarButton.SetTrue(false);
            }
            else
            {
                _toolbarButton.SetFalse(false);
            }
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
