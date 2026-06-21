using UnityEngine;

namespace Shop
{
    /// <summary>
    /// Read-only bridge to the day/night system. Locates a <see cref="DayNightCycle"/> in the scene
    /// without coupling shop code to its lifetime, so availability still works once that system lands.
    /// </summary>
    public static class DayNightPhaseLocator
    {
        private static DayNightCycle _cycle;

        /// <summary>
        /// Attempts to read the current day/night phase.
        /// </summary>
        /// <param name="isDay">True if it is currently day; only valid when this returns true.</param>
        /// <returns>True if a day/night system was found; false if none exists yet.</returns>
        public static bool TryGetIsDay(out bool isDay)
        {
            // Re-find if missing or if the previous reference was destroyed (e.g. a scene reload).
            if (_cycle == null)
                _cycle = Object.FindFirstObjectByType<DayNightCycle>();

            if (_cycle == null)
            {
                isDay = false;
                return false;
            }

            isDay = _cycle.currentPhase == DayNightCycle.Phase.Day;
            return true;
        }
    }
}
