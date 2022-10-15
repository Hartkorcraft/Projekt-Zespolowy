
using System;
using Godot;

public class HealtSystem_Entity : HealthSystem
{
    Entity parent;
    public static event Action? OnDeathEvent;
    public HealtSystem_Entity(int health, int maxHealth, Func<Vector2> getOriginPos, Entity parent, PackedScene? hitParticleScene = null, PackedScene? deathParticleScene = null) : base(health, maxHealth, getOriginPos, hitParticleScene, deathParticleScene)
    {
        Health = health;
        MaxHealth = maxHealth;
        this.parent = parent;
    }

    public override void OnDeath(IAttack attack)
    {
        base.OnDeath(attack);
        OnDeathEvent?.Invoke();
        parent.QueueFree();
    }
}
