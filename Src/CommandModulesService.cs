using System.Collections.Generic;
using com.github.lhervier.ksp.shared;

namespace com.github.lhervier.ksp.controlfromheremod
{
    /// <summary>
    /// Reads the command modules of a vessel and performs the "Control From Here" action. A command
    /// module is a part bearing a <see cref="ModuleCommand"/>; docking ports (which expose "Control
    /// From Here" through ModuleDockingNode, not ModuleCommand) are therefore excluded.
    /// </summary>
    public static class CommandModulesService
    {
        private static readonly ModLogger LOGGER = new ModLogger("CommandModules");

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

                // The module carries its own naming (name/type/priority) when the player named it;
                // otherwise it falls back to the vessel's global name and type, with priority 0.
                VesselNaming naming = part.vesselNaming;
                string vesselName = (naming != null && !string.IsNullOrEmpty(naming.vesselName))
                    ? naming.vesselName
                    : vessel.vesselName;
                VesselType vesselType = naming != null ? naming.vesselType : vessel.vesselType;
                int priority = naming != null ? naming.namingPriority : 0;

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
            if (info == null || info.Part == null || UIPartActionController.Instance == null)
            {
                return;
            }
            LOGGER.LogDebug("Show PAW for part " + info.PartTitle);
            UIPartActionController.Instance.SpawnPartActionWindow(info.Part);
        }
    }
}
