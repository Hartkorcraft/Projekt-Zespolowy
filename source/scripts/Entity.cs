using Godot;
using System;

public class Entity : KinematicBody2D
{
    public Sprite Sprite { get; private set; } = null!;

    public override void _EnterTree()
    {
        Sprite = GetNode<Sprite>("Sprite") ?? throw new Exception("Sprite is null");
    }
}
