using System;
using Godot;

public abstract class HealthSystem
{
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
    bool dead = false; // żeby nie móc umrzeć gdy się nie żyje 
    protected PackedScene? hitParticleScene; // cząstki po oberaniu np krew
    protected Func<Vector2> getOriginPos; // żeby zdobyć środek dla cząstek
    //Action onDeath;

    public bool Damage(Bullet bullet) //TODO atak zamiast pocisku 
    {
        Health -= bullet.Damage;
        OnHit(bullet);
        if (Health <= 0) return Die(bullet);
        return false;
    }

    public virtual void OnHit(Bullet bullet)
    {

    }

    bool Die(Bullet bullet)
    {
        if (dead) return true;
        OnDeath(bullet);
        return true;
    }

    public virtual void OnDeath(Bullet bullet)
    {
        GD.Print("Destroyed " + this.GetType());
    }

    public HealthSystem(int health, int maxHealth, Func<Vector2> getOriginPos, PackedScene? hitParticleScene = null)
    {
        this.Health = health;
        this.MaxHealth = maxHealth;
        this.hitParticleScene = hitParticleScene;
        this.getOriginPos = getOriginPos;
    }
}
