using UnityEngine;
using TMPro;
using Shop;

public class CanvasInteractionPromptView : InteractionPromptView
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Settings")]
    [SerializeField] private float feedbackDuration = 2f;

    private float _feedbackTimer;

    private void Start()
    {
        if (promptText != null) promptText.text = "";
        if (feedbackText != null) feedbackText.text = "";
    }

    private void Update()
    {
        if (_feedbackTimer > 0f)
        {
            _feedbackTimer -= Time.deltaTime;
            if (_feedbackTimer <= 0f && feedbackText != null)
            {
                feedbackText.text = "";
            }
        }
    }

    public override void ShowPrompt(string prompt)
    {
        if (promptText != null)
        {
            promptText.text = prompt;
        }
    }

    public override void HidePrompt()
    {
        if (promptText != null)
        {
            promptText.text = "";
        }
    }

    public override void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            _feedbackTimer = feedbackDuration;
        }
    }
}