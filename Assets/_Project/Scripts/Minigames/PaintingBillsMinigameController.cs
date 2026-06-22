using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    /// <summary>
    /// Owns the state of a painting-bills session: it splits the dirty money into bills, presents each bill's
    /// stains, resolves overlap for the brush, and decides when a bill is cleaned (all stains painted) or
    /// failed (the per-bill timer runs out). The brush and the stains report to it; it never touches the
    /// shared input system. It raises <see cref="SessionFinished"/> once the summary has been dismissed so the
    /// launcher can hide the panel.
    /// </summary>
    public class PaintingBillsMinigameController : MonoBehaviour
    {
        [Header("Scene References")]
        [Tooltip("The bill the player paints clean, carrying its dirty stains.")]
        [SerializeField] private PaintableBill bill;

        [Tooltip("The brush cursor that follows the mouse and reports paint strokes.")]
        [SerializeField] private PaintingBrush brush;

        [Tooltip("Container holding the bill and brush. Shown during play, hidden on the summary.")]
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

        [Tooltip("Optional line showing how many stains are left on the current bill.")]
        [SerializeField] private TextMeshProUGUI stainsText;

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

        [Tooltip("Seconds the player has to paint a single bill clean before it fails.")]
        [SerializeField] private float secondsPerBill = 12f;

        [Header("Timing")]
        [Tooltip("Pause after a bill resolves before the next bill appears.")]
        [SerializeField] private float betweenBillsDelay = 0.6f;

        [Tooltip("Auto-close delay for the summary. 0 disables auto-close (use the close button).")]
        [SerializeField] private float autoCloseDelay = 4f;

        private readonly List<int> _billAmounts = new List<int>();
        private readonly List<PaintableDirtyPatch> _activePatches = new List<PaintableDirtyPatch>();

        private int _currentBillIndex;
        private int _billsCleaned;
        private int _billsFailed;
        private int _moneyLaundered;
        private int _suspicionAdded;

        private float _billTimeRemaining;
        private bool _sessionActive;
        private bool _currentBillResolved;
        private bool _sessionFinishedRaised;

        private PaintingBillsResult _lastResult;
        private Coroutine _advanceRoutine;
        private Coroutine _autoCloseRoutine;

        /// <summary>Raised once the session is fully over and the summary dismissed. Carries the totals.</summary>
        public event Action<PaintingBillsResult> SessionFinished;

        /// <summary>True while a bill is being played and may still receive paint.</summary>
        public bool IsAcceptingPaint => _sessionActive && !_currentBillResolved;

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

        /// <summary>Counts down the current bill's timer and fails the bill when it expires.</summary>
        private void Update()
        {
            if (!IsAcceptingPaint || secondsPerBill <= 0f)
                return;

            _billTimeRemaining = Mathf.Max(0f, _billTimeRemaining - Time.unscaledDeltaTime);
            UpdateTimerText();

            if (_billTimeRemaining <= 0f)
                FailCurrentBill("Out of time!");
        }

        /// <summary>
        /// Starts a new session for the given amount of dirty money. Splits it into bills, resets the
        /// counters and presents the first bill.
        /// </summary>
        public void BeginSession(int dirtyMoneyAvailable)
        {
            StopPendingRoutines();
            _sessionFinishedRaised = false;

            // Refuse to run a half-wired prefab: without a bill (its stains) and a brush there is nothing to
            // paint. Abort gracefully so the launcher still hides the panel and unfreezes the player.
            if (bill == null || brush == null)
            {
                Debug.LogError(
                    $"{nameof(PaintingBillsMinigameController)}: assign the bill and brush before starting; aborting session.",
                    this);
                _sessionActive = false;
                _lastResult = new PaintingBillsResult(0, 0, 0, 0);
                RaiseSessionFinished();
                return;
            }

            _billsCleaned = 0;
            _billsFailed = 0;
            _moneyLaundered = 0;
            _suspicionAdded = 0;
            _currentBillIndex = 0;
            _currentBillResolved = false;

            BuildBillChunks(dirtyMoneyAvailable);
            InitializeInteractables();

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

        /// <summary>Registers a stain as part of the current bill so the brush can paint it.</summary>
        public void RegisterPatch(PaintableDirtyPatch patch)
        {
            if (patch == null || _activePatches.Contains(patch))
                return;

            _activePatches.Add(patch);
        }

        /// <summary>
        /// Applies a paint stroke at a screen point. Every still-dirty stain under the brush gains progress;
        /// any stain finished by this stroke is reported cleaned. Iterates back-to-front so a stain removing
        /// itself mid-stroke cannot disturb the walk.
        /// </summary>
        public void ApplyPaintAt(Vector2 screenPoint, Camera uiCamera, float paintDelta)
        {
            if (!IsAcceptingPaint)
                return;

            for (int i = _activePatches.Count - 1; i >= 0 && i < _activePatches.Count; i--)
            {
                PaintableDirtyPatch patch = _activePatches[i];
                if (patch == null)
                {
                    _activePatches.RemoveAt(i);
                    continue;
                }

                if (patch.ContainsScreenPoint(screenPoint, uiCamera) && patch.AddCleanProgress(paintDelta))
                    NotifyPatchCleaned(patch);
            }
        }

        /// <summary>Removes a fully cleaned stain from the active set and completes the bill when none remain.</summary>
        public void NotifyPatchCleaned(PaintableDirtyPatch patch)
        {
            if (!_activePatches.Remove(patch))
                return;

            UpdateStainsText();

            if (_activePatches.Count == 0)
                CompleteCurrentBill();
        }

        /// <summary>Cleans the current bill, converts its money and schedules the next bill.</summary>
        private void CompleteCurrentBill()
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
            _billsCleaned++;
            if (bill != null)
                bill.ShowCleaned();
            UpdateStatus($"Clean! Laundered ${washed}.");
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
            // The cash is not destroyed: it stays dirty in the GameManager pool and can be attempted again later.
            UpdateStatus($"{reason} Bill botched - still dirty. +{suspicionPerFailedBill} suspicion.");
            ResolveBill();
        }

        /// <summary>Locks painting and starts the pause before the next bill appears.</summary>
        private void ResolveBill()
        {
            _currentBillResolved = true;
            _activePatches.Clear();

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

        /// <summary>Presents a fresh dirty bill for the current index and re-enables painting.</summary>
        private void StartCurrentBill()
        {
            _currentBillResolved = false;
            _billTimeRemaining = secondsPerBill;
            _activePatches.Clear();

            if (bill != null)
            {
                bill.ResetForNewBill();
                if (bill.Patches != null)
                {
                    foreach (PaintableDirtyPatch patch in bill.Patches)
                        RegisterPatch(patch);
                }
            }

            UpdateBillInfo();
            UpdateTimerText();
            UpdateStainsText();
            UpdateStatus("Paint over every stain before time runs out!");
        }

        /// <summary>Ends the session and shows the summary, with optional auto-close.</summary>
        private void FinishSession()
        {
            _sessionActive = false;
            StopPendingRoutines();

            _lastResult = new PaintingBillsResult(_billsCleaned, _billsFailed, _moneyLaundered, _suspicionAdded);

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
            _activePatches.Clear();
            StopPendingRoutines();

            _lastResult = new PaintingBillsResult(_billsCleaned, _billsFailed, _moneyLaundered, _suspicionAdded);

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

        /// <summary>Injects this controller into the brush so it can report strokes back.</summary>
        private void InitializeInteractables()
        {
            if (brush != null)
                brush.Initialize(this);
        }

        /// <summary>Updates the optional bill number/value line if one is assigned.</summary>
        private void UpdateBillInfo()
        {
            if (billInfoText != null)
                billInfoText.text = $"Bill {_currentBillIndex + 1} of {_billAmounts.Count}  -  ${_billAmounts[_currentBillIndex]}";
        }

        /// <summary>Updates the optional timer line if one is assigned.</summary>
        private void UpdateTimerText()
        {
            if (timerText != null)
                timerText.text = $"Time: {_billTimeRemaining:0.0}s";
        }

        /// <summary>Updates the optional stains-remaining line if one is assigned.</summary>
        private void UpdateStainsText()
        {
            if (stainsText != null)
                stainsText.text = $"Stains left: {_activePatches.Count}";
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
