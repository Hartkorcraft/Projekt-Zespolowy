using Godot;
using System;

public class Enemy : Entity
{
    public NpcMind NpcMind { get; private set; } = null!;
    Player player = null!;

    public override void _Ready()
    {
        Movement = new Movement(maxSpeed: 100, acceleration: 400);
        var bloodParticleDeathScene = (PackedScene)ResourceLoader.Load(Imports.BLOOD_PARTICLE_DEATH_PATH);
        var bloodParticleHitScene = (PackedScene)ResourceLoader.Load(Imports.BLOOD_PARTICLE_HIT_PATH);

        HealthSystem = new HealtSystem_Entity(8, 8, () => GlobalPosition, this, hitParticleScene: bloodParticleHitScene, deathParticleScene: bloodParticleDeathScene);

        player = this.GetScene<Player>(ScenesPaths.PLAYER_PATH);
        var map = this.GetScene<Map>(ScenesPaths.MAP_PATH);

        NpcMind = new NpcMind(map);
    }

    public override void _PhysicsProcess(float delta)
    {
        //NpcMind.Update(this, player, Movement, delta);
        Movement.Update(this, NpcMind.GetMovementDir(this, player), delta);

        if (animationPlayer is not null)
            Movement.AnimateWalking(animationPlayer, this);
    }
}
