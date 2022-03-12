using Godot;
using System;

public class Bullet : Area2D
{
    [Export] float speed = 300f;
    [Export] float maxDistance = 500f;
    [Export] float spread = 0.2f;

    float traveledDistance = 0;
    Vector2 fireDirection = Vector2.Right;
    float currentSpread = 0;
    Vector2 motion = Vector2.Zero;

    int damage = 1;


    public override void _PhysicsProcess(float delta)
    {
        var distance = speed * delta;

        motion = (fireDirection * distance).Rotated(currentSpread);
        Position += motion;
        traveledDistance += distance;

        if (traveledDistance > maxDistance)
        {
            RemoveBullet();
        }
    }

    public void FireBullet(Vector2 dir)
    {
        fireDirection = dir;
        traveledDistance = 0;
        currentSpread = Utils.RandomFloat(-spread, spread);
        Visible = true;
        SetProcess(true);
    }

    // Sygnał któy odpala się kiedy pocisk coś uderzy
    void _OnCollided(int body_id, Node node, int body_shape, int local_shape)
    {
        if (this.IsProcessing() is false) return;

        //IHealth? hit = null;

        var asMap = node as Map;
        if (asMap is not null)
        {
            var offset = new Vector2(1f, 0).Rotated(currentSpread + this.Rotation);
            var worldPos = asMap.WorldToMap(Position + offset);
            var mapPos = ((int)worldPos.x, (int)worldPos.y);
            var tileAsIHealth = Map.GetTile(mapPos) as IHealthSystem;
            tileAsIHealth?.HealthSystem.Damage(damage);
        }
        //GD.Print("hit");
        RemoveBullet();
    }

    void BulletImpact()
    {
        RemoveBullet();
    }

    void RemoveBullet()
    {
        Hide();
        SetProcess(false);
    }

}
