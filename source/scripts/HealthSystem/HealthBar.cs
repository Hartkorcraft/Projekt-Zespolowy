using Godot;
using System;

public class HealthBar : TextureProgress
{

    public void UpdateHealthBar(int ammount)
    {
        this.Value = ammount;
    }
}
