using UnityEngine;

public class TurretPatrolState : IState
{
    private TurretController turret;
    
    public TurretPatrolState(TurretController controller)
    {
        this.turret = controller;
    }

    public void Enter()
    {
        Debug.Log("Turret entered PATROL state.");
    }

    public void Execute()
    {
        // This runs every single frame while the turret is patrolling
        //custom code-animation logic
    }

    public void Exit()
    {
        Debug.Log("Turret exited PATROL state.");
    }
}