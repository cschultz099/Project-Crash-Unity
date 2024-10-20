public class EnemyType2 : AIBaseLogic
{
    protected override void Update()
    {
        base.Update();

        if (!isServer) return;

        ControlPlayer();
        FindClosestPlayerByProximity();
        LookAtClosestPlayer();
        CloseRangeAttack();
    }
}
