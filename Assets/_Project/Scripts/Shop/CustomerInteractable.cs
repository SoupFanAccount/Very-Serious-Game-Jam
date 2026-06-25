using UnityEngine;

namespace Shop
{
    /// <summary>
    /// A customer at the counter. Serving them is a legitimate sale that pays clean money and slightly
    /// lowers suspicion. Usually configured as one-shot so each customer is served once.
    /// </summary>
    public class CustomerInteractable : ShopInteractableBase
    {
        [Tooltip("Clean money earned from the sale.")]
        [SerializeField] private int saleValue = 10;

        [Tooltip("Suspicion removed by honest service. 0 = none.")]
        [SerializeField] private int suspicionReduction = 1;

        /// <inheritdoc />
        protected override void Perform(PlayerInteractionController interactor)
        {
            if (Game == null)
                return;

            if (saleValue > 0)
                Game.AddCleanMoney(saleValue);

            if (suspicionReduction > 0)
                Game.LowerSuspicion(suspicionReduction);

            interactor.ShowFeedback($"Sale complete! +${saleValue}");

            // Added by Donags. It tells the queued customer they've been served so they walk off.
            if (TryGetComponent(out Customer customer))
                customer.Serve();
        }
    }
}
