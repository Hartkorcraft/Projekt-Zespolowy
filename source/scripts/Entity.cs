using Godot;
using System;

public abstract class Entity : KinematicBody2D, IMovement, IHealthSystem
{
    public Sprite Sprite { get; private set; } = null!;
    public Movement Movement { get; protected set; } = new Movement();
    public HealthSystem HealthSystem { get; protected set; } = null!;

    protected AnimationPlayer? animationPlayer;

    public override void _Ready()
    {
        Sprite = GetNode<Sprite>("Sprite") ?? throw new Exception("Sprite is null");
        var bloodParticleScene = (PackedScene)ResourceLoader.Load(Imports.BLOOD_PARTICLE_DEATH_PATH);
        HealthSystem = new HealtSystem_Entity(5, 5, () => GlobalPosition, this, bloodParticleScene);
    }

    public override void _PhysicsProcess(float delta)
    {
        Movement.Update(this, GetMovementAxis(), delta, animationPlayer);
    }

    protected virtual Vector2 GetMovementAxis() => Vector2.Zero;

}
