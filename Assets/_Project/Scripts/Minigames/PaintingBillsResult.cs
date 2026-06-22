namespace Minigames
{
    /// <summary>
    /// Immutable summary of a single painting-bills session. Produced by the
    /// <see cref="PaintingBillsMinigameController"/> and handed to the launcher so the result can be
    /// displayed and relayed back to the shop.
    /// </summary>
    public readonly struct PaintingBillsResult
    {
        /// <summary>Number of bills the player cleaned by painting away every stain in time.</summary>
        public int BillsCleaned { get; }

        /// <summary>Number of bills the player failed, typically by running out of time.</summary>
        public int BillsFailed { get; }

        /// <summary>Total dirty money converted to clean money this session.</summary>
        public int MoneyLaundered { get; }

        /// <summary>Total suspicion added by failed bills this session.</summary>
        public int SuspicionAdded { get; }

        /// <summary>Creates a result snapshot.</summary>
        public PaintingBillsResult(int billsCleaned, int billsFailed, int moneyLaundered, int suspicionAdded)
        {
            BillsCleaned = billsCleaned;
            BillsFailed = billsFailed;
            MoneyLaundered = moneyLaundered;
            SuspicionAdded = suspicionAdded;
        }

        /// <summary>Multi-line summary suitable for the in-minigame end-of-session panel.</summary>
        public string ToSummaryString()
        {
            return $"Bills cleaned: {BillsCleaned}\n" +
                   $"Bills failed: {BillsFailed}\n" +
                   $"Money laundered: ${MoneyLaundered}\n" +
                   $"Suspicion added: {SuspicionAdded}";
        }

        /// <summary>Single-line summary suitable for the shop's transient feedback line.</summary>
        public string ToShopFeedbackString()
        {
            return $"Painted ${MoneyLaundered} clean ({BillsCleaned} cleaned, {BillsFailed} ruined).";
        }
    }
}
