using Godot;
using System;

public class Enemy : Entity
{
    public NpcPathfinding NpcPathfinding { get; private set; } = null!;
    Player player = null!;

    public override void _Ready()
    {
        Movement = new Movement(maxSpeed: 50);
        var bloodParticleDeathScene = (PackedScene)ResourceLoader.Load(Imports.BLOOD_PARTICLE_DEATH_PATH);
        var bloodParticleHitScene = (PackedScene)ResourceLoader.Load(Imports.BLOOD_PARTICLE_HIT_PATH);

        HealthSystem = new HealtSystem_Entity(8, 8, () => GlobalPosition, this, hitParticleScene: bloodParticleHitScene, deathParticleScene: bloodParticleDeathScene);

        player = GetTree().Root.GetNode("Main").GetNode<Player>(ScenesPaths.PLAYER_PATH);

        var map = GetTree().Root.GetNode("Main").GetNode<Map>(ScenesPaths.MAP_PATH);
        NpcPathfinding = new NpcPathfinding(map);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);


    }

    protected override Vector2 GetMovementAxis()
    {
        return NpcPathfinding.GetDirToPlayer(this, player);
    }
}
