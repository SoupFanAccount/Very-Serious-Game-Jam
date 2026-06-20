using UnityEngine;

namespace Shop
{
    /// <summary>
    /// Bridge between the interaction system and the UI layer. The shop system only depends on this
    /// base type; the UI programmer subclasses it to drive real UI (Canvas/TMP) and swaps it in.
    /// </summary>
    public abstract class InteractionPromptView : MonoBehaviour
    {
        /// <summary>Shows the interaction prompt with the given text.</summary>
        public abstract void ShowPrompt(string prompt);

        /// <summary>Hides the interaction prompt.</summary>
        public abstract void HidePrompt();

        /// <summary>Shows transient feedback after an interaction (success or failure).</summary>
        public abstract void ShowFeedback(string message);
    }
}
