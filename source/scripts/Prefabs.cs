
using Godot;

public class Prefabs : Node
{
    public PackedScene HitWallParticle { get; private set; } = null!;
    public PackedScene DeathWallParticle { get; private set; } = null!;

    public override void _EnterTree()
    {
        HitWallParticle = (PackedScene)ResourceLoader.Load(Imports.WALL_PARTICLE_PATH);
        DeathWallParticle = (PackedScene)ResourceLoader.Load(Imports.WALL_PARTICLE_PATH);

        GD.Print(HitWallParticle);
    }
}
