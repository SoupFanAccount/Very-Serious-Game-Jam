using UnityEngine;

namespace Shop
{
    /// <summary>
    /// Placeholder prompt view that draws the prompt and feedback with IMGUI. Requires no Canvas setup
    /// so the system is testable immediately. The UI programmer should replace this with a
    /// Canvas-driven <see cref="InteractionPromptView"/>.
    /// </summary>
    public class OnGuiInteractionPromptView : InteractionPromptView
    {
        [Tooltip("How long feedback text stays on screen, in seconds.")]
        [SerializeField] private float feedbackDuration = 2f;

        private string _prompt;
        private string _feedback;
        private float _feedbackTimer;
        private GUIStyle _style;

        /// <inheritdoc />
        public override void ShowPrompt(string prompt) => _prompt = prompt;

        /// <inheritdoc />
        public override void HidePrompt() => _prompt = null;

        /// <inheritdoc />
        public override void ShowFeedback(string message)
        {
            _feedback = message;
            _feedbackTimer = feedbackDuration;
        }

        private void Update()
        {
            if (_feedbackTimer <= 0f)
                return;

            _feedbackTimer -= Time.deltaTime;
            if (_feedbackTimer <= 0f)
                _feedback = null;
        }

        private void OnGUI()
        {
            // GUI.skin is only valid inside OnGUI, so the style is built lazily here.
            _style ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            const float width = 600f;
            float x = (Screen.width - width) * 0.5f;

            if (!string.IsNullOrEmpty(_prompt))
                GUI.Label(new Rect(x, Screen.height - 120f, width, 30f), _prompt, _style);

            if (!string.IsNullOrEmpty(_feedback))
                GUI.Label(new Rect(x, Screen.height - 160f, width, 30f), _feedback, _style);
        }
    }
}
