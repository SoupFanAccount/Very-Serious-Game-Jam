using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("UI Text Elements")]
    [SerializeField] private TextMeshProUGUI dirtyMoneyText;
    [SerializeField] private TextMeshProUGUI cleanMoneyText;
    [SerializeField] private TextMeshProUGUI suspicionText;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI debtText;
    [SerializeField] private TextMeshProUGUI clockText;

    void Update()
    {
        // Debug: check if GameManager is found
        if (GameManager.Instance == null)
        {
            return;
        }
        if (GameManager.Instance != null)
        {
            dirtyMoneyText.text = $"Dirty Money: ${GameManager.Instance.dirtyMoney}";
            cleanMoneyText.text = $"Clean Money: ${GameManager.Instance.cleanMoney}";
            suspicionText.text = $"Suspicion: {GameManager.Instance.suspicion} / {GameManager.Instance.maxSuspicion}";
            dayText.text = $"Day: {GameManager.Instance.currentDay} / {GameManager.Instance.totalDays}";
            debtText.text = $"Debt: ${GameManager.Instance.debt}";

            if (DayNightCycle.Instance != null && clockText != null)
            {
                clockText.text = DayNightCycle.Instance.ClockString();
            }
        }
    }
}