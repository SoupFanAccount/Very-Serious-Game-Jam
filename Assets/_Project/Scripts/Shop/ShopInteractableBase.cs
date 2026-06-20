using UnityEngine;

namespace Shop
{
    /// <summary>
    /// Base class for shop interactables. Handles the shared concerns — labelling, one-shot completion,
    /// day/night availability and the distance reference point — so subclasses only implement the effect.
    /// </summary>
    public abstract class ShopInteractableBase : MonoBehaviour, IInteractable
    {
        [Tooltip("Verb phrase shown in the prompt, e.g. \"Serve Customer\".")]
        [SerializeField] private string interactionLabel = "Interact";

        [Tooltip("When this interactable is usable relative to the day/night cycle.")]
        [SerializeField] private InteractionAvailability availability = InteractionAvailability.Always;

        [Tooltip("If true, the interactable can only be used once.")]
        [SerializeField] private bool oneShot;

        [Tooltip("Max use distance. Leave at 0 to use the player's default interaction range.")]
        [SerializeField] private float interactionRange;

        private bool _completed;

        /// <summary>Shortcut to the shared <see cref="GameManager"/> singleton.</summary>
        protected static GameManager Game => GameManager.Instance;

        /// <inheritdoc />
        public string InteractionLabel => interactionLabel;

        /// <inheritdoc />
        public Vector3 InteractionPosition => transform.position;

        /// <inheritdoc />
        public float InteractionRange => interactionRange;

        /// <inheritdoc />
        public bool CanInteract => !_completed && IsPhaseAllowed() && CanPerform();

        /// <inheritdoc />
        public void Interact(PlayerInteractionController interactor)
        {
            if (!CanInteract)
                return;

            Perform(interactor);

            if (oneShot)
                _completed = true;
        }

        /// <summary>
        /// Subclass-specific availability beyond completion and phase (e.g. "player has dirty money").
        /// Defaults to always allowed.
        /// </summary>
        protected virtual bool CanPerform() => true;

        /// <summary>Applies the interaction's effect. Called only when <see cref="CanInteract"/> is true.</summary>
        protected abstract void Perform(PlayerInteractionController interactor);

        /// <summary>True if the configured availability matches the current day/night phase.</summary>
        private bool IsPhaseAllowed()
        {
            if (availability == InteractionAvailability.Always)
                return true;

            // If the day/night system isn't in the scene yet, keep interactables usable so the shop
            // system stays testable in isolation.
            if (!DayNightPhaseLocator.TryGetIsDay(out bool isDay))
                return true;

            return availability == InteractionAvailability.DayOnly ? isDay : !isDay;
        }
    }
}
