using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// Pula pocisków aby nie tworzyć masowo nowych obiektów
public class BulletPool : Node2D
{
    int poolSize = 100;
    Queue<Bullet> bulletPool = new Queue<Bullet>();
    PackedScene bulletScene = null!;

    public override void _EnterTree()
    {
        bulletScene = (PackedScene)ResourceLoader.Load(Imports.bulletScenePath) ?? throw new Exception("Path not found");

        for (int i = 0; i < poolSize; i++)
        {
            var bullet = (Bullet)bulletScene.Instance();
            AddChild(bullet);
            bullet.SetProcess(false);
            bullet.Hide();
            bulletPool.Enqueue(bullet);
        }
    }

    public Bullet? GetBulletFromPool()
        => bulletPool.Any() ? bulletPool.Dequeue() : null;

    public void ReturnBulletToPool(Bullet bullet)
        => bulletPool.Enqueue(bullet);

}
