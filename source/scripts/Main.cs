using Godot;
using System;

// Główna scena gry
public class Main : Node
{
    public static Node ParticlePool { get; private set; } = null!;

    public override void _EnterTree()
    {
        ParticlePool = GetNode<Node>("ParticlePool");
    }

}
