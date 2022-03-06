using Godot;
using System;

public class Movement
{
    [Export] float maxSpeed = 250f;
    [Export] float acceleration = 500;
    [Export] float frictionMod = 3;
    [Export] float accelerationMod = 10;

    Vector2 motion = Vector2.Zero;
    Vector2 motionDir = Vector2.Zero;

    public void Update(Entity entity, Vector2 moveDir, float delta, AnimationPlayer? animationPlayer = null)
    {
        var axis = moveDir;

        motion = ApplyFriction(motion, acceleration * frictionMod * delta);
        motion = ApplyMovement(motion, axis * acceleration * accelerationMod * delta);

        if (animationPlayer is not null)
            AnimateWalking(animationPlayer, entity, motion);

        if (motion != Vector2.Zero)
            motionDir = motion.Normalized();
        motion = entity.MoveAndSlide(motion);
    }

    Vector2 ApplyFriction(Vector2 motion, float frictionAmount)
        => motion.Length() > frictionAmount ? motion - motion.Normalized() * frictionAmount : Vector2.Zero;

    Vector2 ApplyMovement(Vector2 motion, Vector2 aceleration)
        => (motion + aceleration).Clamped(maxSpeed);

    private void AnimateWalking(AnimationPlayer animationPlayer, Entity entity, Vector2 motion)
    {
        if (motion == Vector2.Zero)
        {
            animationPlayer.Stop(reset: false);
            return;
        }

        if (animationPlayer.IsPlaying() is false)
            animationPlayer.Play();

        entity.Sprite.Scale = motion.x < 0 ? new Vector2(-1, 1) : new Vector2(1, 1);
    }
}





