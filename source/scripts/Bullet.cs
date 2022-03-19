using Godot;
using System;

public class Bullet : Area2D, IAttack
{
    [Export] float speed = 300f;
    [Export] float maxDistance = 500f;
    [Export] float spread = 0.2f;


    public int Damage { get; private set; } = 1;
    public float AttackRotation => GlobalRotation;

    float traveledDistance = 0;
    Vector2 fireDirection = Vector2.Right;
    float currentSpread = 0; // rozrzut broni aka kąt  

    Vector2 motion = Vector2.Zero;
    Vector2 newPos = Vector2.Zero;

    bool moved = false;
    bool active = false;

    public override void _PhysicsProcess(float delta)
    {
        if (active is false) return;

        var distance = speed * delta;
        motion = (fireDirection * distance).Rotated(currentSpread);
        traveledDistance += distance;
        newPos = Position + motion;

        // Zabezpieczenie żeby kolizja się wykryła podczas 1 klatki
        if (moved is false)
        {
            Show();
            moved = true;
            return;
        }

        Position = newPos;

        if (traveledDistance > maxDistance)
            RemoveBullet();
    }

    public void FireBullet(Vector2 dir)
    {
        fireDirection = dir;
        traveledDistance = 0;
        currentSpread = Utils.RandomFloat(-spread, spread);
        moved = false;
        Hide();
        active = true;
    }

    // Sygnał któy odpala się kiedy pocisk coś uderzy
    void _OnCollided(int body_id, Node node, int body_shape, int local_shape)
    {
        if (this.active is false) return;

        //IHealth? hit = null;

        var asTiles = node as Tiles;
        if (asTiles is not null)
        {
            var offset = new Vector2(1f, 0).Rotated(currentSpread + this.Rotation);
            var worldPos = asTiles.WorldToMap(Position + offset);
            var mapPos = ((int)worldPos.x, (int)worldPos.y);
            var tileAsIHealth = Map.GetTile(mapPos) as IHealthSystem;
            tileAsIHealth?.HealthSystem.Damage(this);
        }
        else
        {
            (node as IHealthSystem)?.HealthSystem.Damage(this);
        }

        BulletImpact();
    }

    void BulletImpact()
    {
        RemoveBullet();
    }

    void RemoveBullet()
    {
        Hide();
        active = false;
    }

}
