public class ShopKeeperController : AIBaseLogic
{
    protected override void Start()
    {
        return;
    }

    protected override void Update()
    {
        base.Update();

        if (!isServer) return;

        FindClosestPlayer();
        LookAtClosestPlayer();
    }

    public override void TakeDamage(int damage, PlayerManager p)
    {
        return;
    }
}