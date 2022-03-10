using Godot;
using System;

public class Entity : KinematicBody2D, IMovement
{
    public Sprite Sprite { get; private set; } = null!;
    public Movement Movement { get; private set; } = new Movement();

    public override void _EnterTree()
    {
        Sprite = GetNode<Sprite>("Sprite") ?? throw new Exception("Sprite is null");
    }
}
