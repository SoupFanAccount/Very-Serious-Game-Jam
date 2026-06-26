using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    /// <summary>
    /// The cash bill the player paints clean. It groups the dirty stains that sit on top of it and exposes
    /// them to the <see cref="PaintingBillsMinigameController"/>, and it reflects the bill's state (dirty,
    /// cleaned, ruined) through a background colour and label. The same bill object is reused for every money
    /// chunk in a session; <see cref="ResetForNewBill"/> returns it to a fresh dirty state.
    /// </summary>
    public class PaintableBill : MonoBehaviour
    {
        [Tooltip("Background image whose colour shows the bill's state. Defaults to the Image on this object.")]
        [SerializeField] private Image billImage;

        [Tooltip("Optional label that shows the bill's state text (Dirty / Clean! / Botched!).")]
        [SerializeField] private TextMeshProUGUI stateLabel;

        [Tooltip("Stains on this bill. If left empty, all PaintableDirtyPatch children are used.")]
        [SerializeField] private PaintableDirtyPatch[] patches;

        [Header("State Colours")]
        [SerializeField] private Color dirtyColor = new Color(0.5f, 0.46f, 0.32f);
        [SerializeField] private Color cleanColor = new Color(0.55f, 0.85f, 0.55f);
        [SerializeField] private Color ruinedColor = new Color(0.8f, 0.35f, 0.35f);

        private bool _initialized;

        /// <summary>The stains that must be painted clean to launder this bill.</summary>
        public IReadOnlyList<PaintableDirtyPatch> Patches
        {
            get
            {
                EnsureInitialized();
                return patches;
            }
        }

        /// <summary>Caches the background image and discovers child stains when none are assigned.</summary>
        private void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Caches the background image and discovers child stains on first use. Safe to call repeatedly. This
        /// runs from <see cref="Awake"/> and defensively from every public entry point, because the controller
        /// can drive the bill in the same frame it is activated - before Awake is guaranteed to have run -
        /// which would otherwise leave <see cref="patches"/> empty and the bill would register no stains.
        /// </summary>
        private void EnsureInitialized()
        {
            if (_initialized)
                return;

            _initialized = true;

            if (billImage == null)
                billImage = GetComponent<Image>();

            if (patches == null || patches.Length == 0)
                patches = GetComponentsInChildren<PaintableDirtyPatch>(true);
        }

        /// <summary>Resets the background and every stain to a fresh, fully dirty state.</summary>
        public void ResetForNewBill()
        {
            EnsureInitialized();
            Apply(dirtyColor, "Dirty");

            if (patches == null)
                return;

            foreach (PaintableDirtyPatch patch in patches)
            {
                if (patch != null)
                    patch.ResetPatch();
            }
        }

        /// <summary>Shows the cleaned visual once every stain has been painted away.</summary>
        public void ShowCleaned()
        {
            Apply(cleanColor, "Clean!");
        }

        /// <summary>Shows the botched visual after the bill fails. The cash is not destroyed; it stays dirty.</summary>
        public void ShowRuined()
        {
            Apply(ruinedColor, "Botched!");
        }

        /// <summary>Applies a background colour and label together, guarding missing references.</summary>
        private void Apply(Color color, string label)
        {
            if (billImage != null)
                billImage.color = color;
            if (stateLabel != null)
                stateLabel.text = label;
        }
    }
}
