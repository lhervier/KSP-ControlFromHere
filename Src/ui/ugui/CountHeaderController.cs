using UnityEngine;
using TMPro;
using com.github.lhervier.ksp.shared;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui
{
    /// <summary>
    /// Keeps the "Command modules: X" caption in sync with the active vessel, refreshing on vessel change
    /// and vessel modification (docking/undocking, part add/remove) — the events that change how many
    /// command modules the vessel has.
    /// </summary>
    public class CountHeaderController : MonoBehaviour
    {
        private TextMeshProUGUI _label;

        public CountHeaderController WithLabel(TextMeshProUGUI label)
        {
            _label = label;
            return this;
        }

        public void Start()
        {
            GameEvents.onVesselChange.Add(OnVesselChange);
            GameEvents.onVesselWasModified.Add(OnVesselWasModified);
            Refresh();
        }

        public void OnDestroy()
        {
            GameEvents.onVesselChange.Remove(OnVesselChange);
            GameEvents.onVesselWasModified.Remove(OnVesselWasModified);
        }

        // Catch up on the count that may have changed while the window was hidden.
        public void OnEnable()
        {
            Refresh();
        }

        private void OnVesselChange(Vessel vessel) => Refresh();
        private void OnVesselWasModified(Vessel vessel) => Refresh();

        private void Refresh()
        {
            if (_label == null)
            {
                return;
            }
            int count = CommandModulesService.GetCommandModules(FlightGlobals.ActiveVessel).Count;
            _label.text = ModLocalization.GetString("labelCommandModulesCount", count).ToUpperInvariant();
        }
    }
}
