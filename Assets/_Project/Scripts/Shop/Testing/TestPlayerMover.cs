using UnityEngine;
using UnityEngine.InputSystem;

namespace Shop.Testing
{
    /// <summary>
    /// Minimal WASD mover for exercising the interaction system in the test scene. This is a throwaway
    /// test helper, not the project's real player movement (owned by the movement programmer).
    /// </summary>
    public class TestPlayerMover : MonoBehaviour
    {
        [Tooltip("Movement speed in units per second.")]
        [SerializeField] private float moveSpeed = 4f;

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            Vector3 direction = Vector3.zero;
            if (keyboard.wKey.isPressed) direction.z += 1f;
            if (keyboard.sKey.isPressed) direction.z -= 1f;
            if (keyboard.dKey.isPressed) direction.x += 1f;
            if (keyboard.aKey.isPressed) direction.x -= 1f;

            transform.position += direction.normalized * (moveSpeed * Time.deltaTime);
        }
    }
}
