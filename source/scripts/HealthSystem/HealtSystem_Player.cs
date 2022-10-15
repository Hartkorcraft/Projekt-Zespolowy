
using System;
using Godot;

public class HealtSystem_Player : HealthSystem
{
    Player parent;
    HealthBar HealthBar;
    int healthPercent => (int)(MaxHealth * ((float)Health / (float)MaxHealth));

    public HealtSystem_Player(int health, int maxHealth, Func<Vector2> getOriginPos, Player parent, HealthBar healthBar, PackedScene? hitParticleScene = null, PackedScene? deathParticleScene = null) : base(health, maxHealth, getOriginPos, hitParticleScene, deathParticleScene)
    {
        this.parent = parent;
        this.HealthBar = healthBar;
        healthBar.UpdateHealthBar(healthPercent);
    }

    public override void OnHeal()
    {
        HealthBar.UpdateHealthBar(healthPercent);
    }
    public override void OnHit(IAttack attack)
    {
        //base.OnHit(attack);
        HealthBar.UpdateHealthBar(healthPercent);
    }

    public override void OnDeath(IAttack attack)
    {
        parent.GetNode<AnimationPlayer>("AnimationPlayer").CurrentAnimation = "[stop]";
        parent.GetNode<Sprite>("Sprite").Frame = 5;
        parent.GetNode<Arm>("Arm").Visible = false;
        //base.OnDeath(attack);
        //parent.QueueFree();
    }
}
