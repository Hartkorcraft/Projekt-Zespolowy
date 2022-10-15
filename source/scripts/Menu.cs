using Godot;
using System;

public class Menu : Control
{

    void OnPlayButton()
    {
        GD.Print("Play");
        GetTree().ChangeScene("res://source/scenes/Main.tscn");
    }

    void OnExitButton()
    {
        GD.Print("Exit");

        GetTree().Quit();
    }

}
