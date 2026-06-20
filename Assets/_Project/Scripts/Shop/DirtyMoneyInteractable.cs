using UnityEngine;

namespace Shop
{
    /// <summary>
    /// A shady opportunity (a back-room package, an off-the-books drop-off) that hands the player dirty
    /// money and raises suspicion. Uses <see cref="GameManager.AddDirtyMoney"/>, which already raises
    /// suspicion proportionally; <see cref="extraSuspicion"/> adds more on top when desired.
    /// </summary>
    public class DirtyMoneyInteractable : ShopInteractableBase
    {
        [Tooltip("Dirty money gained when the opportunity is accepted.")]
        [SerializeField] private int dirtyMoneyAmount = 50;

        [Tooltip("Extra suspicion on top of what GameManager already adds. 0 = none.")]
        [SerializeField] private int extraSuspicion;

        /// <inheritdoc />
        protected override void Perform(PlayerInteractionController interactor)
        {
            if (Game == null)
                return;

            Game.AddDirtyMoney(dirtyMoneyAmount);

            if (extraSuspicion > 0)
                Game.AddSuspicion(extraSuspicion);

            interactor.ShowFeedback($"Accepted. +${dirtyMoneyAmount} dirty money");
        }
    }
}
