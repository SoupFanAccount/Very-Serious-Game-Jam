using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public enum Phase { Day, Night }
    public Phase currentPhase = Phase.Day;

    public void StartDay()
    {
        currentPhase = Phase.Day;
        Debug.Log("Day " + GameManager.Instance.currentDay + " started.");
    }

    public void StartNight()
    {
        currentPhase = Phase.Night;
        Debug.Log("Night started. Dirty money to clean: " + GameManager.Instance.dirtyMoney);
    }

    public void EndNight()
    {
        GameManager.Instance.NextDay();
        StartDay();
    }
}