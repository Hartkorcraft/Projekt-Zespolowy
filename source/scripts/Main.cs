using Godot;
using System;

public class Main : Node2D
{
    public static Node2D ParticlePool { get; private set; } = null!;

    public override void _EnterTree()
    {
        ParticlePool = GetNode<Node2D>("ParticlePool");
    }

}
