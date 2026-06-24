using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Customer : MonoBehaviour
{
    private enum State { WaitingInQueue, OrderPlaced, GoingToTakeOrder , Done }
    
    private NavMeshAgent _agent;
    private CustomerQueue _customerQueue;

    [SerializeField] private State customerState;
    
    private Vector3 _spawnPoint;
    private Vector3 _targetPoint;

    [SerializeField] private bool orderPlaced;
    [SerializeField] private bool isDone;

    [SerializeField] private bool timeToGoTakeOrder;
    
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public void Init(Vector3 spawnPoint, CustomerQueue customerQueue)
    {
        customerState = State.WaitingInQueue;
        
        _spawnPoint = spawnPoint;
        _customerQueue = customerQueue;
    }

    private void Update()
    {
        switch (customerState)
        {
            case State.OrderPlaced:
                orderPlaced = true;
                _customerQueue.RemoveCustomerFromQueue(this);
                _agent.SetDestination(_spawnPoint);
                break;
            
            case State.GoingToTakeOrder:
                if (orderPlaced == false) return;
                _customerQueue.AddCustomerToQueue(this);
                break;
            
            case State.Done:
                _customerQueue.RemoveCustomerFromQueue(this);
                _agent.SetDestination(_spawnPoint);
                if (_agent.hasPath && _agent.remainingDistance <= .1f) gameObject.SetActive(false);
                break;
        }
    }

    public void MoveTo(Vector3 targetPoint)
    {
        _targetPoint = targetPoint;
        if(_agent.isOnNavMesh == false) print("Customer is Not On NavMesh!");
        
        _agent.SetDestination(_targetPoint);
    }
}
