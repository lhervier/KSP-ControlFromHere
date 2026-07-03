using System.Collections.Generic;
using UnityEngine;
using com.github.lhervier.ksp.shared;

namespace com.github.lhervier.ksp.controlfromheremod.breaker
{
    /// <summary>
    /// The thrust circuit breaker: while enabled, it watches the active vessel's real thrust and, if the
    /// player throttles up while that thrust points more than <see cref="Threshold"/> degrees away from the
    /// vessel's control direction, it cuts and locks the throttle to 0 (trips). It stays tripped until an
    /// explicit rearm — the banner's "rearm" action, any "Control From Here" (native or ours, via
    /// <see cref="GameEvents.onVesselReferenceTransformSwitch"/>), or a vessel change.
    ///
    /// Model only: it owns the state and the detection, and raises <see cref="OnStateChanged"/> (UI refresh).
    /// The window revealing itself and the toolbar icon blinking on a trip are driven by the UI watching
    /// <see cref="IsTripped"/>. The enable flag and the threshold are global and persisted
    /// (<see cref="BreakerSettings"/>); the armed/tripped state is runtime-only.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ThrustBreaker : MonoBehaviour
    {
        private static readonly ModLogger LOGGER = new ModLogger("ThrustBreaker");

        // Ignore vanishingly small residual thrust so a near-idle engine can't trip the breaker.
        private const float ThrustEpsilonSqr = 1e-4f;

        /// <summary>The single flight-scene instance, or null outside flight / before it starts.</summary>
        public static ThrustBreaker Instance { get; private set; }

        private readonly BreakerSettings _settings = new BreakerSettings();

        // The vessel we currently drive OnFlyByWire on, so we can detach cleanly on change/teardown.
        private Vessel _hooked;

        /// <summary>Fired whenever the observable state changes (enabled, armed, threshold, snapshot).</summary>
        public EventVoid OnStateChanged = new EventVoid("ControlFromHere.Breaker.OnStateChanged");

        // ==========================================================================
        // Observable state
        // ==========================================================================

        public bool Enabled => _settings.Enabled;
        public float Threshold => _settings.Threshold;

        /// <summary>True while enabled and thrust is authorized. Only meaningful when enabled.</summary>
        public bool Armed { get; private set; } = true;

        /// <summary>True while enabled and tripped (throttle cut and locked).</summary>
        public bool IsTripped => _settings.Enabled && !Armed;

        /// <summary>The offending thrust direction (world space, normalized) frozen at trip time, or null
        /// while armed. Used to rank the command modules by alignment while tripped (the live thrust is
        /// already 0 once tripped, so it must be snapshotted).</summary>
        public Vector3? TripThrustDirection { get; private set; }

        // ==========================================================================
        // Lifecycle
        // ==========================================================================

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _settings.Load();
            Armed = true;
            TripThrustDirection = null;

            GameEvents.onVesselChange.Add(OnVesselChange);
            GameEvents.onVesselReferenceTransformSwitch.Add(OnReferenceTransformSwitch);

            HookVessel(FlightGlobals.ActiveVessel);
            LOGGER.LogInfo("Started (enabled=" + Enabled + ", threshold=" + Threshold + "°)");
        }

        private void OnDestroy()
        {
            GameEvents.onVesselChange.Remove(OnVesselChange);
            GameEvents.onVesselReferenceTransformSwitch.Remove(OnReferenceTransformSwitch);
            UnhookVessel();
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // ==========================================================================
        // Commands (from the banner UI)
        // ==========================================================================

        /// <summary>Enable or disable the breaker. Enabling arms it fresh; disabling clears any trip.</summary>
        public void SetEnabled(bool enabled)
        {
            if (_settings.Enabled == enabled)
            {
                return;
            }
            _settings.Enabled = enabled;
            Armed = true;                 // meaningful only when enabled; a fresh enable starts armed
            TripThrustDirection = null;
            _settings.Save();
            OnStateChanged.Fire();
        }

        /// <summary>Set the misalignment threshold (clamped). Persisted.</summary>
        public void SetThreshold(float degrees)
        {
            float clamped = BreakerSettings.ClampThreshold(degrees);
            if (Mathf.Approximately(_settings.Threshold, clamped))
            {
                return;
            }
            _settings.Threshold = clamped;
            _settings.Save();
            OnStateChanged.Fire();
        }

        /// <summary>Rearm the breaker without changing the control point (no-op unless tripped).</summary>
        public void Rearm()
        {
            if (!IsTripped)
            {
                return;
            }
            ClearTrip();
            OnStateChanged.Fire();
        }

        // ==========================================================================
        // Vessel hooking
        // ==========================================================================

        private void OnVesselChange(Vessel vessel)
        {
            // A vessel change rearms (per spec) and moves the OnFlyByWire hook to the new active vessel.
            ClearTrip();
            HookVessel(vessel);
            OnStateChanged.Fire();
        }

        // Any "Control From Here" — ours or the native PAW entry — rearms the breaker.
        private void OnReferenceTransformSwitch(Transform from, Transform to)
        {
            if (IsTripped)
            {
                ClearTrip();
                OnStateChanged.Fire();
            }
        }

        private void HookVessel(Vessel vessel)
        {
            if (_hooked == vessel)
            {
                return;
            }
            UnhookVessel();
            _hooked = vessel;
            if (_hooked != null)
            {
                _hooked.OnFlyByWire += OnFlyByWire;
            }
        }

        private void UnhookVessel()
        {
            if (_hooked != null)
            {
                _hooked.OnFlyByWire -= OnFlyByWire;
                _hooked = null;
            }
        }

        // ==========================================================================
        // Detection & latch
        // ==========================================================================

        // Runs every physics frame on the active vessel's control input. Reactive by design: it reads the
        // thrust actually applied this frame, so an engine that does not push (unstaged, out of fuel,
        // atmospheric cutoff...) has finalThrust ~ 0 and simply does not count.
        private void OnFlyByWire(FlightCtrlState s)
        {
            if (!_settings.Enabled)
            {
                return;
            }

            // While tripped, keep forcing the throttle to 0 every frame (covers any attempt to re-apply it).
            if (!Armed)
            {
                s.mainThrottle = 0f;
                return;
            }

            if (s.mainThrottle <= 0f)
            {
                return;
            }

            Vessel vessel = _hooked;
            if (vessel == null || vessel.ReferenceTransform == null)
            {
                return;
            }

            Vector3 thrust = ComputeRealThrust(vessel);
            if (thrust.sqrMagnitude < ThrustEpsilonSqr)
            {
                return;
            }

            // Control "forward" is the navball's up axis of the reference transform.
            Vector3 control = vessel.ReferenceTransform.up;
            if (Vector3.Angle(thrust, control) > _settings.Threshold)
            {
                Trip(thrust, s);
            }
        }

        private void Trip(Vector3 thrust, FlightCtrlState s)
        {
            // Snapshot the offending direction: once tripped the live thrust is 0, so it can no longer be read.
            TripThrustDirection = thrust.normalized;
            Armed = false;

            // Cut this frame's throttle, and the persistent throttle (like pressing X) so a rearm starts from 0.
            s.mainThrottle = 0f;
            if (FlightInputHandler.state != null)
            {
                FlightInputHandler.state.mainThrottle = 0f;
            }

            LOGGER.LogInfo("Tripped: thrust misaligned by more than " + _settings.Threshold + "°.");
            OnStateChanged.Fire();
        }

        private void ClearTrip()
        {
            Armed = true;
            TripThrustDirection = null;
        }

        // Sum of the forces each engine actually applies this frame, replicating ModuleEngines'
        // AddForceAtPosition(-thrustTransform.forward * finalThrust * multiplier): the true thrust vector.
        private static Vector3 ComputeRealThrust(Vessel vessel)
        {
            Vector3 total = Vector3.zero;
            List<Part> parts = vessel.parts;
            if (parts == null)
            {
                return total;
            }

            for (int p = 0; p < parts.Count; p++)
            {
                List<ModuleEngines> engines = parts[p].FindModulesImplementing<ModuleEngines>();
                for (int e = 0; e < engines.Count; e++)
                {
                    ModuleEngines engine = engines[e];
                    if (engine.finalThrust <= 0f || engine.thrustTransforms == null)
                    {
                        continue;
                    }

                    List<Transform> transforms = engine.thrustTransforms;
                    List<float> multipliers = engine.thrustTransformMultipliers;
                    for (int t = 0; t < transforms.Count; t++)
                    {
                        Transform transform = transforms[t];
                        if (transform == null)
                        {
                            continue;
                        }
                        // Multipliers match the transforms when present; otherwise weight evenly.
                        float multiplier = (multipliers != null && t < multipliers.Count)
                            ? multipliers[t]
                            : 1f / transforms.Count;
                        total += -transform.forward * engine.finalThrust * multiplier;
                    }
                }
            }
            return total;
        }
    }
}
