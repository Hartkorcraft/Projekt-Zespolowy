using Godot;
using System;

public class KillsLabel : Label
{
    int kills = -1;

    public override void _Ready()
    {
        UpdateLabel();
        HealtSystem_Entity.OnDeathEvent += UpdateLabel;
    }

    void UpdateLabel()
    {
        kills++;
        Text = "Kills: " + kills;
    }

}
