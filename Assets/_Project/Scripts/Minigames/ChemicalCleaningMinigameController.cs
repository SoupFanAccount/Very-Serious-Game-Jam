using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    /// <summary>
    /// Owns the state of a chemical cleaning session: it splits the dirty money into bills, tracks the
    /// current bill's progress, and resolves each bill as cleaned or ruined. The draggable bill and the
    /// chemical zones report to it; it never touches the input system. It raises <see cref="SessionFinished"/>
    /// once the summary has been dismissed so the launcher can hide the panel.
    /// </summary>
    public class ChemicalCleaningMinigameController : MonoBehaviour
    {
        [Header("Scene References")]
        [Tooltip("The draggable cash bill for this minigame.")]
        [SerializeField] private DraggableCashBill bill;

        [Tooltip("The three chemical zones, in any order. Each carries its own ChemicalType.")]
        [SerializeField] private ChemicalDropZone[] chemicalZones;

        [Tooltip("Container holding the bill and zones. Shown during play, hidden on the summary.")]
        [SerializeField] private GameObject playArea;

        [Tooltip("Panel shown when the session ends.")]
        [SerializeField] private GameObject summaryPanel;

        [Tooltip("Text that receives the end-of-session summary.")]
        [SerializeField] private TextMeshProUGUI summaryText;

        [Tooltip("Optional live status line describing the current bill and feedback.")]
        [SerializeField] private TextMeshProUGUI statusText;

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

        [Tooltip("If true, the chemical zones swap positions for each new bill so the player must read the labels, not memorise the layout.")]
        [SerializeField] private bool randomizeChemicalPositions = true;

        [Header("Timing")]
        [Tooltip("Pause after a bill resolves before the next bill appears.")]
        [SerializeField] private float betweenBillsDelay = 0.6f;

        [Tooltip("Auto-close delay for the summary. 0 disables auto-close (use the close button).")]
        [SerializeField] private float autoCloseDelay = 4f;

        /// <summary>Number of chemicals that must be applied, in order, to clean a bill.</summary>
        private const int StagesPerBill = 3;

        private readonly List<int> _billAmounts = new List<int>();
        private int _currentBillIndex;
        private int _currentStage;
        private int _billsCleaned;
        private int _billsFailed;
        private int _moneyLaundered;
        private int _suspicionAdded;

        private bool _sessionActive;
        private bool _dragActive;
        private bool _currentBillResolved;
        private bool _sessionFinishedRaised;

        private ChemicalCleaningResult _lastResult;
        private Coroutine _advanceRoutine;
        private Coroutine _autoCloseRoutine;

        private Vector2[] _zoneSlots;
        private bool _slotsCaptured;

        /// <summary>Raised once the session is fully over and the summary dismissed. Carries the totals.</summary>
        public event Action<ChemicalCleaningResult> SessionFinished;

        /// <summary>Wires the optional close button and hides the summary panel.</summary>
        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseSession);
            if (quitButton != null)
                quitButton.onClick.AddListener(CancelSession);
            if (summaryPanel != null)
                summaryPanel.SetActive(false);
        }

        /// <summary>
        /// Starts a new session for the given amount of dirty money. Splits it into bills, resets the
        /// counters and presents the first bill.
        /// </summary>
        public void BeginSession(int dirtyMoneyAvailable)
        {
            StopPendingRoutines();
            _sessionFinishedRaised = false;

            // Refuse to run a half-wired prefab: without a bill and zones there is nothing to drag or clean.
            // Abort gracefully (rather than throwing) so the launcher still hides the panel and unfreezes the player.
            if (bill == null || chemicalZones == null || chemicalZones.Length == 0)
            {
                Debug.LogError(
                    $"{nameof(ChemicalCleaningMinigameController)}: assign the bill and chemical zones before starting; aborting session.",
                    this);
                _sessionActive = false;
                _lastResult = new ChemicalCleaningResult(0, 0, 0, 0);
                RaiseSessionFinished();
                return;
            }

            _billsCleaned = 0;
            _billsFailed = 0;
            _moneyLaundered = 0;
            _suspicionAdded = 0;
            _currentBillIndex = 0;
            _dragActive = false;
            _currentBillResolved = false;

            BuildBillChunks(dirtyMoneyAvailable);
            InitializeInteractables();
            EnsureZoneSlotsCaptured();

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
        /// Asks whether a new drag gesture may begin. Returns false during the between-bill pause or when
        /// no session is active, so a held drag cannot carry over into the next bill.
        /// </summary>
        public bool BeginDrag()
        {
            if (!_sessionActive || _currentBillResolved)
                return false;

            _dragActive = true;
            return true;
        }

        /// <summary>Marks the current drag gesture as ended.</summary>
        public void NotifyDragEnded()
        {
            _dragActive = false;
        }

        /// <summary>
        /// Processes the bill entering a chemical zone. The correct next chemical advances the bill (and
        /// cleans it on the third); any other chemical ruins it.
        /// </summary>
        public void HandleChemicalEntered(ChemicalDropZone zone)
        {
            if (!_sessionActive || _currentBillResolved || !_dragActive || zone == null)
                return;

            int entered = (int)zone.ChemicalType;
            int required = _currentStage + 1;

            if (entered == required)
            {
                zone.PlayCorrectFeedback();
                _currentStage = entered;

                if (_currentStage >= StagesPerBill)
                {
                    CompleteCurrentBill();
                }
                else
                {
                    bill.SetStage(_currentStage);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayItemUse();
                    UpdateStatus($"Chem {_currentStage} applied. Next: Chem {_currentStage + 1}.");
                }
            }
            else
            {
                zone.PlayWrongFeedback();
                FailCurrentBill();
            }
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
            bill.SetStage(StagesPerBill);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayConfirm();
            UpdateStatus($"Clean! Laundered ${washed}.");
            ResolveBill();
        }

        /// <summary>Botches the current bill, adds suspicion and schedules the next bill.</summary>
        private void FailCurrentBill()
        {
            GameManager game = GameManager.Instance;
            if (game != null)
                game.AddSuspicion(suspicionPerFailedBill);

            _suspicionAdded += suspicionPerFailedBill;
            _billsFailed++;
            bill.ShowFailed();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayCancel();
            // The cash is not destroyed: it stays dirty in the GameManager pool and can be attempted again later.
            UpdateStatus($"Wrong chemical! Bill botched - still dirty. +{suspicionPerFailedBill} suspicion.");
            ResolveBill();
        }

        /// <summary>Locks input and starts the pause before the next bill appears.</summary>
        private void ResolveBill()
        {
            _dragActive = false;
            _currentBillResolved = true;

            // Stop the resolved bill from following a still-held pointer; the player must grab the next bill.
            bill.CancelDrag();

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

        /// <summary>Resets the bill to a fresh dirty state for the current index and re-enables input.</summary>
        private void StartCurrentBill()
        {
            _currentStage = 0;
            _currentBillResolved = false;
            bill.ResetToStart();
            bill.SetStage(0);
            ApplyChemicalLayout();
            UpdateStatus(
                $"Bill {_currentBillIndex + 1} of {_billAmounts.Count}  -  ${_billAmounts[_currentBillIndex]}  -  Drag through Chem 1 -> 2 -> 3");
        }

        /// <summary>Ends the session and shows the summary, with optional auto-close.</summary>
        private void FinishSession()
        {
            _sessionActive = false;
            _dragActive = false;
            StopPendingRoutines();

            _lastResult = new ChemicalCleaningResult(_billsCleaned, _billsFailed, _moneyLaundered, _suspicionAdded);

            if (playArea != null)
                playArea.SetActive(false);
            if (summaryPanel != null)
                summaryPanel.SetActive(true);
            if (summaryText != null)
                summaryText.text = _lastResult.ToSummaryString();

            // Tally the laundered cash on the summary; length scales with the amount.
            if (_moneyLaundered > 0 && AudioManager.Instance != null)
                AudioManager.Instance.PlayCashCount(_moneyLaundered * 0.01f);

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
            _dragActive = false;
            StopPendingRoutines();

            // Drop any bill the player is mid-drag with so no stale drag state survives into the next session.
            if (bill != null)
                bill.CancelDrag();

            _lastResult = new ChemicalCleaningResult(_billsCleaned, _billsFailed, _moneyLaundered, _suspicionAdded);

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

        /// <summary>Injects this controller into the bill and zones so they can report back.</summary>
        private void InitializeInteractables()
        {
            if (bill != null)
                bill.Initialize(this);

            if (chemicalZones == null)
                return;

            foreach (ChemicalDropZone zone in chemicalZones)
            {
                if (zone != null)
                    zone.Initialize(this);
            }
        }

        /// <summary>Records each zone's designed position once, before any shuffle moves them.</summary>
        private void EnsureZoneSlotsCaptured()
        {
            if (_slotsCaptured || chemicalZones == null)
                return;

            _zoneSlots = new Vector2[chemicalZones.Length];
            for (int i = 0; i < chemicalZones.Length; i++)
            {
                if (chemicalZones[i] != null)
                    _zoneSlots[i] = GetRect(chemicalZones[i]).anchoredPosition;
            }

            _slotsCaptured = true;
        }

        /// <summary>Positions the zones for the current bill: shuffled when enabled, otherwise at their slots.</summary>
        private void ApplyChemicalLayout()
        {
            if (_zoneSlots == null || chemicalZones == null)
                return;

            if (randomizeChemicalPositions)
                ShuffleChemicalPositions();
            else
                RestoreChemicalPositions();
        }

        /// <summary>Assigns each zone a random slot (Fisher-Yates), so the chemicals appear in a new order.</summary>
        private void ShuffleChemicalPositions()
        {
            int count = chemicalZones.Length;
            int[] order = new int[count];
            for (int i = 0; i < count; i++)
                order[i] = i;

            for (int i = count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                int temp = order[i];
                order[i] = order[j];
                order[j] = temp;
            }

            for (int i = 0; i < count; i++)
            {
                if (chemicalZones[i] != null)
                    GetRect(chemicalZones[i]).anchoredPosition = _zoneSlots[order[i]];
            }
        }

        /// <summary>Returns every zone to its original designed slot.</summary>
        private void RestoreChemicalPositions()
        {
            for (int i = 0; i < chemicalZones.Length; i++)
            {
                if (chemicalZones[i] != null)
                    GetRect(chemicalZones[i]).anchoredPosition = _zoneSlots[i];
            }
        }

        /// <summary>Shortcut to a zone's RectTransform.</summary>
        private static RectTransform GetRect(ChemicalDropZone zone)
        {
            return (RectTransform)zone.transform;
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
