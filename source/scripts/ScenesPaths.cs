// Ścieżki do scen aby łatwo zmienić bez poprawiania w wielu miejscach 
using Godot;

public static class ScenesPaths
{
    // przykładowe użycie GetTree().Root.GetNode("Main").GetNode<Map>(ScenesPaths.MAP_PATH);
    public const string MAP_PATH = "Map";
    public const string PLAYER_PATH = "Map/Player";
    public const string PREFABS = "Prefabs";
    public const string ENEMIE_POOL = "Enemies";
    public const string HEALTH_BAR = "CanvasLayer/HealthBar";
    public const string MUSIC_PLAYER = "Music";
    public const string DEATH_MUSIC_PLAYER = "Music2";

    public static T GetScene<T>(this Node node, string path) where T : Node
        => node.GetTree().Root.GetNode("Main").GetNode<T>(path) ?? throw new System.Exception("Invalid path");

}
