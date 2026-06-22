using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minigames
{
    /// <summary>
    /// A UI container representing one cleaning chemical. While the bill is being dragged it detects when the
    /// bill enters and reports the entry to the controller. A pointer must exit before the same zone can
    /// trigger again, which prevents a stationary bill from re-firing every frame.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ChemicalDropZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("Which chemical this zone represents.")]
        [SerializeField] private ChemicalType chemicalType = ChemicalType.Chem1;

        [Tooltip("Optional label shown on the zone.")]
        [SerializeField] private TextMeshProUGUI label;

        [Header("Feedback Colours")]
        [SerializeField] private Color idleColor = new Color(0.2f, 0.2f, 0.3f, 0.85f);
        [SerializeField] private Color correctFlashColor = new Color(0.4f, 0.85f, 0.4f);
        [SerializeField] private Color wrongFlashColor = new Color(0.9f, 0.4f, 0.4f);
        [SerializeField] private float flashDuration = 0.25f;

        private Image _image;
        private ChemicalCleaningMinigameController _controller;
        private bool _pointerInside;
        private Coroutine _flashRoutine;

        /// <summary>The chemical this zone applies.</summary>
        public ChemicalType ChemicalType => chemicalType;

        /// <summary>Caches the image and applies the idle colour and label.</summary>
        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.color = idleColor;
            if (label != null)
                label.text = $"Chem {(int)chemicalType}";
        }

        /// <summary>Connects the zone to the session controller. Called when a session begins.</summary>
        public void Initialize(ChemicalCleaningMinigameController controller)
        {
            _controller = controller;
        }

        /// <summary>Reports the bill entering this zone, once per entry.</summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_controller == null || _pointerInside)
                return;

            // Only react to the dragged cash bill, never to a bare hover.
            if (eventData.pointerDrag == null ||
                eventData.pointerDrag.GetComponent<DraggableCashBill>() == null)
                return;

            _pointerInside = true;
            _controller.HandleChemicalEntered(this);
        }

        /// <summary>Clears the re-entry guard so the zone can trigger again on the next entry.</summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            _pointerInside = false;
        }

        /// <summary>Flashes the zone to confirm the correct chemical was applied.</summary>
        public void PlayCorrectFeedback()
        {
            Flash(correctFlashColor);
        }

        /// <summary>Flashes the zone to signal the wrong chemical was used.</summary>
        public void PlayWrongFeedback()
        {
            Flash(wrongFlashColor);
        }

        /// <summary>Briefly tints the zone, then returns it to its idle colour.</summary>
        private void Flash(Color color)
        {
            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine(color));
        }

        /// <summary>Coroutine backing <see cref="Flash"/>.</summary>
        private IEnumerator FlashRoutine(Color color)
        {
            _image.color = color;
            yield return new WaitForSecondsRealtime(flashDuration);
            _image.color = idleColor;
            _flashRoutine = null;
        }
    }
}
