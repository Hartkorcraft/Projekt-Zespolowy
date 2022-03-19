using Godot;
using System;

public class Arm : Position2D
{
    IHandAble? itemInHand;
    public Entity ArmParent { get; private set; } = null!;

    public override void _EnterTree()
    {
        itemInHand = GetChild<IHandAble>(0);
        ArmParent = (Entity)GetParent();
    }

    public override void _PhysicsProcess(float delta)
    {
        PointArm();
    }

    public bool TryToUseItemInHand(float delta)
    {
        return itemInHand?.Use(this, delta) ?? false;
    }

    void PointArm()
    {
        this.LookAt(GetGlobalMousePosition());
        //Scale = this.RotationDegrees % 360 is > -275 and < -90 ? Scale = new Vector2(1, -1) : Vector2.One;
    }

}
