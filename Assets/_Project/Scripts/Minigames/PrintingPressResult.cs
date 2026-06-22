namespace Minigames
{
    /// <summary>
    /// Immutable summary of a single printing-press session. Produced by the
    /// <see cref="PrintingPressMinigameController"/> and handed to the launcher so the result can be
    /// displayed and relayed back to the shop.
    /// </summary>
    public readonly struct PrintingPressResult
    {
        /// <summary>Number of bills the player printed clean by aligning and stamping them in time.</summary>
        public int BillsPrinted { get; }

        /// <summary>Number of bills the player ruined, by misalignment or by running out of time.</summary>
        public int BillsFailed { get; }

        /// <summary>Total dirty money converted to clean money this session.</summary>
        public int MoneyLaundered { get; }

        /// <summary>Total suspicion added by ruined bills this session.</summary>
        public int SuspicionAdded { get; }

        /// <summary>Creates a result snapshot.</summary>
        public PrintingPressResult(int billsPrinted, int billsFailed, int moneyLaundered, int suspicionAdded)
        {
            BillsPrinted = billsPrinted;
            BillsFailed = billsFailed;
            MoneyLaundered = moneyLaundered;
            SuspicionAdded = suspicionAdded;
        }

        /// <summary>Multi-line summary suitable for the in-minigame end-of-session panel.</summary>
        public string ToSummaryString()
        {
            return $"Bills printed: {BillsPrinted}\n" +
                   $"Bills failed: {BillsFailed}\n" +
                   $"Money laundered: ${MoneyLaundered}\n" +
                   $"Suspicion added: {SuspicionAdded}";
        }

        /// <summary>Single-line summary suitable for the shop's transient feedback line.</summary>
        public string ToShopFeedbackString()
        {
            return $"Printed ${MoneyLaundered} clean ({BillsPrinted} printed, {BillsFailed} ruined).";
        }
    }
}
