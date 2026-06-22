using UnityEngine;
using TMPro;
using Shop;

public class CanvasInteractionPromptView : InteractionPromptView
{
    [Header("UI References")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Settings")]
    [SerializeField] private float feedbackDuration = 2f;

    private float _feedbackTimer;

    private void Start()
    {
        if (promptPanel != null) promptPanel.SetActive(false);
        if (feedbackText != null) feedbackText.text = "";
    }

    private void Update()
    {
        // Automatically counts down and hides feedback messages
        if (_feedbackTimer > 0f)
        {
            _feedbackTimer -= Time.deltaTime;
            if (_feedbackTimer <= 0f && feedbackText != null)
            {
                feedbackText.text = "";
            }
        }
    }

    // Override methods from InteractionPromptView
    public override void ShowPrompt(string prompt)
    {
        if (promptPanel != null) promptPanel.SetActive(true);
        if (promptText != null) promptText.text = prompt;
    }

    public override void HidePrompt()
    {
        if (promptPanel != null) promptPanel.SetActive(false);
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