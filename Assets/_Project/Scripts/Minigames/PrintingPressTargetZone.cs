using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    /// <summary>
    /// The highlighted alignment target on the press bed. It owns the geometry the controller needs to grade
    /// a placement: the screen-space distance from a bill's centre to this zone's centre, and whether the
    /// bill's centre sits inside the zone rectangle. It also flashes green or red to signal the press result.
    /// It holds no session state; the <see cref="PrintingPressMinigameController"/> reads its measurements and
    /// decides the outcome.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PrintingPressTargetZone : MonoBehaviour
    {
        [Tooltip("Image flashed to signal success/failure. Defaults to the Image on this object.")]
        [SerializeField] private Image zoneImage;

        [Tooltip("Resting colour of the target zone, restored after a flash.")]
        [SerializeField] private Color idleColor = new Color(0.3f, 0.55f, 0.85f, 0.5f);

        [Tooltip("Colour flashed when a bill prints clean.")]
        [SerializeField] private Color successColor = new Color(0.4f, 0.85f, 0.4f, 0.8f);

        [Tooltip("Colour flashed when a bill is ruined.")]
        [SerializeField] private Color failureColor = new Color(0.85f, 0.4f, 0.4f, 0.8f);

        [Tooltip("How long a success/failure flash is held before fading back to the idle colour.")]
        [SerializeField] private float flashDuration = 0.4f;

        private RectTransform _rectTransform;
        private Camera _uiCamera;
        private Coroutine _flashRoutine;

        /// <summary>Caches the rect, resolves the UI camera and applies the idle colour.</summary>
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();

            if (zoneImage == null)
                zoneImage = GetComponent<Image>();

            Canvas canvas = GetComponentInParent<Canvas>();
            // Overlay canvases project with a null camera; camera/world-space canvases use their worldCamera.
            if (canvas != null)
                _uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            ApplyColor(idleColor);
        }

        /// <summary>Screen-space pixel distance between the given bill's centre and this zone's centre.</summary>
        public float ScreenDistanceTo(RectTransform billRect)
        {
            if (billRect == null || _rectTransform == null)
                return float.MaxValue;

            Vector2 billCenter = CenterScreenPoint(billRect);
            Vector2 zoneCenter = CenterScreenPoint(_rectTransform);
            return Vector2.Distance(billCenter, zoneCenter);
        }

        /// <summary>True when the given bill's centre falls within this zone's rectangle.</summary>
        public bool ContainsBillCenter(RectTransform billRect)
        {
            if (billRect == null || _rectTransform == null)
                return false;

            Vector2 billCenter = CenterScreenPoint(billRect);
            return RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, billCenter, _uiCamera);
        }

        /// <summary>Flashes the zone green to acknowledge a successful print.</summary>
        public void FlashSuccess()
        {
            Flash(successColor);
        }

        /// <summary>Flashes the zone red to acknowledge a ruined bill.</summary>
        public void FlashFailure()
        {
            Flash(failureColor);
        }

        /// <summary>Restores the resting colour, cancelling any in-progress flash.</summary>
        public void ResetVisual()
        {
            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
                _flashRoutine = null;
            }

            ApplyColor(idleColor);
        }

        /// <summary>Projects a rect's centre to a screen point, valid for any canvas render mode.</summary>
        private Vector2 CenterScreenPoint(RectTransform rect)
        {
            Vector3 worldCenter = rect.TransformPoint(rect.rect.center);
            return RectTransformUtility.WorldToScreenPoint(_uiCamera, worldCenter);
        }

        /// <summary>Starts a single flash to the given colour, restarting any flash already running.</summary>
        private void Flash(Color color)
        {
            if (zoneImage == null)
                return;

            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine(color));
        }

        /// <summary>Holds the flash colour, then fades back to the idle colour over the flash duration.</summary>
        private IEnumerator FlashRoutine(Color color)
        {
            ApplyColor(color);

            float elapsed = 0f;
            float safeDuration = Mathf.Max(0.01f, flashDuration);
            while (elapsed < safeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                ApplyColor(Color.Lerp(color, idleColor, elapsed / safeDuration));
                yield return null;
            }

            ApplyColor(idleColor);
            _flashRoutine = null;
        }

        /// <summary>Sets the zone image colour if one is assigned.</summary>
        private void ApplyColor(Color color)
        {
            if (zoneImage != null)
                zoneImage.color = color;
        }
    }
}
