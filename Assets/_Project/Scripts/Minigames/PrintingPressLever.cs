using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    /// <summary>
    /// The press activation control. When the player pulls the lever (a UI <see cref="Button"/> click, or a
    /// direct <see cref="ActivatePress"/> call) it asks the <see cref="PrintingPressMinigameController"/> to
    /// evaluate the currently placed bill and flashes for tactile feedback. It owns no alignment or session
    /// logic; the controller decides whether the bill prints clean or is ruined.
    /// </summary>
    public class PrintingPressLever : MonoBehaviour
    {
        [Tooltip("Optional button that triggers the press. If assigned, its click is wired to ActivatePress.")]
        [SerializeField] private Button leverButton;

        [Tooltip("Optional image flashed when the lever is pulled. Defaults to the Image on this object.")]
        [SerializeField] private Image leverImage;

        [Tooltip("Resting colour of the lever, restored after a pull flash.")]
        [SerializeField] private Color idleColor = new Color(0.7f, 0.7f, 0.7f);

        [Tooltip("Colour flashed for an instant when the lever is pulled.")]
        [SerializeField] private Color pressedColor = new Color(0.95f, 0.85f, 0.4f);

        [Tooltip("How long the pull flash is held before fading back to the idle colour.")]
        [SerializeField] private float flashDuration = 0.2f;

        private PrintingPressMinigameController _controller;
        private Coroutine _flashRoutine;

        /// <summary>Caches the lever image, wires the button click and applies the idle colour.</summary>
        private void Awake()
        {
            if (leverImage == null)
                leverImage = GetComponent<Image>();

            if (leverButton != null)
                leverButton.onClick.AddListener(ActivatePress);

            ApplyColor(idleColor);
        }

        /// <summary>Connects the lever to the session controller. Called when a session begins.</summary>
        public void Initialize(PrintingPressMinigameController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Pulls the press. Flashes for feedback, then asks the controller to grade the placed bill. Safe to
        /// call from a UI button. Does nothing useful before a session has connected a controller.
        /// </summary>
        public void ActivatePress()
        {
            Flash();

            if (_controller != null)
                _controller.TryPressCurrentBill();
        }

        /// <summary>Starts a single pull flash, restarting any flash already running.</summary>
        private void Flash()
        {
            if (leverImage == null)
                return;

            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        /// <summary>Holds the pressed colour, then fades back to the idle colour over the flash duration.</summary>
        private IEnumerator FlashRoutine()
        {
            ApplyColor(pressedColor);

            float elapsed = 0f;
            float safeDuration = Mathf.Max(0.01f, flashDuration);
            while (elapsed < safeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                ApplyColor(Color.Lerp(pressedColor, idleColor, elapsed / safeDuration));
                yield return null;
            }

            ApplyColor(idleColor);
            _flashRoutine = null;
        }

        /// <summary>Sets the lever image colour if one is assigned.</summary>
        private void ApplyColor(Color color)
        {
            if (leverImage != null)
                leverImage.color = color;
        }
    }
}
