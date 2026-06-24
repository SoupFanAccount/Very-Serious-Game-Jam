using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minigames
{
    /// <summary>
    /// The press activation control: a crank the player spins. The player drags around the crank's axle and a
    /// connected handle turns to follow the pointer; each full revolution (in either direction) pulls the press,
    /// asking the <see cref="PrintingPressMinigameController"/> to grade the currently placed bill. It owns no
    /// alignment or session logic. Spin is only accepted while a bill is in play, and the progress toward the next
    /// press resets for each new bill so leftover rotation never carries over. Replaces the old click-to-press
    /// lever/button.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PrintingPressCrank : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("Visual handle turned to follow the spin. Defaults to this object's RectTransform.")]
        [SerializeField] private RectTransform handle;

        [Tooltip("Optional radial/filled Image whose fillAmount shows progress toward the next press (0..1).")]
        [SerializeField] private Image progressFill;

        [Tooltip("Net degrees of rotation that make one press. 360 = one full turn.")]
        [SerializeField] private float degreesPerPress = 360f;

        private RectTransform _rectTransform;
        private Camera _uiCamera;
        private PrintingPressMinigameController _controller;

        private bool _dragging;
        private float _lastPointerAngle;
        private float _progressDegrees; // net signed rotation accumulated toward the next press
        private float _handleAngle;     // current visual handle rotation, in degrees
        private bool _wasBillActive;

        /// <summary>Caches the rect, resolves the UI camera and defaults the handle to this object.</summary>
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (handle == null)
                handle = _rectTransform;

            Canvas canvas = GetComponentInParent<Canvas>();
            // Overlay canvases project with a null camera; camera/world-space canvases use their worldCamera.
            if (canvas != null)
                _uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            ResetProgress();
        }

        /// <summary>Connects the crank to the session controller. Called when a session begins.</summary>
        public void Initialize(PrintingPressMinigameController controller)
        {
            _controller = controller;
            _wasBillActive = false;
            _dragging = false;
            ResetProgress();
        }

        /// <summary>Resets the spin progress whenever a fresh bill becomes active.</summary>
        private void Update()
        {
            bool billActive = CanSpin();
            // Rising edge: a new bill just opened, so start its spin from zero.
            if (billActive && !_wasBillActive)
                ResetProgress();
            _wasBillActive = billActive;
        }

        /// <summary>Begins a spin gesture if a bill is currently in play.</summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!CanSpin())
                return;

            _dragging = true;
            _lastPointerAngle = PointerAngle(eventData);
        }

        /// <summary>
        /// Turns the handle with the pointer and accumulates net rotation. Each full turn (in either direction)
        /// pulls the press. Net signed rotation means jiggling back and forth cancels out, so an actual turn is
        /// required.
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;

            // The bill can resolve mid-spin (e.g. the timer runs out); stop accepting spin until the next bill.
            if (!CanSpin())
            {
                _dragging = false;
                return;
            }

            float currentAngle = PointerAngle(eventData);
            float delta = Mathf.DeltaAngle(_lastPointerAngle, currentAngle);
            _lastPointerAngle = currentAngle;

            _handleAngle += delta;
            ApplyHandleRotation();

            _progressDegrees += delta;
            UpdateProgressFill();

            if (Mathf.Abs(_progressDegrees) >= Mathf.Max(1f, degreesPerPress))
            {
                // Consume the turn before grading so a single revolution can never trigger two presses.
                _progressDegrees = 0f;
                UpdateProgressFill();
                _controller.TryPressCurrentBill();
            }
        }

        /// <summary>Ends the spin gesture; progress is kept so the player can re-grab to finish a turn.</summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            _dragging = false;
        }

        /// <summary>True while a controller is connected and a bill is in play.</summary>
        private bool CanSpin()
        {
            return _controller != null && _controller.IsBillActive;
        }

        /// <summary>Pointer angle in degrees around the crank's axle (this rect's pivot) in screen space.</summary>
        private float PointerAngle(PointerEventData eventData)
        {
            Vector2 axle = RectTransformUtility.WorldToScreenPoint(_uiCamera, _rectTransform.position);
            Vector2 dir = eventData.position - axle;
            return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }

        /// <summary>Clears progress toward the next press and updates the optional fill.</summary>
        private void ResetProgress()
        {
            _progressDegrees = 0f;
            UpdateProgressFill();
        }

        /// <summary>Applies the accumulated handle rotation about the Z axis.</summary>
        private void ApplyHandleRotation()
        {
            if (handle != null)
                handle.localRotation = Quaternion.Euler(0f, 0f, _handleAngle);
        }

        /// <summary>Drives the optional progress fill from how far the current turn has come.</summary>
        private void UpdateProgressFill()
        {
            if (progressFill != null)
                progressFill.fillAmount = Mathf.Clamp01(Mathf.Abs(_progressDegrees) / Mathf.Max(1f, degreesPerPress));
        }
    }
}
