using System.Collections.Generic;
using com.github.lhervier.ksp.shared;

namespace com.github.lhervier.ksp.controlfromheremod
{
    /// <summary>
    /// Where the vessel's current control point (reference transform) sits, from the mod's point of view.
    /// </summary>
    public enum ControlStatus
    {
        /// <summary>The control point is a command module we list. Nothing to warn about.</summary>
        OnCommandModule,

        /// <summary>The control point is a part with no <see cref="ModuleCommand"/> — a docking port,
        /// an external seat, a claw... The player may be piloting from an unexpected part.</summary>
        OffCommandModule,

        /// <summary>The vessel has no reference transform part at all: nothing is controlling it.</summary>
        Uncontrolled
    }

    /// <summary>
    /// Reads the command modules of a vessel and performs the "Control From Here" action. A command
    /// module is a part bearing a <see cref="ModuleCommand"/>; docking ports (which expose "Control
    /// From Here" through ModuleDockingNode, not ModuleCommand) are therefore excluded.
    /// </summary>
    public static class CommandModulesService
    {
        private static readonly ModLogger LOGGER = new ModLogger("CommandModules");

        /// <summary>
        /// Classifies the vessel's current control point (<see cref="Vessel.GetReferenceTransformPart"/>)
        /// against the command modules we list. Used to warn when the player might be piloting from an
        /// unexpected part (typical after an undock). A null/empty vessel is treated as non-warning.
        /// </summary>
        public static ControlStatus GetControlStatus(Vessel vessel)
        {
            if (vessel == null || vessel.parts == null || vessel.parts.Count == 0)
            {
                return ControlStatus.OnCommandModule;
            }

            Part reference = vessel.GetReferenceTransformPart();
            if (reference == null)
            {
                return ControlStatus.Uncontrolled;
            }
            if (reference.FindModuleImplementing<ModuleCommand>() == null)
            {
                return ControlStatus.OffCommandModule;
            }
            return ControlStatus.OnCommandModule;
        }

        /// <summary>
        /// The off-list control point of the vessel — the part currently in control when it is not one
        /// of the listed command modules — or null when the vessel is controlled from a command module
        /// (nothing special to show then). Mirrors <see cref="GetControlStatus"/>: a non-null result is
        /// returned exactly when the toolbar icon blinks (<see cref="ControlStatus.OffCommandModule"/> or
        /// <see cref="ControlStatus.Uncontrolled"/>). The name/type follow the same rule as the modules:
        /// the part's own naming only when the player actually named it.
        /// </summary>
        public static OffListControlInfo GetOffListControlPoint(Vessel vessel)
        {
            ControlStatus status = GetControlStatus(vessel);
            if (status == ControlStatus.OnCommandModule)
            {
                return null;
            }
            if (status == ControlStatus.Uncontrolled)
            {
                return new OffListControlInfo(status, null, null, null, VesselType.Unknown);
            }

            // OffCommandModule: the reference part exists but bears no ModuleCommand.
            Part reference = vessel.GetReferenceTransformPart();
            VesselNaming naming = reference.vesselNaming;
            bool hasPlayerNaming = naming != null && !string.IsNullOrEmpty(naming.vesselName);
            string vesselName = hasPlayerNaming ? naming.vesselName : vessel.vesselName;
            VesselType vesselType = hasPlayerNaming ? naming.vesselType : vessel.vesselType;
            string partTitle = reference.partInfo != null ? reference.partInfo.title : reference.name;
            return new OffListControlInfo(status, reference, vesselName, partTitle, vesselType);
        }

        /// <summary>
        /// The command modules of the given vessel, sorted by naming priority (desc), then vessel name,
        /// then part title. Empty list when the vessel is null or has no command module.
        /// </summary>
        public static List<CommandModuleInfo> GetCommandModules(Vessel vessel)
        {
            var result = new List<CommandModuleInfo>();
            if (vessel == null || vessel.parts == null)
            {
                return result;
            }

            // The part currently in control (reference transform), read as-is from KSP — never recomputed.
            Part referencePart = vessel.GetReferenceTransformPart();

            foreach (Part part in vessel.parts)
            {
                ModuleCommand command = part.FindModuleImplementing<ModuleCommand>();
                if (command == null)
                {
                    continue;
                }

                // A part's vesselNaming is non-null as soon as the part's .cfg declares a VESSELNAMING
                // node, which most pods/probes do only to carry a stock default type (Probe, Ship...)
                // with an empty name and priority 0. That default must NOT override the vessel's real
                // name/type: the player named the module only when vesselName is non-empty. So the type
                // and priority follow the same condition as the name (otherwise every command module
                // would show its part's intrinsic type instead of the vessel's actual one).
                VesselNaming naming = part.vesselNaming;
                bool hasPlayerNaming = naming != null && !string.IsNullOrEmpty(naming.vesselName);
                string vesselName = hasPlayerNaming ? naming.vesselName : vessel.vesselName;
                VesselType vesselType = hasPlayerNaming ? naming.vesselType : vessel.vesselType;
                int priority = hasPlayerNaming ? naming.namingPriority : 0;

                result.Add(new CommandModuleInfo(
                    part,
                    command,
                    vesselName,
                    part.partInfo != null ? part.partInfo.title : part.name,
                    priority,
                    vesselType,
                    part == referencePart));
            }

            // Priority desc, then vessel name, then part title. Ties broken best-effort by these keys.
            result.Sort((a, b) =>
            {
                int byPriority = b.NamingPriority.CompareTo(a.NamingPriority);
                if (byPriority != 0) return byPriority;
                int byName = string.Compare(a.VesselName, b.VesselName, System.StringComparison.CurrentCultureIgnoreCase);
                if (byName != 0) return byName;
                return string.Compare(a.PartTitle, b.PartTitle, System.StringComparison.CurrentCultureIgnoreCase);
            });

            return result;
        }

        /// <summary>
        /// Make the given command module the vessel's control point — the exact action of the part's
        /// "Control From Here" PAW entry. Calls the module's own (virtual) event method rather than
        /// simulating a UI click.
        /// </summary>
        public static void ControlFromHere(CommandModuleInfo info)
        {
            if (info == null || info.Command == null)
            {
                return;
            }
            LOGGER.LogDebug("Control From Here on part " + info.PartTitle + " (" + info.VesselName + ")");
            info.Command.MakeReference();
        }

        /// <summary>
        /// Open the part action window (PAW) of the given command module's part, as clicking the part
        /// would. No-op when the part action controller is not available.
        /// </summary>
        public static void ShowPaw(CommandModuleInfo info)
        {
            if (info != null)
            {
                ShowPaw(info.Part);
            }
        }

        /// <summary>Open the part action window (PAW) of the given part. No-op on a null part or when
        /// the part action controller is not available.</summary>
        public static void ShowPaw(Part part)
        {
            if (part == null || UIPartActionController.Instance == null)
            {
                return;
            }
            LOGGER.LogDebug("Show PAW for part " + (part.partInfo != null ? part.partInfo.title : part.name));
            UIPartActionController.Instance.SpawnPartActionWindow(part);
        }
    }
}
