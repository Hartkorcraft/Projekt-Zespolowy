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

    void Fire(Arm arm)
    {
        var bullet = bulletPool.GetBulletFromPool();
        if (bullet is null) return;

        bullet.Position = muzzlePivot.GlobalPosition;
        bullet.Rotation = arm.Rotation;

        var rotatedArmDir = new Vector2(1, 0).Rotated(arm.Rotation);

        bullet.FireBullet(rotatedArmDir);
        gunShotSound?.Play();
        var player = arm.CameraParent as Player;

        if (player is not null)
        {
            player.ShakeCamera.AddShake(0.5f, 0.1f);
        }

        bulletPool.ReturnBulletToPool(bullet);
    }
}
