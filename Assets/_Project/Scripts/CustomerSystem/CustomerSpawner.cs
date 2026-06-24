using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private Customer customerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private CustomerQueue customerQueue;

    [SerializeField] private SpawnSchedule[] spawnSchedule;
    
    private float _timer;

    private void Update()
    {
        if (customerQueue.CanAddCustomer() == false) return;

        _timer += Time.deltaTime;

        if (_timer > 3f)
        {
            SpawnCustomer();
            _timer = 0f;
        }
        
        // THIS COMMENTED CODE IS WORLD TIME BASED AND UPPPER CODE IS FOR TESTING PURPOSE
        
        /*if (customerQueue.CanAddCustomer() == false) return;

        SpawnSchedule s = GetCurrentSchedule();

        if (s == null) return;

        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
            SpawnCustomer();
            _timer = Random.Range(s.minSpawnTime, s.maxSpawnTime);
        }*/
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