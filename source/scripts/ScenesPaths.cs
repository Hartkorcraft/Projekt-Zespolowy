// Ścieżki do scen aby łatwo zmienić bez poprawiania w wielu miejscach 
using Godot;

public static class ScenesPaths
{
    // przykładowe użycie GetTree().Root.GetNode("Main").GetNode<Map>(ScenesPaths.MAP_PATH);
    public const string MAP_PATH = "Map";
    public const string PLAYER_PATH = "Map/Player";
    public const string PREFABS = "Prefabs";
    public const string ENEMIE_POOL = "Enemies";
    
    public static T GetScene<T>(this Node node, string path) where T : Node
        => node.GetTree().Root.GetNode("Main").GetNode<T>(path) ?? throw new System.Exception("Invalid path");

}
