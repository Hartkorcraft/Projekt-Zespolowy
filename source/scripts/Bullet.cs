using Godot;
using System;

public class Bullet : Area2D, IAttack
{
    public static Map Map = null!;
    
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

    public void TryToFireBullet(Vector2 dir, Arm arm)
    {
        fireDirection = dir;
        traveledDistance = 0;
        currentSpread = Utils.RandomFloat(-spread, spread);
        moved = false;
        active = true;

        CheckBarrelBlock(arm);
    }

    // Aby sprawdzić czy lufa jest zablokowana żeby nie było problemów z kolizjami 
    void CheckBarrelBlock(Arm arm)
    {
        //raycast aby sprawdzić czy widzi
        var spaceState = arm.GetWorld2d().DirectSpaceState;
        var result = spaceState.IntersectRay(
            from: arm.GlobalPosition,
            to: this.GlobalPosition,
            exclude: new Godot.Collections.Array { this },
            collisionLayer: 0b01111); //TODO do zmiennej 

        if (result.Count > 0)
        {
            var hit = result["collider"] as Node;
            BulletImpact(hit);
        }
    }

    // Sygnał któy odpala się kiedy pocisk coś uderzy
    void _OnCollided(int body_id, Node node, int body_shape, int local_shape)
    {
        if (this.active is false) return;

        //IHealth? hit = null;
        BulletImpact(node);
    }

    void BulletImpact(Node? hit)
    {
        if (hit is null) GD.PrintErr("null hit");

        var asTiles = hit as Tiles;
        if (asTiles is not null)
        {
            var offset = new Vector2(1f, 0).Rotated(currentSpread + this.Rotation);
            var mapPos = asTiles.WorldToMap(Position - offset).ToTuple();
            var tileAsIHealth = Map.GetTile(mapPos) as IHealthSystem;
            tileAsIHealth?.HealthSystem.Damage(this);
        }
        else
        {
            (hit as IHealthSystem)?.HealthSystem.Damage(this);
        }

        RemoveBullet();
    }

    void RemoveBullet()
    {
        Hide();
        active = false;
    }

}
