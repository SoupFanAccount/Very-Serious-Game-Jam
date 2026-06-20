using UnityEngine;

namespace Shop.Testing
{
    /// <summary>
    /// On-screen readout of the core resources so interaction effects can be verified in the test scene.
    /// Test helper only; the real HUD is owned by the UI programmer.
    /// </summary>
    public class TestHud : MonoBehaviour
    {
        private GUIStyle _style;

        private void OnGUI()
        {
            var game = GameManager.Instance;
            if (game == null)
                return;

            _style ??= new GUIStyle(GUI.skin.label) { fontSize = 16, normal = { textColor = Color.white } };

            GUILayout.BeginArea(new Rect(10, 10, 260, 160), GUI.skin.box);
            GUILayout.Label($"Day: {game.currentDay}/{game.totalDays}", _style);
            GUILayout.Label($"Dirty money: ${game.dirtyMoney}", _style);
            GUILayout.Label($"Clean money: ${game.cleanMoney}", _style);
            GUILayout.Label($"Suspicion: {game.suspicion}/{game.maxSuspicion}", _style);
            GUILayout.Label($"Debt: ${game.debt}", _style);
            GUILayout.EndArea();
        }
    }
}
