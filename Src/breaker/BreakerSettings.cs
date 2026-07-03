using System.IO;
using com.github.lhervier.ksp.shared;

namespace com.github.lhervier.ksp.controlfromheremod.breaker
{
    /// <summary>
    /// The circuit breaker's global, persisted settings: whether it is enabled and the misalignment
    /// threshold (in degrees). Stored in a small .cfg next to the mod DLL, so the choice survives scene
    /// changes and sessions. The armed/tripped state is runtime-only and never persisted.
    /// </summary>
    public class BreakerSettings
    {
        private static readonly ModLogger LOGGER = new ModLogger("BreakerSettings");

        private const string CONFIG_FILE = "circuit_breaker.cfg";
        private const string GENERAL_NODE = "GENERAL";
        private const string KEY_ENABLED = "enabled";
        private const string KEY_THRESHOLD = "threshold";

        /// <summary>Default misalignment threshold, in degrees. Leaves margin for gimbal deflection.</summary>
        public const float DefaultThreshold = 5f;
        public const float MinThreshold = 1f;
        public const float MaxThreshold = 45f;

        public bool Enabled { get; set; } = true;
        public float Threshold { get; set; } = DefaultThreshold;

        // The .cfg lives beside the compiled DLL, like KSP-DrawLayerPlugin's ConfigManager.
        private static string ConfigFilePath
        {
            get
            {
                string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                return Path.Combine(Path.GetDirectoryName(dllPath), CONFIG_FILE);
            }
        }

        /// <summary>Load the settings from disk, falling back to defaults when the file is missing or invalid.</summary>
        public void Load()
        {
            string path = ConfigFilePath;
            if (!File.Exists(path))
            {
                LOGGER.LogInfo("No settings file, using defaults.");
                return;
            }

            ConfigNode root = ConfigNode.Load(path);
            ConfigNode general = root != null ? root.GetNode(GENERAL_NODE) : null;
            if (general == null)
            {
                LOGGER.LogWarning("Invalid settings file, using defaults.");
                return;
            }

            bool enabled;
            if (bool.TryParse(general.GetValue(KEY_ENABLED), out enabled))
            {
                Enabled = enabled;
            }
            float threshold;
            if (float.TryParse(general.GetValue(KEY_THRESHOLD), out threshold))
            {
                Threshold = ClampThreshold(threshold);
            }
            LOGGER.LogDebug("Loaded settings: enabled=" + Enabled + ", threshold=" + Threshold + "°");
        }

        /// <summary>Persist the current settings to disk.</summary>
        public void Save()
        {
            var root = new ConfigNode();
            ConfigNode general = root.AddNode(GENERAL_NODE);
            general.SetValue(KEY_ENABLED, Enabled, true);
            general.SetValue(KEY_THRESHOLD, Threshold, true);
            root.Save(ConfigFilePath);
            LOGGER.LogDebug("Saved settings: enabled=" + Enabled + ", threshold=" + Threshold + "°");
        }

        /// <summary>Clamp a threshold to the allowed range.</summary>
        public static float ClampThreshold(float value)
        {
            if (value < MinThreshold) return MinThreshold;
            if (value > MaxThreshold) return MaxThreshold;
            return value;
        }
    }
}
