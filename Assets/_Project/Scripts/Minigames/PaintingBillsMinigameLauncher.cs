using System.Collections.Generic;
using Shop;
using UnityEngine;

namespace Minigames
{
    /// <summary>
    /// Concrete <see cref="LaunderingMinigameLauncher"/> that opens the painting-bills minigame. Assign it
    /// to a <see cref="WashingStationInteractable"/>. It shows the minigame panel, freezes the triggering
    /// player, starts a session and, when the session ends, hides the panel, restores the player and relays
    /// the result back to them.
    /// </summary>
    public class PaintingBillsMinigameLauncher : LaunderingMinigameLauncher
    {
        [Tooltip("Root panel of the minigame UI. Hidden by default, shown while the minigame runs.")]
        [SerializeField] private GameObject minigameRoot;

        [Tooltip("Controller that runs the session.")]
        [SerializeField] private PaintingBillsMinigameController controller;

        [Tooltip("If true, the player's movement and interaction are disabled while the minigame is open.")]
        [SerializeField] private bool freezePlayerWhileOpen = true;

        private PlayerInteractionController _interactor;
        private readonly List<Behaviour> _frozenBehaviours = new List<Behaviour>();
        private bool _playerFrozen;

        /// <inheritdoc />
        public override void StartLaundering(int dirtyMoneyAvailable, PlayerInteractionController interactor)
        {
            if (controller == null)
            {
                Debug.LogError($"{nameof(PaintingBillsMinigameLauncher)}: no controller assigned.", this);
                return;
            }

            _interactor = interactor;
            SetPlayerFrozen(true);

            if (minigameRoot != null)
                minigameRoot.SetActive(true);

            // Re-subscribe defensively so a re-entry never stacks handlers.
            controller.SessionFinished -= HandleSessionFinished;
            controller.SessionFinished += HandleSessionFinished;

            controller.BeginSession(dirtyMoneyAvailable);
        }

        /// <summary>Hides the minigame, restores the player and reports the outcome to the shop.</summary>
        private void HandleSessionFinished(PaintingBillsResult result)
        {
            controller.SessionFinished -= HandleSessionFinished;

            if (minigameRoot != null)
                minigameRoot.SetActive(false);

            if (_interactor != null)
                _interactor.ShowFeedback(result.ToShopFeedbackString());

            SetPlayerFrozen(false);
            _interactor = null;
        }

        /// <summary>
        /// Disables or restores the triggering player's movement and interaction. Every
        /// <see cref="PlayerController"/> and <see cref="PlayerInteractionController"/> on the player is
        /// toggled (the test scene has duplicates), and the body is brought to rest when freezing.
        /// </summary>
        private void SetPlayerFrozen(bool frozen)
        {
            if (!freezePlayerWhileOpen || frozen == _playerFrozen)
                return;

            _playerFrozen = frozen;

            if (!frozen)
            {
                foreach (Behaviour behaviour in _frozenBehaviours)
                {
                    if (behaviour != null)
                        behaviour.enabled = true;
                }

                _frozenBehaviours.Clear();
                return;
            }

            if (_interactor == null)
                return;

            GameObject player = _interactor.gameObject;
            _frozenBehaviours.Clear();

            foreach (PlayerController movement in player.GetComponents<PlayerController>())
                Freeze(movement);
            foreach (PlayerInteractionController interaction in player.GetComponents<PlayerInteractionController>())
                Freeze(interaction);

            Rigidbody body = player.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }

        /// <summary>Disables a behaviour and records it so it can be restored later.</summary>
        private void Freeze(Behaviour behaviour)
        {
            if (behaviour == null || !behaviour.enabled)
                return;

            behaviour.enabled = false;
            _frozenBehaviours.Add(behaviour);
        }
    }
}
