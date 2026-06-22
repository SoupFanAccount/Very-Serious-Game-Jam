using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minigames
{
    /// <summary>
    /// The draggable cash/bill UI object the player moves through the chemical zones. It follows the mouse
    /// while dragged and reflects its current cleaning stage through colour and a label. It owns no game
    /// logic: zone entry decisions live in the <see cref="ChemicalCleaningMinigameController"/>.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableCashBill : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("Image whose colour shows the bill's current stage. Defaults to the Image on this object.")]
        [SerializeField] private Image billImage;

        [Tooltip("Label that shows the bill's current stage text.")]
        [SerializeField] private TextMeshProUGUI stateLabel;

        [Header("Stage Colours")]
        [SerializeField] private Color dirtyColor = new Color(0.45f, 0.4f, 0.25f);
        [SerializeField] private Color chem1Color = new Color(0.55f, 0.65f, 0.45f);
        [SerializeField] private Color chem2Color = new Color(0.45f, 0.7f, 0.7f);
        [SerializeField] private Color cleanColor = new Color(0.55f, 0.85f, 0.55f);
        [SerializeField] private Color failedColor = new Color(0.8f, 0.35f, 0.35f);

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Canvas _canvas;
        private ChemicalCleaningMinigameController _controller;
        private Vector2 _startPosition;
        private bool _dragging;

        /// <summary>Caches components and remembers the bill's resting position for resets.</summary>
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvas = GetComponentInParent<Canvas>();
            _startPosition = _rectTransform.anchoredPosition;

            if (billImage == null)
                billImage = GetComponent<Image>();
        }

        /// <summary>Connects the bill to the session controller. Called when a session begins.</summary>
        public void Initialize(ChemicalCleaningMinigameController controller)
        {
            _controller = controller;
        }

        /// <summary>Updates the visuals for a clean-progress stage: 0 = dirty, 1-2 = chemical applied, 3 = clean.</summary>
        public void SetStage(int stage)
        {
            switch (stage)
            {
                case 1:
                    Apply(chem1Color, "Chem 1 applied");
                    break;
                case 2:
                    Apply(chem2Color, "Chem 2 applied");
                    break;
                case 3:
                    Apply(cleanColor, "Clean!");
                    break;
                default:
                    Apply(dirtyColor, "Dirty");
                    break;
            }
        }

        /// <summary>Shows the ruined visual after the wrong chemical was used.</summary>
        public void ShowFailed()
        {
            Apply(failedColor, "Ruined!");
        }

        /// <summary>Returns the bill to its resting position so it is ready to be dragged again.</summary>
        public void ResetToStart()
        {
            _rectTransform.anchoredPosition = _startPosition;
        }

        /// <summary>Begins a drag gesture if the controller currently allows one.</summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_controller == null)
                return;

            _dragging = _controller.BeginDrag();
            if (_dragging)
                // Stop the bill from blocking raycasts so the chemical zones underneath receive pointer events.
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
        /// can set it down mid-sequence; the start position is only restored when a new bill begins.
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;

            _dragging = false;
            _canvasGroup.blocksRaycasts = true;
            _controller.NotifyDragEnded();
        }

        /// <summary>
        /// Forcibly stops the current drag without ending the pointer gesture. Called when a bill resolves
        /// mid-drag so the bill stops following the mouse and the player must press again to grab the next
        /// one. The held pointer no longer moves the bill; <see cref="OnEndDrag"/> becomes a no-op on release.
        /// </summary>
        public void CancelDrag()
        {
            if (!_dragging)
                return;

            _dragging = false;
            _canvasGroup.blocksRaycasts = true;
        }

        /// <summary>Applies a colour and label together.</summary>
        private void Apply(Color color, string label)
        {
            if (billImage != null)
                billImage.color = color;
            if (stateLabel != null)
                stateLabel.text = label;
        }
    }
}
