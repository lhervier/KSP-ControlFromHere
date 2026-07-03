using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui.list
{
    /// <summary>
    /// Controller of a command-module row. The row content is static (rebuilt by the
    /// <see cref="ListController"/> whenever the vessel or its control point changes) and the action
    /// buttons are wired to the service directly by the builder; the controller only owns the hover
    /// feedback, tinting the row background while the pointer is over it and restoring the base tint on exit.
    /// </summary>
    public class RowController : MonoBehaviour
    {
        private Image _bg;
        private Color _baseColor;
        private Color _hoverColor;
        private PointerHandler _pointer;
        private GameObject _separator;

        /// <summary>Registers the row's bottom separator line so the <see cref="ListController"/> can
        /// hide it on the last row (no trailing line under the list).</summary>
        public RowController WithSeparator(GameObject separator)
        {
            _separator = separator;
            return this;
        }

        /// <summary>Hides the bottom separator line (used on the last row of the list).</summary>
        public void HideSeparator()
        {
            if (_separator != null) _separator.SetActive(false);
        }

        /// <summary>Wires the row's hover feedback: <paramref name="bg"/> is tinted to
        /// <paramref name="hoverColor"/> while hovered and back to <paramref name="baseColor"/> on exit
        /// (pass the base color as the hover color to opt out of any visible change).</summary>
        public RowController WithHover(Image bg, Color baseColor, Color hoverColor, PointerHandler pointer)
        {
            _bg = bg;
            _baseColor = baseColor;
            _hoverColor = hoverColor;
            _pointer = pointer;
            return this;
        }

        public void Start()
        {
            if (_pointer != null)
            {
                _pointer.OnEnter = OnEnter;
                _pointer.OnExit = OnExit;
            }
        }

        public void OnDestroy()
        {
            if (_pointer != null)
            {
                _pointer.OnEnter = null;
                _pointer.OnExit = null;
            }
        }

        private void OnEnter()
        {
            if (_bg != null) _bg.color = _hoverColor;
        }

        private void OnExit()
        {
            if (_bg != null) _bg.color = _baseColor;
        }
    }
}
