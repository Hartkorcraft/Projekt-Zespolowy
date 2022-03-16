using Godot;
using System;

public abstract class Entity : KinematicBody2D, IMovement
{
    public Sprite Sprite { get; private set; } = null!;
    public Movement Movement { get; protected set; } = new Movement();
    protected AnimationPlayer animationPlayer = null!;

    public override void _EnterTree()
    {
        Sprite = GetNode<Sprite>("Sprite") ?? throw new Exception("Sprite is null");
    }

    public override void _PhysicsProcess(float delta)
    {
        Movement.Update(this, GetMovementAxis(), delta, animationPlayer);
    }

    protected virtual Vector2 GetMovementAxis() => Vector2.Zero;

}
