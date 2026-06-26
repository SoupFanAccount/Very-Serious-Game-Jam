using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Customer : MonoBehaviour
{
    public enum CustomerState {GoingToQueue, WaitingInQueue, OrderPlaced, WaitingForOrder, GoingToTakeOrder , Done , Leave }
    
    private NavMeshAgent _agent;
    private Animator _animator;
    private CustomerQueue _customerQueue;

    [SerializeField] private CustomerState customerState;
    [SerializeField] private CustomerUI customerUI;
    
    private Vector3 _spawnPoint;
    private Vector3 _targetPoint;

    [Space(10f), Header("DressInfo"), Space(5f)]

    [SerializeField] private GameObject[] hats;
    [SerializeField] private GameObject[] shirts;
    
    [Space(10f) , Header("Dialogue Data") , Space(5f)]
    
    [SerializeField] private CustomerDialogueData[] CustomerDialogueDataArray;

    [Space(10), Header("Reaction Info"), Space(5f)]
    
    [SerializeField] private float patienceTimerMin;
    [SerializeField] private float patienceTimerMax;
    
    private float _patienceTimer;
    private float _patienceWarningShownTime;
    private float _waitDialogueShownTime;
    
    private bool _patienceWarningShown;
    private bool _waitingDialogueShown;
    private bool _doneDialogueShown;
    
    private bool _canLeave;
    private bool _isDone;
    
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
    }

    public void Init(Vector3 spawnPoint, CustomerQueue customerQueue)
    {
        customerState = CustomerState.GoingToQueue;
        
        _spawnPoint = spawnPoint;
        _customerQueue = customerQueue;

        _patienceTimer = Random.Range(patienceTimerMin, patienceTimerMax);
        _patienceWarningShownTime = _patienceTimer / 2;
        _waitDialogueShownTime = Random.Range(_patienceWarningShownTime + 2, _patienceTimer);

        ChooseRandomHat();
        ChooseRandomShirt();
    }

    private void ChooseRandomHat()
    {
        if (hats.Length <= 0) return;
        
        for (int i = 0; i < hats.Length; i++)
            hats[i].SetActive(false);
        
        hats[Random.Range(0, hats.Length)].SetActive(true);
    }
    private void ChooseRandomShirt()
    {
        if (shirts.Length <= 0) return;
        
        for (int i = 0; i < shirts.Length; i++)
            shirts[i].SetActive(false);
        
        shirts[Random.Range(0, shirts.Length)].SetActive(true);
    }
    
    private void Update()
    {
        if (_agent.velocity.sqrMagnitude > .1f)
        {
            _animator.SetBool("Move", true);
            _animator.SetBool("Idle", false);
        }
        else
        {
            _animator.SetBool("Move", false);
            _animator.SetBool("Idle", true);
        }   
        
        switch (customerState)
        {
            case CustomerState.GoingToQueue:
                if (_agent.hasPath && _agent.remainingDistance <= .1f) customerState = CustomerState.WaitingInQueue;
                break;
            
            case CustomerState.WaitingInQueue:

                _patienceTimer -= Time.deltaTime;

                
                // WAITING DIALOGUE -----
                if (_patienceTimer <= _waitDialogueShownTime && _waitingDialogueShown == false)
                {
                    customerUI.ShowDialogue(GetDialogue(CustomerState.WaitingInQueue), .3f, .5f, .3f);
                    _waitingDialogueShown = true;
                }
                
                // SHOW CLOCK -----
                if (_patienceTimer <= _patienceWarningShownTime && _patienceWarningShown == false)
                {
                    customerUI.ShowPatienceClock(.3f);
                    _patienceWarningShown = true;
                }

                // UPDATE CLOCK -- ONLY HAPPEN WHEN CLOCK ENABLE ---
                customerUI.UpdateClock(_patienceTimer/_patienceWarningShownTime);

                // CUSTOMER PATIENCE TIME OVER 
                if (_patienceTimer <= 0)
                {
                    // customerUI.PlayAngrySequence("Go To Hell!" , 0.3f ,() => _canLeave = true);
                    customerUI.ShowDialogue(GetDialogue(CustomerState.Leave), .3f, .5f, .3f, () => _canLeave = true);
                    customerState = CustomerState.Leave;
                }
                break;
            
            /*case State.OrderPlaced:
                _customerQueue.RemoveCustomerFromQueue(this);
                _agent.SetDestination(_spawnPoint);
                break;
            
            case State.WaitingForOrder: break;
            
            case State.GoingToTakeOrder:
                _customerQueue.AddCustomerToQueue(this);
                break;*/
            
            case CustomerState.Done:

                if (_doneDialogueShown == false)
                {
                    customerUI.ShowDialogue(GetDialogue(CustomerState.Done), .3f, .3f, .5f, () => _isDone = true);
                    _doneDialogueShown = true;
                }
                
                if (_isDone == false) return;
                
                _customerQueue.RemoveCustomerFromQueue(this);
                _agent.SetDestination(_spawnPoint);
                if (_agent.hasPath && _agent.remainingDistance <= .5f) Destroy(gameObject);
                break;
            
            case CustomerState.Leave:
                if (_canLeave == false) return;
                _customerQueue.RemoveCustomerFromQueue(this);
                _agent.SetDestination(_spawnPoint);
                if (_agent.hasPath && _agent.remainingDistance <= .5f)
                {
                    if(_agent.pathPending == false)
                        Destroy(gameObject);
                }
                break;
        }
    }

    public void MoveTo(Vector3 targetPoint)
    {
        if (_canLeave) return;
        
        _targetPoint = targetPoint;
        if(_agent.isOnNavMesh == false) print("Customer is Not On NavMesh!");
        
        _agent.SetDestination(_targetPoint);
    }

    private string GetDialogue(CustomerState state)
    {
        foreach (var customerDialogueData in CustomerDialogueDataArray)
        {
            if(customerDialogueData.customerState == state) 
                return customerDialogueData.dialogue[Random.Range(0, customerDialogueData.dialogue.Length)];
        }
        
        return "";
    }
    
    // Added by Donags. It lets the shop interaction tell this customer they've been served so they leave.
    public void Serve()
    {
        customerState = CustomerState.Done;
    }
}

[System.Serializable]
public class CustomerDialogueData
{
    public Customer.CustomerState customerState;
    public string[] dialogue;
}
