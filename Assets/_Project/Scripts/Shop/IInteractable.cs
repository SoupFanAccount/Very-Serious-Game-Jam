using UnityEngine;

namespace Shop
{
    /// <summary>
    /// Defines during which day/night phase an interactable can be used.
    /// </summary>
    public enum InteractionAvailability
    {
        /// <summary>Usable regardless of the current day/night phase.</summary>
        Always,

        /// <summary>Usable only during the day.</summary>
        DayOnly,

        /// <summary>Usable only during the night.</summary>
        NightOnly
    }

    /// <summary>
    /// Contract for any object the player can walk up to and interact with.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Whether the interactable can be triggered right now.</summary>
        bool CanInteract { get; }

        /// <summary>
        /// Short verb phrase describing the action, e.g. "Serve Customer".
        /// The controller wraps this into a full prompt such as "Press E to Serve Customer".
        /// </summary>
        string InteractionLabel { get; }

        /// <summary>World-space position used to measure distance to the player.</summary>
        Vector3 InteractionPosition { get; }

        /// <summary>
        /// Maximum distance at which this interactable can be used.
        /// Return a value &lt;= 0 to fall back to the controller's default range.
        /// </summary>
        float InteractionRange { get; }

        /// <summary>Performs the interaction.</summary>
        /// <param name="interactor">The controller that triggered the interaction; used for feedback.</param>
        void Interact(PlayerInteractionController interactor);
    }
}
