using System.IO;
using KSP.UI.Screens;
using UnityEngine;
using com.github.lhervier.ksp.controlfromheremod.ui.styles;
using com.github.lhervier.ksp.shared;

namespace com.github.lhervier.ksp.controlfromheremod.ui
{
    /// <summary>
    /// Blinks the toolbar button red while the thrust circuit breaker is tripped, so the player notices the
    /// throttle has been cut even with the window closed: a hard red blink when tripped, the plain icon
    /// otherwise.
    /// Self-contained: it builds red-tinted frames from the mod's own icon PNG (loaded as a *readable*
    /// texture from disk, since the GameDatabase copy usually is not) and swaps them via
    /// <see cref="ApplicationLauncherButton.SetTexture"/>. If the icon can't be read it stays idle.
    /// </summary>
    public class ToolbarWarningAnimator
    {
        private static readonly ModLogger LOGGER = new ModLogger("ToolbarWarn");

        // Gradient resolution from the plain icon (frame 0) to full warning red (last frame).
        private const int FrameCount = 16;

        private readonly ApplicationLauncherButton _button;
        private readonly Texture2D _baseTexture;         // frame 0, the untouched icon
        private readonly Texture2D[] _frames;            // [0] == _baseTexture, [1..] increasingly red
        private int _appliedIndex = -1;

        public ToolbarWarningAnimator(ApplicationLauncherButton button)
        {
            _button = button;
            _baseTexture = LoadReadableIcon();
            if (_baseTexture != null)
            {
                _frames = BuildRedFrames(_baseTexture);
            }
        }

        // Ready only when both the button and the generated frames are available.
        private bool IsReady => _button != null && _frames != null;

        /// <summary>
        /// Update the icon for the breaker state: a hard red blink while <paramref name="tripped"/>, the
        /// plain icon otherwise. <paramref name="time"/> should be <see cref="Time.unscaledTime"/> so the
        /// blink stays smooth regardless of time warp / pause.
        /// </summary>
        public void Tick(bool tripped, float time)
        {
            if (!IsReady)
            {
                return;
            }

            int index = 0; // plain icon
            if (tripped)
            {
                // Hard blink: full red for half the cycle, plain icon the other half (shared cadence).
                index = WarningBlink.IsOn(time) ? FrameCount - 1 : 0;
            }

            Apply(index);
        }

        /// <summary>Force the plain icon back (e.g. before teardown).</summary>
        public void Reset()
        {
            Apply(0);
        }

        public void Destroy()
        {
            if (_frames != null)
            {
                // _frames[0] == _baseTexture, destroyed once below.
                for (int i = 1; i < _frames.Length; i++)
                {
                    if (_frames[i] != null)
                    {
                        Object.Destroy(_frames[i]);
                    }
                }
            }
            if (_baseTexture != null)
            {
                Object.Destroy(_baseTexture);
            }
        }

        private void Apply(int index)
        {
            if (index == _appliedIndex)
            {
                return;
            }
            _appliedIndex = index;
            _button.SetTexture(_frames[index]);
        }

        // Multiply-tint the icon toward the warning red across FrameCount steps. Multiply keeps the
        // glyph's shape and anti-aliasing (a white glyph becomes a clean red glyph); alpha is untouched
        // so transparent areas stay transparent.
        private static Texture2D[] BuildRedFrames(Texture2D baseTex)
        {
            Color32[] src;
            try
            {
                src = baseTex.GetPixels32();
            }
            catch (System.Exception e)
            {
                LOGGER.LogWarning("Icon texture is not readable, warning animation disabled: " + e.Message);
                return null;
            }

            Color red = Palette.WarningColor;
            var frames = new Texture2D[FrameCount];
            frames[0] = baseTex;
            for (int f = 1; f < FrameCount; f++)
            {
                float t = (float)f / (FrameCount - 1);       // 0 (white) .. 1 (full red)
                float tintR = Mathf.Lerp(1f, red.r, t);
                float tintG = Mathf.Lerp(1f, red.g, t);
                float tintB = Mathf.Lerp(1f, red.b, t);

                var px = new Color32[src.Length];
                for (int i = 0; i < src.Length; i++)
                {
                    px[i] = new Color32(
                        (byte)(src[i].r * tintR),
                        (byte)(src[i].g * tintG),
                        (byte)(src[i].b * tintB),
                        src[i].a);
                }

                var tex = new Texture2D(baseTex.width, baseTex.height, TextureFormat.RGBA32, false);
                tex.SetPixels32(px);
                tex.Apply(false);
                frames[f] = tex;
            }
            return frames;
        }

        // Load the mod's icon straight from disk so the resulting texture is readable (GetPixels32).
        // The GameDatabase copy KSP loads is typically flagged non-readable and would throw.
        private static Texture2D LoadReadableIcon()
        {
            string path = Path.Combine(
                KSPUtil.ApplicationRootPath,
                "GameData/" + Constants.ModName + "/icon.png");
            try
            {
                if (!File.Exists(path))
                {
                    LOGGER.LogWarning("Icon not found at " + path + ", warning animation disabled.");
                    return null;
                }
                byte[] data = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!tex.LoadImage(data))
                {
                    LOGGER.LogWarning("Failed to decode icon at " + path + ", warning animation disabled.");
                    Object.Destroy(tex);
                    return null;
                }
                return tex;
            }
            catch (System.Exception e)
            {
                LOGGER.LogError("Error loading icon for warning animation: " + e.Message);
                return null;
            }
        }
    }
}
