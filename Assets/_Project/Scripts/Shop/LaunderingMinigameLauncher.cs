using UnityEngine;

namespace Shop
{
    /// <summary>
    /// Integration seam for the laundering minigame. The minigame programmer subclasses this and
    /// implements <see cref="StartLaundering"/>. When a <see cref="WashingStationInteractable"/> has one
    /// assigned, it launches the minigame instead of performing the placeholder conversion.
    /// </summary>
    public abstract class LaunderingMinigameLauncher : MonoBehaviour
    {
        /// <summary>
        /// Starts the laundering minigame. The implementation is responsible for converting money
        /// (typically via <see cref="GameManager.WashMoney"/>) based on the minigame's outcome.
        /// </summary>
        /// <param name="dirtyMoneyAvailable">Dirty money the player currently holds.</param>
        /// <param name="interactor">The controller that triggered the station, for feedback.</param>
        public abstract void StartLaundering(int dirtyMoneyAvailable, PlayerInteractionController interactor);
    }
}
