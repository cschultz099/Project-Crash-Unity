public class EnemyType3 : AIBaseLogic
{
    protected override void Update()
    {
        base.Update();

        if (!isServer) return;

        ControlPlayer();
        FindClosestPlayerInSightWithProximity();
        LookAtClosestPlayer();
        CloseRangeAttack();
    }
}
