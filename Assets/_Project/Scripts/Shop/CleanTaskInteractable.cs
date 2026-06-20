using UnityEngine;

namespace Shop
{
    /// <summary>
    /// A legitimate shop task (mopping a spill, restocking, etc.) that lowers suspicion and may pay
    /// a small amount of clean money.
    /// </summary>
    public class CleanTaskInteractable : ShopInteractableBase
    {
        [Tooltip("How much suspicion is removed when the task is completed.")]
        [SerializeField] private int suspicionReduction = 5;

        [Tooltip("Clean money awarded for completing the task. 0 = no reward.")]
        [SerializeField] private int cleanMoneyReward;

        /// <inheritdoc />
        protected override void Perform(PlayerInteractionController interactor)
        {
            if (Game == null)
                return;

            if (suspicionReduction > 0)
                Game.LowerSuspicion(suspicionReduction);

            if (cleanMoneyReward > 0)
                Game.AddCleanMoney(cleanMoneyReward);

            interactor.ShowFeedback(BuildFeedback());
        }

        private string BuildFeedback()
        {
            return cleanMoneyReward > 0
                ? $"Done! -{suspicionReduction} suspicion, +${cleanMoneyReward}"
                : $"Done! -{suspicionReduction} suspicion";
        }
    }
}
