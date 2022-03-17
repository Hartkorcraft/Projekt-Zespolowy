using Godot;
using System;

public class Enemy : Entity
{
    public override void _Ready()
    {
        Movement = new Movement(maxSpeed: 50);
        var bloodParticleScene = (PackedScene)ResourceLoader.Load(Imports.bloodParticlePath);
        HealthSystem = new HealtSystem_Entity(8, 8, () => GlobalPosition, this, bloodParticleScene);
    }

    protected override Vector2 GetMovementAxis()
    {
        return Vector2.Right;
    }
}
