using System.Collections;
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

        // Guards against stacking rebuild coroutines when several events fire in the same frame.
        private bool _rebuildQueued;

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

        private void OnVesselChange(Vessel vessel) => QueueRebuild();
        private void OnVesselWasModified(Vessel vessel) => QueueRebuild();
        private void OnReferenceTransformSwitch(Transform from, Transform to) => QueueRebuild();

        // Rebuild on the next frame rather than synchronously: onVesselReferenceTransformSwitch is
        // fired by Vessel.SetReferenceTransform *before* it updates referenceTransformPart, so an
        // immediate rebuild would read the old control point (and miss the switch to a docking port).
        // A hidden window skips this and catches up through OnEnable on re-open.
        private void QueueRebuild()
        {
            if (_rebuildQueued || !isActiveAndEnabled)
            {
                return;
            }
            _rebuildQueued = true;
            StartCoroutine(RebuildNextFrame());
        }

        private IEnumerator RebuildNextFrame()
        {
            yield return null;
            Rebuild();   // clears _rebuildQueued
        }

        // =======================================
        // Internal helpers
        // =======================================

        private void Rebuild()
        {
            _rebuildQueued = false;

            // Clear existing children.
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            Vessel vessel = FlightGlobals.ActiveVessel;
            if (vessel == null)
            {
                BuildEmptyState(ModLocalization.GetString("labelNoVessel"));
                return;
            }

            // Pinned row for the control point when it is not one of the listed command modules — the
            // very situation that makes the toolbar icon blink (docking port, seat... or nothing at all).
            OffListControlInfo offList = CommandModulesService.GetOffListControlPoint(vessel);
            if (offList != null)
            {
                OffListRowController offRow = new OffListRowBuilder().WithInfo(offList).Build();
                offRow.transform.SetParent(transform, false);
            }

            List<CommandModuleInfo> modules = CommandModulesService.GetCommandModules(vessel);
            if (modules.Count == 0)
            {
                BuildEmptyState(ModLocalization.GetString("labelNoCommandModules"));
                return;
            }

            // Separate the pinned off-list row from the modules the player can actually switch to.
            if (offList != null)
            {
                BuildSeparator(ModLocalization.GetString("labelAvailableModules"));
            }

            foreach (CommandModuleInfo info in modules)
            {
                RowController row = new RowBuilder().WithInfo(info).Build();
                row.transform.SetParent(transform, false);
            }
            LOGGER.LogDebug("Rebuilt list: " + modules.Count + " command module(s) for " + vessel.vesselName
                + (offList != null ? " (off-list control: " + offList.Status + ")" : ""));
        }

        // A thin caption row introducing the switchable modules under the pinned off-list row.
        private void BuildSeparator(string message)
        {
            var go = new GameObject("Separator", typeof(RectTransform));
            go.transform.SetParent(transform, false);

            var layout = go.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt(Palette.RowPaddingH),
                Mathf.RoundToInt(Palette.RowPaddingH),
                Mathf.RoundToInt(Palette.SeparatorPaddingV),
                Mathf.RoundToInt(Palette.SeparatorPaddingV));
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            var label = UGUILabels.AddLabel(labelGo);
            label.text = message.ToUpperInvariant();
            label.fontSize = Palette.SeparatorFontSize;
            label.color = Palette.SeparatorTextColor;
            label.alignment = TextAlignmentOptions.Left;
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
