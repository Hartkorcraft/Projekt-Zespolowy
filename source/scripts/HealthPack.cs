using Godot;
using System;

public class HealthPack : Sprite
{
    void on_body_entered(Node node)
    {
        if (node is Player player)
        {
            player.HealthSystem.Heal(30);
            QueueFree();
        }
    }
}
