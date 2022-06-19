using Godot;

public class ShooterMind : NpcMind
{
    protected float cooldownMax = 90;
    protected float cooldown = 0;

    AudioStreamPlayer? gunShotSound;
    protected PackedScene bulletScene = null!;
    protected BulletPool bulletPool = null!;

    int bulletBurstAmmount = 500;
    int bulletsShootInBurst = 1;

    public override void Update(Entity npc, Player player, float delta)
    {
        if (SeePlayer(npc, player))
        {
            cooldown--;
            if (cooldown <= 0)
            {
                cooldown = Utils.RandomFloat(cooldownMax * 0.8f, cooldownMax);
                TryToFire(npc, player, delta);
            }
        }
    }

    protected bool TryToFire(Entity npc, Player player, float delta)
    {
        if (bulletsShootInBurst >= bulletBurstAmmount) return false;

        var bullet = bulletPool.GetBulletFromPool();
        if (bullet is null) return false;

        bullet.Position = npc.GlobalPosition;
        bullet.Rotation = npc.GetAngleTo(player.GlobalPosition);
        var dir = (player.GlobalPosition - npc.GlobalPosition).Normalized();

        bullet.TryToFireBullet(dir, false);
        gunShotSound?.Play();
        bulletsShootInBurst++;

        // if (player is not null)
        // {
        //     player.ShakeCamera.AddShake(0.5f, 0.1f);
        // }

        bulletPool.ReturnBulletToPool(bullet);
        return true;
    }

    public ShooterMind(Map map, Vector2 playerStartPos, PackedScene bulletScene, BulletPool bulletPool) : base(map, playerStartPos)
    {
        this.bulletPool = bulletPool;
        this.bulletScene = bulletScene;
    }
}

public class ShooterMind_Shotgun : ShooterMind
{
    public ShooterMind_Shotgun(Map map, Vector2 playerStartPos, PackedScene bulletScene, BulletPool bulletPool) : base(map, playerStartPos, bulletScene, bulletPool)
    {
        this.bulletPool = bulletPool;
        this.bulletScene = bulletScene;
        cooldownMax = 150;
    }
    public override void Update(Entity npc, Player player, float delta)
    {
        if (SeePlayer(npc, player))
        {
            cooldown--;
            if (cooldown <= 0)
            {
                cooldown = Utils.RandomFloat(cooldownMax * 0.8f, cooldownMax);
                TryToFire(npc, player, delta);
                TryToFire(npc, player, delta);
                TryToFire(npc, player, delta);
                TryToFire(npc, player, delta);
            }
        }
    }
}