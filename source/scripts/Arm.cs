using Godot;
using System;

public class Arm : Position2D
{
    IHandAble? itemInHand;

    public override void _EnterTree()
    {
        itemInHand = GetChild<IHandAble>(0);
    }

    public override void _Process(float delta)
    {
        PointArm();
    }

    public void TryToUseItemInHand()
    {
        itemInHand?.Use(this);
    }

    void PointArm()
    {
        this.LookAt(GetGlobalMousePosition());
        Scale = this.RotationDegrees % 360 is > -275 and < -90 ? Scale = new Vector2(1, -1) : Vector2.One;
    }

}
