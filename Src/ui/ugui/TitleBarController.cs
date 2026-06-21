using UnityEngine;
using TMPro;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui
{
    /// <summary>
    /// Right-side content of the popup title bar: the active vessel's global name, read as-is from KSP
    /// (never recomputed from naming priorities). Refreshed on vessel change and rename.
    /// </summary>
    public class TitleBarController : MonoBehaviour
    {
        private TextMeshProUGUI _label;
        public TitleBarController WithLabelComponent(TextMeshProUGUI label)
        {
            this._label = label;
            return this;
        }

        public void Start()
        {
            GameEvents.onVesselChange.Add(OnVesselChange);
            GameEvents.onVesselRename.Add(OnVesselRename);
            UpdateName();
        }

        public void OnDestroy()
        {
            GameEvents.onVesselChange.Remove(OnVesselChange);
            GameEvents.onVesselRename.Remove(OnVesselRename);
        }

        public void OnEnable()
        {
            UpdateName();
        }

        private void OnVesselChange(Vessel vessel) => UpdateName();
        private void OnVesselRename(GameEvents.HostedFromToAction<Vessel, string> action) => UpdateName();

        private void UpdateName()
        {
            if (_label == null) return;
            Vessel vessel = FlightGlobals.ActiveVessel;
            _label.text = vessel != null ? vessel.vesselName : string.Empty;
        }
    }
}
