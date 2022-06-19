using Godot;
using System;

// Główna scena gry
public class Main : Node
{
    public static Random rng = new Random();
    public static Node ParticlePool { get; private set; } = null!;

    public static WaveSystem WaveSystem { get; private set; } = null!;

    public override void _EnterTree()
    {
        ParticlePool = GetNode<Node>("ParticlePool");
        WaveSystem = GetNode<WaveSystem>("WaveSystem");
    }




}
