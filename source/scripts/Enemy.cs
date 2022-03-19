using Godot;
using System;

public class Enemy : Entity
{
    public NpcPathfinding NpcPathfinding { get; private set; } = new NpcPathfinding();


    public override void _Ready()
    {
        Movement = new Movement(maxSpeed: 50);
        var bloodParticleDeathScene = (PackedScene)ResourceLoader.Load(Imports.bloodParticleDeathPath);
        var bloodParticleHitScene = (PackedScene)ResourceLoader.Load(Imports.bloodParticleHitPath);
        HealthSystem = new HealtSystem_Entity(8, 8, () => GlobalPosition, this, hitParticleScene: bloodParticleHitScene, deathParticleScene: bloodParticleDeathScene);
    }

    protected override Vector2 GetMovementAxis()
    {
        return Vector2.Right;
    }
}
