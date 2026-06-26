using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minigames
{
    /// <summary>
    /// The draggable cash/bill UI object the player slides onto the printing-press bed. It follows the mouse
    /// while dragged and reflects its state (dirty, printed, ruined) through colour and a label. It owns no
    /// session logic: whether a placed bill prints clean or is ruined is decided by the
    /// <see cref="PrintingPressMinigameController"/> when the lever is pulled. The controller toggles
    /// <see cref="SetInteractable"/> so a resolved bill cannot be picked up again before the next one begins.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class PressDraggableBill : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("Image whose colour shows the bill's current state. Defaults to the Image on this object.")]
        [SerializeField] private Image billImage;

        [Tooltip("Optional label that shows the bill's current state text (Dirty / Printed! / Botched!).")]
        [SerializeField] private TextMeshProUGUI stateLabel;

        [Header("State Colours")]
        [SerializeField] private Color dirtyColor = new Color(0.45f, 0.4f, 0.25f);
        [SerializeField] private Color printedColor = new Color(0.55f, 0.85f, 0.55f);
        [SerializeField] private Color ruinedColor = new Color(0.8f, 0.35f, 0.35f);

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Canvas _canvas;
        private Vector2 _startPosition;
        private bool _dragging;
        private bool _interactable;
        private bool _initialized;

        /// <summary>The bill's RectTransform, read by the target zone to measure alignment.</summary>
        public RectTransform Rect
        {
            get
            {
                EnsureInitialized();
                return _rectTransform;
            }
        }

        /// <summary>Caches components and remembers the bill's resting position for resets.</summary>
        private void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Caches components and the resting position on first use. Safe to call repeatedly. This runs from
        /// <see cref="Awake"/> and defensively from every public entry point, because the controller can drive
        /// the bill in the same frame it is activated - before Awake is guaranteed to have run - and accessing
        /// the cached <see cref="_rectTransform"/> before then would throw.
        /// </summary>
        private void EnsureInitialized()
        {
            if (_initialized)
                return;

            _initialized = true;
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvas = GetComponentInParent<Canvas>();
            _startPosition = _rectTransform.anchoredPosition;

            if (billImage == null)
                billImage = GetComponent<Image>();
        }

        /// <summary>Enables or disables grabbing. The controller opens dragging only while a bill is in play.</summary>
        public void SetInteractable(bool value)
        {
            _interactable = value;
            if (!value)
                CancelDrag();
        }

        /// <summary>Returns the bill to its resting position and shows the fresh dirty state for a new bill.</summary>
        public void ResetForNewBill()
        {
            EnsureInitialized();
            _rectTransform.anchoredPosition = _startPosition;
            _canvasGroup.blocksRaycasts = true;
            Apply(dirtyColor, "Dirty");
        }

        /// <summary>Shows the printed-clean visual after a successful press.</summary>
        public void ShowPrinted()
        {
            Apply(printedColor, "Printed!");
        }

        /// <summary>Shows the botched visual after a misprint or timeout. The cash is not destroyed; it stays dirty.</summary>
        public void ShowRuined()
        {
            Apply(ruinedColor, "Botched!");
        }

        /// <summary>Begins a drag gesture if the controller currently allows one.</summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            EnsureInitialized();

            if (!_interactable)
                return;

            _dragging = true;
            // Stop the bill from blocking raycasts so the press bed underneath still receives pointer events.
            _canvasGroup.blocksRaycasts = false;
        }

        /// <summary>Moves the bill with the pointer while dragging.</summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;

            float scale = _canvas != null ? _canvas.scaleFactor : 1f;
            _rectTransform.anchoredPosition += eventData.delta / scale;
        }

        /// <summary>
        /// Ends the drag gesture and restores raycasts. The bill is left where it was released so the player
        /// can set it down on the press bed; the start position is only restored when a new bill begins.
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;

            _dragging = false;
            _canvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// Forcibly stops the current drag without ending the pointer gesture. Called when a bill resolves
        /// mid-drag so the bill stops following the mouse. The held pointer no longer moves the bill;
        /// <see cref="OnEndDrag"/> becomes a no-op on release.
        /// </summary>
        public void CancelDrag()
        {
            if (!_dragging)
                return;

            _dragging = false;
            _canvasGroup.blocksRaycasts = true;
        }

        /// <summary>Applies a colour and label together, guarding missing references.</summary>
        private void Apply(Color color, string label)
        {
            EnsureInitialized();
            if (billImage != null)
                billImage.color = color;
            if (stateLabel != null)
                stateLabel.text = label;
        }
    }
}
