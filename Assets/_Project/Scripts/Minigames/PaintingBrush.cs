using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Minigames
{
    /// <summary>
    /// The brush cursor. It follows the mouse while the minigame is open and, while the left button is held
    /// and the controller is accepting paint, reports paint strokes at the cursor position. It reads the
    /// pointer through the new Input System (<see cref="Mouse"/>) rather than the shared input asset, so it
    /// never modifies project input bindings. Overlap resolution and progress live in the controller and the
    /// stains; the brush only supplies "where" and "how hard".
    /// </summary>
    public class PaintingBrush : MonoBehaviour
    {
        [Tooltip("Visual that follows the cursor. Defaults to this object's RectTransform.")]
        [SerializeField] private RectTransform brushVisual;

        [Tooltip("Optional image tinted to show whether the brush is currently painting.")]
        [SerializeField] private Image brushImage;

        [Tooltip("Brush colour while idle (button up).")]
        [SerializeField] private Color idleColor = new Color(1f, 1f, 1f, 0.5f);

        [Tooltip("Brush colour while actively painting (button held over the bill).")]
        [SerializeField] private Color paintingColor = new Color(0.55f, 0.85f, 0.55f, 0.9f);

        [Tooltip("Multiplies how fast each stroke cleans. 1 means stain cleanDuration is measured in real seconds.")]
        [SerializeField] private float brushStrength = 1f;

        private PaintingBillsMinigameController _controller;
        private Canvas _canvas;
        private RectTransform _canvasRect;
        private Camera _uiCamera;

        /// <summary>Caches the canvas and resolves the camera used for UI-space conversions.</summary>
        private void Awake()
        {
            if (brushVisual == null)
                brushVisual = transform as RectTransform;

            if (brushImage == null && brushVisual != null)
                brushImage = brushVisual.GetComponent<Image>();

            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null)
            {
                _canvasRect = _canvas.transform as RectTransform;
                // Overlay canvases project with a null camera; camera/world-space canvases use their worldCamera.
                _uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            }

            SetPaintingVisual(false);
        }

        /// <summary>Connects the brush to the session controller. Called when a session begins.</summary>
        public void Initialize(PaintingBillsMinigameController controller)
        {
            _controller = controller;
        }

        /// <summary>Tracks the cursor, then forwards a paint stroke when the player is painting.</summary>
        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
                return;

            Vector2 screenPoint = mouse.position.ReadValue();
            FollowCursor(screenPoint);

            bool painting = mouse.leftButton.isPressed &&
                            _controller != null &&
                            _controller.IsAcceptingPaint;

            SetPaintingVisual(painting);

            if (painting)
                _controller.ApplyPaintAt(screenPoint, _uiCamera, Time.unscaledDeltaTime * brushStrength);
        }

        /// <summary>Moves the brush visual under the cursor, working for any canvas render mode.</summary>
        private void FollowCursor(Vector2 screenPoint)
        {
            if (brushVisual == null || _canvasRect == null)
                return;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    _canvasRect, screenPoint, _uiCamera, out Vector3 worldPoint))
                brushVisual.position = worldPoint;
        }

        /// <summary>Tints the brush to signal whether it is currently painting.</summary>
        private void SetPaintingVisual(bool painting)
        {
            if (brushImage != null)
                brushImage.color = painting ? paintingColor : idleColor;
        }
    }
}
