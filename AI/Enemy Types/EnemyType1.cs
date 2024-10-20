public class EnemyType1 : AIBaseLogic
{
    protected override void Update()
    {
        base.Update();

        if (!isServer) return;

        ControlPlayer();
        FindClosestPlayer();
        LookAtClosestPlayer();
        CloseRangeAttack();
    }
}