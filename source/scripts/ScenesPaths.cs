// Ścieżki do scen aby łatwo zmienić bez poprawiania w wielu miejscach 
using Godot;

public static class ScenesPaths
{
    // przykładowe użycie GetTree().Root.GetNode("Main").GetNode<Map>(ScenesPaths.MAP_PATH);
    public const string MAP_PATH = "Map";
    public const string PLAYER_PATH = "Player";

}
