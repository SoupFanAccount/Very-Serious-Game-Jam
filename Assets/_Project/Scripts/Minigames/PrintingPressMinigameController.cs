using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    /// <summary>
    /// Owns the state of a printing-press session: it splits the dirty money into bills, presents each bill to
    /// be dragged onto the press bed, and decides when a bill prints clean (placed inside the alignment
    /// tolerance when the lever is pulled) or is ruined (misaligned, or the per-bill timer runs out). The bill,
    /// target zone and lever report to it; it never touches the shared input system. It raises
    /// <see cref="SessionFinished"/> once the summary has been dismissed so the launcher can hide the panel.
    /// </summary>
    public class PrintingPressMinigameController : MonoBehaviour
    {
        [Header("Scene References")]
        [Tooltip("The bill the player drags onto the press bed.")]
        [SerializeField] private PressDraggableBill bill;

        [Tooltip("The highlighted alignment target on the press bed.")]
        [SerializeField] private PrintingPressTargetZone targetZone;

        [Tooltip("The lever/button that activates the press. Wired to evaluate the placed bill.")]
        [SerializeField] private PrintingPressLever lever;

        [Tooltip("Container holding the bill, bed and lever. Shown during play, hidden on the summary.")]
        [SerializeField] private GameObject playArea;

        [Tooltip("Panel shown when the session ends.")]
        [SerializeField] private GameObject summaryPanel;

        [Tooltip("Text that receives the end-of-session summary.")]
        [SerializeField] private TextMeshProUGUI summaryText;

        [Tooltip("Optional live status line describing the current bill and feedback.")]
        [SerializeField] private TextMeshProUGUI statusText;

        [Tooltip("Optional line showing the current bill number and value.")]
        [SerializeField] private TextMeshProUGUI billInfoText;

        [Tooltip("Optional line showing the time remaining on the current bill.")]
        [SerializeField] private TextMeshProUGUI timerText;

        [Tooltip("Optional line showing the alignment quality of the last press.")]
        [SerializeField] private TextMeshProUGUI alignmentText;

        [Tooltip("Optional button on the summary panel that closes the minigame immediately.")]
        [SerializeField] private Button closeButton;

        [Tooltip("Optional button shown during play that abandons the session early and returns to the shop.")]
        [SerializeField] private Button quitButton;

        [Header("Session Settings")]
        [Tooltip("Largest value a single bill can hold. Dirty money is split into as many of these as needed, plus a remainder bill.")]
        [SerializeField] private int maxBillValue = 100;

        [Tooltip("Most bills processed in one visit, so a big pile of dirty money never makes an endless session. Any money beyond the cap stays dirty for the next visit. 0 means no cap.")]
        [SerializeField] private int maxBillsPerSession = 8;

        [Tooltip("Suspicion added each time a bill is botched.")]
        [SerializeField] private int suspicionPerFailedBill = 5;

        [Tooltip("Seconds the player has to print a single bill before it fails. 0 or less disables the timer.")]
        [SerializeField] private float secondsPerBill = 10f;

        [Header("Alignment Settings")]
        [Tooltip("Maximum centre-to-centre pixel distance for a bill to print clean.")]
        [SerializeField] private float alignmentTolerance = 40f;

        [Tooltip("Centre-to-centre pixel distance under which the print counts as perfect.")]
        [SerializeField] private float perfectTolerance = 12f;

        [Tooltip("If true, the bill's centre must also sit inside the target rectangle to print clean.")]
        [SerializeField] private bool requireCenterInsideZone;

        [Header("Timing")]
        [Tooltip("Pause after a bill resolves before the next bill appears.")]
        [SerializeField] private float betweenBillsDelay = 0.6f;

        [Tooltip("Auto-close delay for the summary. 0 disables auto-close (use the close button).")]
        [SerializeField] private float autoCloseDelay = 4f;

        private readonly List<int> _billAmounts = new List<int>();

        private int _currentBillIndex;
        private int _billsPrinted;
        private int _billsFailed;
        private int _moneyLaundered;
        private int _suspicionAdded;

        private float _billTimeRemaining;
        private bool _sessionActive;
        private bool _currentBillResolved;
        private bool _sessionFinishedRaised;

        private PrintingPressResult _lastResult;
        private Coroutine _advanceRoutine;
        private Coroutine _autoCloseRoutine;

        /// <summary>Raised once the session is fully over and the summary dismissed. Carries the totals.</summary>
        public event Action<PrintingPressResult> SessionFinished;

        /// <summary>True while a bill is in play and may still be dragged or pressed.</summary>
        public bool IsBillActive => _sessionActive && !_currentBillResolved;

        /// <summary>Wires the optional buttons and hides the summary panel.</summary>
        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseSession);
            if (quitButton != null)
                quitButton.onClick.AddListener(CancelSession);
            if (summaryPanel != null)
                summaryPanel.SetActive(false);
        }

        /// <summary>Counts down the current bill's timer and ruins the bill when it expires.</summary>
        private void Update()
        {
            if (!IsBillActive || secondsPerBill <= 0f)
                return;

            _billTimeRemaining = Mathf.Max(0f, _billTimeRemaining - Time.unscaledDeltaTime);
            UpdateTimerText();

            if (_billTimeRemaining <= 0f)
                FailCurrentBill("Out of time!");
        }

        /// <summary>
        /// Starts a new session for the given amount of dirty money. Splits it into bills, resets the
        /// counters, connects the lever and presents the first bill.
        /// </summary>
        public void BeginSession(int dirtyMoneyAvailable)
        {
            StopPendingRoutines();
            _sessionFinishedRaised = false;

            // Refuse to run a half-wired prefab: without a bill, a target to align to, and a lever to press
            // there is no game. Abort gracefully so the launcher still hides the panel and unfreezes the player.
            if (bill == null || targetZone == null || lever == null)
            {
                Debug.LogError(
                    $"{nameof(PrintingPressMinigameController)}: assign the bill, target zone and lever before starting; aborting session.",
                    this);
                _sessionActive = false;
                _lastResult = new PrintingPressResult(0, 0, 0, 0);
                RaiseSessionFinished();
                return;
            }

            _billsPrinted = 0;
            _billsFailed = 0;
            _moneyLaundered = 0;
            _suspicionAdded = 0;
            _currentBillIndex = 0;
            _currentBillResolved = false;

            BuildBillChunks(dirtyMoneyAvailable);

            if (lever != null)
                lever.Initialize(this);

            if (summaryPanel != null)
                summaryPanel.SetActive(false);
            if (playArea != null)
                playArea.SetActive(true);

            _sessionActive = true;

            if (_billAmounts.Count == 0)
            {
                FinishSession();
                return;
            }

            StartCurrentBill();
        }

        /// <summary>
        /// Evaluates the currently placed bill against the alignment tolerance. Called by the lever. Prints the
        /// bill clean when it is close enough (and, optionally, inside the zone), otherwise ruins it.
        /// </summary>
        public void TryPressCurrentBill()
        {
            if (!IsBillActive)
                return;

            if (bill == null || targetZone == null)
            {
                Debug.LogWarning($"{nameof(PrintingPressMinigameController)}: bill or target zone missing; cannot grade press.", this);
                return;
            }

            float distance = targetZone.ScreenDistanceTo(bill.Rect);
            bool insideOk = !requireCenterInsideZone || targetZone.ContainsBillCenter(bill.Rect);

            if (insideOk && distance <= alignmentTolerance)
                CompleteCurrentBill(distance <= perfectTolerance);
            else
                FailCurrentBill("Misprint!");
        }

        /// <summary>Prints the current bill clean, converts its money and schedules the next bill.</summary>
        private void CompleteCurrentBill(bool perfect)
        {
            int amount = _billAmounts[_currentBillIndex];
            int washed = amount;

            GameManager game = GameManager.Instance;
            if (game != null)
            {
                int before = game.dirtyMoney;
                game.WashMoney(amount);
                washed = before - game.dirtyMoney;
            }

            _moneyLaundered += washed;
            _billsPrinted++;

            if (bill != null)
                bill.ShowPrinted();
            if (targetZone != null)
                targetZone.FlashSuccess();

            UpdateAlignment(perfect ? "Perfect print!" : "Printed clean!");
            UpdateStatus($"Printed! Laundered ${washed}.");
            ResolveBill();
        }

        /// <summary>Botches the current bill, adds suspicion and schedules the next bill.</summary>
        private void FailCurrentBill(string reason)
        {
            GameManager game = GameManager.Instance;
            if (game != null)
                game.AddSuspicion(suspicionPerFailedBill);

            _suspicionAdded += suspicionPerFailedBill;
            _billsFailed++;

            if (bill != null)
                bill.ShowRuined();
            if (targetZone != null)
                targetZone.FlashFailure();

            UpdateAlignment("Misprint! Bill botched.");
            // The cash is not destroyed: it stays dirty in the GameManager pool and can be attempted again later.
            UpdateStatus($"{reason} Bill botched - still dirty. +{suspicionPerFailedBill} suspicion.");
            ResolveBill();
        }

        /// <summary>Locks the current bill and starts the pause before the next bill appears.</summary>
        private void ResolveBill()
        {
            _currentBillResolved = true;

            if (bill != null)
                bill.SetInteractable(false);

            if (_advanceRoutine != null)
                StopCoroutine(_advanceRoutine);
            _advanceRoutine = StartCoroutine(AdvanceAfterDelay());
        }

        /// <summary>Waits the between-bill pause, then moves on.</summary>
        private IEnumerator AdvanceAfterDelay()
        {
            yield return new WaitForSecondsRealtime(betweenBillsDelay);
            _advanceRoutine = null;
            AdvanceToNextBill();
        }

        /// <summary>Advances to the next bill, or finishes the session when none remain.</summary>
        private void AdvanceToNextBill()
        {
            _currentBillIndex++;
            if (_currentBillIndex >= _billAmounts.Count)
            {
                FinishSession();
                return;
            }

            StartCurrentBill();
        }

        /// <summary>Presents a fresh dirty bill for the current index and re-enables dragging.</summary>
        private void StartCurrentBill()
        {
            _currentBillResolved = false;
            _billTimeRemaining = secondsPerBill;

            if (bill != null)
            {
                bill.ResetForNewBill();
                bill.SetInteractable(true);
            }

            if (targetZone != null)
                targetZone.ResetVisual();

            UpdateBillInfo();
            UpdateTimerText();
            UpdateAlignment(string.Empty);
            UpdateStatus("Drag the bill onto the target, then pull the lever to print.");
        }

        /// <summary>Ends the session and shows the summary, with optional auto-close.</summary>
        private void FinishSession()
        {
            _sessionActive = false;
            StopPendingRoutines();

            if (bill != null)
                bill.SetInteractable(false);

            _lastResult = new PrintingPressResult(_billsPrinted, _billsFailed, _moneyLaundered, _suspicionAdded);

            if (playArea != null)
                playArea.SetActive(false);
            if (summaryPanel != null)
                summaryPanel.SetActive(true);
            if (summaryText != null)
                summaryText.text = _lastResult.ToSummaryString();

            if (autoCloseDelay > 0f)
                _autoCloseRoutine = StartCoroutine(AutoCloseAfterDelay());
        }

        /// <summary>Waits the auto-close delay, then closes the session.</summary>
        private IEnumerator AutoCloseAfterDelay()
        {
            yield return new WaitForSecondsRealtime(autoCloseDelay);
            _autoCloseRoutine = null;
            CloseSession();
        }

        /// <summary>Hides the summary and notifies listeners that the session is over.</summary>
        public void CloseSession()
        {
            if (_autoCloseRoutine != null)
            {
                StopCoroutine(_autoCloseRoutine);
                _autoCloseRoutine = null;
            }

            if (summaryPanel != null)
                summaryPanel.SetActive(false);

            RaiseSessionFinished();
        }

        /// <summary>Aborts an in-progress session, reporting whatever was achieved so far.</summary>
        public void CancelSession()
        {
            if (!_sessionActive)
                return;

            _sessionActive = false;
            _currentBillResolved = true;
            StopPendingRoutines();

            if (bill != null)
                bill.SetInteractable(false);

            _lastResult = new PrintingPressResult(_billsPrinted, _billsFailed, _moneyLaundered, _suspicionAdded);

            if (playArea != null)
                playArea.SetActive(false);
            if (summaryPanel != null)
                summaryPanel.SetActive(false);

            RaiseSessionFinished();
        }

        /// <summary>
        /// Raises <see cref="SessionFinished"/> at most once per session. Both the close (button/auto-close) and
        /// cancel paths route through here so a double close, a cancel-then-close, or an auto-close racing the
        /// close button can never fire the event twice for the same session.
        /// </summary>
        private void RaiseSessionFinished()
        {
            if (_sessionFinishedRaised)
                return;

            _sessionFinishedRaised = true;
            SessionFinished?.Invoke(_lastResult);
        }

        /// <summary>
        /// Splits the available dirty money into as many <see cref="maxBillValue"/> bills as possible, plus a
        /// final bill for any remainder. No bill exceeds the max and the total never exceeds the amount
        /// available. At most <see cref="maxBillsPerSession"/> bills are produced (0 = unlimited) so a large
        /// pile never makes an endless session; any money beyond the cap stays dirty for the next visit.
        /// </summary>
        private void BuildBillChunks(int dirtyMoneyAvailable)
        {
            _billAmounts.Clear();

            if (dirtyMoneyAvailable <= 0)
                return;

            int safeMax = Mathf.Max(1, maxBillValue);
            int cap = maxBillsPerSession > 0 ? maxBillsPerSession : int.MaxValue;
            int remaining = dirtyMoneyAvailable;

            while (remaining > safeMax && _billAmounts.Count < cap)
            {
                _billAmounts.Add(safeMax);
                remaining -= safeMax;
            }

            if (remaining > 0 && _billAmounts.Count < cap)
                _billAmounts.Add(remaining);
        }

        /// <summary>Updates the optional bill number/value line if one is assigned.</summary>
        private void UpdateBillInfo()
        {
            if (billInfoText != null)
                billInfoText.text = $"Bill {_currentBillIndex + 1} of {_billAmounts.Count}  -  ${_billAmounts[_currentBillIndex]}";
        }

        /// <summary>Updates the optional timer line, or clears it when the timer is disabled.</summary>
        private void UpdateTimerText()
        {
            if (timerText == null)
                return;

            timerText.text = secondsPerBill > 0f ? $"Time: {_billTimeRemaining:0.0}s" : string.Empty;
        }

        /// <summary>Updates the optional alignment-quality line if one is assigned.</summary>
        private void UpdateAlignment(string message)
        {
            if (alignmentText != null)
                alignmentText.text = message;
        }

        /// <summary>Updates the optional status line if one is assigned.</summary>
        private void UpdateStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        /// <summary>Stops the between-bill and auto-close coroutines if running.</summary>
        private void StopPendingRoutines()
        {
            if (_advanceRoutine != null)
            {
                StopCoroutine(_advanceRoutine);
                _advanceRoutine = null;
            }

            if (_autoCloseRoutine != null)
            {
                StopCoroutine(_autoCloseRoutine);
                _autoCloseRoutine = null;
            }
        }
    }
}
