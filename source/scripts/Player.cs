using Godot;
using System;

public class Player : Entity
{
    public ShakeCamera ShakeCamera { get; private set; } = null!;
  
    Movement movement = new Movement();
    AnimationPlayer animationPlayer = null!;
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
        TryToUseItemInHand();
    }

    public override void _PhysicsProcess(float delta)
    {
        movement.Update(this, GetInputAxis(), delta, animationPlayer);
    }

    Vector2 GetInputAxis()
    {
        var axis = Vector2.Zero;
        axis.x = (Input.IsActionPressed(InputActions.MoveRightActionInput) ? 1 : 0) - (Input.IsActionPressed(InputActions.MoveLeftActionInput) ? 1 : 0);
        axis.y = (Input.IsActionPressed(InputActions.MoveDownActionInput) ? 1 : 0) - (Input.IsActionPressed(InputActions.MoveUpActionInput) ? 1 : 0);
        return axis.Normalized();
    }

    void TryToUseItemInHand()
    {
        var tryingToShoot = Input.IsActionPressed(InputActions.ShootAction);
        if (tryingToShoot)
            arm.TryToUseItemInHand();
    }

}
