using Godot;
using System;

public class Leafs : Particles2D
{

    public override void _Ready()
    {
        Visible = false;
        RLMapGenerator.OnMapGenerationFinished += () => Visible = true;
    }

}
