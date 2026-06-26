using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private CustomerQueue customerQueue;

    [SerializeField] private Customer[] customerPrefabs;
    [SerializeField] private SpawnSchedule[] spawnSchedule;

    private float _timer;

    private void Start()
    {
        _timer = Random.Range(1, 2f);
    }

    private void Update()
    {
        // Only let customers in while the shop is actually open.
        if (DayNightCycle.Instance != null && DayNightCycle.Instance.isOpen == false)
        {
            _timer = 0f;
            return;
        }

        if (customerQueue.CanAddCustomer() == false) return;

        _timer -= Time.deltaTime;

        // added/changed by donags
        if (_timer <= 0f)
        {
            SpawnCustomer();
            _timer = Random.Range(2, 5f);
        }
    }

    private SpawnSchedule GetCurrentSchedule()
    {
        if (DayNightCycle.Instance == null) Debug.LogError("There is No Day Night Cycle!");

        foreach (var schedule in spawnSchedule)
        {
            if (DayNightCycle.Instance.CurrentHour > schedule.startHour && DayNightCycle.Instance.CurrentHour < schedule.endHour) return schedule;
        }

        return null;
    }

    private void SpawnCustomer()
    {
        Customer customer = Instantiate(customerPrefabs[Random.Range(0, customerPrefabs.Length)], transform.position, Quaternion.identity);
        customer.Init(transform.position, customerQueue);

        customerQueue.AddCustomerToQueue(customer);
    }
}

[System.Serializable]
public class SpawnSchedule
{
    public string time;
    public float startHour;
    public float endHour;

    [Range(1, 10)] public float minSpawnTime, maxSpawnTime;
}