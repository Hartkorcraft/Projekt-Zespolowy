using Godot;
using System;

public class Arm : Position2D
{
    IHandAble itemInHand = null!;
    int itemInHandIndex = 0;
    public Entity ArmParent { get; private set; } = null!;

    public override void _EnterTree()
    {
        SelectNextItemInHand();
        ArmParent = (Entity)GetParent();
    }

    public void SelectNextItemInHand()
    {
        var asNode = itemInHand as Node2D;
        if (asNode is not null) asNode.Visible = false;

        itemInHandIndex = ++itemInHandIndex >= GetChildCount() ? 0 : ++itemInHandIndex;
        itemInHand = GetChild<IHandAble>(itemInHandIndex);

        asNode = itemInHand as Node2D;
        if (asNode is not null) asNode.Visible = true;
    }

    public override void _PhysicsProcess(float delta)
    {
        PointArm();
    }

    public bool TryToUseItemInHand(float delta)
    {
        return itemInHand.Use(this, delta);
    }

    void PointArm()
    {
        this.LookAt(GetGlobalMousePosition());
        //Scale = this.RotationDegrees % 360 is > -275 and < -90 ? Scale = new Vector2(1, -1) : Vector2.One;
    }

}
