using Godot;
using System;
using System.Collections.Generic;

public class Pistol : Sprite, IHandAble
{
    Node2D muzzlePivot = null!;
    AudioStreamPlayer? gunShotSound;
    PackedScene bulletScene = null!;
    BulletPool bulletPool = null!;

    int bulletBurstAmmount = 200;
    int bulletsShootInBurst = 0;

    public override void _EnterTree()
    {
        muzzlePivot = GetNode<Position2D>("Muzzle");
        bulletScene = (PackedScene)ResourceLoader.Load(Imports.BULLET_SCENE_PATH) ?? throw new Exception("Path not found");
        bulletPool = GetNode<BulletPool>("/root/BulletPool");

        gunShotSound = GetNode<AudioStreamPlayer>("ShootSound");
    }

    public override void _Process(float delta)
    {
        this.FlipV = Mathf.Abs(this.GlobalRotationDegrees) >= 90;

        if (Input.IsActionPressed(InputActions.SHOOT_INPUT) is false)
        {
            bulletsShootInBurst = 0;
        }
    }

    public bool Use(Arm arm, float delta)
    {
        return TryToFire(arm, delta);
    }

    bool TryToFire(Arm arm, float delta)
    {
        if (bulletsShootInBurst >= bulletBurstAmmount) return false;

        var bullet = bulletPool.GetBulletFromPool();
        if (bullet is null) return false;

        bullet.Position = muzzlePivot.GlobalPosition + arm.ArmParent.Movement.Motion * delta;
        bullet.Rotation = arm.Rotation;

        var rotatedArmDir = new Vector2(1, 0).Rotated(arm.Rotation);

        bullet.TryToFireBullet(rotatedArmDir, arm);
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
