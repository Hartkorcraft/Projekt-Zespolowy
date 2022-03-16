using Godot;
using System;

public class Enemy : Entity
{
    public override void _Ready()
    {
        Movement = new Movement(maxSpeed: 50);
    }
    protected override Vector2 GetMovementAxis()
    {
        return Vector2.Right;
    }
}
