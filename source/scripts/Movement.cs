using Godot;
using System;

public class Movement
{
    [Export] float maxSpeed = 200f;
    [Export] float acceleration = 500;
    [Export] float frictionMod = 3;
    [Export] float accelerationMod = 10;

    public Vector2 Motion { get; private set; } = Vector2.Zero;
    Vector2 motionDir = Vector2.Zero;

    public Movement() { }
    public Movement(float maxSpeed = 200, float acceleration = 500, float frictionMod = 3, float accelerationMod = 10)
    {
        this.maxSpeed = maxSpeed;
        this.acceleration = acceleration;
        this.frictionMod = 3;
        this.accelerationMod = 10;
    }

    public void Update(Entity entity, Vector2 moveDir, float delta)
    {
        var axis = moveDir;

        Motion = ApplyFriction(Motion, acceleration * frictionMod * delta);
        Motion = ApplyMovement(Motion, axis * acceleration * accelerationMod * delta);

        if (Motion != Vector2.Zero)
            motionDir = Motion.Normalized();
        Motion = entity.MoveAndSlide(Motion);
    }

    Vector2 ApplyFriction(Vector2 motion, float frictionAmount)
        => motion.Length() > frictionAmount ? motion - motion.Normalized() * frictionAmount : Vector2.Zero;

    Vector2 ApplyMovement(Vector2 motion, Vector2 aceleration)
        => (motion + aceleration).Clamped(maxSpeed);

    public void AnimateWalking(AnimationPlayer animationPlayer, Entity entity)
    {
        if (Motion == Vector2.Zero)
        {
            animationPlayer.Stop(reset: false);
            return;
        }

        if (animationPlayer.IsPlaying() is false)
            animationPlayer.Play();

        entity.Sprite.Scale = Motion.x < 0 ? new Vector2(-1, 1) : new Vector2(1, 1);
    }
}





