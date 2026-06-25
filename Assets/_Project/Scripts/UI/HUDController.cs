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

    private bool _gameHasEnded = false;

    void Update()
    {
        if (GameManager.Instance != null)
        {
            dirtyMoneyText.text = $"Dirty Money: ${GameManager.Instance.dirtyMoney}";
            cleanMoneyText.text = $"Clean Money: ${GameManager.Instance.cleanMoney}";
            suspicionText.text = $"Suspicion: {GameManager.Instance.suspicion} / {GameManager.Instance.maxSuspicion}";
            dayText.text = $"Day: {GameManager.Instance.currentDay} / {GameManager.Instance.totalDays}";
            debtText.text = $"Debt: ${GameManager.Instance.debt}";

            if (!_gameHasEnded)
            {
                CheckGameEndConditions();
            }
        }

        if (DayNightCycle.Instance != null && clockText != null)
        {
            clockText.text = $"Time: {DayNightCycle.Instance.ClockString()}";
        }
    }

    private void CheckGameEndConditions()
    {
        // Suspicion reaches max limit -> LOSE
        if (GameManager.Instance.suspicion >= GameManager.Instance.maxSuspicion)
        {
            _gameHasEnded = true;
            GameEndController.Instance.TriggerLose();
            return;
        }

        // The days ran out
        if (GameManager.Instance.currentDay > GameManager.Instance.totalDays)
        {
            _gameHasEnded = true;

            // Check if player has enough clean money to pay off the debt
            if (GameManager.Instance.cleanMoney >= GameManager.Instance.debt)
            {
                GameEndController.Instance.TriggerWin();
            }
            else
            {
                GameEndController.Instance.TriggerLose();
            }
        }
    }
}