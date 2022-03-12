

using System;
using Godot;

public class HealthSystem
{
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
    bool dead = false;
    Action onDeath;

    public bool Damage(int ammount)
    {
        Health -= ammount;
        if (Health <= 0) return Die();
        return false;
    }

    bool Die()
    {
        if (dead) return true;
        GD.Print("Destroyed tile ");
        onDeath();
        return true;
    }

    public HealthSystem(int health, int maxHealth, Action onDeath)
    {
        Health = health;
        MaxHealth = maxHealth;
        this.onDeath = onDeath;
    }
}
