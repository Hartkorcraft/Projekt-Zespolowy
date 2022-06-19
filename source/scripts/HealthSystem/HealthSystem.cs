using System;
using Godot;

public abstract class HealthSystem
{
    public int Health { get; protected set; }
    public int MaxHealth { get; protected set; }
    bool dead = false; // żeby nie móc umrzeć gdy się nie żyje 
    protected PackedScene? hitParticleScene; // cząstki po oberaniu np krew
    protected PackedScene? deathParticleScene; // cząstki po śmierci

    protected Func<Vector2> getOriginPos; // żeby zdobyć środek dla cząstek
    //Action onDeath;

    public bool Damage(IAttack attack) //TODO atak zamiast pocisku 
    {
        Health -= attack.Damage;
        OnHit(attack);
        if (Health <= 0) return Die(attack);
        return false;
    }

    public virtual void OnHit(IAttack attack)
    {
        SpawnParticlesOnHit(attack);
    }

    bool Die(IAttack attack)
    {
        if (dead) return true;
        OnDeath(attack);
        return true;
    }

    public virtual void OnDeath(IAttack attack)
    {
        //GD.Print("Destroyed " + this.GetType());
        SpawnParticlesOnDeath(attack);
    }

    public virtual void SpawnParticlesOnDeath(IAttack attack)
    {
        var deathParticle = deathParticleScene?.Instance() as HitParticle;
        if (deathParticle is not null)
        {
            Main.ParticlePool.AddChild(deathParticle);
            deathParticle.GlobalPosition = getOriginPos();
            deathParticle.GlobalRotation = attack.AttackRotation;
        }
    }

    public virtual void SpawnParticlesOnHit(IAttack attack)
    {
        var hitParticle = hitParticleScene?.Instance() as HitParticle;
        if (hitParticle is not null)
        {
            hitParticle.DestroyAfter = true;
            hitParticle.ZIndex = 1; //TODO ?

            Main.ParticlePool.AddChild(hitParticle);
            hitParticle.GlobalPosition = getOriginPos();
            hitParticle.GlobalRotation = attack.AttackRotation;
        }
    }



    public HealthSystem(int health, int maxHealth, Func<Vector2> getOriginPos, PackedScene? hitParticleScene = null, PackedScene? deathParticleScene = null)
    {
        this.Health = health;
        this.MaxHealth = maxHealth;
        this.hitParticleScene = hitParticleScene;
        this.deathParticleScene = deathParticleScene;
        this.getOriginPos = getOriginPos;
    }
}
