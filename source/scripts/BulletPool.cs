using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// Pula pocisków aby nie tworzyć masowo nowych obiektów
//TODO tymczasowo autoload przydałoby się zmienić to 
public class BulletPool : Node2D
{
    int poolSize = 500;
    Queue<Bullet> bulletPool = new Queue<Bullet>();
    PackedScene bulletScene = null!;

    public override void _EnterTree()
    {
        bulletScene = (PackedScene)ResourceLoader.Load(Imports.BULLET_SCENE_PATH) ?? throw new Exception("Path not found");

        for (int i = 0; i < poolSize; i++)
        {
            var bullet = (Bullet)bulletScene.Instance();
            AddChild(bullet);
            bullet.Position = new Vector2(-500, -500);
            bullet.Hide();
            bulletPool.Enqueue(bullet);
        }
    }

    public Bullet? GetBulletFromPool()
        => bulletPool.Any() ? bulletPool.Dequeue() : null;

    public void ReturnBulletToPool(Bullet bullet)
        => bulletPool.Enqueue(bullet);

}
