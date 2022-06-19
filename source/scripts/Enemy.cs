using Godot;
using System;

public class Enemy : Entity
{
    public NpcMind NpcMind { get; protected set; } = null!;
    protected Player player = null!;
    protected Map map = null!;
    IAttack meleeAttack = null!;

    public override void _Ready()
    {
        Movement = new Movement(maxSpeed: 60, acceleration: 200);
        var bloodParticleDeathScene = (PackedScene)ResourceLoader.Load(Imports.BLOOD_PARTICLE_DEATH_PATH);
        var bloodParticleHitScene = (PackedScene)ResourceLoader.Load(Imports.BLOOD_PARTICLE_HIT_PATH);

        meleeAttack = new MeleeAttack();

        HealthSystem = new HealtSystem_Entity(8, 8, () => GlobalPosition, this, hitParticleScene: bloodParticleHitScene, deathParticleScene: bloodParticleDeathScene);

        player = this.GetScene<Player>(ScenesPaths.PLAYER_PATH);
        map = this.GetScene<Map>(ScenesPaths.MAP_PATH);

        NpcMind = new NpcMind(map, player.Position);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Godot.Object.IsInstanceValid(player) is false) return;

        //NpcMind.Update(this, player, Movement, delta);
        Movement.Update(this, NpcMind.GetMovementDir(this, player), delta);

        if (animationPlayer is not null)
            Movement.AnimateWalking(animationPlayer, this);

        if (this.Position.DistanceTo(player.Position) <= Map.TILE_SIZE)
        {
            player.HealthSystem.Damage(meleeAttack);
        }
        
        NpcMind.Update(this, player, delta);
    }
}

public class EnemyRange : Enemy
{

}

public class MeleeAttack : IAttack
{
    public int Damage => 1;
    public float AttackRotation => 0;


}