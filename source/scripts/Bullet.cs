using Godot;
using System;

public class Bullet : Area2D
{
    //public static AudioStreamPlayer2D? pistolBulletHit;

    [Export] float speed = 300f;
    [Export] float maxDistance = 500f;
    [Export] float spread = 0.2f;

    float traveledDistance = 0;
    Shape2D shape;
    Vector2 fireDirection = Vector2.Right;
    float currentSpread = 0;

    public override void _EnterTree()
    {
        //pistolBulletHit ??= Bullet.BulletPoolNode?.GetNode<AudioStreamPlayer2D>("PistolBulletHitSound");
    }

    public override void _PhysicsProcess(float delta)
    {
        var distance = speed * delta;

        var motion = (fireDirection * distance).Rotated(currentSpread);
        Position += motion;
        traveledDistance += distance;

        if (traveledDistance > maxDistance)
        {
            RemoveBullet();
        }

    }

    void _OnCollided(Node node)
    {
        GD.Print("hit");
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

    public void Fire(Vector2 dir)
    {
        fireDirection = dir;
        traveledDistance = 0;
        currentSpread = Utils.RandomFloat(-spread, spread);
        Visible = true;
        SetProcess(true);
    }

}
