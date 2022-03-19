using System;
using Godot;

public class HealthSystem_Tile : HealthSystem
{
    Map map;
    Tile newTile;

    public HealthSystem_Tile(int health, int maxHealth, Func<Vector2> getOriginPos, Map map, Tile newTile, PackedScene? hitParticleScene = null) : base(health, maxHealth, getOriginPos, hitParticleScene)
    {
        this.map = map;
        this.newTile = newTile;
    }

    public override void OnDeath(Bullet bullet)
    {
        base.OnDeath(bullet);
        map.SetCell(newTile);
    }
}