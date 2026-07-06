using UnityEngine;

public class TurretController : MonoBehaviour
{
    private TurretStateMachine stateMachine;
    
    public TurretPatrolState PatrolState { get; private set; }

    [Header("Pivots for Code Animation")]
    [SerializeField] private Transform slidingBasePivot;
    [SerializeField] private Transform barrelRotationPivot;

    private void Awake()
    {
        stateMachine = new TurretStateMachine();

        PatrolState = new TurretPatrolState(this);
    }

    private void Start()
    {
        stateMachine.Initialize(PatrolState);
    }

    private void Update()
    {
        stateMachine.ExecuteActiveState();
    }

    public void SwitchState(IState newState)
    {
        stateMachine.ChangeState(newState);
    }
}