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

            //GD.Print("Map pos: ", (node as TileMap).WorldToMap(Position + offset), "WorldPos: ", this.Position, " offset ", offset);

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
