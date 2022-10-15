using Godot;
using System;

public class SpawnPoint : Sprite
{

    [Export] float spawnTime = 5f;
    float spawnStart;
    float spawnEnd;
    Node enemyPool = null!;
    PackedScene testEnemyScene = null!;
    PackedScene shooterEnemyScene = null!;
    PackedScene shooterShotgunEnemyScene = null!;
    PackedScene healthPack = null!;
    Node wallTiles = null!;

    public override void _EnterTree()
    {
        spawnStart = OS.GetUnixTime();
        spawnEnd = spawnStart + spawnTime;
        enemyPool = ScenesPaths.GetScene<Node>(this, ScenesPaths.ENEMIE_POOL);
        testEnemyScene = (PackedScene)ResourceLoader.Load(Imports.ENEMY_TEST_SCENE);
        shooterEnemyScene = (PackedScene)ResourceLoader.Load(Imports.ENEMY_SHOOTER_SCENE);
        shooterShotgunEnemyScene = (PackedScene)ResourceLoader.Load(Imports.ENEMY_SHOOTER_SHOTGUN_SCENE);
        healthPack = (PackedScene)ResourceLoader.Load(Imports.HEALTH_PACK);
        wallTiles = this.GetScene<Node>("Map/Tiles_Walls");
    }


    public override void _Process(float delta)
    {
        var time = OS.GetUnixTime();
        this.Frame = this.Frame < 9 ? this.Frame + 1 : 0;
    }

    public async void Spawn()
    {
        this.Visible = true;
        await ToSignal(GetTree().CreateTimer(spawnTime), "timeout");
        Node2D enemy = Utils.rng.Next(0, 100) > 30 ? testEnemyScene.Instance<Enemy>() :
                      Utils.rng.Next(0, 100) > 30 ? shooterEnemyScene.Instance<ShootingEnemy>() :
                      Utils.rng.Next(0, 100) > 90 ? healthPack.Instance<HealthPack>() :
                                                    shooterShotgunEnemyScene.Instance<ShootingEnemy_Shotgun>();

        enemy.Position = this.Position + new Vector2(this.GetRect().Size.x / 2, this.GetRect().Size.y);
        if (enemy is Enemy) enemyPool.AddChild(enemy);
        else
        {
            wallTiles.AddChild(enemy);
        }
        QueueFree();
    }

}
