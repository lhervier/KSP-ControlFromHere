namespace com.github.lhervier.ksp.controlfromheremod
{
    /// <summary>
    /// The vessel's current control point when it is <em>not</em> one of the listed command modules:
    /// a part with no <see cref="ModuleCommand"/> (docking port, external seat, claw...) or nothing at
    /// all. Drives the special row pinned at the top of the window, mirroring the toolbar icon that
    /// blinks in the very same situation. Built by <see cref="CommandModulesService"/>; never created
    /// when the control point is a command module (<see cref="ControlStatus.OnCommandModule"/>).
    /// </summary>
    public class OffListControlInfo
    {
        /// <summary>Either <see cref="ControlStatus.OffCommandModule"/> or <see cref="ControlStatus.Uncontrolled"/>.</summary>
        public ControlStatus Status { get; }

        /// <summary>The controlling part, or null when the vessel is uncontrolled (no PAW to show then).</summary>
        public Part Part { get; }

        /// <summary>Vessel name carried by the controlling part. Unused when uncontrolled.</summary>
        public string VesselName { get; }

        /// <summary>Localized title of the controlling part. Unused when uncontrolled.</summary>
        public string PartTitle { get; }

        /// <summary>Vessel type driving the row icon (the part's real type, so a docking port shows its
        /// stock icon). <see cref="VesselType.Unknown"/> when uncontrolled, giving a neutral icon.</summary>
        public VesselType VesselType { get; }

        public OffListControlInfo(ControlStatus status, Part part, string vesselName, string partTitle, VesselType vesselType)
        {
            this.Status = status;
            this.Part = part;
            this.VesselName = vesselName;
            this.PartTitle = partTitle;
            this.VesselType = vesselType;
        }
    }
}
