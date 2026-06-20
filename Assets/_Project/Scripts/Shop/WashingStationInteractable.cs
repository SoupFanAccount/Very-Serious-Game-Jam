using UnityEngine;

namespace Shop
{
    /// <summary>
    /// The laundering station. Converts dirty money into clean money. If a
    /// <see cref="LaunderingMinigameLauncher"/> is assigned it starts the minigame; otherwise it performs
    /// a simple placeholder conversion through <see cref="GameManager.WashMoney"/>. Interacting with no
    /// dirty money does nothing except give feedback.
    /// </summary>
    public class WashingStationInteractable : ShopInteractableBase
    {
        [Tooltip("Optional minigame entry point. If empty, a placeholder conversion is used.")]
        [SerializeField] private LaunderingMinigameLauncher minigameLauncher;

        [Tooltip("Dirty money laundered per use by the placeholder conversion. 0 = launder everything.")]
        [SerializeField] private int placeholderWashAmount;

        /// <inheritdoc />
        protected override void Perform(PlayerInteractionController interactor)
        {
            if (Game == null)
                return;

            if (Game.dirtyMoney <= 0)
            {
                interactor.ShowFeedback("No dirty money to wash.");
                return;
            }

            if (minigameLauncher != null)
            {
                minigameLauncher.StartLaundering(Game.dirtyMoney, interactor);
                return;
            }

            int amount = placeholderWashAmount > 0
                ? Mathf.Min(placeholderWashAmount, Game.dirtyMoney)
                : Game.dirtyMoney;

            Game.WashMoney(amount);
            interactor.ShowFeedback($"Laundered ${amount}.");
        }
    }
}
