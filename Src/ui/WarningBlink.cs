namespace com.github.lhervier.ksp.controlfromheremod.ui
{
    /// <summary>
    /// Shared blink cadence for the "breaker tripped" warning, so the toolbar icon and the in-window state
    /// dot pulse in lock-step. Both read <see cref="UnityEngine.Time.unscaledTime"/>, so the same
    /// <see cref="IsOn"/> phase keeps them synchronized.
    /// </summary>
    internal static class WarningBlink
    {
        /// <summary>One full on/off cycle, in seconds.</summary>
        public const float PeriodSeconds = 0.9f;

        /// <summary>Hard blink: "on" for the first half of each period, "off" for the second half.</summary>
        public static bool IsOn(float time)
        {
            return (time % PeriodSeconds) < (PeriodSeconds * 0.5f);
        }
    }
}
