using Godot;
using System;
using System.Collections.Generic;

public class Pistol : Sprite, IHandAble
{
    Node2D muzzlePivot = null!;
    AudioStreamPlayer? gunShotSound;
    PackedScene bulletScene = null!;
    BulletPool bulletPool = null!;

    int bulletBurstAmmount = 20;
    int bulletsShootInBurst = 0;

    public override void _EnterTree()
    {
        muzzlePivot = GetNode<Position2D>("Muzzle");
        bulletScene = (PackedScene)ResourceLoader.Load(Imports.bulletScenePath) ?? throw new Exception("Path not found");
        bulletPool = GetNode<BulletPool>("/root/BulletPool");

        gunShotSound = GetNode<AudioStreamPlayer>("ShootSound");
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionPressed(InputActions.ShootAction) is false)
        {
            bulletsShootInBurst = 0;
        }
    }

    public bool Use(Arm arm)
    {
        return TryToFire(arm);
    }

    bool TryToFire(Arm arm)
    {
        if (bulletsShootInBurst >= bulletBurstAmmount) return false;

        var bullet = bulletPool.GetBulletFromPool();
        if (bullet is null) return false;

        bullet.Position = muzzlePivot.GlobalPosition;
        bullet.Rotation = arm.Rotation;

        var rotatedArmDir = new Vector2(1, 0).Rotated(arm.Rotation);

        bullet.FireBullet(rotatedArmDir);
        gunShotSound?.Play();
        bulletsShootInBurst++;
        var player = arm.ArmParent as Player;

        if (player is not null)
        {
            player.ShakeCamera.AddShake(0.5f, 0.1f);
        }

        bulletPool.ReturnBulletToPool(bullet);
        return true;
    }
}
