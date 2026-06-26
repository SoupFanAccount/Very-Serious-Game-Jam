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

    [Tooltip("Seconds a served customer is given to walk out before they are force-despawned. " +
             "Stops the queue soft-locking behind someone who can't path to the exit.")]
    [SerializeField] private float leaveTimeout = 8f;

    private bool _leaving;
    private float _leaveTimer;

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
                // Run the leave setup once, not every frame. Re-issuing SetDestination each
                // frame keeps the path permanently pending so the agent never settles.
                if (_leaving == false)
                {
                    _leaving = true;
                    _leaveTimer = 0f;
                    _customerQueue.RemoveCustomerFromQueue(this);
                    if (_agent.isOnNavMesh) _agent.SetDestination(_spawnPoint);
                }

                _leaveTimer += Time.deltaTime;

                bool arrived = _agent.isOnNavMesh && _agent.pathPending == false &&
                               _agent.remainingDistance <= _agent.stoppingDistance + 0.5f;

                // Despawn on arrival, or bail out after a timeout so a customer who can't
                // reach the exit (small/blocked shop) can never soft-lock the queue.
                if (arrived || _leaveTimer >= leaveTimeout) Destroy(gameObject);
                break;
        }
    }

    public void MoveTo(Vector3 targetPoint)
    {
        _targetPoint = targetPoint;
        if(_agent.isOnNavMesh == false) print("Customer is Not On NavMesh!");
        
        _agent.SetDestination(_targetPoint);
    }

    // Added by Donags. It lets the shop interaction tell this customer they've been served so they leave.
    public void Serve()
    {
        customerState = State.Done;
    }
}
