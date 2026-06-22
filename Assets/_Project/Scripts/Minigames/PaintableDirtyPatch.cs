using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    /// <summary>
    /// One dirty stain on a bill. The player "paints" it clean by holding the brush over it: each painted
    /// frame raises <see cref="cleanDuration"/>-paced progress from dirty to clean, fading, recolouring and
    /// (optionally) shrinking the stain as it goes. It owns no session logic and never touches input; the
    /// <see cref="PaintingBrush"/> tests it for overlap and the <see cref="PaintingBillsMinigameController"/>
    /// decides what a cleaned patch means for the bill.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PaintableDirtyPatch : MonoBehaviour
    {
        [Tooltip("Image tinted/faded as the stain cleans. Defaults to the Image on this object.")]
        [SerializeField] private Image patchImage;

        [Tooltip("Total seconds of brushing needed to fully clean this stain.")]
        [SerializeField] private float cleanDuration = 1.2f;

        [Header("Appearance")]
        [Tooltip("Colour of the fresh, fully dirty stain.")]
        [SerializeField] private Color dirtyColor = new Color(0.42f, 0.32f, 0.18f, 1f);

        [Tooltip("Colour the stain lerps toward as it cleans.")]
        [SerializeField] private Color cleanColor = new Color(0.55f, 0.85f, 0.55f, 1f);

        [Tooltip("Image alpha once fully cleaned. 0 fades the stain out completely.")]
        [Range(0f, 1f)]
        [SerializeField] private float fullyCleanAlpha;

        [Tooltip("If true, the stain shrinks as it cleans for extra feedback.")]
        [SerializeField] private bool shrinkWhenClean = true;

        [Tooltip("Scale (relative to the starting scale) reached when fully cleaned, if shrinking.")]
        [Range(0.05f, 1f)]
        [SerializeField] private float cleanScale = 0.4f;

        private RectTransform _rectTransform;
        private Vector3 _startScale;
        private float _cleanProgress;
        private bool _cleaned;

        /// <summary>True once the stain has been fully painted clean.</summary>
        public bool IsCleaned => _cleaned;

        /// <summary>Caches components and records the resting scale used for shrink feedback.</summary>
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _startScale = _rectTransform.localScale;

            if (patchImage == null)
                patchImage = GetComponent<Image>();
        }

        /// <summary>Returns the stain to its fresh, fully dirty state so the bill can be reused.</summary>
        public void ResetPatch()
        {
            _cleanProgress = 0f;
            _cleaned = false;
            _rectTransform.localScale = _startScale;
            ApplyVisual();
        }

        /// <summary>
        /// Adds cleaning progress for one painted step. <paramref name="paintDelta"/> is measured in seconds,
        /// so <see cref="cleanDuration"/> is the total brushing time needed. Returns true on the single step
        /// that completes the stain, so the controller can react exactly once.
        /// </summary>
        public bool AddCleanProgress(float paintDelta)
        {
            if (_cleaned)
                return false;

            float gain = cleanDuration > 0f ? paintDelta / cleanDuration : 1f;
            _cleanProgress = Mathf.Clamp01(_cleanProgress + gain);
            ApplyVisual();

            if (_cleanProgress < 1f)
                return false;

            _cleaned = true;
            return true;
        }

        /// <summary>True when the given screen point falls inside this stain's rectangle and it is still dirty.</summary>
        public bool ContainsScreenPoint(Vector2 screenPoint, Camera uiCamera)
        {
            if (_cleaned)
                return false;

            return RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, screenPoint, uiCamera);
        }

        /// <summary>Updates colour, alpha and scale to reflect the current clean progress.</summary>
        private void ApplyVisual()
        {
            if (patchImage != null)
            {
                Color color = Color.Lerp(dirtyColor, cleanColor, _cleanProgress);
                color.a = Mathf.Lerp(dirtyColor.a, fullyCleanAlpha, _cleanProgress);
                patchImage.color = color;
            }

            if (shrinkWhenClean)
                _rectTransform.localScale = Vector3.Lerp(_startScale, _startScale * cleanScale, _cleanProgress);
        }
    }
}
