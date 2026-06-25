using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Lazy lookup so callers never get null due to script execution order
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<GameManager>();
            return _instance;
        }
        private set { _instance = value; }
    }

    public int dirtyMoney;
    public int cleanMoney;
    public int suspicion;

    public int currentDay = 1;
    public int totalDays = 14;
    public int debt = 1000;

    public int maxSuspicion = 100;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddDirtyMoney(int amount)
    {
        dirtyMoney += amount;
        AddSuspicion(amount / 10);
    }

    public void AddCleanMoney(int amount)
    {
        cleanMoney += amount;
        CheckWin();
    }

    public void AddSuspicion(int amount)
    {
        suspicion = Mathf.Clamp(suspicion + amount, 0, maxSuspicion);
        if (suspicion >= maxSuspicion)
            LoseGame();
    }

    public void LowerSuspicion(int amount)
    {
        suspicion = Mathf.Clamp(suspicion - amount, 0, maxSuspicion);
    }

    public bool SpendCleanMoney(int amount)
    {
        if (cleanMoney < amount)
            return false;
        cleanMoney -= amount;
        return true;
    }

    public void WashMoney(int amount)
    {
        int washed = Mathf.Min(amount, dirtyMoney);
        dirtyMoney -= washed;
        cleanMoney += washed;
        CheckWin();
    }

    public void NextDay()
    {
        currentDay++;
        if (currentDay > totalDays)
            LoseGame();
    }

    void WinGame()
    {
        Debug.Log("Debt paid. You win.");
        if (GameEndController.Instance != null)
            GameEndController.Instance.TriggerWin();
    }
    void LoseGame()
    {
        Debug.Log("Game over.");
        if (GameEndController.Instance != null)
            GameEndController.Instance.TriggerLose();
    }

    void CheckWin()
    {
        if (cleanMoney >= debt)
            WinGame();
    }
}