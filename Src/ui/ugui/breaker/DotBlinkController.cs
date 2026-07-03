using UnityEngine;
using TMPro;
using com.github.lhervier.ksp.controlfromheremod.ui;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui.breaker
{
    /// <summary>
    /// Blinks a TMP label (the breaker's "disarmed" state dot) in lock-step with the toolbar warning icon,
    /// using the shared <see cref="WarningBlink"/> cadence: full opacity on, dimmed off. Blinking is toggled
    /// with <see cref="SetBlinking"/>; when off, the label stays fully opaque.
    /// </summary>
    public class DotBlinkController : MonoBehaviour
    {
        private const float OffAlpha = 0.2f; // matches the mockup's blink keyframe (opacity .2)

        private TextMeshProUGUI _label;
        private bool _blinking;

        public DotBlinkController WithLabel(TextMeshProUGUI label)
        {
            _label = label;
            return this;
        }

        /// <summary>Start or stop blinking. Stopping restores full opacity.</summary>
        public void SetBlinking(bool blinking)
        {
            _blinking = blinking;
            if (!_blinking && _label != null)
            {
                _label.alpha = 1f;
            }
        }

        public void Update()
        {
            if (!_blinking || _label == null)
            {
                return;
            }
            _label.alpha = WarningBlink.IsOn(Time.unscaledTime) ? 1f : OffAlpha;
        }
    }
}
