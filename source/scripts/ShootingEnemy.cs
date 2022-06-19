using System;
using Godot;

public class ShootingEnemy : Enemy
{
    public override void _Ready()
    {
        base._Ready();
        var bulletScene = (PackedScene)ResourceLoader.Load(Imports.BULLET_SCENE_PATH) ?? throw new Exception("Path not found");
        var bulletPool = GetNode<BulletPool>("/root/BulletPool");
        this.NpcMind = new ShooterMind(map, player.Position, bulletScene, bulletPool);
    }
}
