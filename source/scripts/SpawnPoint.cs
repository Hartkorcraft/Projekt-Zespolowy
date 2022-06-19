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

    public override void _EnterTree()
    {
        spawnStart = OS.GetUnixTime();
        spawnEnd = spawnStart + spawnTime;
        enemyPool = ScenesPaths.GetScene<Node>(this, ScenesPaths.ENEMIE_POOL);
        testEnemyScene = (PackedScene)ResourceLoader.Load(Imports.ENEMY_TEST_SCENE);
        shooterEnemyScene = (PackedScene)ResourceLoader.Load(Imports.ENEMY_SHOOTER_SCENE);
        shooterShotgunEnemyScene = (PackedScene)ResourceLoader.Load(Imports.ENEMY_SHOOTER_SHOTGUN_SCENE);
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
        Enemy enemy = Utils.rng.Next(0, 100) > 30 ? testEnemyScene.Instance<Enemy>() :
                      Utils.rng.Next(0, 100) > 30 ? shooterEnemyScene.Instance<ShootingEnemy>() :
                                                    shooterShotgunEnemyScene.Instance<ShootingEnemy_Shotgun>();

        enemy.Position = this.Position + new Vector2(this.GetRect().Size.x / 2, this.GetRect().Size.y);
        enemyPool.AddChild(enemy);
        QueueFree();
    }

}
