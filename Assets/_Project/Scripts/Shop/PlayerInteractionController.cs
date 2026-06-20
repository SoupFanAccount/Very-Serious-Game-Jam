using UnityEngine;
using UnityEngine.InputSystem;

namespace Shop
{
    /// <summary>
    /// Lives on the player. Detects nearby interactables, shows a prompt for the closest available one,
    /// and triggers it on the interact input. This is the only component shop interactions add to the player.
    /// </summary>
    public class PlayerInteractionController : MonoBehaviour
    {
        [Tooltip("Broad-phase radius used to find nearby interactables.")]
        [SerializeField] private float detectionRadius = 2.5f;

        [Tooltip("Physics layers searched for interactables. Default is everything.")]
        [SerializeField] private LayerMask interactableMask = ~0;

        [Tooltip("Optional point to measure interaction distance from (e.g. the player's hands). Defaults to this transform.")]
        [SerializeField] private Transform interactionOrigin;

        [Tooltip("Prompt UI bridge. Auto-found in the scene if left empty.")]
        [SerializeField] private InteractionPromptView promptView;

        [Tooltip("Interact input. Defaults to the keyboard 'E' key.")]
        [SerializeField] private InputAction interactAction =
            new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");

        private const int OverlapBufferSize = 16;
        private readonly Collider[] _overlapBuffer = new Collider[OverlapBufferSize];

        private IInteractable _current;

        private Vector3 Origin => interactionOrigin != null ? interactionOrigin.position : transform.position;

        private void Awake()
        {
            if (promptView == null)
                promptView = FindFirstObjectByType<InteractionPromptView>();
        }

        private void OnEnable() => interactAction.Enable();

        private void OnDisable() => interactAction.Disable();

        private void Update()
        {
            _current = FindClosestInteractable();
            RefreshPrompt();

            if (_current != null && interactAction.WasPerformedThisFrame())
                _current.Interact(this);
        }

        /// <summary>Shows transient feedback to the player via the prompt view.</summary>
        public void ShowFeedback(string message)
        {
            if (promptView != null)
                promptView.ShowFeedback(message);
            else
                Debug.Log($"[Interaction] {message}");
        }

        /// <summary>Finds the closest interactable that is in range and currently usable.</summary>
        private IInteractable FindClosestInteractable()
        {
            int count = Physics.OverlapSphereNonAlloc(
                Origin, detectionRadius, _overlapBuffer, interactableMask, QueryTriggerInteraction.Collide);

            IInteractable closest = null;
            float closestSqr = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var candidate = _overlapBuffer[i].GetComponentInParent<IInteractable>();
                if (candidate == null || !candidate.CanInteract)
                    continue;

                float range = candidate.InteractionRange > 0f ? candidate.InteractionRange : detectionRadius;
                float sqrDistance = (candidate.InteractionPosition - Origin).sqrMagnitude;
                if (sqrDistance > range * range || sqrDistance >= closestSqr)
                    continue;

                closest = candidate;
                closestSqr = sqrDistance;
            }

            return closest;
        }

        /// <summary>Shows or hides the prompt based on the current interactable.</summary>
        private void RefreshPrompt()
        {
            if (promptView == null)
                return;

            if (_current != null)
                promptView.ShowPrompt($"Press {InteractKeyLabel()} to {_current.InteractionLabel}");
            else
                promptView.HidePrompt();
        }

        /// <summary>Human-readable name of the interact binding, e.g. "E".</summary>
        private string InteractKeyLabel()
        {
            string display = interactAction.GetBindingDisplayString();
            return string.IsNullOrEmpty(display) ? "E" : display;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(Origin, detectionRadius);
        }
    }
}
