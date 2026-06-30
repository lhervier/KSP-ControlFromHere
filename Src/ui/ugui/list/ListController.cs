using System.Collections.Generic;
using UnityEngine;
using TMPro;
using com.github.lhervier.ksp.controlfromheremod.ui.styles;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui.list
{
    /// <summary>
    /// Builds and refreshes the command-module rows from the active vessel. Rebuilds on vessel change,
    /// vessel modification (docking/undocking, part add/remove) and control-point switch. Skips rebuilds
    /// while the window is hidden and catches up on re-open (OnEnable).
    /// </summary>
    public class ListController : MonoBehaviour
    {
        private static readonly ModLogger LOGGER = new ModLogger("List");

        public void Start()
        {
            GameEvents.onVesselChange.Add(OnVesselChange);
            GameEvents.onVesselWasModified.Add(OnVesselWasModified);
            GameEvents.onVesselReferenceTransformSwitch.Add(OnReferenceTransformSwitch);
            Rebuild();
        }

        public void OnDestroy()
        {
            GameEvents.onVesselChange.Remove(OnVesselChange);
            GameEvents.onVesselWasModified.Remove(OnVesselWasModified);
            GameEvents.onVesselReferenceTransformSwitch.Remove(OnReferenceTransformSwitch);
        }

        // Catch up on the state that may have changed while the window was hidden.
        public void OnEnable()
        {
            Rebuild();
        }

        // ============================================
        // Methods bound to events
        // ============================================

        private void OnVesselChange(Vessel vessel) => RebuildIfVisible();
        private void OnVesselWasModified(Vessel vessel) => RebuildIfVisible();
        private void OnReferenceTransformSwitch(Transform from, Transform to) => RebuildIfVisible();

        private void RebuildIfVisible()
        {
            // A hidden window catches up through OnEnable; no need to rebuild into the void.
            if (!isActiveAndEnabled)
            {
                return;
            }
            Rebuild();
        }

        // =======================================
        // Internal helpers
        // =======================================

        private void Rebuild()
        {
            // Clear existing children.
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            Vessel vessel = FlightGlobals.ActiveVessel;
            if (vessel == null)
            {
                BuildEmptyState(ModLocalization.GetString("CFHM_labelNoVessel"));
                return;
            }

            List<CommandModuleInfo> modules = CommandModulesService.GetCommandModules(vessel);
            if (modules.Count == 0)
            {
                BuildEmptyState(ModLocalization.GetString("CFHM_labelNoCommandModules"));
                return;
            }

            foreach (CommandModuleInfo info in modules)
            {
                RowController row = new RowBuilder().WithInfo(info).Build();
                row.transform.SetParent(transform, false);
            }
            LOGGER.LogDebug("Rebuilt list: " + modules.Count + " command module(s) for " + vessel.vesselName);
        }

        private void BuildEmptyState(string message)
        {
            var go = new GameObject("Empty", typeof(RectTransform));
            go.transform.SetParent(transform, false);

            var layout = go.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt(Palette.EmptyPaddingH),
                Mathf.RoundToInt(Palette.EmptyPaddingH),
                Mathf.RoundToInt(Palette.EmptyPaddingV),
                Mathf.RoundToInt(Palette.EmptyPaddingV));
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            var label = UGUILabels.AddLabel(labelGo);
            label.text = message;
            label.fontSize = Palette.EmptyFontSize;
            label.color = Palette.EmptyTextColor;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = true;
        }
    }
}
