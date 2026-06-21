using UnityEngine;

namespace com.github.lhervier.ksp.controlfromheremod
{
    /// <summary>
    /// One command module of the current vessel, with the display data the window needs: the vessel
    /// name carried by the module, its part title, its naming priority, its vessel type (for the stock
    /// icon) and whether it currently controls the vessel. Built by <see cref="CommandModulesService"/>.
    /// </summary>
    public class CommandModuleInfo
    {
        /// <summary>The part bearing the command module.</summary>
        public Part Part { get; }

        /// <summary>The command module driving the "Control From Here" action.</summary>
        public ModuleCommand Command { get; }

        /// <summary>Vessel name carried by this module (its own naming, or the vessel's global name).</summary>
        public string VesselName { get; }

        /// <summary>Localized part title (e.g. "Mk1-3 Command Pod").</summary>
        public string PartTitle { get; }

        /// <summary>Naming priority of this module (0 when the part carries no custom naming).</summary>
        public int NamingPriority { get; }

        /// <summary>Vessel type carried by this module, used to pick the stock vessel-type icon.</summary>
        public VesselType VesselType { get; }

        /// <summary>True when this part is the vessel's current control point (reference transform).</summary>
        public bool IsActive { get; }

        public CommandModuleInfo(
            Part part,
            ModuleCommand command,
            string vesselName,
            string partTitle,
            int namingPriority,
            VesselType vesselType,
            bool isActive)
        {
            this.Part = part;
            this.Command = command;
            this.VesselName = vesselName;
            this.PartTitle = partTitle;
            this.NamingPriority = namingPriority;
            this.VesselType = vesselType;
            this.IsActive = isActive;
        }
    }
}
