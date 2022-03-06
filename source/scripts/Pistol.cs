using Godot;
using System;
using System.Collections.Generic;

public class Pistol : Sprite, IHandAble
{
    Node2D muzzlePivot = null!;
    AudioStreamPlayer? gunShotSound;
    PackedScene bulletScene = null!;
    BulletPool bulletPool = null!;

    public override void _EnterTree()
    {
        muzzlePivot = GetNode<Position2D>("Muzzle");
        bulletScene = (PackedScene)ResourceLoader.Load(Imports.bulletScenePath) ?? throw new Exception("Path not found");
        bulletPool = GetNode<BulletPool>("/root/BulletPool");

        gunShotSound = GetNode<AudioStreamPlayer>("ShootSound");
    }

    public void Use(Arm arm)
    {
        Fire(arm);
    }

    void Fire(Position2D armPivot)
    {
        var bullet = bulletPool.GetBulletFromPool();
        if (bullet is null) return;

        bullet.Position = muzzlePivot.GlobalPosition;
        bullet.Rotation = armPivot.Rotation;

        var rotatedArmDir = new Vector2(1, 0).Rotated(armPivot.Rotation);

        bullet.Fire(rotatedArmDir);
        gunShotSound?.Play();

        bulletPool.ReturnBulletToPool(bullet);
    }
}
