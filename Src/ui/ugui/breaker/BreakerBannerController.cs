using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.controlfromheremod.breaker;
using com.github.lhervier.ksp.controlfromheremod.ui.styles;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui.breaker
{
    /// <summary>
    /// Fills and refreshes the circuit-breaker banner from <see cref="ThrustBreaker"/>: the master enable
    /// switch, the arm-state dot + label, the live threshold stepper, and — while tripped — the alert row
    /// with its "rearm without switching" action.
    ///
    /// The structure is built once and then <b>updated in place</b> on every breaker state change (only
    /// texts, colors and visibility change) — nothing is destroyed and recreated, so buttons and labels
    /// never flash while the state toggles.
    /// </summary>
    public class BreakerBannerController : MonoBehaviour
    {
        // The bolt is a registered inline sprite (the SDF font has no ⚡); tint=1 makes it take the label color.
        private const string Bolt = "<sprite name=\"bolt\" tint=1>";

        // Glyphs that may be missing from the game SDF font: pick the first the font can render.
        private static string DotGlyph => DefaultPalette.PickGlyph("●", "•", "*");
        private static string RearmGlyph => DefaultPalette.PickGlyph("⤒", "↑", "^");
        private static string CheckGlyph => DefaultPalette.PickGlyph("✓", "√", "»");

        private bool _built;
        private bool _subscribed;

        // Persistent elements updated by Refresh (never destroyed).
        private TextMeshProUGUI _bannerLabel;
        private Image _switchBg;
        private TextMeshProUGUI _switchLabel;
        private Color _switchRestingBg;
        private GameObject _stateDot;
        private TextMeshProUGUI _stateDotLabel;
        private TextMeshProUGUI _stateText;
        private DotBlinkController _dotBlink;
        private GameObject _thresholdGroup;
        private TextMeshProUGUI _thresholdLabel;
        private TextMeshProUGUI _thresholdValue;
        private GameObject _alertRow;
        private TextMeshProUGUI _alertMsg;

        // ============================================================
        // Lifecycle
        // ============================================================

        public void Start()
        {
            Subscribe();
            EnsureBuilt();
            Refresh();
        }

        // OnEnable can run before Start (and after re-show): build if needed and catch up on the state.
        public void OnEnable()
        {
            Subscribe();
            EnsureBuilt();
            Refresh();
        }

        public void OnDestroy()
        {
            if (_subscribed && ThrustBreaker.Instance != null)
            {
                ThrustBreaker.Instance.OnStateChanged.Remove(OnBreakerStateChanged);
            }
            _subscribed = false;
        }

        private void Subscribe()
        {
            if (_subscribed || ThrustBreaker.Instance == null)
            {
                return;
            }
            ThrustBreaker.Instance.OnStateChanged.Add(OnBreakerStateChanged);
            _subscribed = true;
        }

        private void OnBreakerStateChanged() => Refresh();

        // ============================================================
        // Build (once)
        // ============================================================

        private void EnsureBuilt()
        {
            if (_built)
            {
                return;
            }
            BuildStructure();
            _built = true;
        }

        private void BuildStructure()
        {
            GameObject row = NewHorizontalRow(
                "Main",
                new RectOffset(
                    Mathf.RoundToInt(Palette.BannerPaddingH),
                    Mathf.RoundToInt(Palette.BannerPaddingH),
                    Mathf.RoundToInt(Palette.BannerPaddingV),
                    Mathf.RoundToInt(Palette.BannerPaddingV)),
                Palette.BannerSpacing);
            row.transform.SetParent(transform, false);

            _bannerLabel = AddLabel(row.transform, "Label",
                Bolt + " " + ModLocalization.GetString("breakerLabel"),
                Palette.BannerLabelFontSize, Palette.BannerLabelColor, FontStyles.Bold);

            BuildSwitch(row.transform);
            BuildState(row.transform);
            AddSpacer(row.transform);
            BuildThreshold(row.transform);

            BuildAlert(transform);
        }

        // Enable switch, built without ButtonController so it can be restyled in place (the shared button
        // bakes its colors and offers no runtime restyle). A raycast-target background + a PointerHandler
        // gives click and hover; colors are driven directly by Refresh.
        private void BuildSwitch(Transform parent)
        {
            var go = new GameObject("EnableSwitch", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = Palette.BannerMiniHeight;

            _switchBg = go.AddComponent<Image>();
            _switchBg.sprite = SpritesGlobal.FillSprite;
            _switchBg.type = Image.Type.Simple;
            _switchBg.raycastTarget = true;

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt(Palette.BannerSwitchPaddingH),
                Mathf.RoundToInt(Palette.BannerSwitchPaddingH), 0, 0);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            _switchLabel = UGUILabels.AddLabel(labelGo);
            _switchLabel.fontSize = Palette.BannerSwitchFontSize;
            _switchLabel.alignment = TextAlignmentOptions.Center;

            var pointer = go.AddComponent<PointerHandler>();
            pointer.OnClick = () =>
            {
                ThrustBreaker breaker = ThrustBreaker.Instance;
                if (breaker != null) breaker.SetEnabled(!breaker.Enabled);
            };
            pointer.OnEnter = () => { if (_switchBg != null) _switchBg.color = Brighten(_switchRestingBg); };
            pointer.OnExit = () => { if (_switchBg != null) _switchBg.color = _switchRestingBg; };

            Tooltips.Attach(go, ModLocalization.GetString("tooltipBreakerToggle"));
        }

        // State cell: a dot (hidden when disabled) next to a steady text label. The dot blinks (in
        // lock-step with the toolbar icon) only while tripped.
        private void BuildState(Transform parent)
        {
            GameObject row = NewHorizontalRow("State", new RectOffset(0, 0, 0, 0), 5f);
            row.transform.SetParent(parent, false);

            _stateDotLabel = AddLabel(row.transform, "Dot", DotGlyph,
                Palette.BannerStateFontSize, Palette.BannerArmedColor, FontStyles.Bold);
            _stateDot = _stateDotLabel.gameObject;
            _dotBlink = _stateDot.AddComponent<DotBlinkController>().WithLabel(_stateDotLabel);

            _stateText = AddLabel(row.transform, "Text", string.Empty,
                Palette.BannerStateFontSize, Palette.BannerArmedColor, FontStyles.Bold);
        }

        private void BuildThreshold(Transform parent)
        {
            _thresholdGroup = NewHorizontalRow("Threshold", new RectOffset(0, 0, 0, 0), 3f);
            _thresholdGroup.transform.SetParent(parent, false);

            _thresholdLabel = AddLabel(_thresholdGroup.transform, "SeuilLabel",
                ModLocalization.GetString("breakerThreshold"),
                Palette.BannerThresholdFontSize, Palette.BannerLabelColor, FontStyles.Normal);

            BuildStepButton(_thresholdGroup.transform, "Minus", "−", -1f); // − (minus sign)

            _thresholdValue = AddLabel(_thresholdGroup.transform, "Value", string.Empty,
                Palette.BannerThresholdValueFontSize, Palette.BannerThresholdValueColor, FontStyles.Bold);

            BuildStepButton(_thresholdGroup.transform, "Plus", "+", 1f);

            Tooltips.Attach(_thresholdGroup, ModLocalization.GetString("tooltipBreakerThreshold"));
        }

        private void BuildStepButton(Transform parent, string name, string glyph, float delta)
        {
            ButtonController button = new ButtonBuilder()
                .WithObjectName(name)
                .WithLabel(glyph)
                .WithSize(Palette.BannerStepButtonSize)
                .WithFontSize(Palette.BannerThresholdFontSize)
                .WithBackgroundColor(Palette.BannerStepBgColor)
                .WithHoverColor(Palette.BannerStepHoverColor)
                .WithTextColor(Palette.BannerStepTextColor)
                .Build();
            button.transform.SetParent(parent, false);
            button.OnClick.Add(() =>
            {
                ThrustBreaker breaker = ThrustBreaker.Instance;
                if (breaker != null) breaker.SetThreshold(breaker.Threshold + delta);
            });
        }

        // Alert row (built once, shown only while tripped): dynamic message + static recommendation + rearm.
        private void BuildAlert(Transform parent)
        {
            _alertRow = new GameObject("Alert", typeof(RectTransform));
            _alertRow.transform.SetParent(parent, false);

            var bg = _alertRow.AddComponent<Image>();
            bg.sprite = SpritesGlobal.FillSprite;
            bg.type = Image.Type.Simple;
            bg.color = Palette.BannerAlertBgColor;
            bg.raycastTarget = false;

            var layout = _alertRow.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt(Palette.BannerAlertPaddingH),
                Mathf.RoundToInt(Palette.BannerAlertPaddingH),
                Mathf.RoundToInt(Palette.BannerAlertPaddingV),
                Mathf.RoundToInt(Palette.BannerAlertPaddingV));
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            _alertMsg = AddLabel(_alertRow.transform, "Message", string.Empty,
                Palette.BannerAlertMsgFontSize, Palette.BannerAlertMsgColor, FontStyles.Bold);
            _alertMsg.enableWordWrapping = true;

            // Recommendation caption (static). The "✓ Aligné" fragment is colored green (rich text) so it
            // reads as the same cue as the chip on the suggested rows.
            string alignedFragment = "<color=#" + ColorUtility.ToHtmlStringRGB(Palette.AlignedTextColor) + ">"
                + CheckGlyph + " " + ModLocalization.GetString("badgeAligned") + "</color>";
            var recoLabel = AddLabel(_alertRow.transform, "RecoLabel",
                ModLocalization.GetString("breakerReco", alignedFragment),
                Palette.BannerRecoLabelFontSize, Palette.BannerRecoLabelColor, FontStyles.Normal);
            recoLabel.enableWordWrapping = true;

            // "Rearm without switching" on its own left-aligned line.
            GameObject buttonRow = NewHorizontalRow("RearmRow", new RectOffset(0, 0, 0, 0), 0f);
            buttonRow.transform.SetParent(_alertRow.transform, false);

            ButtonController rearm = new ButtonBuilder()
                .WithObjectName("Rearm")
                .WithLabel(RearmGlyph + " " + ModLocalization.GetString("breakerRearm"))
                .WithAutoWidth(Palette.BannerMiniPaddingH)
                .WithSize(Palette.BannerMiniHeight)
                .WithFontSize(Palette.BannerMiniFontSize)
                .WithBackgroundColor(Palette.BannerMiniBgColor)
                .WithHoverColor(Palette.BannerMiniHoverColor)
                .WithTextColor(Palette.BannerMiniTextColor)
                .Build();
            rearm.transform.SetParent(buttonRow.transform, false);
            rearm.OnClick.Add(() =>
            {
                ThrustBreaker breaker = ThrustBreaker.Instance;
                if (breaker != null) breaker.Rearm();
            });
        }

        // ============================================================
        // Refresh (in place)
        // ============================================================

        private void Refresh()
        {
            if (!_built)
            {
                return;
            }

            ThrustBreaker breaker = ThrustBreaker.Instance;
            bool enabled = breaker != null && breaker.Enabled;
            bool tripped = breaker != null && breaker.IsTripped;
            float threshold = breaker != null ? breaker.Threshold : BreakerSettings.DefaultThreshold;

            _bannerLabel.color = enabled ? Palette.BannerLabelColor : Palette.BannerLabelDisabledColor;

            _switchLabel.text = ModLocalization.GetString(enabled ? "breakerEnabled" : "breakerDisabled");
            _switchLabel.color = enabled ? Palette.BannerSwitchOnText : Palette.BannerSwitchOffText;
            _switchRestingBg = enabled ? Palette.BannerSwitchOnBg : Palette.BannerSwitchOffBg;
            _switchBg.color = _switchRestingBg;

            if (!enabled)
            {
                _stateDot.SetActive(false);
                _dotBlink.SetBlinking(false);
                _stateText.text = ModLocalization.GetString("breakerIdle");
                _stateText.color = Palette.BannerIdleColor;
                _stateText.fontStyle = FontStyles.Italic;
            }
            else
            {
                Color color = tripped ? Palette.BannerTrippedColor : Palette.BannerArmedColor;
                _stateDot.SetActive(true);
                _stateDotLabel.color = color;
                _dotBlink.SetBlinking(tripped);
                _stateText.text = ModLocalization.GetString(tripped ? "breakerDisarmed" : "breakerArmed");
                _stateText.color = color;
                _stateText.fontStyle = FontStyles.Bold;
            }

            // Threshold stepper is hidden while tripped (the angle is recalled in the alert; rearm first).
            _thresholdGroup.SetActive(!tripped);
            _thresholdLabel.color = enabled ? Palette.BannerLabelColor : Palette.BannerLabelDisabledColor;
            _thresholdValue.text = Mathf.RoundToInt(threshold) + "°";

            _alertRow.SetActive(tripped);
            if (tripped)
            {
                _alertMsg.text = Bolt + " " + ModLocalization.GetString("breakerAlertMsg", Mathf.RoundToInt(threshold));
            }
        }

        // ============================================================
        // Helpers
        // ============================================================

        private static GameObject NewHorizontalRow(string name, RectOffset padding, float spacing)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = padding;
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return go;
        }

        private static TextMeshProUGUI AddLabel(
            Transform parent, string name, string text, int fontSize, Color color, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var label = UGUILabels.AddLabel(go);
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.fontStyle = style;
            label.alignment = TextAlignmentOptions.Left;
            return label;
        }

        // Empty greedy cell that consumes leftover width, pushing what follows to the right edge.
        private static void AddSpacer(Transform parent)
        {
            var go = new GameObject("Spacer", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;
        }

        // Slightly lightened color for the switch hover (mockup: filter brightness(1.2)), alpha kept.
        private static Color Brighten(Color c)
        {
            return new Color(
                Mathf.Min(1f, c.r * 1.25f + 0.03f),
                Mathf.Min(1f, c.g * 1.25f + 0.03f),
                Mathf.Min(1f, c.b * 1.25f + 0.03f),
                c.a);
        }
    }
}
