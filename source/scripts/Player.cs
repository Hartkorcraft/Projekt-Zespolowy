using Godot;
using System;

public class Player : Entity, IMovement
{
    public ShakeCamera ShakeCamera { get; private set; } = null!;

    [Export] float bulletTimeSpeed = 0.2f;
    [Export] float bulletTimeDecay = 0.2f;

    Arm arm = null!;

    public override void _EnterTree()
    {
        base._EnterTree();
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer") ?? throw new Exception("Animation player is null");
        arm = GetNode<Arm>("Arm");
        ShakeCamera = GetNode<ShakeCamera>("Camera2D");
    }

    public override void _Process(float delta)
    {
        var usedItem = TryToUseItemInHand(delta);
        BulletTime(usedItem);
    }

    private void BulletTime(bool usedItem = false)
    {
        if (Movement.Motion == Vector2.Zero && usedItem is false) Engine.TimeScale = bulletTimeSpeed;
        else Engine.TimeScale = Mathf.Min(Engine.TimeScale + bulletTimeDecay, 1.0f);
    }

    protected override Vector2 GetMovementAxis()
    {
        var axis = Vector2.Zero;
        axis.x = (Input.IsActionPressed(InputActions.MoveRightActionInput) ? 1 : 0) - (Input.IsActionPressed(InputActions.MoveLeftActionInput) ? 1 : 0);
        axis.y = (Input.IsActionPressed(InputActions.MoveDownActionInput) ? 1 : 0) - (Input.IsActionPressed(InputActions.MoveUpActionInput) ? 1 : 0);
        return axis.Normalized();
    }

    bool TryToUseItemInHand(float delta)
    {
        var tryingToShoot = Input.IsActionPressed(InputActions.ShootAction);
        if (tryingToShoot)
            return arm.TryToUseItemInHand(delta);
        return false;
    }

}
