
using System;
using Godot;

public class HealtSystem_Entity : HealthSystem
{
    Entity parent;

    public HealtSystem_Entity(int health, int maxHealth, Func<Vector2> getOriginPos, Entity parent, PackedScene? hitParticleScene = null) : base(health, maxHealth, getOriginPos, hitParticleScene)
    {
        this.parent = parent;
    }

    public override void OnDeath(Bullet bullet)
    {
        base.OnDeath(bullet);
        
        var hitParticle = hitParticleScene?.Instance() as Node;
        if (hitParticle is not null)
        {
            var asNode = (Node2D)hitParticle;
            Main.ParticlePool.AddChild(hitParticle);
            asNode.GlobalPosition = getOriginPos();
            asNode.GlobalRotation = bullet.GlobalRotation;
        }

        parent.QueueFree();
    }
}
