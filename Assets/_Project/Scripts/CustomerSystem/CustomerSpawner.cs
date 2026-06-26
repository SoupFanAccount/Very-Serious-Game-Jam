using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private Customer customerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private CustomerQueue customerQueue;

    [SerializeField] private SpawnSchedule[] spawnSchedule;

    [Header("Spawn pacing")]
    [Tooltip("Seconds between customers on day 1.")]
    [SerializeField] private float baseSpawnInterval = 5f;
    [Tooltip("Fastest the shop ever gets (seconds between customers).")]
    [SerializeField] private float minSpawnInterval = 2f;
    [Tooltip("How much the gap between customers shrinks each day. Higher = busier sooner.")]
    [SerializeField] private float intervalReductionPerDay = 0.5f;

    private float _timer;

    private void Update()
    {
        // Only let customers in while the shop is actually open.
        if (DayNightCycle.Instance != null && DayNightCycle.Instance.isOpen == false)
        {
            _timer = 0f;
            return;
        }

        if (customerQueue.CanAddCustomer() == false) return;

        _timer += Time.deltaTime;

        if (_timer >= CurrentSpawnInterval())
        {
            SpawnCustomer();
            _timer = 0f;
        }
    }

    // Quieter early days, ramping up to minSpawnInterval as the days go on.
    private float CurrentSpawnInterval()
    {
        int day = GameManager.Instance != null ? GameManager.Instance.currentDay : 1;
        float interval = baseSpawnInterval - intervalReductionPerDay * (day - 1);
        return Mathf.Max(minSpawnInterval, interval);
    }

    private SpawnSchedule GetCurrentSchedule()
    {
        if(DayNightCycle.Instance == null) Debug.LogError("There is No Day Night Cycle!");
        
        foreach (var schedule in spawnSchedule)
        {
            if (DayNightCycle.Instance.CurrentHour > schedule.startHour && DayNightCycle.Instance.CurrentHour < schedule.endHour) return schedule;
        }

        return null;
    }
    
    private void SpawnCustomer()
    {
        Customer customer = Instantiate(customerPrefab, transform.position, Quaternion.identity);
        customer.Init(transform.position,customerQueue);
        
        customerQueue.AddCustomerToQueue(customer);
    }
}

[System.Serializable]
public class SpawnSchedule
{
    public string time;
    public float startHour;
    public float endHour;

    [Range(1,10)]public float minSpawnTime , maxSpawnTime;
}